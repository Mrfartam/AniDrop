namespace AniDrop.Models;

public class RollHistoryDTO
{
    public Guid Id { get; set; }
    public string TitleRu { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? TierName { get; set; }
    public DateTime RolledAt { get; set; }
}
