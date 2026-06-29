using System.Collections.ObjectModel;

namespace ArtisanalVCS.Client.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _api;
    private bool _showMyProjects;

    public MainViewModel(ApiService api)
    {
        _api = api;
    }

    [RelayCommand]
    private void Logout()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ArtisanalVCS");

        var tokenPath = Path.Combine(appData, "token.txt");
        var userPath = Path.Combine(appData, "user.txt");

        try
        {
            if (File.Exists(tokenPath)) File.Delete(tokenPath);
            if (File.Exists(userPath)) File.Delete(userPath);
        }
        catch { }

        _api.Token = null;
        _api.Username = null;

        System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        System.Windows.Application.Current.MainWindow.Close();

        var login = new Views.LoginWindow(_api);
        if (login.ShowDialog() == true)
        {
            Directory.CreateDirectory(appData);
            File.WriteAllText(tokenPath, _api.Token ?? "");
            File.WriteAllText(userPath, _api.Username ?? "");

            var mainVm = new MainViewModel(_api);
            var main = new Views.MainWindow(mainVm);
            System.Windows.Application.Current.MainWindow = main;
            System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        else
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    [ObservableProperty]
    private ObservableCollection<ProjectResponse> _projects = new();

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusText = "";

    public string Username => _api.Username ?? "";

    public string ToggleButtonText => _showMyProjects ? "All Projects" : "My Projects";

    [RelayCommand]
    private async Task ToggleView()
    {
        _showMyProjects = !_showMyProjects;
        OnPropertyChanged(nameof(ToggleButtonText));
        await LoadProjects();
    }

    [RelayCommand]
    private async Task LoadProjects()
    {
        IsLoading = true;
        StatusText = "загрузка...";
        try
        {
            List<ProjectResponse> projects;
            var my = await _api.GetMyProjectsAsync();
            var myIds = my.Select(p => p.Id).ToHashSet();

            if (_showMyProjects)
            {
                var member = await _api.GetMemberProjectsAsync();
                projects = my.UnionBy(member, p => p.Id).ToList();
                StatusText = $"моих проектов: {projects.Count}";
            }
            else
            {
                var all = await _api.GetProjectsAsync();
                projects = all.ToList();
                StatusText = $"найдено {all.Count} проектов";
            }

            foreach (var p in projects)
                p.IsOwner = myIds.Contains(p.Id);

            Projects = new ObservableCollection<ProjectResponse>(projects);
        }
        catch (Exception ex) { StatusText = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadProjects();
            return;
        }

        IsLoading = true;
        StatusText = "поиск...";
        try
        {
            var all = await _api.GetProjectsAsync();
            var filtered = all.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            var my = await _api.GetMyProjectsAsync();
            var myIds = my.Select(p => p.Id).ToHashSet();
            foreach (var p in filtered) p.IsOwner = myIds.Contains(p.Id);

            Projects = new ObservableCollection<ProjectResponse>(filtered);
            StatusText = $"найдено {filtered.Count} проектов";
        }
        catch (Exception ex) { StatusText = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenProject(ProjectResponse? project)
    {
        if (project == null) return;
        var w = new Views.BranchesWindow(_api, project.Id, project.Name, project.IsOwner);
        w.ShowDialog();
        _ = LoadProjects();
    }

    [RelayCommand]
    private void CreateProject()
    {
        var w = new Views.CreateProjectWindow(_api);
        if (w.ShowDialog() == true)
            _ = LoadProjects();
    }

    [RelayCommand]
    private async Task DeleteProject(ProjectResponse? project)
    {
        if (project == null || !project.IsOwner) return;
        var confirm = System.Windows.MessageBox.Show(
            $"Удалить проект \"{project.Name}\"?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var ok = await _api.DeleteProjectAsync(project.Id);
            if (ok)
            {
                StatusText = $"проект \"{project.Name}\" удалён";
                await LoadProjects();
            }
            else StatusText = "не удалось удалить проект";
        }
        catch (Exception ex) { StatusText = $"ошибка: {ex.Message}"; }
    }
}
