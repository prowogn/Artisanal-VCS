using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.Models;

public class Project
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Commit> Commits { get; set; } = new List<Commit>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}
