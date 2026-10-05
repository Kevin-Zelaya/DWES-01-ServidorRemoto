using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;
using CSharpFunctionalExtensions;

public class PostgreSql
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private PostgreSqlRepository _repository = null!;
    private Mock<ILogger<PostgreSqlRepository>> _logger = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("test_db")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();
    }
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(_container.GetConnectionString());

        _context = new AppDbContext(optionsBuilder.Options);
        await _context.Database.EnsureCreatedAsync();
        _context.EnsureCreated();

        _logger = new Mock<ILogger<PostgreSqlRepository>>();
        _repository = new PostgreSqlRepository( 
            _logger.Object,
            _context

        );
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Test]
    public async Task CreateUser_ShouldReturnUser()
    {
        // Arrange
        var user = new UserEntity
        {
            id = 1,
            name = "Leanne Graham",
            username = "Bret",
            email = "Sincere@april.biz",
            address = """
            {
                "street": "Kulas Light",
                "suite": "Apt. 556",
                "city": "Gwenborough",
                "zipcode": "92998-3874",
                "geo": {
                    "lat": "-37.3159",
                    "lng": "81.1496"
                }
            }
            """,
            phone = "1-770-736-8031",
            website = "hildegard.org",
            company = """
            {
                "name": "Romaguera-Crona",
                "catchPhrase": "Multi-layered client-server neural-net",
                "bs": "harness real-time e-markets"
            }
            """
        };
        // Arc
        var response = await _repository.CreateAsync(
            user
        );
        // Assert
        Assert.That(response.IsSuccess, Is.True);
    }
    [Test]
    public async Task FindUser_ShouldReturnNotFound_WheUserExist()
    {
        // Arrange
        await _repository.CreateAsync(new UserEntity
        {
            id = 1,
            name = "Leanne Graham",
            username = "Bret",
            email = "Sincere@april.biz",
            address = """
            {
                "street": "Kulas Light",
                "suite": "Apt. 556",
                "city": "Gwenborough",
                "zipcode": "92998-3874",
                "geo": {
                    "lat": "-37.3159",
                    "lng": "81.1496"
                }
            }
            """,
            phone = "1-770-736-8031",
            website = "hildegard.org",
            company = """
            {
                "name": "Romaguera-Crona",
                "catchPhrase": "Multi-layered client-server neural-net",
                "bs": "harness real-time e-markets"
            }
            """
        });
        
        // Arc
        var user = await _repository.GetUserByIdAsync(1);
        // Aserrt
        Assert.That(user.IsSuccess, Is.True);
    }
    [Test]
    public async Task FindUser_ShouldReturnNotFound_WheUserDontExist()
    {
        // Arrange / Arc
        var user = await _repository.GetUserByIdAsync(1);
        // Assert
        Assert.That(user.IsFailure, Is.True);
    }
    [Test]
    public async Task UpdateUser_ShouldReturnSucces()
    {
        // Arrange
        var user = await _repository.CreateAsync(new UserEntity
        {
            id = 1,
            name = "Leanne Graham",
            username = "Bret",
            email = "Sincere@april.biz",
            address = """
            {
                "street": "Kulas Light",
                "suite": "Apt. 556",
                "city": "Gwenborough",
                "zipcode": "92998-3874",
                "geo": {
                    "lat": "-37.3159",
                    "lng": "81.1496"
                }
            }
            """,
            phone = "1-770-736-8031",
            website = "hildegard.org",
            company = """
            {
                "name": "Romaguera-Crona",
                "catchPhrase": "Multi-layered client-server neural-net",
                "bs": "harness real-time e-markets"
            }
            """
        });
        
        // Arc
        var response = await _repository.UpdateAsync(
            user.Value,
            1);
        // Aserrt
        Assert.That(response.IsSuccess, Is.True);
    }
    [Test]
    public async Task UpdateUser_ShouldReturnSucces_WhenUserDontExist()
    {
        // Arrange / Arc
        var user = new UserEntity()
        {
            id = 1,
            name = "Leanne Graham",
            username = "Bret",
            email = "Sincere@april.biz",
            address = """
            {
                "street": "Kulas Light",
                "suite": "Apt. 556",
                "city": "Gwenborough",
                "zipcode": "92998-3874",
                "geo": {
                    "lat": "-37.3159",
                    "lng": "81.1496"
                }
            }
            """,
            phone = "1-770-736-8031",
            website = "hildegard.org",
            company = """
            {
                "name": "Romaguera-Crona",
                "catchPhrase": "Multi-layered client-server neural-net",
                "bs": "harness real-time e-markets"
            }
            """
        };
        
        
        var response = await _repository.UpdateAsync(
            user,
            1);
        // Aserrt
        Assert.That(response.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNotFound_WheUserExist()
    {
        // Arrange
        await _repository.CreateAsync(new UserEntity
        {
            id = 1,
            name = "Leanne Graham",
            username = "Bret",
            email = "Sincere@april.biz",
            address = """
            {
                "street": "Kulas Light",
                "suite": "Apt. 556",
                "city": "Gwenborough",
                "zipcode": "92998-3874",
                "geo": {
                    "lat": "-37.3159",
                    "lng": "81.1496"
                }
            }
            """,
            phone = "1-770-736-8031",
            website = "hildegard.org",
            company = """
            {
                "name": "Romaguera-Crona",
                "catchPhrase": "Multi-layered client-server neural-net",
                "bs": "harness real-time e-markets"
            }
            """
        });
        
        // Arc
        var user = await _repository.DeleteAsync(1);
        // Aserrt
        Assert.That(user.IsSuccess, Is.True);
    }
    [Test]
    public async Task DeleteUser_ShouldReturnNotFound_WheUserDontExist()
    {   
        // Arrange / Arc
        var user = await _repository.DeleteAsync(1);
        // Aserrt
        Assert.That(user.IsSuccess, Is.False);
    }

    [Test]
    public async Task GetAll_ShouldReturnUser()
    {
        // Arrange
        await _repository.CreateAsync(new UserEntity
        {
            id = 1,
            name = "Leanne Graham",
            username = "Bret",
            email = "Sincere@april.biz",
            address = """
            {
                "street": "Kulas Light",
                "suite": "Apt. 556",
                "city": "Gwenborough",
                "zipcode": "92998-3874",
                "geo": {
                    "lat": "-37.3159",
                    "lng": "81.1496"
                }
            }
            """,
            phone = "1-770-736-8031",
            website = "hildegard.org",
            company = """
            {
                "name": "Romaguera-Crona",
                "catchPhrase": "Multi-layered client-server neural-net",
                "bs": "harness real-time e-markets"
            }
            """
        });
        // Arc
        var response = await _repository.GetAllAsync();
        // Assert
        Assert.That(response.IsSuccess, Is.True);
    }
    
    [Test]
    public async Task CreateRangeAsync_ShouldCreateAllUsers_WhenUsersAreValid()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new() { name = "User 1", username = "user1" },
            new() { name = "User 2", username = "user2" },
            new() { name = "User 3", username = "user3" }
        };

        // Act
        var response = await _repository.CreateRangeAsync(users);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(await _context.Users.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task CreateRangeAsync_ShouldFail_WhenUsersListIsEmpty()
    {
        // Arrange
        var users = new List<UserEntity>();

        // Act
        var response = await _repository.CreateRangeAsync(users);

        // Assert
        Assert.That(response.IsFailure, Is.True);
        Assert.That(await _context.Users.CountAsync(), Is.EqualTo(0));
    }
    [Test]
    public async Task CreateRangeAsync_ShouldFail_WhenPrimaryKeyAlreadyExists()
    {
        // Arrange
        _context.Users.Add(new UserEntity
        {
            id = 1,
            name = "Existing user",
            username = "existing"
        });

        await _context.SaveChangesAsync();

        var users = new List<UserEntity>
        {
            new()
            {
                id = 1,
                name = "Duplicate user",
                username = "duplicate"
            }
        };

        // Act
        var response = await _repository.CreateRangeAsync(users);

        // Assert
        Assert.That(response.IsFailure, Is.True);
        Assert.That(await _context.Users.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Act
        var response = await _repository.GetAllAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.Empty);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var existingUser = new UserEntity
        {
            id = 1,
            name = "Prueba",
            username = "Kenrro"
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var updatedUser = new UserEntity
        {
            id = 1,
            name = "Nuevo nombre",
            username = "Kenrro 2"
        };

        // Act
        var response = await _repository.UpdateAsync(updatedUser, 1);

        // Assert
        Assert.That(response.IsSuccess, Is.True);

        _context.ChangeTracker.Clear();

        var userInDatabase = await _context.Users
            .AsNoTracking()
            .FirstAsync(u => u.id == 1);

        Assert.That(userInDatabase.name, Is.EqualTo("Nuevo nombre"));
        Assert.That(userInDatabase.username, Is.EqualTo("Kenrro 2"));
    }

    [Test]
    public async Task DeleteAllAsync_ShouldDeleteAllUsers_WhenUsersExist()
    {
        // Arrange
        _context.Users.AddRange(
            new UserEntity { name = "User 1", username = "user1" },
            new UserEntity { name = "User 2", username = "user2" },
            new UserEntity { name = "User 3", username = "user3" }
        );

        await _context.SaveChangesAsync();

        // Act
        var response = await _repository.DeleteAllAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);
        Assert.That(await _context.Users.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteAllAsync_ShouldSucceed_WhenNoUsersExist()
    {
        // Act
        var response = await _repository.DeleteAllAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);
        Assert.That(await _context.Users.CountAsync(), Is.EqualTo(0));
    }

   

    [Test]
    public async Task GetAllAsync_ShouldReturnReadFailure_WhenTableDoesNotExist()
    {
        // Arrange
        await _context.Database.ExecuteSqlRawAsync(
            "DROP TABLE users CASCADE");

        // Act
        var response = await _repository.GetAllAsync();

        // Assert
        Assert.That(response.IsFailure, Is.True);
        Assert.That(response.Error, Is.TypeOf<DatabaseError.ReadFailure>());
    }
    [Test]
    public void DeleteAllAsync_ShouldThrowOperationCanceledException_WhenTokenIsCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _repository.DeleteAllAsync(cts.Token));
    }
        
    [Test]
    public async Task CreateRangeAsync_ShouldReturnWriteFailure_WhenListIsEmpty()
    {
        // Arrange
        var users = new List<UserEntity>();

        // Act
        var result = await _repository.CreateRangeAsync(users);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.WriteFailure>());
    }

    [Test]
    public void GetUserByIdAsync_ShouldThrow_WhenOperationIsCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _repository.GetUserByIdAsync(1, cts.Token));
    }

    [Test]
    public async Task DeleteAllAsync_ShouldReturnWriteFailure_WhenTableDoesNotExist()
    {
        // Arrange
        await _context.Database.ExecuteSqlRawAsync(
            """DROP TABLE "users" CASCADE""");

        // Act
        var result = await _repository.DeleteAllAsync();

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.WriteFailure>());
    }
    [Test]
    public async Task CreateAsync_ShouldReturnConstraintViolation_WhenRequiredFieldIsNull()
    {
        // Arrange
        var user = new UserEntity
        {
            name = null!,
            username = "kevin",
            email = "kevin@test.com"
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.ConstraintViolation>());
    }
    [Test]
    public async Task CreateRangeAsync_ShouldReturnConstraintViolation_WhenRequiredFieldIsNull()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new()
            {
                name = null!,
                username = "kevin",
                email = "kevin@test.com"
            }
        };

        // Act
        var result = await _repository.CreateRangeAsync(users);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.ConstraintViolation>());
    }
    [Test]
    public async Task CreateRangeAsync_ShouldReturnConstraintViolation_WhenRequiredFieldIsNull2()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new()
            {
                name = null!,
                username = "kevin",
                email = "kevin@test.com"
            }
        };

        // Act
        var result = await _repository.CreateRangeAsync(users);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.ConstraintViolation>());
    }
    [Test]
    public async Task UpdateAsync_ShouldReturnWriteFailure_WhenRequiredFieldIsNull()
    {
        // Arrange
        var user = new UserEntity
        {
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        var created = await _repository.CreateAsync(user);
        Assert.That(created.IsSuccess, Is.True);

        user.name = null!;

        // Act
        var result = await _repository.UpdateAsync(user, created.Value.id);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.TypeOf<DatabaseError.WriteFailure>());
    }
    [Test]
    public async Task CreateAsync_ShouldReturnConstraintViolation_WhenRequiredFieldIsNull3()
    {
        // Arrange
        var user = new UserEntity
        {
            name = null!,
            username = "kevin",
            email = "kevin@test.com"
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.ConstraintViolation>());
    }

    [Test]
    public async Task CreateAsync_ShouldReturnUnknown_WhenContextIsDisposed()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        var disposedContext = new AppDbContext(options);
        var repository = new PostgreSqlRepository(_logger.Object, disposedContext);

        await disposedContext.DisposeAsync();

        var user = new UserEntity
        {
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        // Act
        var result = await repository.CreateAsync(user);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.Unknown>());
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnWriteFailure_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var user = new UserEntity
        {
            name = "Kevin",
            username = "kevin",
            email = "kevin@test.com"
        };

        var created = await _repository.CreateAsync(user);
        Assert.That(created.IsSuccess, Is.True);

        // Provocamos una violación de NOT NULL.
        user.name = null!;

        // Act
        var result = await _repository.UpdateAsync(user, created.Value.id);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.WriteFailure>());
    }
    [Test]
    public async Task CreateAsync_ShouldReturnConstraintViolation_WhenRequiredFieldIsNull2()
    {
        // Arrange
        var user = new UserEntity
        {
            name = null!,
            username = "kevin",
            email = "kevin@test.com"
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<DatabaseError.ConstraintViolation>());
    }
}