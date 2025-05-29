using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using PulseTrace.Config;

namespace PulseTrace.Middleware
{
    public class PulseTraceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PulseTraceOptions _options;
        private readonly List<TraceEntry> _traceEntries = new();

        public PulseTraceMiddleware(RequestDelegate next, IOptions<PulseTraceOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            if (_options.IncludeControllers != null && _options.IncludeControllers.Any())
            {
                var match = _options.IncludeControllers.Any(f => path.Contains(f, StringComparison.OrdinalIgnoreCase));
                if (!match)
                {
                    await _next(context);
                    return;
                }
            }

            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds >= _options.ThresholdMilliseconds)
            {
                var entry = new TraceEntry
                {
                    Path = path,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                };
                lock (_traceEntries)
                {
                    _traceEntries.Add(entry);
                }
            }
        }

        public string GetTraceDataJson()
        {
            lock (_traceEntries)
            {
                return JsonSerializer.Serialize(_traceEntries);
            }
        }

        private class TraceEntry
        {
            public string Path { get; set; }
            public long ElapsedMilliseconds { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
