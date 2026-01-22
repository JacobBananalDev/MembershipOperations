using System.Windows;
using System.Windows.Controls;
using MembershipOperations.Client.ViewModels;

namespace MembershipOperations.Client.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.LoginSucceeded += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            vm.Password = pb.Password;
    }
}
