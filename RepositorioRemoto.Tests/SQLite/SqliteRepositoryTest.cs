using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

[TestFixture]
public class SqliteRepositoryTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _context = null!;
    private SqliteRepository _repository = null!;

    // Se ejecuta antes de cada test para levantar una base de datos limpia en memoria
    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        var logger = NullLogger<SqliteRepository>.Instance;
        _repository = new SqliteRepository(_context, logger);
    }

    // Se ejecuta después de cada test para liberar recursos de la base de datos
    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        //Añadimos un par de usuarios
        _context.Users.AddRange(
            new UserEntity { name = "User 1", email = "1@test.com" },
            new UserEntity { name = "User 2", email = "2@test.com" }
        );
        await _context.SaveChangesAsync();

        //Obtenemos todos
        var result = await _repository.GetAllAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var user = new UserEntity { name = "Sergio", email = "sergio@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserByIdAsync(user.id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.name, Is.EqualTo("Sergio"));
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        //Buscar ID inexistente
        var result = await _repository.GetUserByIdAsync(999);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldSupportCancellationToken()
    {
        var user = new UserEntity { name = "TokenUser", email = "token@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        using var cts = new CancellationTokenSource();

        //Consultar pasando el token de cancelación
        var result = await _repository.GetUserByIdAsync(user.id, cts.Token);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.name, Is.EqualTo("TokenUser"));
    }

    [Test]
    public async Task CreateAsync_ShouldInsertUserSuccessfully()
    {
        var user = new UserEntity { name = "Nuevo", email = "nuevo@test.com" };

        var result = await _repository.CreateAsync(user);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.id, Is.GreaterThan(0));
    }

    [Test]
    public async Task CreateRangeAsync_ShouldInsertUsersSuccessfully()
    {
        var users = new List<UserEntity>
        {
            new() { name = "R1", email = "r1@test.com" },
            new() { name = "R2", email = "r2@test.com" }
        };

        var result = await _repository.CreateRangeAsync(users);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    public async Task CreateRangeAsync_ShouldReturnFailure_WhenCollectionIsEmpty()
    {
        //Pasar colección vacía para cubrir la validación
        var result = await _repository.CreateRangeAsync(new List<UserEntity>());

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateAsync_ShouldModifyExistingUser()
    {
        var user = new UserEntity { name = "Viejo", email = "viejo@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.name = "Modificado";
        var result = await _repository.UpdateAsync(user, user.id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.name, Is.EqualTo("Modificado"));
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var user = new UserEntity { id = 999, name = "No Existe", email = "no@test.com" };

        //Intentar actualizar un usuario que no está en la BD
        var result = await _repository.UpdateAsync(user, 999);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveUser_WhenUserExists()
    {
        var user = new UserEntity { name = "Borrar", email = "borrar@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var deleteResult = await _repository.DeleteAsync(user.id);
        Assert.That(deleteResult.IsSuccess, Is.True);

        // Comprobar que ya no se encuentra
        var getResult = await _repository.GetUserByIdAsync(user.id);
        Assert.That(getResult.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var result = await _repository.DeleteAsync(999);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAllAsync_ShouldClearAllUsers()
    {
        _context.Users.Add(new UserEntity { name = "U1", email = "u1@test.com" });
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAllAsync();
        Assert.That(result.IsSuccess, Is.True);

        var all = await _repository.GetAllAsync();
        Assert.That(all.Value, Is.Empty);
    }

    [Test]
    public async Task DeleteAllAsync_ShouldSupportCancellationToken()
    {
        _context.Users.Add(new UserEntity { name = "U2", email = "u2@test.com" });
        await _context.SaveChangesAsync();

        using var cts = new CancellationTokenSource();

        var result = await _repository.DeleteAllAsync(cts.Token);

        Assert.That(result.IsSuccess, Is.True);
    }
}