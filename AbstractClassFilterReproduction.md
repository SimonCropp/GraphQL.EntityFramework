# Abstract Class Filter Bug Reproduction

## Issue Description
When querying entities that have filters with projections and inherit from an abstract base class with TPH inheritance,
the query fails with: `Can't compile a NewExpression with a constructor declared on an abstract class`

## Exact Scenario from LegislationApi

The error occurs in `ProgramBillForecastQueryTests.ForecastExposesBillsOnSittingWeeks()` when:

1. **Entity Structure**:
   - `ProgramBillBase` is an **abstract class** with TPH (Table-Per-Hierarchy) inheritance
   - Discriminator on `BillFormat` enum
   - `ProgramBill` and `TreasuryProgramBill` are concrete derived types
   - Both share the same table `ProgramBills`

2. **Query Structure** (from SittingWeekGraph.cs):
   ```csharp
   return data.ProgramBills.Include(_ => _.Forecast)
       .Where(_ => _.Forecast != null && _.Forecast.SittingWeekId == context.Source.Id);
   ```

3. **Filters** (from EntityFilters.cs):
   ```csharp
   filters.For<ProgramBill>().Add(
       projection: _ => _.Id,
       filter: async (context, data, user, id) => {...});

   filters.For<TreasuryProgramBill>().Add(
       projection: _ => _.Id,
       filter: async (context, data, user, id) => {...});
   ```

4. **DbContext Configuration**:
   - `DbSet<ProgramBill> ProgramBills` (concrete type, not abstract base)
   - `DbSet<TreasuryProgramBill> TreasuryProgramBills` (separate DbSet for other type)

## Key Configuration Details

### ProgramBillBase.cs
```csharp
public abstract class ProgramBillBase : BaseEntity, IDeleted
{
    // Properties...
    public ProgramBillForecast? Forecast { get; set; }

    class ConfigureModel : IEntityTypeConfiguration<ProgramBillBase>
    {
        public void Configure(EntityTypeBuilder<ProgramBillBase> builder)
        {
            builder.ApplyConventions(_ => _.TableName = "ProgramBills");
            builder.HasQueryFilter(_ => !_.Deleted);

            // TPH with discriminator
            builder
                .HasDiscriminator(_ => _.BillFormat)
                .HasValue<ProgramBill>(BillFormat.Standard)
                .HasValue<TreasuryProgramBill>(BillFormat.Treasury)
                .IsComplete();
        }
    }
}
```

### ProgramBillForecast.cs
```csharp
public class ProgramBillForecast : BaseEntity
{
    public ProgramBillBase? ProgramBill { get; set; }  // Navigation to abstract base!
    public required Guid ProgramBillId { get; set; }

    // Configuration with back-reference
    builder.HasOne(p => p.ProgramBill)
        .WithOne(p => p.Forecast)
        .HasForeignKey<ProgramBillForecast>(p => p.ProgramBillId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

## Tests Added

I've added three test files to help reproduce this:

1. **IntegrationTests_abstract_entity_filter.cs**
   - Tests direct queries on entities inheriting from abstract classes with filters

2. **IntegrationTests_abstract_entity_navigation_filter.cs**
   - Tests queries with navigation properties to filtered entities inheriting from abstract classes

## Current Status

The tests added above all **pass successfully** without triggering the error. The issue appears to be specific to the combination of:

1. **TPH inheritance with abstract base class**
2. **Navigation properties that reference the abstract base** (like `ProgramBillForecast.ProgramBill`)
3. **Filters with projections on the concrete derived types**
4. **Queries that use `.Include()` on these navigation properties**

The tests in GraphQL.EntityFramework may not be triggering the error because the `BaseEntity` abstract class might not have the same navigation property configuration as `ProgramBillBase`.

## Next Steps for Reproduction

To help identify the root cause, please try:

1. **Check your entity configuration**:
   - Is `ProgramBillBase` marked as abstract?
   - Are you using TPH (Table Per Hierarchy) or TPT (Table Per Type) inheritance?
   - Do you have any custom value converters or query filters on these entities?

2. **Simplify the SittingWeekGraph query**:
   - Try removing the `.Include(_ => _.Forecast)` and see if the error persists
   - Try querying `ProgramBills` directly instead of through `SittingWeeks`

3. **Check if it's the filter or the query**:
   - Temporarily remove the filters for `ProgramBill` and `TreasuryProgramBill`
   - If the query works without filters, the issue is in how filters interact with abstract inheritance

4. **Share more details**:
   - Can you share the `ProgramBill`, `TreasuryProgramBill`, and `ProgramBillBase` entity definitions?
   - Can you share the exact query that's failing?
   - Can you share the EF Core configuration for these entities?

## Potential Root Causes

Based on the error message, possible causes include:

1. **EF Core trying to materialize abstract class**: If EF Core's query compilation creates an expression
   that tries to instantiate the abstract base class instead of the concrete derived classes.

2. **Filter projection issue**: The filter projection expression might be getting modified in a way that
   references the abstract base type instead of the concrete type.

3. **Navigation property type mismatch**: If a navigation property is typed as the abstract base class
   and EF Core tries to create a NewExpression for it during query compilation.

## Running the Tests

```bash
cd C:\Code\GraphQL.EntityFramework\src\Tests
dotnet test --filter "FullyQualifiedName~abstract"
```

All tests should pass (with verification failures since .verified.txt files don't exist yet).
If any test fails with the abstract class error, that would help isolate the issue.
