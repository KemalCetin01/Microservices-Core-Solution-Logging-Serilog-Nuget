using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace MS.Services.Core.Logging.Enrich;


public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        var correlationId = httpContext.Request.Headers["CorrelationId"].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            httpContext.Request.Headers.Add("CorrelationId", correlationId);
        }

        var correlationIdProperty = new LogEventProperty("CorrelationId", new ScalarValue(correlationId));
        logEvent.AddPropertyIfAbsent(correlationIdProperty);
    }
}