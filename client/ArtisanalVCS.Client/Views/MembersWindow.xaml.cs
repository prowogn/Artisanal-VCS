namespace ArtisanalVCS.Client.Views;

public partial class MembersWindow : Window
{
    private readonly MembersViewModel _vm;

    public MembersWindow(MembersViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
        Loaded += async (_, _) => await _vm.LoadMembersCommand.ExecuteAsync(null);
    }
}
