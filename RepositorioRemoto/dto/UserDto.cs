public class AddressDto
{
    public string street { get; set; } = string.Empty;
    public string suite { get; set; } = string.Empty;
    public string city { get; set; } = string.Empty;
    public string zipcode { get; set; } = string.Empty;
    public GeoDto geo { get; set; } = new();
}

public class CompanyDto
{
    public string name { get; set; } = string.Empty;
    public string catchPhrase { get; set; } = string.Empty;
    public string bs { get; set; } = string.Empty;
}

public class GeoDto
{
    public string lat { get; set; } = string.Empty;
    public string lng { get; set; } = string.Empty;
}

public class UserDto
{
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public AddressDto address { get; set; } = new();
    public string phone { get; set; } = string.Empty;
    public string website { get; set; } = string.Empty;
    public CompanyDto company { get; set; } = new();
}

    // Creando el createDto 