using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtisanalVCS.Server.Data;
using ArtisanalVCS.Server.DTOs;
using ArtisanalVCS.Server.Models;

namespace ArtisanalVCS.Server.Controllers;

[Authorize]
[Route("api/projects/{projectId}/members")]
public class ProjectMembersController : BaseApiController
{
    private readonly AppDbContext _db;

    public ProjectMembersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId)
    {
        var userId = GetUserId();
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();
        if (project.OwnerId != userId) return Forbid();

        var members = await _db.ProjectMembers
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .OrderBy(pm => pm.AddedAt)
            .Select(pm => new MemberResponse
            {
                Id = pm.Id,
                Username = pm.User.Username,
                AddedAt = pm.AddedAt,
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int projectId, AddMemberRequest request)
    {
        var userId = GetUserId();
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();
        if (project.OwnerId != userId) return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null) return NotFound(new { error = "user not found" });
        if (user.Id == project.OwnerId) return BadRequest(new { error = "owner is already a member" });

        var exists = await _db.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id);
        if (exists) return BadRequest(new { error = "already a member" });

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = user.Id,
        };
        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync();

        return Ok(new MemberResponse
        {
            Id = member.Id,
            Username = user.Username,
            AddedAt = member.AddedAt,
        });
    }

    [HttpDelete("{memberId}")]
    public async Task<IActionResult> Remove(int projectId, int memberId)
    {
        var userId = GetUserId();
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();
        if (project.OwnerId != userId) return Forbid();

        var member = await _db.ProjectMembers.FindAsync(memberId);
        if (member == null || member.ProjectId != projectId) return NotFound();

        if (member.UserId == userId) return BadRequest(new { error = "cannot remove owner" });

        _db.ProjectMembers.Remove(member);
        await _db.SaveChangesAsync();

        return NoContent();
    }

}
