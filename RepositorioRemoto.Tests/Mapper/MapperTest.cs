
public class MapperTest
{
    public UserModel model = new();
    public UserDto dto = new();
    [SetUp]
    public void Setup()
    {
        model = new UserModel()
        {
            name = "Kevin",
            username = "kevin123",
            email = "kevin@gmail.com",

            address = new Address
            {
                street = "Calle Mayor",
                suite = "1A",
                city = "Madrid",
                zipcode = "28001",

                geo = new Geo
                {
                    lat = "40.4168",
                    lng = "-3.7038"
                }
            },

            phone = "600123456",
            website = "kevin.com",

            company = new Company
            {
                name = "Mi Empresa",
                catchPhrase = "Una empresa de ejemplo",
                bs = "business"
            }
        };

        dto = new UserDto()
        {
            name = "Kevin",
            username = "kevin123",
            email = "kevin@gmail.com",

            address = new AddressDto
            {
                street = "Calle Mayor",
                suite = "1A",
                city = "Madrid",
                zipcode = "28001",

                geo = new GeoDto
                {
                    lat = "40.4168",
                    lng = "-3.7038"
                }
            },

            phone = "600123456",
            website = "kevin.com",

            company = new CompanyDto
            {
                name = "Mi Empresa",
                catchPhrase = "Una empresa de ejemplo",
                bs = "business"
            }
        };
    }
    [Test]
    public void MapUser_Model_CorrectMap()
    {
        
        // Arranve Act
        var model = dto.ToModel();
        // Arrange
        Assert.That(model.name, Is.EqualTo(dto.name));
        Assert.That(model.username, Is.EqualTo(dto.username));
        Assert.That(model.email, Is.EqualTo(dto.email));
        Assert.That(model.address.city, Is.EqualTo(dto.address.city));
        Assert.That(model.phone, Is.EqualTo(dto.phone));
        Assert.That(model.website, Is.EqualTo(dto.website));
        Assert.That(model.company.name, Is.EqualTo(dto.company.name));
    }
    [Test]
    public void MapUser_dto_CorrectMap()
    {
        
        // Arranve Act
        var dto = model.ToDto();
        // Arrange
        Assert.That(dto.name, Is.EqualTo(model.name));
        Assert.That(dto.username, Is.EqualTo(model.username));
        Assert.That(dto.email, Is.EqualTo(model.email));
        Assert.That(dto.address.city, Is.EqualTo(model.address.city));
        Assert.That(dto.phone, Is.EqualTo(model.phone));
        Assert.That(dto.website, Is.EqualTo(model.website));
        Assert.That(dto.company.name, Is.EqualTo(model.company.name));
    }

}