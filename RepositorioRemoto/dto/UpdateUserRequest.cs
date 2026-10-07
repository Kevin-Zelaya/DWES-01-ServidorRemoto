public class UpdateUserRequest
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