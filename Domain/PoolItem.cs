using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AniDrop.Domain;

public class PoolItem
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [ForeignKey("TitlePool")]
    public Guid PoolId { get; set; }
    [Required]
    [ForeignKey("AnimeTitle")]
    public int AnimeTitleId {  get; set; }
    [AllowNull]
    [ForeignKey("TierList")]
    public Guid? TierListId { get; set; }
    public bool IsExcluded {  get; set; } = false;
    public virtual TitlePool Pool { get; set; } = null!;
    public virtual AnimeTitle AnimeTitle { get; set; } = null!;
    public virtual TierList? Tier { get; set; }
}
