using System.Net;
using Moq;
using Microsoft.Extensions.Logging;
using Refit;

public class ApiServiceTests
{
    private UserApiService _service;  
    private Mock<IUserApi> _apiMock;
    private Mock<ILogger<UserApiService>> _logger;
    [SetUp]
    public void Setup()
    {
        _apiMock = new Mock<IUserApi>();
        _logger = new Mock<ILogger<UserApiService>>();

        _service = new UserApiService(_apiMock.Object, logger: _logger.Object);
    }

    [Test]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new UserDto
                {
                    id = userId,
                    name = "John Doe",
                    username = "johndoe",
                    email = "john@example.com",

                    address = new AddressDto
                    {
                        street = "Main Street",
                        suite = "Apt. 1",
                        city = "Madrid",
                        zipcode = "28001",

                        geo = new GeoDto
                        {
                            lat = "40.4168",
                            lng = "-3.7038"
                        }
                    },

                    phone = "600000000",
                    website = "example.com",

                    company = new CompanyDto
                    {
                        name = "Example Company",
                        catchPhrase = "Example phrase",
                        bs = "Example bs"
                    }
                };
        _apiMock.Setup(api => api.GetUsuarioByIdAsync(userId)).ReturnsAsync(expectedUser);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.id, Is.EqualTo(expectedUser.id));
        Assert.That(result.Value.name, Is.EqualTo(expectedUser.name));
        Assert.That(result.Value.username, Is.EqualTo(expectedUser.username));
        Assert.That(result.Value.email, Is.EqualTo(expectedUser.email));
    }
    [Test]
    public async Task GetUserById_ShouldReturnUser_WhenUserNotExists()
    {
        // Arrange
        var userId = 1;
        _apiMock.Setup(api => api.GetUsuarioByIdAsync(userId)).ReturnsAsync((UserDto?)null);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.NotFound>());
    }
    [Test]
    public async Task GetUserById_ShouldReturnError_WhenApiThrowsException()
    {
        // Arrange
        var userId = 1;

        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            ReasonPhrase = "API error"
        };

        var exception = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://example.com/users/1"),
            HttpMethod.Get,
            response,
            new RefitSettings()
        );

        _apiMock
            .Setup(api => api.GetUsuarioByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.HttpFailure>());
    }
    [Test]
    public async Task GetUserById_ShouldReturnError_WhenApiThrowsTimeoutException()
    {
        // Arrange
        var userId = 1;
        _apiMock
            .Setup(api => api.GetUsuarioByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.Timeout>());
    }
    [Test]
    public async Task GetUserById_ShouldReturnError_WhenApiThrowsHttpRequestException()
    {
        // Arrange
        var userId = 1;
        _apiMock
            .Setup(api => api.GetUsuarioByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.NetworkError>());
    }
    /// <summary>
    /// Update
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task UpdateUser_ShouldReturnUpdatedUser_WhenUserExists()
    {
        // Arrange
        var user = new UpdateUserRequest
        {
            id = 1,
            name = "John Updated",
            username = "john",
            email = "john@example.com",

            address = new AddressDto
            {
                street = "Main Street",
                suite = "1",
                city = "Madrid",
                zipcode = "28001",
                geo = new GeoDto
                {
                    lat = "40.4168",
                    lng = "-3.7038"
                }
            },

            phone = "600000000",
            website = "example.com",

            company = new CompanyDto
            {
                name = "Company",
                catchPhrase = "Phrase",
                bs = "bs"
            }
        };

        // var updatedDto = user.ToDto();
        _apiMock
            .Setup(api => api.UpdateUsuarioAsync(
                user.id,
                It.IsAny<UpdateUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.ToDto());

        // Act
        var result = await _service.UpdateAsync(user.id, user);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.id, Is.EqualTo(user.id));
        Assert.That(result.Value.name, Is.EqualTo(user.name));
    }


    /// <summary>
    ///  create
    /// </summary>
    /// <returns></returns>
   [Test]
    public async Task Create_ShouldReturnSuccess_WhenApiCallSucceeds()
    {
        // Arrange
        var request = new CreateUserDto
        {
            name = "John Updated",
            username = "john",
            email = "john@example.com",
            address = new AddressDto
            {
                street = "Main Street",
                suite = "1",
                city = "Madrid",
                zipcode = "28001",
                geo = new GeoDto
                {
                    lat = "40.4168",
                    lng = "-3.7038"
                }
            },
            phone = "600000000",
            website = "example.com",
            company = new CompanyDto
            {
                name = "Company",
                catchPhrase = "Phrase",
                bs = "bs"
            }
        };

        var expectedUser = new UserDto
        {
            id = 1,
            name = "John Updated",
            username = "john",
            email = "john@example.com",
            address = new AddressDto
            {
                street = "Main Street",
                suite = "1",
                city = "Madrid",
                zipcode = "28001",
                geo = new GeoDto
                {
                    lat = "40.4168",
                    lng = "-3.7038"
                }
            },
            phone = "600000000",
            website = "example.com",
            company = new CompanyDto
            {
                name = "Company",
                catchPhrase = "Phrase",
                bs = "bs"
            }
        };

        _apiMock
            .Setup(api => api.CreateUsuarioAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.name, Is.EqualTo(expectedUser.name));
        Assert.That(result.Value.email, Is.EqualTo(expectedUser.email));

        _apiMock.Verify(
            api => api.CreateUsuarioAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task Create_ShouldReturnTimeout_WhenOperationIsCancelled()
    {
        // Arrange
        var request = new CreateUserDto();

        _apiMock
            .Setup(api => api.CreateUsuarioAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.Timeout>());
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenApiReturns400()
    {
        // Arrange
        var request = new CreateUserDto();

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        var exception = await ApiException.Create(
            new HttpRequestMessage(
                HttpMethod.Post,
                "https://example.com/users"),
            HttpMethod.Post,
            response,
            new RefitSettings());

        _apiMock
            .Setup(api => api.CreateUsuarioAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.BadRequest>());
    }

    [Test]
    public async Task Create_ShouldReturnNetworkError_WhenConnectionFails()
    {
        // Arrange
        var request = new CreateUserDto();

        _apiMock
            .Setup(api => api.CreateUsuarioAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.NetworkError>());

        var error = (ApiError.NetworkError)result.Error;

        Assert.That(
            error.Message,
            Is.EqualTo("Error de red al conectar con la API remota."));

        Assert.That(error.Details, Is.EqualTo("Connection failed"));
        Assert.That(
            error.StatusCode,
            Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public async Task Update_ShouldReturnTimeout_WhenOperationIsCancelled()
    {
        var userId = 1;
        var request = new UpdateUserRequest();

        _apiMock
            .Setup(api => api.UpdateUsuarioAsync(userId, request))
            .ThrowsAsync(new OperationCanceledException());

        var result = await _service.UpdateAsync(userId, request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.Timeout>());
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenApiReturns400()
    {
        var userId = 1;
        var request = new UpdateUserRequest();

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        var exception = await ApiException.Create(
            new HttpRequestMessage(
                HttpMethod.Put,
                "https://example.com/users/1"),
            HttpMethod.Put,
            response,
            new RefitSettings());

        _apiMock
            .Setup(api => api.UpdateUsuarioAsync(userId, request))
            .ThrowsAsync(exception);

        var result = await _service.UpdateAsync(userId, request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.BadRequest>());
    }

    [Test]
    public async Task Update_ShouldReturnNetworkError_WhenConnectionFails()
    {
        var userId = 1;
        var request = new UpdateUserRequest();

        _apiMock
            .Setup(api => api.UpdateUsuarioAsync(userId, request))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var result = await _service.UpdateAsync(userId, request);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.NetworkError>());

        var error = (ApiError.NetworkError)result.Error;

        Assert.That(
            error.Message,
            Is.EqualTo("Error de red al conectar con la API remota."));

        Assert.That(error.Details, Is.EqualTo("Connection failed"));
        Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }
    /// Fin update
    /// 
    [Test]
    public async Task DeleteUser_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        _apiMock
            .Setup(api => api.DeleteUsuarioAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }
    [Test]
    public async Task DeleteUser_ShouldReturnError_WhenUserNotExists()
    {
        // Arrange
        var userId = 1;
        _apiMock
            .Setup(api => api.DeleteUsuarioAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.False);
    }
    [Test]
    public async Task Delete_ShouldReturnTimeout_WhenOperationIsCancelled()
    {
        // Arrange
        var userId = 1;

        _apiMock
            .Setup(api => api.DeleteUsuarioAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.Timeout>());
    }
    
    [Test]
    public async Task Delete_ShouldReturnHttpFailure_WhenApiReturnsServerError()
    {
        // Arrange
        var userId = 1;

        var response = new HttpResponseMessage(
            HttpStatusCode.InternalServerError)
        {
            ReasonPhrase = "API error"
        };

        var exception = await ApiException.Create(
            new HttpRequestMessage(
                HttpMethod.Delete,
                "https://example.com/users/1"),
            HttpMethod.Delete,
            response,
            new RefitSettings()
        );

        _apiMock
            .Setup(api => api.DeleteUsuarioAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.HttpFailure>());

        var error = (ApiError.HttpFailure)result.Error;

        Assert.That(
            error.StatusCode,
            Is.EqualTo(HttpStatusCode.InternalServerError));
    }
    [Test]
    public async Task Delete_ShouldReturnNetworkError_WhenConnectionFails()
    {
        // Arrange
        var userId = 1;

        _apiMock
            .Setup(api => api.DeleteUsuarioAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.DeleteAsync(userId);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.NetworkError>());

        var error = (ApiError.NetworkError)result.Error;

        Assert.That(
            error.Message,
            Is.EqualTo("Error de red al conectar con la API remota."));

        Assert.That(error.Details, Is.EqualTo("Connection failed"));
        Assert.That(
            error.StatusCode,
            Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public async Task GetAll_ShouldReturnUsers_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedUsers = new List<UserDto>
        {
            new UserDto
            {
                id = 1,
                name = "John",
                username = "john",
                email = "john@example.com",
                address = new AddressDto
                {
                    street = "Main Street",
                    suite = "1",
                    city = "Madrid",
                    zipcode = "28001",
                    geo = new GeoDto
                    {
                        lat = "40.4168",
                        lng = "-3.7038"
                    }
                },
                phone = "600000000",
                website = "example.com",
                company = new CompanyDto
                {
                    name = "Company",
                    catchPhrase = "Phrase",
                    bs = "bs"
                }
            },
            new UserDto
            {
                id = 2,
                name = "Jane",
                username = "jane",
                email = "jane@example.com",
                address = new AddressDto
                {
                    street = "Second Street",
                    suite = "2",
                    city = "Leganés",
                    zipcode = "28911",
                    geo = new GeoDto
                    {
                        lat = "40.3280",
                        lng = "-3.7650"
                    }
                },
                phone = "611111111",
                website = "jane.com",
                company = new CompanyDto
                {
                    name = "Company 2",
                    catchPhrase = "Phrase 2",
                    bs = "bs 2"
                }
            }
        };

        _apiMock
            .Setup(api => api.GetUsuariosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Count.EqualTo(2));
        Assert.That(result.Value[0].name, Is.EqualTo("John"));
        Assert.That(result.Value[1].name, Is.EqualTo("Jane"));

        _apiMock.Verify(
            api => api.GetUsuariosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetAll_ShouldReturnEmptyList_WhenApiReturnsNoUsers()
    {
        // Arrange
        _apiMock
            .Setup(api => api.GetUsuariosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetAll_ShouldReturnTimeout_WhenOperationIsCancelled()
    {
        // Arrange
        _apiMock
            .Setup(api => api.GetUsuariosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.Timeout>());
    }

    [Test]
    public async Task GetAll_ShouldReturnNetworkError_WhenConnectionFails()
    {
        // Arrange
        _apiMock
            .Setup(api => api.GetUsuariosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ApiError.NetworkError>());

        var error = (ApiError.NetworkError)result.Error;

        Assert.That(
            error.Message,
            Is.EqualTo("Error de red al conectar con la API remota."));
        Assert.That(error.Details, Is.EqualTo("Connection failed"));
        Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }
}