using MembershipOperations.Client.Core;

namespace MembershipOperations.Client.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly AuthSession _session;

    private object? _currentView;

    public MainViewModel(AuthSession session, MembersViewModel membersVm)
    {
        _session = session;
        CurrentView = membersVm;
    }

    public object? CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    public string SignedInAsText
        => _session.IsAuthenticated
            ? $"Signed in as {_session.Username} ({_session.Role})"
            : "Not signed in";
}
