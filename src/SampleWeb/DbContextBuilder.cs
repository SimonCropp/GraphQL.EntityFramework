using System.Threading;
using System.Threading.Tasks;
using EfLocalDb;
using Microsoft.Extensions.Hosting;

// LocalDb is used to make the sample simpler.
// Replace with a real DbContext
public class DbContextBuilder :
    IHostedService
{
    static SqlDatabase<SampleDbContext> database = null!;

    internal static async Task CreateDb(SampleDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        Company company1 = new()
        {
            Id = 1,
            Content = "Company1"
        };
        Employee employee1 = new()
        {
            Id = 2,
            CompanyId = company1.Id,
            Content = "Employee1",
            Age = 25
        };
        Employee employee2 = new()
        {
            Id = 3,
            CompanyId = company1.Id,
            Content = "Employee2",
            Age = 31
        };
        Company company2 = new()
        {
            Id = 4,
            Content = "Company2"
        };
        Employee employee4 = new()
        {
            Id = 5,
            CompanyId = company2.Id,
            Content = "Employee4",
            Age = 34
        };
        Company company3 = new()
        {
            Id = 6,
            Content = "Company3"
        };
        Company company4 = new()
        {
            Id = 7,
            Content = "Company4"
        };
        Employee employee5 = new()
        {
            Id = 8,
            Content = null,
            CompanyId = company2.Id
        };
        context.AddRange(company1, employee1, employee2, company2, company3, company4, employee4, employee5);

        OrderDetail orderDetail1 = new()
        {
            Id = 1,
            BillingAddress = new()
            {
                AddressLine1 = "1 Street",
                AreaCode = "1234",
                State = "New South Wales",
                Country = "Australia"
            },
            ShippingAddress = new()
            {
                AddressLine1 = "1 Street",
                AreaCode = "1234",
                State = "New South Wales",
                Country = "Australia"
            }
        };
        context.OrderDetails.Add(orderDetail1);

        await context.SaveChangesAsync();
    }

    public SampleDbContext BuildDbContext()
    {
        return database.NewDbContext();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        SqlInstance<SampleDbContext> sqlInstance = new(
            buildTemplate: CreateDb,
            constructInstance: builder => new(builder.Options));

        database = await sqlInstance.Build("GraphQLEntityFrameworkSample");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
