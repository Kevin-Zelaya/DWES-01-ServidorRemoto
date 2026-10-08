using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;
using CSharpFunctionalExtensions;

public class PostgreSqlTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private PostgreSqlRepository _repository = null!;
    private Mock<ILogger<PostgreSqlRepository>> _logger = null!;

    [SetUp]
    public async Task Setup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .Build();

        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);

        await _context.Database.EnsureCreatedAsync();

        _logger = new Mock<ILogger<PostgreSqlRepository>>();

        _repository = new PostgreSqlRepository(
            _logger.Object,
            _context);
    }
    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
        await _container.DisposeAsync();
    }
    [Test]
    public async Task Create_ShouldReturnCreatedUser()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        var result = await _repository.CreateAsync(user);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.id, Is.EqualTo(1));
        Assert.That(result.Value.name, Is.EqualTo("Kevin"));
    }
    [Test]
    public async Task Create_WhenUserAlreadyExists_ShouldReturnFailure()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        await _repository.CreateAsync(user);

        _context.ChangeTracker.Clear();

        var duplicatedUser = new UserEntity
        {
            id = 1,
            name = "Otro",
            username = "otro",
            email = "otro@test.com"
        };

        var result = await _repository.CreateAsync(duplicatedUser);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.WriteFailure>());
    }
    [Test]
public async Task CreateRange_ShouldCreateUsers()
{
    var users = new List<UserEntity>
    {
        new() { id = 1, name = "Kevin", username = "kevin", email = "kevin@test.com" },
        new() { id = 2, name = "Juan", username = "juan", email = "juan@test.com" }
    };

    var result = await _repository.CreateRangeAsync(users);

    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value, Is.True);
}

[Test]
public async Task CreateRange_WhenEmpty_ShouldReturnFailure()
{
    var users = new List<UserEntity>();

    var result = await _repository.CreateRangeAsync(users);

    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Is.TypeOf<DatabaseError.WriteFailure>());
}

[Test]
public async Task CreateRange_WhenDuplicateId_ShouldReturnWriteFailure()
{
    await _repository.CreateAsync(
        new UserEntity { id = 1, name = "Kevin", username = "kevin" });

    _context.ChangeTracker.Clear();

    var users = new List<UserEntity>
    {
        new() { id = 1, name = "Duplicado", username = "duplicado" },
        new() { id = 2, name = "Juan", username = "juan" }
    };

    var result = await _repository.CreateRangeAsync(users);

    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Is.TypeOf<DatabaseError.WriteFailure>());
}

[Test]
public async Task GetAll_ShouldReturnUsers()
{
    await _repository.CreateRangeAsync(
        new List<UserEntity>
        {
            new() { id = 1, name = "Kevin", username = "kevin" },
            new() { id = 2, name = "Juan", username = "juan" }
        });

    var result = await _repository.GetAllAsync();

    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value, Has.Count.EqualTo(2));
}

[Test]
public async Task GetAll_WhenEmpty_ShouldReturnEmptyList()
{
    var result = await _repository.GetAllAsync();

    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value, Is.Empty);
}

[Test]
public async Task GetUserById_ShouldReturnUser()
{
    await _repository.CreateAsync(
        new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin"
        });

    var result = await _repository.GetUserByIdAsync(1);

    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.id, Is.EqualTo(1));
    Assert.That(result.Value.name, Is.EqualTo("Kevin"));
}

[Test]
public async Task GetUserById_WhenNotFound_ShouldReturnFailure()
{
    var result = await _repository.GetUserByIdAsync(999);

    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Is.TypeOf<DatabaseError.NotFound>());
}

[Test]
public void GetUserById_WhenCancelled_ShouldThrow()
{
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    Assert.ThrowsAsync<OperationCanceledException>(
        async () => await _repository.GetUserByIdAsync(1, cts.Token));
}

[Test]
public async Task Update_ShouldUpdateUser()
{
    await _repository.CreateAsync(
        new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin"
        });

    var updated = new UserEntity
    {
        id = 1,
        name = "Kevin Updated",
        username = "kevin"
    };

    var result = await _repository.UpdateAsync(updated, 1);

    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.name, Is.EqualTo("Kevin Updated"));
}

[Test]
public async Task Update_WhenNotFound_ShouldReturnFailure()
{
    var user = new UserEntity
    {
        id = 999,
        name = "Kevin",
        username = "kevin"
    };

    var result = await _repository.UpdateAsync(user, 999);

    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Is.TypeOf<DatabaseError.NotFound>());
}

[Test]
public async Task Delete_ShouldDeleteUser()
{
    await _repository.CreateAsync(
        new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin"
        });

    var result = await _repository.DeleteAsync(1);

    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value, Is.True);

    var user = await _context.Users.FindAsync(1);

    Assert.That(user, Is.Null);
}

    [Test]
    public async Task Delete_WhenNotFound_ShouldReturnFailure()
    {
        var result = await _repository.DeleteAsync(999);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.NotFound>());
    }

    [Test]
    public async Task DeleteAll_ShouldDeleteAllUsers()
    {
        await _repository.CreateRangeAsync(
            new List<UserEntity>
            {
                new() { id = 1, name = "Kevin", username = "kevin" },
                new() { id = 2, name = "Juan", username = "juan" },
                new() { id = 3, name = "Ana", username = "ana" }
            });

        var result = await _repository.DeleteAllAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);

        var total = await _context.Users.CountAsync();

        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void DeleteAll_WhenCancelled_ShouldThrow()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _repository.DeleteAllAsync(cts.Token));
    }
    [Test]
    public async Task GetUserById_WhenNotFound_ShouldReturnNotFound()
    {
        var result = await _repository.GetUserByIdAsync(999);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.NotFound>());
    }
    [Test]
    public async Task Update_ShouldReturnUpdatedUser()
    {
        _context.Users.Add(new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin",
            email = "old@test.com"
        });

        await _context.SaveChangesAsync();

        var updated = new UserEntity
        {
            id = 1,
            name = "Kevin Updated",
            username = "kevin",
            email = "new@test.com"
        };

        var result = await _repository.UpdateAsync(updated, 1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(
            result.Value.name,
            Is.EqualTo("Kevin Updated"));
        Assert.That(
            result.Value.email,
            Is.EqualTo("new@test.com"));
    }
    [Test]
    public async Task Update_WhenNotFound_ShouldReturnNotFound()
    {
        var user = new UserEntity
        {
            id = 999,
            name = "Kevin",
            username = "kevin"
        };

        var result = await _repository.UpdateAsync(user, 999);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.NotFound>());
    }
    [Test]
    public async Task Delete_ShouldReturnSuccess()
    {
        _context.Users.Add(new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin"
        });

        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);

        var user = await _context.Users.FindAsync(1);

        Assert.That(user, Is.Null);
    }

    [Test]
    public async Task Delete_WhenNotFound_ShouldReturnNotFound()
    {
        var result = await _repository.DeleteAsync(999);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.NotFound>());
    }

    [Test]
    public async Task DeleteAll_ShouldReturnSuccess()
    {
        _context.Users.AddRange(
            new UserEntity
            {
                id = 1,
                name = "Kevin",
                username = "kevin"
            },
            new UserEntity
            {
                id = 2,
                name = "Juan",
                username = "juan"
            });

        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAllAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);

        var total = await _context.Users.CountAsync();

        Assert.That(total, Is.EqualTo(0));
    }
    [Test]
    public void DeleteAll_WhenCancelled_ShoulThrow()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () =>
                await _repository.DeleteAllAsync(cts.Token));
    }
}