using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.DTOs;

public class FileSnapshotDto
{
    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

public class CreateCommitRequest
{
    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string BranchName { get; set; } = string.Empty;

    [Required]
    public List<FileSnapshotDto> Files { get; set; } = new();
}

public class CommitResponse
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public List<FileSnapshotDto> Files { get; set; } = new();
}
