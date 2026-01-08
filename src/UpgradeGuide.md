# Upgrade Guide

## API Changes in v33.2.0+

### Filters API Simplified

The filters API has been simplified to use a single consistent fluent interface for all filter types.

#### Old API (v33.1.3 and earlier)

Multiple ways to add filters:
```csharp
// Explicit type parameters
filters.Add<Product, int>(projection, filter);
filters.Add<Product, ProductProjection>(projection, filter);

// Convenience overloads for simple types
filters.Add<Product>(projection: _ => _.Quantity, filter);
```

#### New API (v33.2.0+)

Single fluent API for all cases:
```csharp
// Works for all projection types - single field, anonymous, or named
filters.For<Product>().Add(projection, filter);
```

### Migration Examples

**Single field projection:**
```csharp
// Before
filters.Add<Product, int>(
    projection: _ => _.Quantity,
    filter: (_, _, _, qty) => qty > 0);

// After
filters.For<Product>().Add(
    projection: _ => _.Quantity,
    filter: (_, _, _, qty) => qty > 0);
```

**Anonymous type projection:**
```csharp
// Before - required explicit type parameter
filters.Add<Product, ???>(  // Can't use anonymous types!
    projection: p => new { p.Quantity, p.Price },
    filter: ...);

// After - anonymous types work!
filters.For<Product>().Add(
    projection: p => new { p.Quantity, p.Price },
    filter: (_, _, _, x) => x.Quantity > 0 && x.Price >= 10);
```

**Named projection type:**
```csharp
// Before
filters.Add<Product, ProductProjection>(
    projection: p => new ProductProjection { Quantity = p.Quantity },
    filter: (_, _, _, x) => x.Quantity > 0);

// After
filters.For<Product>().Add(
    projection: p => new ProductProjection { Quantity = p.Quantity },
    filter: (_, _, _, x) => x.Quantity > 0);
```

### Benefits of New API

1. **One way to do it**: No need to remember different overloads
2. **Anonymous types supported**: No projection classes needed for multi-field filters
3. **Type inference**: Compiler infers projection type automatically
4. **Consistent syntax**: Same pattern for single fields, multiple fields, and named types
5. **Cleaner code**: Less visual clutter, more readable

### Migration Checklist

- [ ] Find all `filters.Add<TEntity, TProjection>(...)` calls
- [ ] Replace with `filters.For<TEntity>().Add(...)`
- [ ] Remove explicit `TProjection` type parameter
- [ ] Consider using anonymous types instead of projection classes where appropriate
- [ ] Test that filters still work correctly

### Breaking Change

The `Add<TEntity, TProjection>()` methods are now internal and not part of the public API. All filter registration must use `For<TEntity>().Add()`.
