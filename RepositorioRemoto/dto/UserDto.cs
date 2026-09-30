public class AddressDto
{
    public string street { get; set; }
    public string suite { get; set; }
    public string city { get; set; }
    public string zipcode { get; set; }
    public GeoDto geo { get; set; }
}

public class CompanyDto
{
    public string name { get; set; }
    public string catchPhrase { get; set; }
    public string bs { get; set; }
}

public class GeoDto
{
    public string lat { get; set; }
    public string lng { get; set; }
}

    public class UserDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public Address address { get; set; }
        public string phone { get; set; }
        public string website { get; set; }
        public Company company { get; set; }
    }

    // Creando el createDto 