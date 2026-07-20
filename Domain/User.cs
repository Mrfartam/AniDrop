using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AniDrop.Domain;

[Index(nameof(Username), IsUnique = true)]
public class User
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ShikimoriProfile? ShikimoriProfile { get; set; }
}
