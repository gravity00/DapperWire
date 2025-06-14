﻿namespace DapprWire;

/// <summary>
/// Represents a database transaction.
/// </summary>
/// <param name="options">The database options.</param>
/// <param name="transaction">The database transaction.</param>
/// <param name="onDispose">Callback to be invoked when the transaction is disposed.</param>
public class DatabaseTransaction(
    DatabaseOptions options,
    DbTransaction transaction,
    Action? onDispose
) : IDatabaseTransaction
{
    private DbTransaction? _transaction = transaction;

    /// <summary>
    /// Finalizes the database connection.
    /// </summary>
    ~DatabaseTransaction() => Dispose(false);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the database transaction.
    /// </summary>
    /// <param name="disposing">Is the transaction explicitly being disposed</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _transaction?.Dispose();

        _transaction = null;
        onDispose?.Invoke();
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the database transaction asynchronously.
    /// </summary>
    /// <returns>A task to be awaited for the result.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync().ConfigureAwait(false);

        _transaction = null;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken ct)
    {
        if (_transaction is null)
            throw new ObjectDisposedException(nameof(DatabaseTransaction));

        options.Logger.LogDebug<DatabaseTransaction>("Committing the database transaction...");
        await _transaction.CommitAsync(ct);
        options.Logger.LogInfo<DatabaseTransaction>("Database transaction committed successfully.");
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_transaction is null)
            throw new ObjectDisposedException(nameof(DatabaseTransaction));

        options.Logger.LogDebug<DatabaseTransaction>("Rolling back the database transaction...");
        await _transaction.RollbackAsync(ct);
        options.Logger.LogInfo<DatabaseTransaction>("Database transaction rolled back successfully.");
    }

#else

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Commit();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Rollback();
        return Task.CompletedTask;
    }

#endif

    /// <inheritdoc />
    public void Commit()
    {
        if (_transaction is null)
            throw new ObjectDisposedException(nameof(DatabaseTransaction));

        options.Logger.LogDebug<DatabaseTransaction>("Committing the database transaction...");
        _transaction.Commit();
        options.Logger.LogInfo<DatabaseTransaction>("Database transaction committed successfully.");
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (_transaction is null)
            throw new ObjectDisposedException(nameof(DatabaseTransaction));

        options.Logger.LogDebug<DatabaseTransaction>("Rolling back the database transaction...");
        _transaction.Rollback();
        options.Logger.LogInfo<DatabaseTransaction>("Database transaction rolled back successfully.");
    }
}