using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class BranchesViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly int _projectId;

    public BranchesViewModel(ApiService api, int projectId, string projectName, bool isOwner)
    {
        _api = api;
        _projectId = projectId;
        ProjectName = projectName;
        IsOwner = isOwner;
    }

    [ObservableProperty]
    private string _projectName = "";

    [ObservableProperty]
    private bool _isOwner;

    [ObservableProperty]
    private ObservableCollection<BranchResponse> _branches = new();

    [ObservableProperty]
    private ObservableCollection<CommitResponse> _commits = new();

    [ObservableProperty]
    private BranchResponse? _selectedBranch;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusText = "";

    partial void OnSelectedBranchChanged(BranchResponse? value)
    {
        if (value != null)
            _ = SelectBranch(value);
    }

    [RelayCommand]
    private async Task LoadBranches()
    {
        IsLoading = true;
        try
        {
            var branches = await _api.GetBranchesAsync(_projectId);
            Branches = new ObservableCollection<BranchResponse>(branches);
            StatusText = $"веток: {branches.Count}";
            if (branches.Count > 0 && SelectedBranch == null)
                SelectedBranch = Branches[0];
        }
        catch (Exception) { StatusText = "ошибка загрузки веток"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SelectBranch(BranchResponse? branch)
    {
        if (branch == null) return;
        SelectedBranch = branch;
        IsLoading = true;
        try
        {
            var commits = await _api.GetCommitsAsync(_projectId, branch.Name);
            Commits = new ObservableCollection<CommitResponse>(commits);
            StatusText = $"коммитов: {commits.Count}";
        }
        catch (Exception) { StatusText = "ошибка загрузки коммитов"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task DeleteBranch(BranchResponse? branch)
    {
        if (branch == null) return;
        var ok = await _api.DeleteBranchAsync(_projectId, branch.Name);
        if (ok) await LoadBranches();
        else StatusText = "нельзя удалить main или ветка не найдена";
    }

    [RelayCommand]
    private void OpenCreateCommit()
    {
        var vm = new CreateCommitViewModel(_api, _projectId, Branches.Select(b => b.Name).ToList());
        var w = new Views.CreateCommitWindow(vm);
        if (w.ShowDialog() == true)
        {
            if (SelectedBranch != null)
                _ = SelectBranch(SelectedBranch);
        }
    }

    [RelayCommand]
    private void OpenHistory()
    {
        var w = new Views.HistoryWindow(new HistoryViewModel(_api, _projectId));
        w.ShowDialog();
    }

    [RelayCommand]
    private void OpenMerge()
    {
        var branches = Branches.Select(b => b.Name).ToList();
        var w = new Views.MergeWindow(new MergeViewModel(_api, _projectId, branches));
        if (w.ShowDialog() == true)
            _ = LoadBranches();
    }

    [RelayCommand]
    private void OpenMembers()
    {
        var w = new Views.MembersWindow(new MembersViewModel(_api, _projectId));
        w.ShowDialog();
    }

    [RelayCommand]
    private void OpenCommitDetail(CommitResponse? commit)
    {
        if (commit == null) return;
        System.Windows.MessageBox.Show(
            string.Join("\n", commit.Files.Select(f => $"{f.FilePath}:\n{f.Content}")),
            commit.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task DownloadCommitZip(CommitResponse? commit)
    {
        if (commit == null) return;
        IsLoading = true;
        try
        {
            var data = await _api.DownloadCommitZipAsync(_projectId, commit.Id);
            if (data == null)
            {
                StatusText = "download failed";
                return;
            }

            var dlg = new System.Windows.Forms.SaveFileDialog
            {
                FileName = $"commit-{commit.Id}.zip",
                Filter = "ZIP files|*.zip"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await File.WriteAllBytesAsync(dlg.FileName, data);
                StatusText = $"saved to {dlg.FileName}";
            }
        }
        catch (Exception ex) { StatusText = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }
}
