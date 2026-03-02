namespace NexAlytics.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var empresaIdClaim = context.User.FindFirst("EmpresaId");
            if (empresaIdClaim != null && int.TryParse(empresaIdClaim.Value, out var empresaId))
            {
                context.Items["EmpresaId"] = empresaId;
            }
        }

        await _next(context);
    }
}
