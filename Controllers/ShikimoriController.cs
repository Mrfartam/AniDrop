using AniDrop.Domain;
using AniDrop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AniDrop.Controllers;

[ApiController]
[Route("api/shikimori")]
public class ShikimoriController: Controller
{
    private readonly IShikimoriService _shikimoriService;
    public ShikimoriController(IShikimoriService shikimoriService)
    {
        _shikimoriService = shikimoriService;
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
    [HttpGet("login-url")]
    [Authorize]
    public IActionResult GetLoginUrl()
    {
        var url = _shikimoriService.GetAuthorizationUrl();
        return Ok(new { Url = url });
    }
    [HttpGet("callback")]
    [Authorize]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Код авторизации не передан");
        try
        {
            var userID = GetCurrentUserId();
            var profile = await _shikimoriService.LinkAccountAsync(userID, code);

            //return Ok(new { Message = "Аккаунт Шикимори успешно привязан!", ShikimoriId = profile.ShikimoriId });
            return Redirect("https://localhost:44394/index.html?auth=success");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("planned")]
    public async Task<ActionResult<List<AnimeTitle>>> GetPlannedTitles(
        [FromQuery] string criteria = "oldest",
        [FromQuery] int count = 10)
    {
        try
        {
            var userId = GetCurrentUserId();

            var titles = await _shikimoriService.GetPlannedTitlesAsync(userId, criteria, count);
            return Ok(titles);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("season")]
    public async Task<ActionResult<List<AnimeTitle>>> GetTitlesBySeason(
        [FromQuery] string season,
        [FromQuery] int year)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(season))
                return BadRequest(new { message = "Параметр season не может быть пустым" });

            var titles = await _shikimoriService.GetTitlesBySeasonAsync(season, year);
            return Ok(titles);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
