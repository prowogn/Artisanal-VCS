using System.Text.Json.Serialization;

namespace ArtisanalVCS.Client.Models;

public class CommitResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("authorName")]
    public string AuthorName { get; set; } = "";

    [JsonPropertyName("branchName")]
    public string BranchName { get; set; } = "";

    [JsonPropertyName("files")]
    public List<FileSnapshotDto> Files { get; set; } = new();
}

public class FileSnapshotDto
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class CreateCommitRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("branchName")]
    public string BranchName { get; set; } = "";

    [JsonPropertyName("files")]
    public List<FileSnapshotDto> Files { get; set; } = new();
}

public class BranchResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class CreateBranchRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}
