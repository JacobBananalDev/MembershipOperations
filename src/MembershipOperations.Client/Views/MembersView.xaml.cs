using System.Windows.Controls;
using MembershipOperations.Client.ViewModels;

namespace MembershipOperations.Client.Views;

public partial class MembersView : UserControl
{
    public MembersView(MembersViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
