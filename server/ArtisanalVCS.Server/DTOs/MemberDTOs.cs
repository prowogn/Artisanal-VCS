using System.ComponentModel.DataAnnotations;

namespace ArtisanalVCS.Server.DTOs;

public class MemberResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

public class AddMemberRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
}
