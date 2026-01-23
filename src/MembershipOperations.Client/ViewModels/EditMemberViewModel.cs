using MembershipOperations.Shared.Dto.Members;

namespace MembershipOperations.Client.ViewModels;

public class EditMemberViewModel : ViewModelBase
{
    private int _id;
    private string _firstName = "";
    private string _lastName = "";
    private string? _email;
    private bool _isActive = true;

    private string _statusMessage = "";
    private bool _isBusy;

    public EditMemberViewModel()
    {
        SaveCommand = new RelayCommand(() => SaveRequested?.Invoke(), CanSave);
        CancelCommand = new RelayCommand(() => CancelRequested?.Invoke());
    }

    public int Id
    {
        get => _id;
        private set { _id = value; OnPropertyChanged(); }
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

    public string? Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
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

    public void LoadFrom(MemberDto dto)
    {
        Id = dto.Id;
        FirstName = dto.FirstName ?? "";
        LastName = dto.LastName ?? "";
        Email = dto.Email;
        IsActive = dto.IsActive;
        StatusMessage = "";
    }

    public UpdateMemberRequest ToRequest()
    {
        return new UpdateMemberRequest
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            IsActive = IsActive
        };
    }

    private bool CanSave()
        => !IsBusy
           && Id > 0
           && !string.IsNullOrWhiteSpace(FirstName)
           && !string.IsNullOrWhiteSpace(LastName);

    private void RaiseSaveCanExecute()
        => (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
}
