using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AniDrop.Domain;

public class RollHistory
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [ForeignKey("User")]
    public int UserId {  get; set; }
    [Required]
    [ForeignKey("PoolItem")]
    public Guid PoolItem {  get; set; }
    public DateTime RolledAt {  get; set; } = DateTime.UtcNow;
}
