using MembershipOperations.Client.Services;
using MembershipOperations.Shared.Dto.Members;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace MembershipOperations.Client.ViewModels;

public class MembersViewModel : ViewModelBase
{
    private readonly MembersApi _membersApi;

    private string _searchText = "";
    private bool _activeOnly = true;
    private string _statusMessage = "";
    private bool _isBusy;
    private readonly IServiceProvider _sp;

    public MembersViewModel(MembersApi membersApi, IServiceProvider sp)
    {
        _membersApi = membersApi;
        _sp = sp;

        Members = new ObservableCollection<MemberDto>();

        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsBusy);
        CreateCommand = new RelayCommand(async () => await CreateAsync(), () => !IsBusy);

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
        set 
        { 
            _isBusy = value; 
            OnPropertyChanged();
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CreateCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
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

    public System.Windows.Input.ICommand CreateCommand { get; }

    private async Task CreateAsync()
    {
        var win = _sp.GetRequiredService<MembershipOperations.Client.Views.CreateMemberWindow>();
        win.Owner = System.Windows.Application.Current.MainWindow;

        var vm = (CreateMemberViewModel)win.DataContext;

        var tcs = new TaskCompletionSource<bool>();

        void OnSaveRequested() => tcs.TrySetResult(true);
        void OnCancelRequested() => tcs.TrySetResult(false);

        vm.SaveRequested += OnSaveRequested;
        vm.CancelRequested += OnCancelRequested;

        try
        {
            win.Show();

            while (true)
            {
                // Wait until user clicks Save or Cancel
                var wantsSave = await tcs.Task;
                tcs = new TaskCompletionSource<bool>();

                if (!wantsSave)
                {
                    win.Close();
                    return;
                }

                // Save clicked -> attempt API call
                try
                {
                    vm.IsBusy = true;
                    vm.StatusMessage = "Creating member...";

                    var created = await _membersApi.CreateMemberAsync(vm.ToRequest());

                    // success -> close dialog
                    win.DialogResult = true;
                    win.Close();

                    StatusMessage = $"Created member #{created.Id}. Refreshing...";
                    await RefreshAsync();
                    return;
                }
                catch (Exception ex)
                {
                    // keep dialog open, show error
                    vm.StatusMessage = ex.Message;
                }
                finally
                {
                    vm.IsBusy = false;
                }
            }
        }
        finally
        {
            vm.SaveRequested -= OnSaveRequested;
            vm.CancelRequested -= OnCancelRequested;

            if (win.IsVisible)
                win.Close();
        }
    }


}
