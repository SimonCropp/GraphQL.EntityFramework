using EfLocalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// InMemory is used to make the sample simpler.
// Replace with a real DbContext
public static class DbContextBuilder
{
    static SqlDatabase<GraphQlEfSampleDbContext> database;
    static DbContextBuilder()
    {
        var sqlInstance = new SqlInstance<GraphQlEfSampleDbContext>(
            buildTemplate: (connection, builder) => { CreateDb(builder); },
            constructInstance: builder => new GraphQlEfSampleDbContext(builder.Options));

        database = sqlInstance.Build("GraphQLEntityFrameworkSample").GetAwaiter().GetResult();
        using (var context = database.NewDbContext())
        {
            Model = context.Model;
        }
    }

    static void CreateDb(DbContextOptionsBuilder<GraphQlEfSampleDbContext> builder)
    {
        using (var context = new GraphQlEfSampleDbContext(builder.Options))
        {
            context.Database.EnsureCreated();

            Model = context.Model;

            var company1 = new Company
            {
                Id = 1,
                Content = "Company1"
            };
            var employee1 = new Employee
            {
                Id = 2,
                CompanyId = company1.Id,
                Content = "Employee1",
                Age = 25
            };
            var employee2 = new Employee
            {
                Id = 3,
                CompanyId = company1.Id,
                Content = "Employee2",
                Age = 31
            };
            var company2 = new Company
            {
                Id = 4,
                Content = "Company2"
            };
            var employee4 = new Employee
            {
                Id = 5,
                CompanyId = company2.Id,
                Content = "Employee4",
                Age = 34
            };
            var company3 = new Company
            {
                Id = 6,
                Content = "Company3"
            };
            var company4 = new Company
            {
                Id = 7,
                Content = "Company4"
            };
            context.AddRange(company1, employee1, employee2, company2, company3, company4, employee4);
            context.SaveChanges();
        }
    }

    public static IModel Model;

    public static GraphQlEfSampleDbContext BuildDbContext()
    {
        return database.NewDbContext();
    }
}
