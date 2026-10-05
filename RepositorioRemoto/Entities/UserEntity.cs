using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class UserEntity
{
    /// <summary>Para el mapper</summary>
    [Key]
    public int id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("username")]
    public string username { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("email")]
    public string? email { get; set; }

    /// <summary>Dirección serializada como JSON</summary>
    [Column("address")]
    public string? address { get; set; }

    [MaxLength(100)]
    [Column("phone")]
    public string? phone { get; set; }

    [MaxLength(100)]
    [Column("website")]
    public string? website { get; set; }

    /// <summary>Empresa serializada como JSON</summary>
    [Column("company")]
    public string? company { get; set; }
}