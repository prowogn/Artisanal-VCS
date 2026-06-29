using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class MembersViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly int _projectId;

    public MembersViewModel(ApiService api, int projectId)
    {
        _api = api;
        _projectId = projectId;
    }

    [ObservableProperty]
    private ObservableCollection<MemberResponse> _members = new();

    [ObservableProperty]
    private string _newUsername = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusText = "";

    [RelayCommand]
    private async Task LoadMembers()
    {
        IsLoading = true;
        try
        {
            var members = await _api.GetMembersAsync(_projectId);
            Members = new ObservableCollection<MemberResponse>(members);
            StatusText = $"участников: {members.Count}";
        }
        catch (Exception) { StatusText = "ошибка загрузки"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AddMember()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(NewUsername))
        {
            ErrorMessage = "введи имя";
            return;
        }

        IsLoading = true;
        try
        {
            var req = new AddMemberRequest { Username = NewUsername };
            var res = await _api.AddMemberAsync(_projectId, req);
            if (res == null)
            {
                ErrorMessage = "юзер не найден";
                return;
            }
            NewUsername = "";
            await LoadMembers();
        }
        catch (Exception ex) { ErrorMessage = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RemoveMember(MemberResponse? member)
    {
        if (member == null) return;
        IsLoading = true;
        try
        {
            await _api.RemoveMemberAsync(_projectId, member.Id);
            await LoadMembers();
        }
        catch (Exception) { ErrorMessage = "не удалось удалить"; }
        finally { IsLoading = false; }
    }
}
