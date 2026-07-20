using AniDrop.Models;
using AniDrop.Services;
using AniDrop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AniDrop.DBInfrastructure;
using AniDrop.Domain;
using Microsoft.EntityFrameworkCore;

namespace AniDrop.Controllers;

[ApiController]
[Route("api/titles")]
public class AnimeTitleControllers : Controller
{
    private readonly AniDropDBContext _context;
    public AnimeTitleControllers(AniDropDBContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddAnimeTitle([FromBody] AnimeTitle title)
    {
        var exists = await _context.AnimeTitles.AnyAsync(t => t.Id == title.Id);
        if (exists)
        {
            return BadRequest($"Тайтл с ID {title.Id} уже существует в базе");
        }

        _context.AnimeTitles.Add(title);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAnimeTitleById), new { id = title.Id }, title);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAnimeTitleById(int id)
    {
        var title = await _context.AnimeTitles.FindAsync(id);

        if (title == null)
            return NotFound();

        return Ok(title);
    }
}
