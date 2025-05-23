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

    private int GetUserId() =>
        int.Parse(User.FindFirstValue("UserId") ?? "0");

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
    public async Task<IActionResult> Add(Note note)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Index");

        note.UserId = GetUserId();
        note.CreatedAt = DateTime.UtcNow;

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
