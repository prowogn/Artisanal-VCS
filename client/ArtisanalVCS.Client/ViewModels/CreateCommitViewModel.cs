using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class CreateCommitViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly int _projectId;

    public CreateCommitViewModel(ApiService api, int projectId, List<string> branchNames)
    {
        _api = api;
        _projectId = projectId;
        Branches = new ObservableCollection<string>(branchNames);
        if (branchNames.Count > 0) _selectedBranch = branchNames[0];
    }

    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    private ObservableCollection<string> _branches = new();

    [ObservableProperty]
    private string _selectedBranch = "";

    [ObservableProperty]
    private string _projectFolder = "";

    [ObservableProperty]
    private ObservableCollection<FileSnapshotDto> _files = new();

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action<CommitResponse>? CommitCreated;

    [RelayCommand]
    private void BrowseFolder()
    {
        using var dlg = new System.Windows.Forms.FolderBrowserDialog();
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ProjectFolder = dlg.SelectedPath;
            LoadFiles();
        }
    }

    private void LoadFiles()
    {
        Files.Clear();
        if (string.IsNullOrWhiteSpace(ProjectFolder) || !Directory.Exists(ProjectFolder)) return;

        foreach (var file in Directory.GetFiles(ProjectFolder, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(ProjectFolder, file);
            try
            {
                var content = File.ReadAllText(file);
                Files.Add(new FileSnapshotDto { FilePath = relative, Content = content });
            }
            catch { }
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Message))
        {
            ErrorMessage = "нужен message";
            return;
        }
        if (string.IsNullOrWhiteSpace(ProjectFolder) || !Directory.Exists(ProjectFolder))
        {
            ErrorMessage = "выбери папку проекта";
            return;
        }

        LoadFiles();
        if (Files.Count == 0)
        {
            ErrorMessage = "в папке нет файлов";
            return;
        }

        IsLoading = true;
        try
        {
            var req = new CreateCommitRequest
            {
                Message = Message,
                BranchName = SelectedBranch,
                Files = Files.ToList()
            };
            var res = await _api.CreateCommitAsync(_projectId, req);
            if (res == null)
            {
                ErrorMessage = "не удалось создать коммит";
                return;
            }
            CommitCreated?.Invoke(res);
        }
        catch (Exception ex) { ErrorMessage = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }
}
