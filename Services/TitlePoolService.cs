using AniDrop.DBInfrastructure;
using AniDrop.Domain;
using AniDrop.Models;
using AniDrop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Tracing;
namespace AniDrop.Services;

public class TitlePoolService : ITitlePoolService
{
    private readonly AniDropDBContext _context;
    public TitlePoolService(AniDropDBContext context)
    {
        _context = context;
    }

    public async Task<TitlePool> CreatePoolAsync(int userId, CreatePoolDTO dto)
    {
        var hasActivePool = await _context.TitlePools.AnyAsync(p => p.UserId == userId && p.IsActive);

        var pool = new TitlePool
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            IsActive = !hasActivePool
        };

        _context.TitlePools.Add(pool);
        await _context.SaveChangesAsync();
        return pool;
    }
    public async Task<List<TitlePool>> GetUserPoolsAsync(int userId)
    {
        return await _context.TitlePools
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    public async Task<TitlePool?> GetPoolByIdAsync(Guid poolId, int userId)
    {
        return await _context.TitlePools
            .Include(p => p.TierLists)
            .Include(p => p.PoolItems)
                .ThenInclude(pi => pi.AnimeTitle)
            .FirstOrDefaultAsync(p => p.Id == poolId && p.UserId == userId);
    }
    public async Task<bool> DeletePoolAsync(Guid poolId, int userId)
    {
        var pool = await _context.TitlePools.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == poolId);
        if (pool == null) return false;

        _context.TitlePools.Remove(pool);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> SetPoolActiveAsync(Guid poolId, int userId)
    {
        var pool = await _context.TitlePools.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == poolId);
        if (pool == null) return false;

        var activePool = await _context.TitlePools.Where(p => p.UserId == userId && p.IsActive).ToListAsync();
        foreach (var p in activePool)
            p.IsActive = false;

        pool.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TierList> AddTierToPoolAsync(Guid poolId, int userId, CreateTierDTO dto)
    {
        var poolExists = await _context.TitlePools.AnyAsync(p => p.Id == poolId && p.UserId == userId);
        if (!poolExists) throw new UnauthorizedAccessException("У вас нет доступа к этому пулу");

        var tier = new TierList
        {
            Id = Guid.NewGuid(),
            PoolId = poolId,
            Name = dto.Name,
            DropChance = dto.DropChance
        };

        _context.TierLists.Add(tier);
        await _context.SaveChangesAsync();
        return tier;
    }
    public async Task<bool> DeleteTierAsync(Guid tierId, int userId)
    {
        var tier = await _context.TierLists
            .Include(t => t.Pool)
            .FirstOrDefaultAsync(t => t.Id == tierId);

        if (tier == null || tier.Pool.UserId != userId) return false;

        var itemsInTier = await _context.PoolItems.Where(pi => pi.TierListId == tierId).ToListAsync();
        foreach(var item in itemsInTier)
        {
            item.TierListId = null;
        }

        _context.TierLists.Remove(tier);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AddTitlesToPoolAsync(Guid poolId, int userId, List<int> animeIds)
    {
        var pool = await _context.TitlePools.FirstOrDefaultAsync(p => p.Id == poolId && p.UserId == userId);
        if (pool == null) throw new UnauthorizedAccessException("Пул не найден или доступ запрещён");

        foreach(var animeId in animeIds)
        {
            var exist = await _context.PoolItems.AnyAsync(pi => pi.PoolId == poolId && pi.AnimeTitleId == animeId);
            if (!exist)
            {
                var poolItem = new PoolItem
                {
                    Id = Guid.NewGuid(),
                    PoolId = poolId,
                    AnimeTitleId = animeId,
                    IsExcluded = false,
                    IsRolled = false
                };
                _context.PoolItems.Add(poolItem);
            }
        }

        await _context.SaveChangesAsync();
    }
    public async Task AssignTitleToTierAsync(Guid poolItemId, Guid? tierId, int userId)
    {
        var item = await _context.PoolItems.Include(pi => pi.Pool).FirstOrDefaultAsync(pi => pi.Id == poolItemId);

        if (item == null || item.Pool.UserId != userId) throw new UnauthorizedAccessException("Элемент пула не найден или доступ запрещён");

        if (tierId.HasValue)
        {
            var tierExists = await _context.TierLists.AnyAsync(t => t.Id == tierId.Value && t.PoolId == item.PoolId);
            if (!tierExists) throw new InvalidOperationException("Выбранный тир не принадлежит этому пулу");
        }

        item.TierListId = tierId;
        await _context.SaveChangesAsync();
    }
    public async Task ExcludeTitleFromPoolAsync(Guid poolItemId, bool exclude, int userId)
    {
        var item = await _context.PoolItems.Include(pi => pi.Pool).FirstOrDefaultAsync(pi => pi.Id == poolItemId);

        if (item == null || item.Pool.UserId != userId)
            throw new UnauthorizedAccessException("Элемент пула не найден или доступ запрещён");

        item.IsExcluded = exclude;

        if (exclude)
            item.TierListId = null;
        await _context.SaveChangesAsync();
    }
    public async Task<TierList> RollTierAsync(Guid poolId, int userId)
    {
        var pool = await _context.TitlePools
            .Include(p => p.TierLists)
            .Include(p => p.PoolItems)
            .FirstOrDefaultAsync(p => p.Id == poolId && p.UserId == userId);

        if (pool == null)
            throw new KeyNotFoundException("Пул не найден или у вас нет к нему доступа");

        var availableItems = pool.PoolItems.Where(pi => !pi.IsRolled && !pi.IsExcluded).ToList();

        if (!availableItems.Any())
            throw new InvalidOperationException("В пуле нет ни одного тайтла");

        var activeTierIds = availableItems
            .Where(pi => pi.TierListId.HasValue)
            .Select(pi => pi.TierListId!.Value)
            .Distinct()
            .ToHashSet();

        var activeTiers = pool.TierLists
            .Where(t => activeTierIds.Contains(t.Id))
            .ToList();

        if (!activeTiers.Any())
            throw new InvalidOperationException("Нет активных тиров");

        double totalWeight = activeTiers.Sum(t => (double)t.DropChance);
        double randomWeightPoint = Random.Shared.NextDouble() * totalWeight;
        double currentWeightSum = 0;
        TierList selectedTier = null;

        foreach(var tier in activeTiers)
        {
            currentWeightSum += (double)tier.DropChance;
            if (randomWeightPoint <= currentWeightSum)
            {
                selectedTier = tier;
                break;
            }
        }

        if (selectedTier == null)
            selectedTier = activeTiers.Last();

        return selectedTier;
    }

    public async Task<PoolItem> RollTitleFromTierAsync(Guid poolId, Guid tierId, int userId)
    {
        var pool = await _context.TitlePools
            .Include(p => p.PoolItems)
                .ThenInclude(pi => pi.AnimeTitle)
            .FirstOrDefaultAsync(p => p.Id == poolId && p.UserId == userId);

        if (pool == null)
            throw new KeyNotFoundException("Пул не найден или у вас нет к нему доступа");

        var tierItems = pool.PoolItems
            .Where(pi => pi.TierListId == tierId && !pi.IsExcluded && !pi.IsRolled)
            .ToList();

        if (!tierItems.Any())
            throw new InvalidOperationException("В выбранном тире не осталось тайтлов");

        int randomIndex = Random.Shared.Next(tierItems.Count);
        var rolledItem = tierItems[randomIndex];

        rolledItem.IsRolled = true;

        var rollHistoryEntry = new RollHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PoolItem = rolledItem.Id,
            RolledAt = DateTime.UtcNow
        };

        _context.RollHistory.Add(rollHistoryEntry);
        await _context.SaveChangesAsync();

        return rolledItem;
    }
}
