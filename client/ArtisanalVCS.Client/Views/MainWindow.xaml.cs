namespace ArtisanalVCS.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.LoadProjectsCommand.Execute(null);
    }
}
