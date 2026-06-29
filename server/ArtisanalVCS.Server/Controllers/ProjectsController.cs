using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtisanalVCS.Server.Data;
using ArtisanalVCS.Server.DTOs;
using ArtisanalVCS.Server.Models;

namespace ArtisanalVCS.Server.Controllers;

public class ProjectsController : BaseApiController
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? q)
    {
        var query = _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Where(p => p.IsPublic)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p =>
                p.Name.Contains(q) || (p.Description != null && p.Description.Contains(q)));

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        if (!project.IsPublic)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || int.Parse(userIdClaim) != project.OwnerId)
                return NotFound();
        }

        return Ok(ToResponse(project));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMy()
    {
        var userId = GetUserId();

        var projects = await _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("member")]
    [Authorize]
    public async Task<IActionResult> GetMember()
    {
        var userId = GetUserId();

        var projectIds = await _db.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var projects = await _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Where(p => projectIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateProjectRequest request)
    {
        var userId = GetUserId();

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            IsPublic = request.IsPublic,
            OwnerId = userId,
        };

        project.Branches.Add(new Branch { Name = "main" });
        project.Members.Add(new ProjectMember { UserId = userId });

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        await _db.Entry(project).Reference(p => p.Owner).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, ToResponse(project));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        var userId = GetUserId();
        if (project.OwnerId != userId)
            return Forbid();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ProjectResponse ToResponse(Project p)
    {
        return new ProjectResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsPublic = p.IsPublic,
            CreatedAt = p.CreatedAt,
            OwnerName = p.Owner.Username,
            MemberCount = p.Members?.Count ?? 0,
        };
    }
}
