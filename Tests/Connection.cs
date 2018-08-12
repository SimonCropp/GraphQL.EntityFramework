using System;

public static class Connection
{
    public static string ConnectionString;

    static Connection()
    {
        if (Environment.GetEnvironmentVariable("AppVeyor") == "True")
        {
            ConnectionString = @"Server=(local)\SQL2017;Database=GraphQLEntityFrameworkTests;User ID=sa;Password=Password12!;MultipleActiveResultSets=true";
            SqlHelper.EnsureDatabaseExists(ConnectionString);
            return;
        }
        ConnectionString = @"Data Source=.\SQLExpress;Database=GraphQLEntityFrameworkTests; Integrated Security=True;Max Pool Size=100;MultipleActiveResultSets=true";
    }
}