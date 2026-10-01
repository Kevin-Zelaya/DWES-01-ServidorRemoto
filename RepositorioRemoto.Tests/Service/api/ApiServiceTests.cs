using Moq;

public class ApiServiceTests
{
    private IUserApiService _service;  
    private Mock<IUserApi> _apiMock;
    [SetUp]
    public void Setup()
    {
        _apiMock = new Mock<IUserApi>();
        _service = new UserApiService(_apiMock.Object);
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
}