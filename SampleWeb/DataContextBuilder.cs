using EfCore.InMemoryHelpers;

// InMemory is used to make the sample simpler.
// Replace with a real DataContext
static class DataContextBuilder
{
    public static MyDataContext BuildDataContext()
    {
        var company1 = new Company
        {
            Content = "Company1"
        };
        var employee1 = new Employee
        {
            CompanyId = company1.Id,
            Content = "Employee1"
        };
        var employee2 = new Employee
        {
            CompanyId = company1.Id,
            Content = "Employee2"
        };
        var company2 = new Company
        {
            Content = "Company2"
        };
        var employee4 = new Employee
        {
            CompanyId = company2.Id,
            Content = "Employee4"
        };
        var company3 = new Company
        {
            Content = "Company3"
        };
        var company4 = new Company
        {
            Content = "Company4"
        };
        var myDataContext = InMemoryContextBuilder.Build<MyDataContext>();
        myDataContext.AddRange(company1, employee1, employee2, company2, company3, company4, employee4);
        myDataContext.SaveChanges();
        return myDataContext;
    }
}
