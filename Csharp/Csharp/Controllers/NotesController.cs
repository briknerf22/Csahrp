using Csharp.Data;
using Csharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class NotesController : Controller
{
    private readonly AppDbContext _db;

    public NotesController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue("UserId");
        if (int.TryParse(userIdClaim, out var id))
            return id;

        throw new Exception("UserId claim missing or invalid");
    }

    public async Task<IActionResult> Index(bool onlyImportant = false)
    {
        var userId = GetUserId();
        var notesQuery = _db.Notes
            .Where(n => n.UserId == userId);

        if (onlyImportant)
            notesQuery = notesQuery.Where(n => n.IsImportant);

        var notes = await notesQuery.OrderByDescending(n => n.CreatedAt).ToListAsync();
        return View(notes);
    }

    [HttpPost]
    public async Task<IActionResult> Add(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            // Můžeš přidat chybu do ModelState pokud chceš zobrazit validaci
            return RedirectToAction("Index");
        }

        var note = new Note
        {
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsImportant = false,
            UserId = GetUserId()
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (note != null)
        {
            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleImportant(int id)
    {
        var userId = GetUserId();
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (note != null)
        {
            note.IsImportant = !note.IsImportant;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }
}
