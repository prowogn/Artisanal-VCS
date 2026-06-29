using System.Net.Http.Headers;

namespace ArtisanalVCS.Client.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private string? _token;

    public string? Username { get; set; }

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:5273") };
        _http.Timeout = TimeSpan.FromSeconds(300);
        _http.MaxResponseContentBufferSize = 200 * 1024 * 1024;
    }

    public string? Token
    {
        get => _token;
        set
        {
            _token = value;
            ApplyAuth();
        }
    }

    private void ApplyAuth()
    {
        if (!string.IsNullOrEmpty(_token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var res = await _http.PostAsJsonAsync("/api/auth/login", request);
        if (!res.IsSuccessStatusCode) return null;
        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth != null)
        {
            Token = auth.Token;
            Username = auth.Username;
        }
        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var res = await _http.PostAsJsonAsync("/api/auth/register", request);
        if (!res.IsSuccessStatusCode) return null;
        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth != null)
        {
            Token = auth.Token;
            Username = auth.Username;
        }
        return auth;
    }

    public async Task<List<ProjectResponse>> GetProjectsAsync()
    {
        var res = await _http.GetAsync("/api/projects");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<ProjectResponse>>() ?? new();
    }

    public async Task<List<ProjectResponse>> GetMyProjectsAsync()
    {
        var res = await _http.GetAsync("/api/projects/my");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<ProjectResponse>>() ?? new();
    }

    public async Task<List<ProjectResponse>> GetMemberProjectsAsync()
    {
        var res = await _http.GetAsync("/api/projects/member");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<ProjectResponse>>() ?? new();
    }

    public async Task<ProjectResponse?> GetProjectAsync(int id)
    {
        var res = await _http.GetAsync($"/api/projects/{id}");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<ProjectResponse>();
    }

    public async Task<ProjectResponse?> CreateProjectAsync(CreateProjectRequest request)
    {
        var res = await _http.PostAsJsonAsync("/api/projects", request);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<ProjectResponse>();
    }

    public async Task<bool> DeleteProjectAsync(int projectId)
    {
        var res = await _http.DeleteAsync($"/api/projects/{projectId}");
        return res.IsSuccessStatusCode;
    }

    public async Task<List<BranchResponse>> GetBranchesAsync(int projectId)
    {
        var res = await _http.GetAsync($"/api/projects/{projectId}/branches");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<BranchResponse>>() ?? new();
    }

    public async Task<BranchResponse?> CreateBranchAsync(int projectId, CreateBranchRequest request)
    {
        var res = await _http.PostAsJsonAsync($"/api/projects/{projectId}/branches", request);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<BranchResponse>();
    }

    public async Task<bool> DeleteBranchAsync(int projectId, string branchName)
    {
        var res = await _http.DeleteAsync($"/api/projects/{projectId}/branches?name={Uri.EscapeDataString(branchName)}");
        return res.IsSuccessStatusCode;
    }

    public async Task<List<CommitResponse>> GetCommitsAsync(int projectId, string? branch = null)
    {
        var url = $"/api/projects/{projectId}/commits";
        if (!string.IsNullOrWhiteSpace(branch))
            url += $"?branch={Uri.EscapeDataString(branch)}";
        var res = await _http.GetAsync(url);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<CommitResponse>>() ?? new();
    }

    public async Task<CommitResponse?> CreateCommitAsync(int projectId, CreateCommitRequest request)
    {
        var res = await _http.PostAsJsonAsync($"/api/projects/{projectId}/commits", request);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CommitResponse>();
    }

    public async Task<CommitResponse?> CheckoutCommitAsync(int projectId, int commitId)
    {
        var res = await _http.PostAsync($"/api/projects/{projectId}/commits/{commitId}/checkout", null);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CommitResponse>();
    }

    public async Task<byte[]?> DownloadCommitZipAsync(int projectId, int commitId)
    {
        var res = await _http.GetAsync($"/api/projects/{projectId}/commits/{commitId}/download");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadAsByteArrayAsync();
    }

    public async Task<bool> MergeBranchAsync(int projectId, string source, string target)
    {
        var res = await _http.PostAsync(
            $"/api/projects/{projectId}/branches/merge?source={Uri.EscapeDataString(source)}&target={Uri.EscapeDataString(target)}",
            null);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<MemberResponse>> GetMembersAsync(int projectId)
    {
        var res = await _http.GetAsync($"/api/projects/{projectId}/members");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<MemberResponse>>() ?? new();
    }

    public async Task<MemberResponse?> AddMemberAsync(int projectId, AddMemberRequest request)
    {
        var res = await _http.PostAsJsonAsync($"/api/projects/{projectId}/members", request);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<MemberResponse>();
    }

    public async Task<bool> RemoveMemberAsync(int projectId, int memberId)
    {
        var res = await _http.DeleteAsync($"/api/projects/{projectId}/members/{memberId}");
        return res.IsSuccessStatusCode;
    }
}
