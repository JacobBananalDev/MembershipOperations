using MembershipOperations.Shared.Dto.Members;

namespace MembershipOperations.Client.ViewModels;

public class CreateMemberViewModel : ViewModelBase
{
    private string _firstName = "";
    private string _lastName = "";
    private string _email = "";
    private string _statusMessage = "";
    private bool _isBusy;

    public CreateMemberViewModel()
    {
        SaveCommand = new RelayCommand(() => SaveRequested?.Invoke(), CanSave);
        CancelCommand = new RelayCommand(() => CancelRequested?.Invoke());
    }

    public string FirstName
    {
        get => _firstName;
        set { _firstName = value; OnPropertyChanged(); RaiseSaveCanExecute(); }
    }

    public string LastName
    {
        get => _lastName;
        set { _lastName = value; OnPropertyChanged(); RaiseSaveCanExecute(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); RaiseSaveCanExecute(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); RaiseSaveCanExecute(); }
    }

    public System.Windows.Input.ICommand SaveCommand { get; }
    public System.Windows.Input.ICommand CancelCommand { get; }

    public event Action? SaveRequested;
    public event Action? CancelRequested;

    public CreateMemberRequest ToRequest()
    {
        return new CreateMemberRequest
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim()
        };
    }

    private bool CanSave()
        => !IsBusy
           && !string.IsNullOrWhiteSpace(FirstName)
           && !string.IsNullOrWhiteSpace(LastName);

    private void RaiseSaveCanExecute()
        => (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
}
