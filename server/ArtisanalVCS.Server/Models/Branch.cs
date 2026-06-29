using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.Models;

public class Branch
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Commit> Commits { get; set; } = new List<Commit>();
}
