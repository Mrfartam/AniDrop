using System.Text.Json.Serialization;

namespace AniDrop.Models;

public class GraphQLResponse
{
    [JsonPropertyName("data")]
    public GraphQLData? Data { get; set; }
}

public class GraphQLData
{
    [JsonPropertyName("userRates")]
    public List<GraphQLUserRate>? UserRates { get; set; }

    [JsonPropertyName("animes")]
    public List<GraphQLAnime>? Animes { get; set; }
}

public class GraphQLUserRate
{
    [JsonPropertyName("anime")]
    public GraphQLAnime? Anime { get; set; }
}

public class GraphQLAnime
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("russian")]
    public string RussianName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("episodes")]
    public int Episodes { get; set; }

    [JsonPropertyName("airedOn")]
    public GraphQLAiredOn? AiredOn { get; set; }

    [JsonPropertyName("poster")]
    public GraphQLPoster? Poster { get; set; }
}

public class GraphQLAiredOn
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

public class GraphQLPoster
{
    [JsonPropertyName("originalUrl")]
    public string? OriginalUrl { get; set; }
}