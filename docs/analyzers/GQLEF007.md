# GQLEF007: Identity projection with abstract navigation access

## Cause

A filter uses identity projection (`_ => _` or 4-parameter syntax) but the filter lambda accesses properties through an abstract navigation type.

## Problem

Abstract types cannot be instantiated in SQL projections. When Entity Framework Core encounters a projection that would require instantiating an abstract class, it cannot generate the SQL expression. Instead, EF Core falls back to using `Include()` which loads **all columns** from the related table.

This creates a significant performance problem:
- The filter only needs 1-2 fields from the navigation
- But EF Core loads all 30+ fields because of the Include fallback
- Query performance degrades and memory usage increases unnecessarily

## Rule Details

This diagnostic is reported when:
1. A filter uses identity projection (explicit `_ => _` or implicit via 4-parameter syntax)
2. The filter lambda accesses a property through a navigation property
3. That navigation property's type is abstract (has the `abstract` keyword)

### Examples of violations

```csharp
// ❌ Identity projection with abstract navigation access
public abstract class BaseEntity
{
    public string Property { get; set; }
}

public class Child
{
    public Guid Id { get; set; }
    public BaseEntity Parent { get; set; }
}

// This triggers GQLEF007:
filters.For<Child>().Add(
    projection: _ => _,  // Identity projection
    filter: (_, _, _, c) => c.Parent.Property == "test");

// This also triggers GQLEF007 (4-parameter syntax uses identity projection):
filters.For<Child>().Add(
    filter: (_, _, _, c) => c.Parent.Property == "test");
```

### Examples of correct code

```csharp
// ✅ Explicit projection extracting required properties
filters.For<Child>().Add(
    projection: c => new { c.Id, ParentProperty = c.Parent.Property },
    filter: (_, _, _, proj) => proj.ParentProperty == "test");

// ✅ Concrete (non-abstract) navigation is fine with identity projection
public class ConcreteParent  // Not abstract
{
    public string Property { get; set; }
}

public class Child
{
    public ConcreteParent Parent { get; set; }
}

filters.For<Child>().Add(
    projection: _ => _,
    filter: (_, _, _, c) => c.Parent.Property == "test");  // OK - ConcreteParent is not abstract
```

## Solution

Convert the identity projection to an explicit projection that extracts only the required properties from the abstract navigation.

### Manual Fix

1. Identify all properties accessed through the abstract navigation
2. Create an explicit projection that extracts those properties with flattened names
3. Update the filter to use the projected property names
4. Rename the filter parameter to `proj` for clarity

**Before:**
```csharp
filters.For<Child>().Add(
    projection: _ => _,
    filter: (_, _, _, c) => c.Parent.Property == "test");
```

**After:**
```csharp
filters.For<Child>().Add(
    projection: c => new { c.Id, ParentProperty = c.Parent.Property },
    filter: (_, _, _, proj) => proj.ParentProperty == "test");
```

### Automatic Fix

The code fixer can automatically perform this transformation:
1. Place cursor on the diagnostic
2. Press `Ctrl+.` (or `Cmd+.` on Mac)
3. Select "Convert to explicit projection"

The fixer will:
- Extract all accessed navigation properties
- Create an anonymous type projection with flattened property names
- Update the filter lambda to use the new property names
- Rename the filter parameter from entity name to `proj`

## Performance Impact

Using explicit projections instead of identity projections with abstract navigations can significantly improve performance:

**Identity Projection (Inefficient):**
```sql
-- Loads all 34 columns from BaseRequest table
SELECT a.*, br.*
FROM Accommodation a
LEFT JOIN BaseRequest br ON a.TravelRequestId = br.Id
```

**Explicit Projection (Efficient):**
```sql
-- Loads only required columns
SELECT a.Id, br.GroupOwnerId, br.HighestStatusAchieved
FROM Accommodation a
LEFT JOIN BaseRequest br ON a.TravelRequestId = br.Id
```

## When to Suppress

**Never.** This diagnostic indicates a real performance issue that should be fixed. There is no valid scenario where loading all columns from an abstract navigation is preferred over loading only the required columns.

## Related Rules

- **GQLEF004** - Suggests using 4-parameter filter syntax for identity projections with key-only access
- **GQLEF005** - Prevents accessing non-key properties with 4-parameter filter syntax
- **GQLEF006** - Prevents accessing non-key properties with explicit identity projection

## See Also

- [Filter Projections Documentation](/docs/filters.md#abstract-type-navigations)
- [Understanding EF Core Projections](https://learn.microsoft.com/en-us/ef/core/querying/how-query-works)
