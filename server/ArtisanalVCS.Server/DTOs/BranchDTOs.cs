using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.DTOs;

public class CreateBranchRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class BranchResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
