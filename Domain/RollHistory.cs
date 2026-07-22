using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AniDrop.Domain;

public class RollHistory
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public int UserId {  get; set; }
    [Required]
    public Guid PoolItem {  get; set; }
    public DateTime RolledAt {  get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    [ForeignKey(nameof(PoolItem))]
    public virtual PoolItem PoolItemNavigation { get; set; } = null!;
}
