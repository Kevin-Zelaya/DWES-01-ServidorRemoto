using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RepositorioRemoto.Cache.Common;

public class UserServiceTests
{
    private Mock<ILogger<UserService>> _logger;
    private Mock<ICache<UserModel>> _cache;
    private Mock<IUserApiService> _api;

    private IRepository _repository;
    private UnitOfWork _unitOfWork;
    private AppDbContext _context;
    private UserService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new AppDbContext(options);

        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        var loggerRepository = new Mock<ILogger<SqliteRepository>>();

        _logger = new Mock<ILogger<UserService>>();
        _cache = new Mock<ICache<UserModel>>();
        _api = new Mock<IUserApiService>();

        _repository = new SqliteRepository(
            _context,
            loggerRepository.Object
        );

        _unitOfWork = new UnitOfWork(_context);

        _service = new UserService(
            _repository,
            _api.Object,
            _unitOfWork,
            _cache.Object,
            _logger.Object
        );
    }
    [TearDown]
    public async Task TearDown()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Test]
    public async Task SyncUsers_SycnAllServices()
    {
        List<UserModel> users = new List<UserModel>
        {
            new UserModel
            {
                id = 1,
                name = "Alejandro Pérez",
                username = "alexperez",
                email = "alex.perez@example.com",
                phone = "+34 600 123 456",
                website = "alexperez.dev",
                address = new Address
                {
                    street = "Gran Vía",
                    suite = "Piso 3B",
                    city = "Madrid",
                    zipcode = "28013",
                    geo = new Geo
                    {
                        lat = "40.4199",
                        lng = "-3.7026"
                    }
                },
                company = new Company
                {
                    name = "Innovación Digital S.L.",
                    catchPhrase = "Soluciones tecnológicas a tu medida",
                    bs = "desarrollo web cloud it"
                }
            },
            new UserModel
            {
                id = 2,
                name = "Sofía Gómez",
                username = "sofiag",
                email = "sofia.gomez@example.com",
                phone = "+34 611 987 654",
                website = "sofiait.com",
                address = new Address
                {
                    street = "Avinguda Diagonal",
                    suite = "Ático 2",
                    city = "Barcelona",
                    zipcode = "08018",
                    geo = new Geo
                    {
                        lat = "41.4036",
                        lng = "2.1744"
                    }
                },
                company = new Company
                {
                    name = "ByteCore Studio",
                    catchPhrase = "Transformando datos en conexiones reales",
                    bs = "big data ia automatizacion"
                }
            }
        };
        // Configuracion de la api
        _api
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(Result.Success<List<UserModel>, DomainError>(users));
        // configuración del repositorio
        _cache
            .Setup(x => x.ClearAsync())
            .ReturnsAsync(Result.Success<bool, DomainError>(true));
        

        // Arc
        var response = await _service.SyncUsersAsync();
        // Assert
        Assert.That(response.IsSuccess, Is.True);

    }
    [Test]
    public async Task GetUserAndCreate_shouldReturnUser()
    {
        var user = new CreateUserDto()
            {
                name = "Sofía Gómez",
                username = "sofiag",
                email = "sofia.gomez@example.com",
                phone = "+34 611 987 654",
                website = "sofiait.com",
                address = new AddressDto
                {
                    street = "Avinguda Diagonal",
                    suite = "Ático 2",
                    city = "Barcelona",
                    zipcode = "08018",
                    geo = new GeoDto
                    {
                        lat = "41.4036",
                        lng = "2.1744"
                    }
                },
                company = new CompanyDto
                {
                    name = "ByteCore Studio",
                    catchPhrase = "Transformando datos en conexiones reales",
                    bs = "big data ia automatizacion"
                }
            };
        var model = new UserModel()
            {
                id = 1,
                name = "Sofía Gómez",
                username = "sofiag",
                email = "sofia.gomez@example.com",
                phone = "+34 611 987 654",
                website = "sofiait.com",
                address = new Address
                {
                    street = "Avinguda Diagonal",
                    suite = "Ático 2",
                    city = "Barcelona",
                    zipcode = "08018",
                    geo = new Geo
                    {
                        lat = "41.4036",
                        lng = "2.1744"
                    }
                },
                company = new Company
                {
                    name = "ByteCore Studio",
                    catchPhrase = "Transformando datos en conexiones reales",
                    bs = "big data ia automatizacion"
                }
            };
        // Configuracion de la api
        _api
            .Setup(x => x.CreateAsync(user))
            .ReturnsAsync(Result.Success<UserModel, DomainError>(model));
        

        // Arc

        var createdUser = await _service.CreateUserAsync(user); // Creamos
        var returningUser = await _service.GetUserByIdAsync(1); // Buscamos
        // Assert
        Assert.That(createdUser.IsSuccess, Is.True);
        Assert.That(returningUser.IsSuccess, Is.True);

    }
    [Test]
    public async Task UpdateUser_ShoulBeReturnUpdatedUser()
    {
        var user = new CreateUserDto()
        {
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com",
            phone = "+34 611 987 654",
            website = "sofiait.com",
            address = new AddressDto
            {
                street = "Avinguda Diagonal",
                suite = "Ático 2",
                city = "Barcelona",
                zipcode = "08018",
                geo = new GeoDto
                {
                    lat = "41.4036",
                    lng = "2.1744"
                }
            },
            company = new CompanyDto
            {
                name = "ByteCore Studio",
                catchPhrase = "Transformando datos en conexiones reales",
                bs = "big data ia automatizacion"
            }
        };

        var model = new UserModel()
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com",
            phone = "+34 611 987 654",
            website = "sofiait.com",
            address = new Address
            {
                street = "Avinguda Diagonal",
                suite = "Ático 2",
                city = "Barcelona",
                zipcode = "08018",
                geo = new Geo
                {
                    lat = "41.4036",
                    lng = "2.1744"
                }
            },
            company = new Company
            {
                name = "ByteCore Studio",
                catchPhrase = "Transformando datos en conexiones reales",
                bs = "big data ia automatizacion"
            }
        };

        var update = new UpdateUserRequest()
        {
            id = 1,
            name = "Kevin Zelaya",
            username = "sofiag",
            email = "sofia.gomez@example.com",
            phone = "+34 611 987 654",
            website = "sofiait.com",
            address = new AddressDto
            {
                street = "Avinguda Diagonal",
                suite = "Ático 2",
                city = "Barcelona",
                zipcode = "08018",
                geo = new GeoDto
                {
                    lat = "41.4036",
                    lng = "2.1744"
                }
            },
            company = new CompanyDto
            {
                name = "ByteCore Studio",
                catchPhrase = "Transformando datos en conexiones reales",
                bs = "big data ia automatizacion"
            }
        };

        // API Create
        _api
            .Setup(x => x.CreateAsync(user))
            .ReturnsAsync(
                Result.Success<UserModel, DomainError>(model));

        // Cache Create
        _cache
            .Setup(x => x.SetAsync(
                "user:1",
                It.IsAny<UserModel>()))
            .ReturnsAsync(
                Result.Success<bool, DomainError>(true));

        // API Update
        var updatedModel = new UserModel()
        {
            id = 1,
            name = "Kevin Zelaya",
            username = "sofiag",
            email = "sofia.gomez@example.com"
        };

        _api
            .Setup(x => x.UpdateAsync(1, update))
            .ReturnsAsync(
                Result.Success<UserModel, DomainError>(updatedModel));

        var createdUser = await _service.CreateUserAsync(user);

        var updatedUser = await _service.UpdateUserAsync(1, update);

        Assert.That(createdUser.IsSuccess, Is.True);
        Assert.That(updatedUser.IsSuccess, Is.True);
        Assert.That(updatedUser.Value.name, Is.EqualTo(update.name));
    }
    [Test]
    public async Task GetAllUsers_ShouldReturnUsers()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com",

            address = """
            {
                "street": "Avinguda Diagonal",
                "suite": "Ático 2",
                "city": "Barcelona",
                "zipcode": "08018",
                "geo": {
                    "lat": "41.4036",
                    "lng": "2.1744"
                }
            }
            """,

            company = """
            {
                "name": "ByteCore Studio",
                "catchPhrase": "Transformando datos en conexiones reales",
                "bs": "big data ia automatizacion"
            }
            """
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = await _service.GetAllUsersAsync();

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Count, Is.EqualTo(1));
        Assert.That(response.Value[0].name, Is.EqualTo("Sofía Gómez"));
    }

    [Test]
    public async Task GetUserById_ShouldReturnUser()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com",

            address = """
            {
                "street": "Avinguda Diagonal",
                "suite": "Ático 2",
                "city": "Barcelona",
                "zipcode": "08018",
                "geo": {
                    "lat": "41.4036",
                    "lng": "2.1744"
                }
            }
            """,

            company = """
            {
                "name": "ByteCore Studio",
                "catchPhrase": "Transformando datos en conexiones reales",
                "bs": "big data ia automatizacion"
            }
            """
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _cache
            .Setup(x => x.GetAsync("user:1"))
            .ReturnsAsync(
                Result.Failure<UserModel, DomainError>(
                    new DatabaseError.NotFound(
                        "No se encontró en la cache",
                        "user:1")));

        var response = await _service.GetUserByIdAsync(1);

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.name, Is.EqualTo("Sofía Gómez"));
    }

    [Test]
    public async Task GetUserById_WhenUserIsInCache_ShouldReturnUser()
    {
        var user = new UserModel
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag"
        };

        _cache
            .Setup(x => x.GetAsync("user:1"))
            .ReturnsAsync(Result.Success<UserModel, DomainError>(user));

        var response = await _service.GetUserByIdAsync(1);

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.name, Is.EqualTo("Sofía Gómez"));
    }

    [Test]
    public async Task GetUserById_WhenUserDoesNotExist_ShouldReturnApiUser()
    {
        var user = new UserModel
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag"
        };

        _cache
            .Setup(x => x.GetAsync("user:1"))
            .ReturnsAsync(Result.Failure<UserModel, DomainError>(
                new DatabaseError.NotFound("No se encontró en la cache", "user:1")));

        _api
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(Result.Success<UserModel, DomainError>(user));

        var response = await _service.GetUserByIdAsync(1);

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.name, Is.EqualTo("Sofía Gómez"));
    }
    [Test]
    public async Task CreateUser_WhenApiFails_ShouldReturnFailure()
    {
        var user = new CreateUserDto
        {
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com"
        };

        _api
            .Setup(x => x.CreateAsync(user))
            .ReturnsAsync(Result.Failure<UserModel, DomainError>(
                new DatabaseError.Unknown("Error API")));

        var response = await _service.CreateUserAsync(user);

        Assert.That(response.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateUser_ShouldReturnUpdatedUser()
    {
        // Usuario que ya existe en la BD
        _context.Users.Add(new UserEntity
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com"
        });

        await _context.SaveChangesAsync();

        var update = new UpdateUserRequest
        {
            id = 1,
            name = "Kevin Zelaya",
            username = "sofiag",
            email = "sofia.gomez@example.com"
        };

        var model = new UserModel
        {
            id = 1,
            name = "Kevin Zelaya",
            username = "sofiag",
            email = "sofia.gomez@example.com"
        };

        _api
            .Setup(x => x.UpdateAsync(1, update))
            .ReturnsAsync(
                Result.Success<UserModel, DomainError>(model));

        _cache
            .Setup(x => x.SetAsync(
                "user:1",
                It.IsAny<UserModel>()))
            .ReturnsAsync(
                Result.Success<bool, DomainError>(true));

        var response = await _service.UpdateUserAsync(1, update);

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.name, Is.EqualTo(update.name));
    }

    [Test]
    public async Task DeleteUser_ShouldReturnSuccess()
    {
        var user = new UserEntity
        {
            id = 1,
            name = "Sofía Gómez",
            username = "sofiag",
            email = "sofia.gomez@example.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _cache
            .Setup(x => x.RemoveAsync("user:1"))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        var response = await _service.DeleteUserAsync(1);

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);
    }

    [Test]
    public async Task DeleteUser_WhenApiFails_ShouldReturnFailure()
    {
        _api
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown("Error API")));

        var response = await _service.DeleteUserAsync(1);

        Assert.That(response.IsFailure, Is.True);
    }

    [Test]
    public async Task SyncUsers_WhenApiFails_ShouldReturnFailure()
    {
        _api
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(Result.Failure<List<UserModel>, DomainError>(
                new DatabaseError.Unknown("Error API")));

        var response = await _service.SyncUsersAsync();

        Assert.That(response.IsFailure, Is.True);
    }
    
}