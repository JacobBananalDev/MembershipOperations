using System.Windows;
using MembershipOperations.Client.ViewModels;

namespace MembershipOperations.Client.Views;

public partial class EditMemberWindow : Window
{
    public EditMemberWindow(EditMemberViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
