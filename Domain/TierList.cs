using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AniDrop.Domain;

public class TierList
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [ForeignKey("TitlePool")]
    public Guid PoolId {  get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public decimal DropChance { get; set; } = 0;
    public virtual TitlePool Pool { get; set; } = null!;
}
