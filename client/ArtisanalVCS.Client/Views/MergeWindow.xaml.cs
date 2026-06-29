namespace ArtisanalVCS.Client.Views;

public partial class MergeWindow : Window
{
    private readonly MergeViewModel _vm;

    public MergeWindow(MergeViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
        _vm.MergeCompleted += OnCompleted;
    }

    private void OnCompleted()
    {
        DialogResult = true;
        Close();
    }
}
