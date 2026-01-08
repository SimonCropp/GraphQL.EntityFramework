namespace GraphQL.EntityFramework;

public partial class Filters<TDbContext>
    where TDbContext : DbContext
{
    #region bool

    /// <summary>
    /// Add a filter for entities projecting to <see cref="bool"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a bool value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, bool&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, bool>> projection,
        Filter<bool> filter)
        where TEntity : class =>
        Add<TEntity, bool>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="bool"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a bool value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, bool&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, bool>> projection,
        AsyncFilter<bool> filter)
        where TEntity : class =>
        Add<TEntity, bool>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="bool"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable bool value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, bool?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, bool?>> projection,
        Filter<bool?> filter)
        where TEntity : class =>
        Add<TEntity, bool?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="bool"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable bool value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, bool?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, bool?>> projection,
        AsyncFilter<bool?> filter)
        where TEntity : class =>
        Add<TEntity, bool?>(projection, filter);

    #endregion

    #region byte

    /// <summary>
    /// Add a filter for entities projecting to <see cref="byte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a byte value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, byte&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, byte>> projection,
        Filter<byte> filter)
        where TEntity : class =>
        Add<TEntity, byte>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="byte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a byte value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, byte&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, byte>> projection,
        AsyncFilter<byte> filter)
        where TEntity : class =>
        Add<TEntity, byte>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="byte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable byte value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, byte?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, byte?>> projection,
        Filter<byte?> filter)
        where TEntity : class =>
        Add<TEntity, byte?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="byte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable byte value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, byte?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, byte?>> projection,
        AsyncFilter<byte?> filter)
        where TEntity : class =>
        Add<TEntity, byte?>(projection, filter);

    #endregion

    #region char

    /// <summary>
    /// Add a filter for entities projecting to <see cref="char"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a char value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, char&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, char>> projection,
        Filter<char> filter)
        where TEntity : class =>
        Add<TEntity, char>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="char"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a char value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, char&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, char>> projection,
        AsyncFilter<char> filter)
        where TEntity : class =>
        Add<TEntity, char>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="char"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable char value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, char?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, char?>> projection,
        Filter<char?> filter)
        where TEntity : class =>
        Add<TEntity, char?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="char"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable char value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, char?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, char?>> projection,
        AsyncFilter<char?> filter)
        where TEntity : class =>
        Add<TEntity, char?>(projection, filter);

    #endregion

    #region DateOnly

    /// <summary>
    /// Add a filter for entities projecting to <see cref="DateOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a DateOnly value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateOnly&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Date>> projection,
        Filter<Date> filter)
        where TEntity : class =>
        Add<TEntity, Date>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="DateOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a DateOnly value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateOnly&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Date>> projection,
        AsyncFilter<Date> filter)
        where TEntity : class =>
        Add<TEntity, Date>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="DateOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable DateOnly value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateOnly?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Date?>> projection,
        Filter<Date?> filter)
        where TEntity : class =>
        Add<TEntity, Date?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="DateOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable DateOnly value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateOnly?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Date?>> projection,
        AsyncFilter<Date?> filter)
        where TEntity : class =>
        Add<TEntity, Date?>(projection, filter);

    #endregion

    #region DateTime

    /// <summary>
    /// Add a filter for entities projecting to <see cref="DateTime"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a DateTime value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTime&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTime>> projection,
        Filter<DateTime> filter)
        where TEntity : class =>
        Add<TEntity, DateTime>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="DateTime"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a DateTime value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTime&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTime>> projection,
        AsyncFilter<DateTime> filter)
        where TEntity : class =>
        Add<TEntity, DateTime>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="DateTime"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable DateTime value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTime?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTime?>> projection,
        Filter<DateTime?> filter)
        where TEntity : class =>
        Add<TEntity, DateTime?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="DateTime"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable DateTime value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTime?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTime?>> projection,
        AsyncFilter<DateTime?> filter)
        where TEntity : class =>
        Add<TEntity, DateTime?>(projection, filter);

    #endregion

    #region DateTimeOffset

    /// <summary>
    /// Add a filter for entities projecting to <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a DateTimeOffset value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTimeOffset&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTimeOffset>> projection,
        Filter<DateTimeOffset> filter)
        where TEntity : class =>
        Add<TEntity, DateTimeOffset>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a DateTimeOffset value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTimeOffset&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTimeOffset>> projection,
        AsyncFilter<DateTimeOffset> filter)
        where TEntity : class =>
        Add<TEntity, DateTimeOffset>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable DateTimeOffset value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTimeOffset?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> projection,
        Filter<DateTimeOffset?> filter)
        where TEntity : class =>
        Add<TEntity, DateTimeOffset?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable DateTimeOffset value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, DateTimeOffset?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> projection,
        AsyncFilter<DateTimeOffset?> filter)
        where TEntity : class =>
        Add<TEntity, DateTimeOffset?>(projection, filter);

    #endregion

    #region decimal

    /// <summary>
    /// Add a filter for entities projecting to <see cref="decimal"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a decimal value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, decimal&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, decimal>> projection,
        Filter<decimal> filter)
        where TEntity : class =>
        Add<TEntity, decimal>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="decimal"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a decimal value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, decimal&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, decimal>> projection,
        AsyncFilter<decimal> filter)
        where TEntity : class =>
        Add<TEntity, decimal>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="decimal"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable decimal value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, decimal?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, decimal?>> projection,
        Filter<decimal?> filter)
        where TEntity : class =>
        Add<TEntity, decimal?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="decimal"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable decimal value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, decimal?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, decimal?>> projection,
        AsyncFilter<decimal?> filter)
        where TEntity : class =>
        Add<TEntity, decimal?>(projection, filter);

    #endregion

    #region double

    /// <summary>
    /// Add a filter for entities projecting to <see cref="double"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a double value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, double&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, double>> projection,
        Filter<double> filter)
        where TEntity : class =>
        Add<TEntity, double>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="double"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a double value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, double&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, double>> projection,
        AsyncFilter<double> filter)
        where TEntity : class =>
        Add<TEntity, double>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="double"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable double value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, double?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, double?>> projection,
        Filter<double?> filter)
        where TEntity : class =>
        Add<TEntity, double?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="double"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable double value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, double?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, double?>> projection,
        AsyncFilter<double?> filter)
        where TEntity : class =>
        Add<TEntity, double?>(projection, filter);

    #endregion

    #region float

    /// <summary>
    /// Add a filter for entities projecting to <see cref="float"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a float value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, float&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, float>> projection,
        Filter<float> filter)
        where TEntity : class =>
        Add<TEntity, float>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="float"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a float value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, float&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, float>> projection,
        AsyncFilter<float> filter)
        where TEntity : class =>
        Add<TEntity, float>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="float"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable float value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, float?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, float?>> projection,
        Filter<float?> filter)
        where TEntity : class =>
        Add<TEntity, float?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="float"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable float value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, float?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, float?>> projection,
        AsyncFilter<float?> filter)
        where TEntity : class =>
        Add<TEntity, float?>(projection, filter);

    #endregion

    #region Guid

    /// <summary>
    /// Add a filter for entities projecting to <see cref="Guid"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a Guid value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, Guid&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Guid>> projection,
        Filter<Guid> filter)
        where TEntity : class =>
        Add<TEntity, Guid>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="Guid"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a Guid value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, Guid&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Guid>> projection,
        AsyncFilter<Guid> filter)
        where TEntity : class =>
        Add<TEntity, Guid>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="Guid"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable Guid value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, Guid?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Guid?>> projection,
        Filter<Guid?> filter)
        where TEntity : class =>
        Add<TEntity, Guid?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="Guid"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable Guid value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, Guid?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Guid?>> projection,
        AsyncFilter<Guid?> filter)
        where TEntity : class =>
        Add<TEntity, Guid?>(projection, filter);

    #endregion

    #region int

    /// <summary>
    /// Add a filter for entities projecting to <see cref="int"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to an int value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, int&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, int>> projection,
        Filter<int> filter)
        where TEntity : class =>
        Add<TEntity, int>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="int"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to an int value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, int&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, int>> projection,
        AsyncFilter<int> filter)
        where TEntity : class =>
        Add<TEntity, int>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="int"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable int value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, int?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, int?>> projection,
        Filter<int?> filter)
        where TEntity : class =>
        Add<TEntity, int?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="int"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable int value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, int?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, int?>> projection,
        AsyncFilter<int?> filter)
        where TEntity : class =>
        Add<TEntity, int?>(projection, filter);

    #endregion

    #region long

    /// <summary>
    /// Add a filter for entities projecting to <see cref="long"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a long value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, long&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, long>> projection,
        Filter<long> filter)
        where TEntity : class =>
        Add<TEntity, long>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="long"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a long value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, long&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, long>> projection,
        AsyncFilter<long> filter)
        where TEntity : class =>
        Add<TEntity, long>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="long"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable long value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, long?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, long?>> projection,
        Filter<long?> filter)
        where TEntity : class =>
        Add<TEntity, long?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="long"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable long value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, long?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, long?>> projection,
        AsyncFilter<long?> filter)
        where TEntity : class =>
        Add<TEntity, long?>(projection, filter);

    #endregion

    #region nint

    /// <summary>
    /// Add a filter for entities projecting to <see cref="nint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to an nint value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nint&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nint>> projection,
        Filter<nint> filter)
        where TEntity : class =>
        Add<TEntity, nint>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="nint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to an nint value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nint&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nint>> projection,
        AsyncFilter<nint> filter)
        where TEntity : class =>
        Add<TEntity, nint>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="nint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable nint value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nint?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nint?>> projection,
        Filter<nint?> filter)
        where TEntity : class =>
        Add<TEntity, nint?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="nint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable nint value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nint?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nint?>> projection,
        AsyncFilter<nint?> filter)
        where TEntity : class =>
        Add<TEntity, nint?>(projection, filter);

    #endregion

    #region nuint

    /// <summary>
    /// Add a filter for entities projecting to <see cref="nuint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nuint value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nuint&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nuint>> projection,
        Filter<nuint> filter)
        where TEntity : class =>
        Add<TEntity, nuint>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="nuint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nuint value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nuint&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nuint>> projection,
        AsyncFilter<nuint> filter)
        where TEntity : class =>
        Add<TEntity, nuint>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="nuint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable nuint value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nuint?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nuint?>> projection,
        Filter<nuint?> filter)
        where TEntity : class =>
        Add<TEntity, nuint?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="nuint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable nuint value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, nuint?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, nuint?>> projection,
        AsyncFilter<nuint?> filter)
        where TEntity : class =>
        Add<TEntity, nuint?>(projection, filter);

    #endregion

    #region sbyte

    /// <summary>
    /// Add a filter for entities projecting to <see cref="sbyte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to an sbyte value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, sbyte&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, sbyte>> projection,
        Filter<sbyte> filter)
        where TEntity : class =>
        Add<TEntity, sbyte>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="sbyte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to an sbyte value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, sbyte&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, sbyte>> projection,
        AsyncFilter<sbyte> filter)
        where TEntity : class =>
        Add<TEntity, sbyte>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="sbyte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable sbyte value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, sbyte?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, sbyte?>> projection,
        Filter<sbyte?> filter)
        where TEntity : class =>
        Add<TEntity, sbyte?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="sbyte"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable sbyte value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, sbyte?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, sbyte?>> projection,
        AsyncFilter<sbyte?> filter)
        where TEntity : class =>
        Add<TEntity, sbyte?>(projection, filter);

    #endregion

    #region short

    /// <summary>
    /// Add a filter for entities projecting to <see cref="short"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a short value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, short&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, short>> projection,
        Filter<short> filter)
        where TEntity : class =>
        Add<TEntity, short>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="short"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a short value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, short&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, short>> projection,
        AsyncFilter<short> filter)
        where TEntity : class =>
        Add<TEntity, short>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="short"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable short value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, short?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, short?>> projection,
        Filter<short?> filter)
        where TEntity : class =>
        Add<TEntity, short?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="short"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable short value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, short?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, short?>> projection,
        AsyncFilter<short?> filter)
        where TEntity : class =>
        Add<TEntity, short?>(projection, filter);

    #endregion

    #region string

    /// <summary>
    /// Add a filter for entities projecting to <see cref="string"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a string value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, string?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, string?>> projection,
        Filter<string?> filter)
        where TEntity : class =>
        Add<TEntity, string?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="string"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a string value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, string?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, string?>> projection,
        AsyncFilter<string?> filter)
        where TEntity : class =>
        Add<TEntity, string?>(projection, filter);

    #endregion

    #region TimeOnly

    /// <summary>
    /// Add a filter for entities projecting to <see cref="TimeOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a TimeOnly value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeOnly&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Time>> projection,
        Filter<Time> filter)
        where TEntity : class =>
        Add<TEntity, Time>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="TimeOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a TimeOnly value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeOnly&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Time>> projection,
        AsyncFilter<Time> filter)
        where TEntity : class =>
        Add<TEntity, Time>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="TimeOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable TimeOnly value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeOnly?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Time?>> projection,
        Filter<Time?> filter)
        where TEntity : class =>
        Add<TEntity, Time?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="TimeOnly"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable TimeOnly value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeOnly?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, Time?>> projection,
        AsyncFilter<Time?> filter)
        where TEntity : class =>
        Add<TEntity, Time?>(projection, filter);

    #endregion

    #region TimeSpan

    /// <summary>
    /// Add a filter for entities projecting to <see cref="TimeSpan"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a TimeSpan value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeSpan&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, TimeSpan>> projection,
        Filter<TimeSpan> filter)
        where TEntity : class =>
        Add<TEntity, TimeSpan>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="TimeSpan"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a TimeSpan value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeSpan&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, TimeSpan>> projection,
        AsyncFilter<TimeSpan> filter)
        where TEntity : class =>
        Add<TEntity, TimeSpan>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="TimeSpan"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable TimeSpan value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeSpan?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, TimeSpan?>> projection,
        Filter<TimeSpan?> filter)
        where TEntity : class =>
        Add<TEntity, TimeSpan?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="TimeSpan"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable TimeSpan value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, TimeSpan?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, TimeSpan?>> projection,
        AsyncFilter<TimeSpan?> filter)
        where TEntity : class =>
        Add<TEntity, TimeSpan?>(projection, filter);

    #endregion

    #region uint

    /// <summary>
    /// Add a filter for entities projecting to <see cref="uint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a uint value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, uint&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, uint>> projection,
        Filter<uint> filter)
        where TEntity : class =>
        Add<TEntity, uint>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="uint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a uint value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, uint&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, uint>> projection,
        AsyncFilter<uint> filter)
        where TEntity : class =>
        Add<TEntity, uint>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="uint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable uint value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, uint?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, uint?>> projection,
        Filter<uint?> filter)
        where TEntity : class =>
        Add<TEntity, uint?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="uint"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable uint value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, uint?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, uint?>> projection,
        AsyncFilter<uint?> filter)
        where TEntity : class =>
        Add<TEntity, uint?>(projection, filter);

    #endregion

    #region ulong

    /// <summary>
    /// Add a filter for entities projecting to <see cref="ulong"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a ulong value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ulong&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ulong>> projection,
        Filter<ulong> filter)
        where TEntity : class =>
        Add<TEntity, ulong>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="ulong"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a ulong value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ulong&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ulong>> projection,
        AsyncFilter<ulong> filter)
        where TEntity : class =>
        Add<TEntity, ulong>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="ulong"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable ulong value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ulong?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ulong?>> projection,
        Filter<ulong?> filter)
        where TEntity : class =>
        Add<TEntity, ulong?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="ulong"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable ulong value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ulong?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ulong?>> projection,
        AsyncFilter<ulong?> filter)
        where TEntity : class =>
        Add<TEntity, ulong?>(projection, filter);

    #endregion

    #region ushort

    /// <summary>
    /// Add a filter for entities projecting to <see cref="ushort"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a ushort value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ushort&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ushort>> projection,
        Filter<ushort> filter)
        where TEntity : class =>
        Add<TEntity, ushort>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to <see cref="ushort"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a ushort value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ushort&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ushort>> projection,
        AsyncFilter<ushort> filter)
        where TEntity : class =>
        Add<TEntity, ushort>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="ushort"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable ushort value.</param>
    /// <param name="filter">Synchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ushort?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ushort?>> projection,
        Filter<ushort?> filter)
        where TEntity : class =>
        Add<TEntity, ushort?>(projection, filter);

    /// <summary>
    /// Add a filter for entities projecting to nullable <see cref="ushort"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to filter.</typeparam>
    /// <param name="projection">Expression projecting the entity to a nullable ushort value.</param>
    /// <param name="filter">Asynchronous filter function to determine if the value should be included.</param>
    /// <remarks>
    /// This is a convenience overload that infers the projection type.
    /// Equivalent to calling Add&lt;TEntity, ushort?&gt;(projection, filter).
    /// </remarks>
    public void Add<TEntity>(
        Expression<Func<TEntity, ushort?>> projection,
        AsyncFilter<ushort?> filter)
        where TEntity : class =>
        Add<TEntity, ushort?>(projection, filter);

    #endregion
}
