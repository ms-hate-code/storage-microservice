using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web
{
    public static class CorrelationExtension
    {
        public const string CORRELATION_ID = "CorrelationId";

        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.TryGetValue(CORRELATION_ID, out var correlationId))
                    correlationId = context.GetCorrelationId().ToString();

                context.Items[CORRELATION_ID] = correlationId;

                await next();
            });
        }

        public static Guid GetCorrelationId(this HttpContext context)
        {
            context.Items.TryGetValue(CORRELATION_ID, out var correlationId);

            return string.IsNullOrEmpty(correlationId?.ToString())
                ? Guid.NewGuid()
                : Guid.Parse(correlationId.ToString());
        }
    }
}
