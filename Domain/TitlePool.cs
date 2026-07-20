using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AniDrop.Domain;

public class TitlePool
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public virtual List<TierList> TierLists { get; set; } = new();
    public virtual List<PoolItem> PoolItems { get; set; } = new();
}
