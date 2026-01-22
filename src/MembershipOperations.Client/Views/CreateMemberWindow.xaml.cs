using System.Windows;
using MembershipOperations.Client.ViewModels;

namespace MembershipOperations.Client.Views;

public partial class CreateMemberWindow : Window
{
    public CreateMemberWindow(CreateMemberViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
