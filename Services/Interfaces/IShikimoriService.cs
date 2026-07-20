using AniDrop.Domain;

namespace AniDrop.Services.Interfaces;
public interface IShikimoriService
{
    string GetAuthorizationUrl();
    Task<ShikimoriProfile> LinkAccountAsync(int userId, string code);
    Task<string> GetValidAccessTokenAsync(int userId);
    Task<List<AnimeTitle>> GetPlannedTitlesAsync(int userId, string criteria, int count);
    Task<List<AnimeTitle>> GetTitlesBySeasonAsync(string seasonName, int year);
}
