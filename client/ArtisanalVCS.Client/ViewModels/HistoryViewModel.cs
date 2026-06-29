using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly int _projectId;

    public HistoryViewModel(ApiService api, int projectId)
    {
        _api = api;
        _projectId = projectId;
    }

    [ObservableProperty]
    private ObservableCollection<CommitResponse> _commits = new();

    [ObservableProperty]
    private CommitResponse? _selectedCommit;

    [ObservableProperty]
    private string _commitDetail = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusText = "";

    [RelayCommand]
    private async Task LoadHistory()
    {
        IsLoading = true;
        StatusText = "загрузка...";
        try
        {
            var commits = await _api.GetCommitsAsync(_projectId);
            Commits = new ObservableCollection<CommitResponse>(commits);
            StatusText = $"коммитов: {commits.Count}";
        }
        catch (Exception ex) { StatusText = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SelectCommit(CommitResponse? commit)
    {
        if (commit == null) return;
        SelectedCommit = commit;
        IsLoading = true;
        try
        {
            var detail = await _api.GetProjectAsync(_projectId);
            if (detail != null && commit.Files.Count > 0)
            {
                CommitDetail = string.Join("\n---\n",
                    commit.Files.Select(f => $"{f.FilePath}:\n{f.Content}"));
            }
            else
            {
                var commits = await _api.GetCommitsAsync(_projectId);
                var full = commits.FirstOrDefault(c => c.Id == commit.Id);
                if (full != null)
                {
                    CommitDetail = string.Join("\n---\n",
                        full.Files.Select(f => $"{f.FilePath}:\n{f.Content}"));
                }
            }
        }
        catch (Exception) { CommitDetail = "не удалось загрузить"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task Rollback()
    {
        if (SelectedCommit == null) return;
        var confirm = System.Windows.MessageBox.Show(
            $"Rollback to commit \"{SelectedCommit.Message}\"? This creates a new revert commit.",
            "Rollback", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            var res = await _api.CheckoutCommitAsync(_projectId, SelectedCommit.Id);
            if (res != null)
            {
                StatusText = "rollback done";
                await LoadHistory();
            }
            else StatusText = "rollback failed";
        }
        catch (Exception ex) { StatusText = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task DownloadZip()
    {
        if (SelectedCommit == null) return;
        IsLoading = true;
        try
        {
            var data = await _api.DownloadCommitZipAsync(_projectId, SelectedCommit.Id);
            if (data == null)
            {
                StatusText = "download failed";
                return;
            }

            var dlg = new System.Windows.Forms.SaveFileDialog
            {
                FileName = $"commit-{SelectedCommit.Id}.zip",
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
