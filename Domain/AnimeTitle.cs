using System.ComponentModel.DataAnnotations;

namespace AniDrop.Domain;

public class AnimeTitle
{
    [Required]
    public int Id { get; set; }
    public string TitleRu {  get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    [Required]
    public string Url { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int EpisodesNum { get; set; } = -1;
    public string Season { get; set; } = string.Empty;
}
