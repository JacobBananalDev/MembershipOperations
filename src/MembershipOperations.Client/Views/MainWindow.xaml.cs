using System.Windows;
using MembershipOperations.Client.ViewModels;

namespace MembershipOperations.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
