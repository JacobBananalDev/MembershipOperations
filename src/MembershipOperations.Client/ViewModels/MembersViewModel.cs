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
        EditCommand = new RelayCommand(async () => await EditAsync(), () => !IsBusy && SelectedMember != null);
        DeactivateCommand = new RelayCommand(async () => await DeactivateAsync(), CanDeactivate);

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
            (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        // Wire events
        async void OnSaveRequested()
        {
            try
            {
                vm.IsBusy = true;
                vm.StatusMessage = "Creating member...";

                var created = await _membersApi.CreateMemberAsync(vm.ToRequest());

                win.DialogResult = true;   // OK because we will show with ShowDialog()
                win.Close();

                StatusMessage = $"Created member #{created.Id}. Refreshing...";
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                vm.StatusMessage = ex.Message; // keep dialog open
            }
            finally
            {
                vm.IsBusy = false;
            }
        }

        void OnCancelRequested()
        {
            win.DialogResult = false; // OK with ShowDialog()
            win.Close();
        }

        vm.SaveRequested += OnSaveRequested;
        vm.CancelRequested += OnCancelRequested;

        try
        {
            // This blocks until window closes, but Save is async and will NOT freeze UI
            win.ShowDialog();
        }
        finally
        {
            vm.SaveRequested -= OnSaveRequested;
            vm.CancelRequested -= OnCancelRequested;
        }
    }

    private MemberDto? _selectedMember;

    public MemberDto? SelectedMember
    {
        get => _selectedMember;
        set
        {
            _selectedMember = value;
            OnPropertyChanged();
            (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeactivateCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public System.Windows.Input.ICommand EditCommand { get; }

    private async Task EditAsync()
    {
        if (SelectedMember == null)
            return;

        var win = _sp.GetRequiredService<MembershipOperations.Client.Views.EditMemberWindow>();
        win.Owner = System.Windows.Application.Current.MainWindow;

        var vm = (EditMemberViewModel)win.DataContext;
        vm.LoadFrom(SelectedMember);

        async void OnSaveRequested()
        {
            try
            {
                vm.IsBusy = true;
                vm.StatusMessage = "Saving changes...";

                await _membersApi.UpdateMemberAsync(vm.Id, vm.ToRequest());

                win.Close();

                StatusMessage = $"Updated member #{vm.Id}. Refreshing...";
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                vm.StatusMessage = ex.Message;
            }
            finally
            {
                vm.IsBusy = false;
            }
        }

        void OnCancelRequested() => win.Close();

        vm.SaveRequested += OnSaveRequested;
        vm.CancelRequested += OnCancelRequested;

        try
        {
            win.ShowDialog(); // modal edit dialog
        }
        finally
        {
            vm.SaveRequested -= OnSaveRequested;
            vm.CancelRequested -= OnCancelRequested;
        }
    }

    private bool CanDeactivate()
    => !IsBusy && SelectedMember != null && SelectedMember.IsActive;

    public System.Windows.Input.ICommand DeactivateCommand { get; }

    private async Task DeactivateAsync()
    {
        if (SelectedMember == null)
            return;

        if (!SelectedMember.IsActive)
        {
            StatusMessage = "Member is already inactive.";
            return;
        }

        var name = $"{SelectedMember.FirstName} {SelectedMember.LastName}".Trim();

        var result = System.Windows.MessageBox.Show(
            $"Deactivate member #{SelectedMember.Id} ({name})?\n\nThis will mark the member as inactive (soft delete).",
            "Confirm Deactivation",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Deactivating member...";

            await _membersApi.DeactivateMemberAsync(SelectedMember.Id);

            StatusMessage = $"Deactivated member #{SelectedMember.Id}. Refreshing...";
            await RefreshAsync();
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
