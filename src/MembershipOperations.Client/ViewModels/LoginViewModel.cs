using MembershipOperations.Client.Core;
using MembershipOperations.Client.Services;
using MembershipOperations.Shared.Dto.Auth;

namespace MembershipOperations.Client.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthApi _authApi;
    private readonly AuthSession _session;

    private string _username = "";
    private string _password = "";
    private string _statusMessage = "";
    private bool _isBusy;

    public LoginViewModel(AuthApi authApi, AuthSession session)
    {
        _authApi = authApi;
        _session = session;

        LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
    }

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); ((RelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    // NOTE: binding Password directly is not secure; we’ll wire PasswordBox properly in the View step
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); ((RelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((RelayCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    public System.Windows.Input.ICommand LoginCommand { get; }

    public event Action? LoginSucceeded;

    private bool CanLogin()
        => !IsBusy
           && !string.IsNullOrWhiteSpace(Username)
           && !string.IsNullOrWhiteSpace(Password);

    private async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Signing in...";

            var auth = await _authApi.LoginAsync(new LoginRequest
            {
                Username = Username.Trim(),
                Password = Password
            });

            _session.Set(auth.Token, auth.Username, auth.Role, auth.ExpiresUtc);

            StatusMessage = "Signed in.";
            LoginSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
