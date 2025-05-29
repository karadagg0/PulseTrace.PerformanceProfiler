using Microsoft.Extensions.DependencyInjection;
using PulseTrace.Config;
namespace PulseTrace.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPulseTrace(this IServiceCollection services, Action<PulseTraceOptions>? configure = null)
    {
        if (configure != null)
            services.Configure(configure);

        services.AddScoped<PulseTracer>();

        return services;
    }
}
