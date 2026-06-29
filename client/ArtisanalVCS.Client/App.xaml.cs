using System.Windows;
using ArtisanalVCS.Client.Services;
using ArtisanalVCS.Client.ViewModels;
using ArtisanalVCS.Client.Views;

namespace ArtisanalVCS.Client;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var api = new ApiService();
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ArtisanalVCS");

        var tokenPath = Path.Combine(appData, "token.txt");
        var userPath = Path.Combine(appData, "user.txt");

        if (File.Exists(tokenPath))
        {
            var token = File.ReadAllText(tokenPath).Trim();
            if (!string.IsNullOrEmpty(token))
            {
                api.Token = token;
                if (File.Exists(userPath))
                    api.Username = File.ReadAllText(userPath).Trim();
                var vm = new MainViewModel(api);
                var main = new Views.MainWindow(vm);
                main.Show();
                return;
            }
        }

        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var login = new LoginWindow(api);
        if (login.ShowDialog() == true)
        {
            Directory.CreateDirectory(appData);
            File.WriteAllText(tokenPath, api.Token ?? "");
            File.WriteAllText(userPath, api.Username ?? "");

            var vm = new MainViewModel(api);
            var main = new Views.MainWindow(vm);
            Current.MainWindow = main;
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        else
        {
            Current.Shutdown();
        }
    }
}
