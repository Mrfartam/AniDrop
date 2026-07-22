using AniDrop.Models;
using AniDrop.Services;
using AniDrop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;

namespace AniDrop.Controllers;

[ApiController]
[Route("api/pools")]
[Authorize]
public class TitlePoolController : Controller
{
    private readonly ITitlePoolService _poolService;
    public TitlePoolController(ITitlePoolService poolService)
    {
        _poolService = poolService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        if (!int.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("Некорректный ID пользователя в токене");

        return userId;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePool([FromBody] CreatePoolDTO dto)
    {
        var userId = GetCurrentUserId();
        var pool = await _poolService.CreatePoolAsync(userId, dto);
        return CreatedAtAction(nameof(GetPoolById), new { poolId = pool.Id }, pool);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPools()
    {
        var userId = GetCurrentUserId();
        var pools = await _poolService.GetUserPoolsAsync(userId);
        return Ok(pools);
    }

    [HttpGet("{poolId:guid}")]
    public async Task<IActionResult> GetPoolById(Guid poolId)
    {
        var userId = GetCurrentUserId();
        var pool = await _poolService.GetPoolByIdAsync(poolId, userId);
        if (pool == null)
            return NotFound("Пул не найден или у вас нет к нему доступа");

        return Ok(pool);
    }

    [HttpDelete("{poolId:guid}")]
    public async Task<IActionResult> DeletePool(Guid poolId)
    {
        var userId = GetCurrentUserId();
        var success = await _poolService.DeletePoolAsync(poolId, userId);

        if (!success)
            return NotFound("Пул не найден или не может быть удалён");

        return NoContent();
    }

    [HttpPost("{poolId:guid}/activate")]
    public async Task<IActionResult> SetPoolActive(Guid poolId)
    {
        var userId = GetCurrentUserId();
        var success = await _poolService.SetPoolActiveAsync(poolId, userId);

        if (!success)
            return NotFound("Пул не найден");

        return Ok("Пул успешно активирован");
    }

    [HttpPost("{poolId:guid}/tiers")]
    public async Task<IActionResult> AddTier(Guid poolId, [FromBody] CreateTierDTO dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var tier = await _poolService.AddTierToPoolAsync(poolId, userId, dto);
            return Ok(tier);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("tiers/{tierId:guid}")]
    public async Task<IActionResult> DeleteTier(Guid tierId)
    {
        var userId = GetCurrentUserId();
        var success = await _poolService.DeleteTierAsync(tierId, userId);

        if (!success)
            return NotFound("Тир не найден или доступ запрещён");

        return NoContent();
    }

    [HttpPost("{poolId:guid}/titles")]
    public async Task<IActionResult> AddTitlesToPool(Guid poolId, [FromBody] List<int> animeIds)
    {
        var userId = GetCurrentUserId();
        try
        {
            await _poolService.AddTitlesToPoolAsync(poolId, userId, animeIds);
            return Ok("Тайтлы успешно добавлены в пул");
        }
        catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("items/{poolItemId:guid}/assign")]
    public async Task<IActionResult> AssignTitleToTier(Guid poolItemId, [FromQuery] Guid? tierId)
    {
        var userId = GetCurrentUserId();
        try
        {
            await _poolService.AssignTitleToTierAsync(poolItemId, tierId, userId);
            return Ok("Тайтл успешно добавлен в тир");
        }
        catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("items/{poolItemId:guid}/exclude")]
    public async Task<IActionResult> ExcludeTitle(Guid poolItemId, [FromQuery] bool exclude)
    {
        var userId = GetCurrentUserId();
        try
        {
            await _poolService.ExcludeTitleFromPoolAsync(poolItemId, exclude, userId);
            return Ok(exclude ? "Тайтл исключён из ролла" : "Тайтл возвращён в пул для ролла");
        }
        catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{poolId:guid}/roll-tier")]
    public async Task<IActionResult> RollTier(Guid poolId)
    {
        var userId = GetCurrentUserId();
        try
        {
            var tier = await _poolService.RollTierAsync(poolId, userId);
            return Ok(new
            {
                Message = "Определён тир для следующего ролла",
                TierId = tier.Id,
                TierName = tier.Name,
                DropChance = tier.DropChance
            });
        }
        catch(KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{poolId:guid}/roll-title/{tierId:guid}")]
    public async Task<IActionResult> RollTitleFromTier(Guid poolId, Guid tierId)
    {
        var userId = GetCurrentUserId();
        try
        {
            var rolledItem = await _poolService.RollTitleFromTierAsync(poolId, tierId, userId);
            return Ok(new
            {
                Message = "Был выбит тайтл",
                TitleRu = rolledItem.AnimeTitle.TitleRu,
                TitleEn = rolledItem.AnimeTitle.TitleEn,
                ImageUrl = rolledItem.AnimeTitle.ImageUrl,
                Url = rolledItem.AnimeTitle.Url,
                PoolItemId = rolledItem.Id
            });
        }
        catch(KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{poolId}/history")]
    public async Task<IActionResult> GetRollHistory(Guid poolId)
    {
        var userId = GetCurrentUserId();
        var history = await _poolService.GetRollHistoryAsync(poolId, userId);
        return Ok(history);
    }
}
