using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SqliteRepositoryTests
{
    private AppDbContext _context = null!;
    private IRepository _repository = null!;
    private Mock<ILogger<SqliteRepository>> _logger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new AppDbContext(options);

        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _logger = new Mock<ILogger<SqliteRepository>>();

        _repository = new SqliteRepository(
            _context,
            _logger.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Test]
    public async Task GetAll_ShouldReturnUsers()
    {
        _context.Users.AddRange(
            new UserEntity
            {
                id = 1,
                name = "Kevin",
                username = "kevin",
                email = "kevin@test.com"
            },
            new UserEntity
            {
                id = 2,
                name = "Juan",
                username = "juan",
                email = "juan@test.com"
            });

        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Count.EqualTo(2));
    }
    [Test]
    public async Task GetAll_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        var result = await _repository.GetAllAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
    }
    [Test]
    public async Task GetUserById_ShouldReturnUser()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserByIdAsync(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.id, Is.EqualTo(1));
        Assert.That(result.Value.name, Is.EqualTo("Kevin"));
    }
    [Test]
    public async Task GetUserById_WhenUserDoesNotExist_ShouldReturnFailure()
    {
        var result = await _repository.GetUserByIdAsync(999);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.NotFound>());
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
    public async Task Create_WhenIdAlreadyExists_ShouldReturnFailure()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

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
    public async Task CreateRange_ShouldReturnSuccess()
    {
        var users = new List<UserEntity>
        {
            new()
            {
                id = 1,
                name = "Kevin",
                username = "kevin",
                email = "kevin@test.com"
            },
            new()
            {
                id = 2,
                name = "Juan",
                username = "juan",
                email = "juan@test.com"
            }
        };

        var result = await _repository.CreateRangeAsync(users);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }
    [Test]
    public async Task CreateRange_WhenListIsEmpty_ShouldReturnFailure()
    {
        var result = await _repository.CreateRangeAsync(
            new List<UserEntity>());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.WriteFailure>());
    }
    [Test]
    public async Task Update_ShouldReturnUpdatedUser()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin",
            email = "old@test.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var updatedUser = new UserEntity
        {
            id = 1,
            name = "Kevin Updated",
            username = "kevin",
            email = "new@test.com"
        };

        var result = await _repository.UpdateAsync(
            updatedUser,
            1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(
            result.Value.name,
            Is.EqualTo("Kevin Updated"));
    }
    [Test]
    public async Task Update_WhenUserDoesNotExist_ShouldReturnFailure()
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
        var user = new UserEntity
        {
            id = 1,
            name = "Kevin",
            username = "kevin"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    public async Task Delete_WhenUserDoesNotExist_ShouldReturnFailure()
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

        var users = await _context.Users.ToListAsync();

        Assert.That(users, Is.Empty);
    }
    // [Test]
    // public void DeleteAll_WhenCancelled_ShouldThrow()
    // {
    //     var cts = new CancellationTokenSource();
    //     cts.Cancel();

    //     Assert.ThrowsAsync<TaskCanceledException>(
    //         async () =>
    //             await _repository.DeleteAllAsync(cts.Token));
    // }
    // [Test]
    // public void GetUserById_WhenCancelled_ShouldThrow()
    // {
    //     var cts = new CancellationTokenSource();
    //     cts.Cancel();

    //     Assert.ThrowsAsync<OperationCanceledException>(
    //         async () =>
    //             await _repository.GetUserByIdAsync(
    //                 1,
    //                 cts.Token));
    // }
}