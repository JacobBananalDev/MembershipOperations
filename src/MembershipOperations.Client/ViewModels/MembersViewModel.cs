using System.Collections.ObjectModel;
using MembershipOperations.Client.Services;
using MembershipOperations.Shared.Dto.Members;

namespace MembershipOperations.Client.ViewModels;

public class MembersViewModel : ViewModelBase
{
    private readonly MembersApi _membersApi;

    private string _searchText = "";
    private bool _activeOnly = true;
    private string _statusMessage = "";
    private bool _isBusy;

    public MembersViewModel(MembersApi membersApi)
    {
        _membersApi = membersApi;

        Members = new ObservableCollection<MemberDto>();

        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsBusy);

        // load immediately
        _ = RefreshAsync();
    }

    public ObservableCollection<MemberDto> Members { get; }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    public bool ActiveOnly
    {
        get => _activeOnly;
        set { _activeOnly = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged(); }
    }

    public System.Windows.Input.ICommand RefreshCommand { get; }

    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading members...";

            var result = await _membersApi.GetMembersAsync(
                pageNumber: 1,
                pageSize: 100,
                search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
                activeOnly: ActiveOnly);

            Members.Clear();

            if (result?.Items != null)
            {
                foreach (var m in result.Items)
                    Members.Add(m);
            }

            StatusMessage = $"Loaded {Members.Count} members.";
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
