using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MS.Services.Core.Logging.Enrich;
using Serilog;
using Serilog.Formatting.Elasticsearch;

namespace MS.Services.Core.Logging;

public static class ServiceRegistration
{
    public static WebApplicationBuilder UseSerilogLogging(this WebApplicationBuilder builder,IConfiguration configuration)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<CorrelationIdEnricher>();

        builder.Logging.ClearProviders();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;


        builder.Host.UseSerilog((_, serviceProvider, config) =>
        {
            var httpContext = serviceProvider.GetRequiredService<CorrelationIdEnricher>();

            config
                .Enrich.With(httpContext)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("environment", environment)
                .Enrich.WithCorrelationIdHeader("CorrelationId")
                .WriteTo.Console(new ElasticsearchJsonFormatter())
                .ReadFrom.Configuration(configuration);
        });
        return builder;
    }
}
