namespace ArtisanalVCS.Client.Views;

public partial class CreateCommitWindow : Window
{
    private readonly CreateCommitViewModel _vm;

    public CreateCommitWindow(CreateCommitViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
        _vm.CommitCreated += OnCreated;
    }

    private void OnCreated(CommitResponse commit)
    {
        DialogResult = true;
        Close();
    }
}
