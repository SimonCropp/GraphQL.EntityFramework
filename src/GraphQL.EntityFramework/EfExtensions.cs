static class EfExtensions
{
    public static string SafeToQueryString(this IQueryable query)
    {
        try
        {
            return query.ToQueryString();
        }
        catch (Exception exception)
        {
            return "Could not resolve ToQueryString: " + exception.Message;
        }
    }
}