using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtisanalVCS.Server.Data;
using ArtisanalVCS.Server.DTOs;
using ArtisanalVCS.Server.Models;

namespace ArtisanalVCS.Server.Controllers;

[Route("api/projects/{projectId}/branches")]
[Authorize]
public class BranchesController : BaseApiController
{
    private readonly AppDbContext _db;

    public BranchesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId)
            return NotFound();

        var branches = await _db.Branches
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.Name)
            .Select(b => ToResponse(b))
            .ToListAsync();

        return Ok(branches);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int projectId, CreateBranchRequest request)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId)
            return NotFound();

        var exists = await _db.Branches.AnyAsync(b =>
            b.ProjectId == projectId && b.Name == request.Name);

        if (exists)
            return Conflict("Branch already exists");

        var branch = new Branch
        {
            Name = request.Name,
            ProjectId = projectId,
        };

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { projectId }, ToResponse(branch));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int projectId, [FromQuery] string name)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId)
            return NotFound();

        var branch = await _db.Branches
            .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.Name == name);

        if (branch == null)
            return NotFound();

        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("merge")]
    public async Task<IActionResult> Merge(int projectId, [FromQuery] string source, [FromQuery] string target)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId) return NotFound();

        var src = await _db.Branches
            .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.Name == source);
        var dst = await _db.Branches
            .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.Name == target);

        if (src == null || dst == null) return NotFound();

        var commits = await _db.Commits
            .Include(c => c.Snapshots)
            .Where(c => c.BranchId == src.Id)
            .ToListAsync();

        foreach (var c in commits)
        {
            var copy = new Commit
            {
                Message = c.Message,
                Timestamp = c.Timestamp,
                AuthorId = c.AuthorId,
                BranchId = dst.Id,
                ProjectId = projectId,
            };

            foreach (var s in c.Snapshots)
                copy.Snapshots.Add(new Snapshot { FilePath = s.FilePath, Content = s.Content });

            _db.Commits.Add(copy);
        }

        await _db.SaveChangesAsync();
        return Ok(new { copied = commits.Count });
    }

    private static BranchResponse ToResponse(Branch b)
    {
        return new BranchResponse
        {
            Id = b.Id,
            Name = b.Name,
            CreatedAt = b.CreatedAt,
        };
    }
}
