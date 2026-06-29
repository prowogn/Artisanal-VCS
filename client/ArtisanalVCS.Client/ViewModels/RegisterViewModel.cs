namespace ArtisanalVCS.Client.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiService _api;

    public RegisterViewModel(ApiService api)
    {
        _api = api;
    }

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _confirmPassword = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action<AuthResponse>? RegisterSucceeded;

    [RelayCommand]
    private async Task Register()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "заполни все поля";
            return;
        }
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "пароли не совпадают";
            return;
        }

        IsLoading = true;
        try
        {
            var req = new RegisterRequest { Username = Username, Email = Email, Password = Password };
            var res = await _api.RegisterAsync(req);
            if (res == null)
            {
                ErrorMessage = "этот юзер или почта уже заняты";
                return;
            }
            RegisterSucceeded?.Invoke(res);
        }
        catch (Exception ex) { ErrorMessage = $"ошибка: {ex.Message}"; }
        finally { IsLoading = false; }
    }
}
