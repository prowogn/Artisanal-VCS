using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class MergeViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly int _projectId;

    public MergeViewModel(ApiService api, int projectId, List<string> branchNames)
    {
        _api = api;
        _projectId = projectId;
        Branches = new ObservableCollection<string>(branchNames);
    }

    [ObservableProperty]
    private ObservableCollection<string> _branches = new();

    [ObservableProperty]
    private string _sourceBranch = "";

    [ObservableProperty]
    private string _targetBranch = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _resultMessage = "";

    public event Action? MergeCompleted;

    [RelayCommand]
    private async Task Merge()
    {
        ErrorMessage = "";
        ResultMessage = "";
        if (string.IsNullOrWhiteSpace(SourceBranch) || string.IsNullOrWhiteSpace(TargetBranch))
        {
            ErrorMessage = "выбери обе ветки";
            return;
        }
        if (SourceBranch == TargetBranch)
        {
            ErrorMessage = "нельзя мержить ветку в саму себя";
            return;
        }

        IsLoading = true;
        try
        {
            var ok = await _api.MergeBranchAsync(_projectId, SourceBranch, TargetBranch);
            if (ok)
            {
                ResultMessage = "мерж выполнен";
                MergeCompleted?.Invoke();
            }
            else ErrorMessage = "мерж не удался";
        }
        catch (Exception ex) { ErrorMessage = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }
}
