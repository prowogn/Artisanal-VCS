namespace ArtisanalVCS.Client.Views;

public partial class CreateProjectWindow : Window
{
    private readonly CreateProjectViewModel _vm;

    public CreateProjectWindow(ApiService api)
    {
        InitializeComponent();
        _vm = new CreateProjectViewModel(api);
        DataContext = _vm;
        _vm.ProjectCreated += OnCreated;
    }

    private void OnCreated(ProjectResponse project)
    {
        DialogResult = true;
        Close();
    }
}
