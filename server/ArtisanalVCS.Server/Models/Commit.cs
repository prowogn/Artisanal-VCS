using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.Models;

public class Commit
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();
}
