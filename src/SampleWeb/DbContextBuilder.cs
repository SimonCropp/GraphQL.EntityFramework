﻿// LocalDb is used to make the sample simpler.
// Replace with a real DbContext
public class DbContextBuilder :
    IHostedService
{
    static SqlDatabase<SampleDbContext> database = null!;

    static async Task CreateDb(SampleDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

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
        var employee5 = new Employee
        {
            Id = 8,
            Content = null,
            CompanyId = company2.Id
        };
        context.AddRange(company1, employee1, employee2, company2, company3, company4, employee4, employee5);

        var orderDetail1 = new OrderDetail
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

    public SampleDbContext BuildDbContext() =>
        database.NewDbContext();

    public async Task StartAsync(Cancel cancel)
    {
        var sqlInstance = new SqlInstance<SampleDbContext>(buildTemplate: CreateDb, constructInstance: builder => new(builder.Options));

        database = await sqlInstance.Build("GraphQLEntityFrameworkSample");
    }

    public Task StopAsync(Cancel cancel) =>
        Task.CompletedTask;
}
