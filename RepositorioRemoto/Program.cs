


// Prueba



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;



var provider = DependencyProvider.Configure();

var scoped = provider.CreateScope();

var services = provider.GetRequiredService<IUserApiService>();


var users = await services.GetAllAsync();
/*
if(users is Result<List<UserDto>, DomainError>.Success usersOk)
{
    
    foreach(var user in usersOk.value)
    {
        
        Console.WriteLine(user.company.name);
    }
}

var userr = await services.CreateAsync(
    new CreateUserDto
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
    }
);

if(userr is Result<UserDto, DomainError>.Success useryes)
{
    var valor = useryes.value;
    Console.WriteLine($"Nombre: {valor.name} compañia: {valor.company.name} {valor.address.geo.lat}");
}
*/


var contex = scoped.ServiceProvider.GetRequiredService<AppDbContext>();
