using Microsoft.AspNetCore.Http;

public class HttpContextCapture
{
    public IHttpContextAccessor HttpContextAccessor { get; }

    public HttpContextCapture(IHttpContextAccessor httpContextAccessor)
    {
        HttpContextAccessor = httpContextAccessor;
    }
}