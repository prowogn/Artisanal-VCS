namespace ArtisanalVCS.Client.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(ApiService api)
    {
        InitializeComponent();
        _vm = new LoginViewModel(api);
        DataContext = _vm;
        _vm.LoginSucceeded += OnLoginSucceeded;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.Password = PasswordBox.Password;
    }

    private void OnLoginSucceeded(AuthResponse res)
    {
        _vm.GetType().GetProperty("Password")?.SetValue(_vm, "");
        DialogResult = true;
        Close();
    }
}
