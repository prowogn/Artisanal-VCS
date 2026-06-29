using System.Text.Json.Serialization;

namespace ArtisanalVCS.Client.Models;

public class ProjectResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; } = "";

    [JsonPropertyName("memberCount")]
    public int MemberCount { get; set; }

    public bool IsOwner { get; set; }
}

public class CreateProjectRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }
}

public class AddMemberRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";
}

public class MemberResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("addedAt")]
    public DateTime AddedAt { get; set; }
}
