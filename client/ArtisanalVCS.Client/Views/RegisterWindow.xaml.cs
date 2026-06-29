namespace ArtisanalVCS.Client.Views;

public partial class RegisterWindow : Window
{
    private readonly RegisterViewModel _vm;

    public RegisterWindow(ApiService api)
    {
        InitializeComponent();
        _vm = new RegisterViewModel(api);
        DataContext = _vm;
        _vm.RegisterSucceeded += OnRegisterSucceeded;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.Password = PasswordBox.Password;
    }

    private void OnConfirmChanged(object sender, RoutedEventArgs e)
    {
        _vm.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    private void OnRegisterSucceeded(AuthResponse res)
    {
        System.Windows.MessageBox.Show("зарегистрирован!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }
}
