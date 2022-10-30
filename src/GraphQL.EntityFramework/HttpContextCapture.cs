class HttpContextCapture
{
    public IHttpContextAccessor HttpContextAccessor { get; }

    public HttpContextCapture(IHttpContextAccessor httpContextAccessor) =>
        HttpContextAccessor = httpContextAccessor;
}