using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class CreateProjectViewModel : ObservableObject
{
    private readonly ApiService _api;

    public CreateProjectViewModel(ApiService api) => _api = api;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private bool _isPublic = true;

    [ObservableProperty]
    private string _projectFolder = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action<ProjectResponse>? ProjectCreated;

    [RelayCommand]
    private void BrowseFolder()
    {
        using var dlg = new System.Windows.Forms.FolderBrowserDialog();
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            ProjectFolder = dlg.SelectedPath;
    }

    [RelayCommand]
    private async Task Create()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "название обязательно";
            return;
        }
        if (string.IsNullOrWhiteSpace(ProjectFolder) || !Directory.Exists(ProjectFolder))
        {
            ErrorMessage = "выбери папку проекта";
            return;
        }

        IsLoading = true;
        try
        {
            var req = new CreateProjectRequest
            {
                Name = Name,
                Description = Description,
                IsPublic = IsPublic
            };
            var project = await _api.CreateProjectAsync(req);
            if (project == null)
            {
                ErrorMessage = "не удалось создать проект";
                return;
            }

            var files = new List<FileSnapshotDto>();
            foreach (var file in Directory.GetFiles(ProjectFolder, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(ProjectFolder, file);
                try
                {
                    files.Add(new FileSnapshotDto { FilePath = relative, Content = await File.ReadAllTextAsync(file) });
                }
                catch { }
            }

            if (files.Count > 0)
            {
                var commitReq = new CreateCommitRequest
                {
                    Message = "initial commit",
                    BranchName = "main",
                    Files = files
                };
                await _api.CreateCommitAsync(project.Id, commitReq);
            }

            ProjectCreated?.Invoke(project);
        }
        catch (Exception ex) { ErrorMessage = $"ошибка: {ex.InnerException?.Message ?? ex.Message}"; }
        finally { IsLoading = false; }
    }
}
