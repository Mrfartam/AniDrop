using AniDrop.Domain;
using AniDrop.Models;

namespace AniDrop.Services.Interfaces;

public interface ITitlePoolService
{
    Task<TitlePool> CreatePoolAsync(int userId, CreatePoolDTO dto);
    Task<List<TitlePool>> GetUserPoolsAsync(int userId);
    Task<TitlePool?> GetPoolByIdAsync(Guid poolId, int userId);
    Task<bool> DeletePoolAsync(Guid poolId, int userId);
    Task<bool> SetPoolActiveAsync(Guid poolId, int userId);

    Task<TierList> AddTierToPoolAsync(Guid poolId, int userId, CreateTierDTO dto);
    Task<bool> DeleteTierAsync(Guid tierId, int userId);

    Task AddTitlesToPoolAsync(Guid poolId, int userId, List<int> animeIds);
    Task AssignTitleToTierAsync(Guid poolItemId, Guid? tierId, int userId);
    Task ExcludeTitleFromPoolAsync(Guid poolItemId, bool exclude, int userId);

    Task<TierList> RollTierAsync(Guid poolId, int userId);
    Task<PoolItem> RollTitleFromTierAsync(Guid poolId, Guid tierId, int userId);
}
