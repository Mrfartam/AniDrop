using AniDrop.DBInfrastructure;
using AniDrop.Domain;
using AniDrop.Models;
using AniDrop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml;

namespace AniDrop.Services;

public class ShikimoriService : IShikimoriService
{
    private readonly AniDropDBContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    public ShikimoriService(AniDropDBContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        var settings = _config.GetSection("ShikimoriSettings");

        client.DefaultRequestHeaders.UserAgent.ParseAdd(settings["UserAgent"] ?? "AniDrop");
        return client;
    }
    public string GetAuthorizationUrl()
    {
        var settings = _config.GetSection("ShikimoriSettings");
        return $"https://shikimori.io/oauth/authorize?client_id={settings["ClientId"]}&redirect_uri={Uri.EscapeDataString(settings["RedirectUri"]!)}&response_type=code&scope=user_rates";
    }
    public async Task<ShikimoriProfile> LinkAccountAsync(int userId, string code)
    {
        var client = CreateClient();
        var settings = _config.GetSection("ShikimoriSettings");

        var tokenRequestParams = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", settings["ClientId"]! },
            { "client_secret", settings["ClientSecret"]! },
            { "code", code },
            { "redirect_uri", settings["RedirectUri"]! }
        };

        var response = await client.PostAsync("https://shikimori.io/oauth/token", new FormUrlEncodedContent(tokenRequestParams));

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Не удалось получить токены от Шикимори: {await response.Content.ReadAsStringAsync()}");
        
        var tokenData = await response.Content.ReadFromJsonAsync<ShikimoriTokenResponseDTO>();
        if (tokenData == null)
            throw new Exception("Ошибка десериалзизации токенов Шикимори");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
        var userResponse = await client.GetAsync("https://shikimori.io/api/users/whoami");
        if (!userResponse.IsSuccessStatusCode)
            throw new HttpRequestException("Не удалось получить данные пользователя Шикимори");

        var userData = await userResponse.Content.ReadFromJsonAsync<ShikimoriUserResponseDTO>();
        if (userData == null)
            throw new Exception("Ошибка десериализации данных пользователя");

        var profile = await _context.ShikimoriProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var expireaAt = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        if(profile == null)
        {
            profile = new ShikimoriProfile
            {
                UserId = userId,
                ShikimoriId = userData.Id,
                AccessToken = tokenData.AccessToken,
                RefreshToken = tokenData.RefreshToken,
                ExpiresAt = expireaAt
            };
            _context.ShikimoriProfiles.Add(profile);
        }
        else
        {
            profile.ShikimoriId = userData.Id;
            profile.AccessToken = tokenData.AccessToken;
            profile.RefreshToken = tokenData.RefreshToken;
            profile.ExpiresAt = expireaAt;
        }

