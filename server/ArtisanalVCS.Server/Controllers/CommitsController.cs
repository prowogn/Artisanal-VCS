using System.Security.Claims;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtisanalVCS.Server.Data;
using ArtisanalVCS.Server.DTOs;
using ArtisanalVCS.Server.Models;
 
namespace ArtisanalVCS.Server.Controllers;

[Route("api/projects/{projectId}/commits")]
public class CommitsController : BaseApiController
{
    private readonly AppDbContext _db;

    public CommitsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> Create(int projectId, CreateCommitRequest request)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId)
            return NotFound();

        var branch = await _db.Branches
            .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.Name == request.BranchName);

        if (branch == null)
            return NotFound("Branch not found");

        var commit = new Commit
        {
            Message = request.Message,
            ProjectId = projectId,
            BranchId = branch.Id,
            AuthorId = userId,
        };

        foreach (var file in request.Files)
        {
            commit.Snapshots.Add(new Snapshot
            {
                FilePath = file.FilePath,
                Content = file.Content,
            });
        }

        _db.Commits.Add(commit);
        await _db.SaveChangesAsync();

        await _db.Entry(commit).Reference(c => c.Author).LoadAsync();
        await _db.Entry(commit).Reference(c => c.Branch).LoadAsync();
        await _db.Entry(commit).Collection(c => c.Snapshots).LoadAsync();
        return Ok(ToResponse(commit));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId, [FromQuery] string? branch)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        if (!project.IsPublic)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || int.Parse(userIdClaim) != project.OwnerId)
                return NotFound();
        }

        var query = _db.Commits
            .Include(c => c.Author)
            .Include(c => c.Branch)
            .Where(c => c.ProjectId == projectId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branch))
            query = query.Where(c => c.Branch.Name == branch);

        var commits = await query
            .OrderByDescending(c => c.Timestamp)
            .Select(c => new CommitResponse
            {
                Id = c.Id,
                Message = c.Message,
                Timestamp = c.Timestamp,
                AuthorName = c.Author.Username,
                BranchName = c.Branch.Name,
            })
            .ToListAsync();

        return Ok(commits);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int projectId, int id)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        if (!project.IsPublic)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || int.Parse(userIdClaim) != project.OwnerId)
                return NotFound();
        }

        var commit = await _db.Commits
            .Include(c => c.Author)
            .Include(c => c.Branch)
            .Include(c => c.Snapshots)
            .FirstOrDefaultAsync(c => c.Id == id && c.ProjectId == projectId);

        if (commit == null)
            return NotFound();

        return Ok(ToResponse(commit));
    }

    [HttpPost("{id}/checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(int projectId, int id)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId) return Forbid();

        var source = await _db.Commits
            .Include(c => c.Snapshots)
            .FirstOrDefaultAsync(c => c.Id == id && c.ProjectId == projectId);
        if (source == null) return NotFound();

        var commit = new Commit
        {
            Message = $"rollback to commit #{source.Id}: {source.Message}",
            ProjectId = projectId,
            BranchId = source.BranchId,
            AuthorId = userId,
        };

        foreach (var snap in source.Snapshots)
        {
            commit.Snapshots.Add(new Snapshot
            {
                FilePath = snap.FilePath,
                Content = snap.Content,
            });
        }

        _db.Commits.Add(commit);
        await _db.SaveChangesAsync();

        await _db.Entry(commit).Reference(c => c.Author).LoadAsync();
        await _db.Entry(commit).Reference(c => c.Branch).LoadAsync();
        await _db.Entry(commit).Collection(c => c.Snapshots).LoadAsync();
        return Ok(ToResponse(commit));
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int projectId, int id)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        if (!project.IsPublic)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || int.Parse(userIdClaim) != project.OwnerId)
                return NotFound();
        }

        var commit = await _db.Commits
            .Include(c => c.Snapshots)
            .FirstOrDefaultAsync(c => c.Id == id && c.ProjectId == projectId);
        if (commit == null) return NotFound();

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var snap in commit.Snapshots.OrderBy(s => s.FilePath))
            {
                var entry = archive.CreateEntry(snap.FilePath, CompressionLevel.Fastest);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                writer.Write(snap.Content);
            }
        }

        ms.Seek(0, SeekOrigin.Begin);
        return File(ms.ToArray(), "application/zip", $"commit-{id}-{commit.Message[..Math.Min(commit.Message.Length, 40)].Replace(" ", "_")}.zip");
    }

    private static CommitResponse ToResponse(Commit c)
    {
        return new CommitResponse
        {
            Id = c.Id,
            Message = c.Message,
            Timestamp = c.Timestamp,
            AuthorName = c.Author.Username,
            BranchName = c.Branch.Name,
            Files = c.Snapshots
                .OrderBy(s => s.FilePath)
                .Select(s => new FileSnapshotDto
                {
                    FilePath = s.FilePath,
                    Content = s.Content,
                })
                .ToList(),
        };
    }
}
