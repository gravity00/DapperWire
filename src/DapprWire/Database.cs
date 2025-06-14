﻿namespace DapprWire;

/// <summary>
/// Represents a factory for creating database connections.
/// </summary>
/// <param name="options">The database options.</param>
/// <param name="dbConnectionFactory">The <see cref="DbConnection"/> factory.</param>
/// <exception cref="ArgumentNullException"></exception>
public class Database(
    DatabaseOptions options,
    DbConnectionFactory dbConnectionFactory
) : IDatabase
{
    private readonly DatabaseOptions _options = options.NotNull(nameof(options));
    private readonly DbConnectionFactory _dbConnectionFactory = dbConnectionFactory.NotNull(nameof(dbConnectionFactory));

    /// <inheritdoc />
    public async Task<IDatabaseSession> ConnectAsync(CancellationToken ct)
    {
        options.Logger.LogDebug<Database>("Starting a new database session...");
        var database = new DatabaseSession(_options, _dbConnectionFactory);

        try
        {
            await database.ConnectAsync(ct).ConfigureAwait(false);
        }
        catch
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            await database.DisposeAsync().ConfigureAwait(false);
#else
            database.Dispose();
#endif
            throw;
        }

        options.Logger.LogInfo<Database>("Database session started successfully.");

        return database;
    }

    /// <inheritdoc />
    public IDatabaseSession Connect()
    {
        options.Logger.LogDebug<Database>("Starting a new database session...");
        var database = new DatabaseSession(_options, _dbConnectionFactory);

        try
        {
            database.Connect();
        }
        catch
        {
            database.Dispose();
            throw;
        }

        options.Logger.LogInfo<Database>("Database session started successfully.");

        return database;
    }
}

/// <summary>
/// Represents a strongly-typed factory for creating database connections.
/// </summary>
/// <typeparam name="TName">The database name.</typeparam>
public class Database<TName> : Database, IDatabase<TName>
    where TName : IDatabaseName
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="options">The database options.</param>
    /// <param name="dbConnectionFactory">The strongly-typed <see cref="DbConnection"/> factory.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public Database(
        DatabaseOptions options,
        DbConnectionFactory<TName> dbConnectionFactory
    ) : base(
        options.NotNull(nameof(options)),
        () => dbConnectionFactory()
    )
    {
        dbConnectionFactory.NotNull(nameof(dbConnectionFactory));
    }
}