        await _context.SaveChangesAsync();
        return profile;
    }
    public async Task<string> GetValidAccessTokenAsync(int userId)
    {
        var profile = await _context.ShikimoriProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
            throw new KeyNotFoundException("Профиль Шикимори не привязан к аккаунту");

        if (profile.ExpiresAt > DateTime.UtcNow.AddMinutes(1))
            return profile.AccessToken;

        var settings = _config.GetSection("ShikimoriSettings");
        var client = CreateClient();

        var refreshParams = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", settings["ClientId"]! },
            { "client_secret", settings["ClientSecret"]! },
            { "refresh_token", profile.RefreshToken }
        };

        var response = await client.PostAsync("https://shikimori.io/oauth/token", new FormUrlEncodedContent(refreshParams));
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("Сессия Шикимори истекла. Требуется новый вход");

        var tokenData = await response.Content.ReadFromJsonAsync<ShikimoriTokenResponseDTO>();
        if (tokenData == null)
            throw new Exception("Ошибка при обновлении токенов");

        profile.AccessToken = tokenData.AccessToken;
        profile.RefreshToken = tokenData.RefreshToken;
        profile.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        await _context.SaveChangesAsync();
        return profile.AccessToken;
    }
    public async Task<List<AnimeTitle>> GetPlannedTitlesAsync(int userId, string criteria, int count)
    {
        var profile = await _context.ShikimoriProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) throw new Exception("Профиль Шикимори не привязан");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AniDropApp");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", profile.AccessToken);

        int page = 1;
        bool hasMore = true;
        var allPlannedTitles = new List<AnimeTitle>();

        while (hasMore)
        {
            var graphQLQuery = new
            {
                query = @"
                query($userId: ID!, $page: Int!) {
                    userRates(userId: $userId, targetType: Anime, status: planned, page: $page, limit: 50) {
                    anime {
                        id
                        name
                        russian
                        status
                        episodes
                        airedOn { date }
                        poster { originalUrl }
                    }
                    }
                }",
                variables = new
                {
                    userId = profile.ShikimoriId.ToString(),
                    page = page
                }
            };

            var response = await client.PostAsJsonAsync("https://shikimori.io/api/graphql", graphQLQuery);
            response.EnsureSuccessStatusCode();

            var rawJson = await response.Content.ReadAsStringAsync();

            if (rawJson.Contains("\"errors\""))
            {
                throw new Exception($"Ошибка Шикимори GraphQL: {rawJson}");
            }

            var jsonResult = JsonSerializer.Deserialize<GraphQLResponse>(rawJson);
            var pageRates = jsonResult?.Data?.UserRates;

            if (pageRates != null && pageRates.Count > 0)
            {
                foreach (var rate in pageRates)
                {
                    if (rate.Anime == null || rate.Anime.Status == "anons") continue;

                    string seasonYear = "Unknown";
                    if (!string.IsNullOrEmpty(rate.Anime.AiredOn?.Date) && rate.Anime.AiredOn.Date.Length >= 4)
                    {
                        seasonYear = $"{rate.Anime.AiredOn.Date.Substring(0, 4)}";
                    }

                    allPlannedTitles.Add(new AnimeTitle
                    {
                        Id = int.Parse(rate.Anime.Id),
                        TitleRu = string.IsNullOrEmpty(rate.Anime.RussianName) ? rate.Anime.Name : rate.Anime.RussianName,
                        TitleEn = rate.Anime.Name,
                        Url = $"https://shikimori.io/animes/{rate.Anime.Id}",
                        ImageUrl = rate.Anime.Poster?.OriginalUrl ?? string.Empty,
                        EpisodesNum = rate.Anime.Episodes,
                        Season = seasonYear
                    });
                }

                if (pageRates.Count < 50)
                {
                    hasMore = false;
                }
                else
                {
                    page++;
                }
            }
            else
            {
                hasMore = false;
            }
        }

        await SyncTitlesWithDatabaseAsync(allPlannedTitles);

        if (criteria.ToLower() == "random")
        {
            return allPlannedTitles.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }
        else if (criteria.ToLower() == "oldest")
        {
            return allPlannedTitles.OrderBy(t => t.Id).Take(count).ToList();
        }

        return allPlannedTitles.Take(count).ToList();
    }
    public async Task<List<AnimeTitle>> GetTitlesBySeasonAsync(string seasonName, int year)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AniDrop");

        var seasonParameter = $"{seasonName.ToLower()}_{year}";
        var seasonTitles = new List<AnimeTitle>();
        int page = 1;
        bool hasMore = true;

        while (hasMore)
        {
            var graphQLQuery = new
            {
                query = @"
                query($season: SeasonString!, $page: Int!) {
                  animes(season: $season, page: $page, limit: 50, order: id) {
                    id
                    name
                    russian
                    status
                    episodes
                    airedOn { date }
                    poster { originalUrl }
                  }
                }",
                variables = new
                {
                    season = seasonParameter,
                    page = page
                }
            };

            var response = await client.PostAsJsonAsync("https://shikimori.io/api/graphql", graphQLQuery);
            response.EnsureSuccessStatusCode();

            var rawJson = await response.Content.ReadAsStringAsync();

            if (rawJson.Contains("\"errors\""))
            {
                throw new Exception($"Ошибка Шикимори GraphQL: {rawJson}");
            }

            var jsonResult = JsonSerializer.Deserialize<GraphQLResponse>(rawJson);
            var pageAnimes = jsonResult?.Data?.Animes;

            if (pageAnimes != null && pageAnimes.Count > 0)
            {
                foreach (var anime in pageAnimes)
                {
                    if (anime.Status == "anons") continue;

                    seasonTitles.Add(new AnimeTitle
                    {
                        Id = int.Parse(anime.Id),
                        TitleRu = string.IsNullOrEmpty(anime.RussianName) ? anime.Name : anime.RussianName,
                        TitleEn = anime.Name,
                        Url = $"https://shikimori.io/animes/{anime.Id}",
                        ImageUrl = anime.Poster?.OriginalUrl ?? string.Empty,
                        EpisodesNum = anime.Episodes,
                        Season = $"{seasonName} {year}"
                    });
                }

                if (pageAnimes.Count < 50)
                {
                    hasMore = false;
                }
                else
                {
                    page++;
                }
            }
            else
            {
                hasMore = false;
            }
        }

        await SyncTitlesWithDatabaseAsync(seasonTitles);

        return seasonTitles;
    }
    private async Task SyncTitlesWithDatabaseAsync(List<AnimeTitle> titles)
    {
        if (!titles.Any()) return;

        var titleIds = titles.Select(t => t.Id).ToList();
        var existingIds = await _context.AnimeTitles
            .Where(t => titleIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        var newTitles = titles.Where(t => !existingIds.Contains(t.Id)).ToList();
        if (newTitles.Any())
        {
            _context.AnimeTitles.AddRange(newTitles);
            await _context.SaveChangesAsync();
        }
    }
}
