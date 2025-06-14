﻿// ReSharper disable UseRawString
namespace DapprWire.Core.DatabaseSessions;

[Collection(nameof(RequireDatabase))]
public class ExecuteTests(DatabaseFixture fixture, ITestOutputHelper output)
{
    #region Execute

    [Fact]
    public async Task Execute_NoParams_ShouldCreateRows()
    {
        var ct = CancellationToken.None;
        var externalId = Guid.NewGuid();

        var database = CoreHelpers.CreateTestDatabase(output, fixture.GetDbConnection);

        await using var session = await database.ConnectAsync(ct);

        var result = await session.ExecuteAsync($@"
insert into TestTable (ExternalId, Name)
values ('{externalId}', 'Test {externalId}')", ct);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Execute_WithParams_ShouldCreateRows()
    {
        var ct = CancellationToken.None;
        var externalId = Guid.NewGuid();

        var database = CoreHelpers.CreateTestDatabase(output, fixture.GetDbConnection);

        await using var session = await database.ConnectAsync(ct);

        var result = await session.ExecuteAsync(@"
insert into TestTable (ExternalId, Name)
values (@ExternalId, @Name)", new
        {
            ExternalId = externalId,
            Name = $"Test {externalId}"
        }, ct);

        Assert.Equal(1, result);
    }

    #endregion

    #region ExecuteScalar

    [Fact]
    public async Task ExecuteScalar_NoParams_ShouldCreateRows()
    {
        var ct = CancellationToken.None;
        var externalId = Guid.NewGuid();

        var database = CoreHelpers.CreateTestDatabase(output, fixture.GetDbConnection);

        await using var session = await database.ConnectAsync(ct);

        var result = await session.ExecuteScalarAsync<int>($@"
insert into TestTable (ExternalId, Name)
values ('{externalId}', 'Test {externalId}');
select cast(SCOPE_IDENTITY() as int);", ct);

        Assert.True(result > 0, "result > 0");
    }

    [Fact]
    public async Task ExecuteScalar_WithParams_ShouldCreateRows()
    {
        var ct = CancellationToken.None;
        var externalId = Guid.NewGuid();

        var database = CoreHelpers.CreateTestDatabase(output, fixture.GetDbConnection);

        await using var session = await database.ConnectAsync(ct);

        var result = await session.ExecuteScalarAsync<int>(@"
insert into TestTable (ExternalId, Name)
values (@ExternalId, @Name);
select cast(SCOPE_IDENTITY() as int);", new
        {
            ExternalId = externalId,
            Name = $"Test {externalId}"
        }, ct);

        Assert.True(result > 0, "result > 0");
    }

    #endregion

    #region ExecuteReader

    [Fact]
    public async Task ExecuteReader_NoParams_ReturnsExpectedResults()
    {
        var ct = CancellationToken.None;

        var database = CoreHelpers.CreateTestDatabase(output, fixture.GetDbConnection);

        await using var session = await database.ConnectAsync(ct);

        await using var reader = await session.ExecuteReaderAsync(@"with
TestDataCte as (
    select null as Value union all

    select 1 union all
    select 2 union all
    select 3 union all
    select 4
)
select *
from TestDataCte
where
    Value is not null", ct);

        Assert.NotNull(reader);

        var entries = new List<int>();
        while (await reader.ReadAsync(ct))
            entries.Add(reader.GetInt32(0));

        Assert.Collection(entries,
            e => Assert.Equal(1, e),
            e => Assert.Equal(2, e),
            e => Assert.Equal(3, e),
            e => Assert.Equal(4, e)
        );
    }

    [Fact]
    public async Task ExecuteReader_WithParams_ReturnsExpectedResults()
    {
        var ct = CancellationToken.None;

        var database = CoreHelpers.CreateTestDatabase(output, fixture.GetDbConnection);

        await using var session = await database.ConnectAsync(ct);

        await using var reader = await session.ExecuteReaderAsync(@"with
TestDataCte as (
    select null as Value union all

    select 1 Value union all
    select 2 union all
    select 3 union all
    select 4
)
select *
from TestDataCte
where
    Value in @Values", new
        {
            Values = (int[])[2, 4]
        }, ct);

        Assert.NotNull(reader);

        var entries = new List<int>();
        while (await reader.ReadAsync(ct))
            entries.Add(reader.GetInt32(0));

        Assert.Collection(entries,
            e => Assert.Equal(2, e),
            e => Assert.Equal(4, e)
        );
    }

    #endregion
}