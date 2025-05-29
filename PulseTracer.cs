using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using PulseTrace.Config;

namespace PulseTrace;

public class PulseTracer : IActionFilter
{
    private readonly PulseTraceOptions _options;
    private static readonly List<object> _profileResults = new();

    public PulseTracer(IOptions<PulseTraceOptions> options)
    {
        _options = options.Value;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var action = context.ActionDescriptor as ControllerActionDescriptor;
        if (action == null) return;

        if (!ShouldProfile(action)) return;

        var sw = new Stopwatch();
        context.HttpContext.Items["__pulse_sw"] = sw;
        sw.Start();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items["__pulse_sw"] is not Stopwatch sw) return;

        sw.Stop();

        var action = context.ActionDescriptor as ControllerActionDescriptor;
        if (action == null) return;

        if (!ShouldProfile(action)) return;

        if (sw.ElapsedMilliseconds >= _options.ThresholdMilliseconds)
        {
            var result = new
            {
                Controller = action.ControllerName,
                Action = action.ActionName,
                DurationMs = sw.ElapsedMilliseconds,
                Time = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"),
                Path = context.HttpContext.Request.Path.Value,
                Method = context.HttpContext.Request.Method,
                Query = context.HttpContext.Request.QueryString.Value,
                StatusCode = context.HttpContext.Response.StatusCode,
                User = context.HttpContext.User.Identity?.Name ?? "Anonymous"
            };

            _profileResults.Add(result);
            try
            {
                File.WriteAllText(_options.OutputJsonPath!, JsonSerializer.Serialize(_profileResults, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                File.WriteAllText(_options.OutputJsonPath!,$"Logging err: {ex.Message}");
            }
        }
    }

    private bool ShouldProfile(ControllerActionDescriptor action)
    {
        if (_options.IncludeControllers.Any() && !_options.IncludeControllers.Contains(action.ControllerName))
            return false;

        var hasProfile = action.MethodInfo.GetCustomAttributes(typeof(Attributes.ProfileAttribute), true).Any();
        return hasProfile;
    }
}
