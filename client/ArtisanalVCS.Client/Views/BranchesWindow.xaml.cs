namespace ArtisanalVCS.Client.Views;

public partial class BranchesWindow : Window
{
    private readonly BranchesViewModel _vm;

    public BranchesWindow(ApiService api, int projectId, string projectName, bool isOwner)
    {
        InitializeComponent();
        _vm = new BranchesViewModel(api, projectId, projectName, isOwner);
        DataContext = _vm;
        Loaded += async (_, _) => await _vm.LoadBranchesCommand.ExecuteAsync(null);
    }
}
