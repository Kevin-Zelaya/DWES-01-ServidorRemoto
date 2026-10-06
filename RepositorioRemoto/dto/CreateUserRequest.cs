
using System.ComponentModel.DataAnnotations;


public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string name { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string email { get; set; } = string.Empty;

    [Required]
    public AddressDto address { get; set; } = new();

    [Phone]
    [StringLength(100)]
    public string? phone { get; set; }

    [Url]
    [StringLength(100)]
    public string? website { get; set; }

    [Required]
    public CompanyDto company { get; set; } = new();
}