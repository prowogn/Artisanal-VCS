using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.Models;

public class Snapshot
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public int CommitId { get; set; }
    public Commit Commit { get; set; } = null!;
}
