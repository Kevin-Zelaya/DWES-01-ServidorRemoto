public static class UserMapper
{
    /// <summary>
    /// De dto a modelo
    /// </summary>
    public static UserModel ToModel(this UserDto dto) => new()
    {
        id = dto.id,
        name = dto.name,
        username = dto.username,
        email = dto.email,

        address = new Address
        {
            street = dto.address.street,
            suite = dto.address.suite,
            city = dto.address.city,
            zipcode = dto.address.zipcode,

            geo = new Geo
            {
                lat = dto.address.geo.lat,
                lng = dto.address.geo.lng
            }
        },

        phone = dto.phone,
        website = dto.website,

        company = new Company
        {
            name = dto.company.name,
            catchPhrase = dto.company.catchPhrase,
            bs = dto.company.bs
        }
    };
    /// <summary>
    /// De modelo a dto
    /// </summary>
    public static UserDto ToDto(this UserModel model) => new()
    {
        id = model.id,
        name = model.name,
        username = model.username,
        email = model.email,

        address = new AddressDto
        {
            street = model.address.street,
            suite = model.address.suite,
            city = model.address.city,
            zipcode = model.address.zipcode,

            geo = new GeoDto
            {
                lat = model.address.geo.lat,
                lng = model.address.geo.lng
            }
        },

        phone = model.phone,
        website = model.website,

        company = new CompanyDto
        {
            name = model.company.name,
            catchPhrase = model.company.catchPhrase,
            bs = model.company.bs
        }
    };
}