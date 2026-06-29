namespace ArtisanalVCS.Client.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _api;

    public LoginViewModel(ApiService api)
    {
        _api = api;
    }

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action<AuthResponse>? LoginSucceeded;

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "заполни поля";
            return;
        }

        IsLoading = true;
        try
        {
            var req = new LoginRequest { Username = Username, Password = Password };
            var res = await _api.LoginAsync(req);
            if (res == null)
            {
                ErrorMessage = "неверный логин или пароль";
                return;
            }
            LoginSucceeded?.Invoke(res);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"ошибка: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenRegister()
    {
        var w = new Views.RegisterWindow(_api);
        w.ShowDialog();
    }
}
