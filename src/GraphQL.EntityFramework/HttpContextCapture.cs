class HttpContextCapture(IHttpContextAccessor httpContextAccessor)
{
    public IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;
}