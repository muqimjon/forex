namespace Forex.Wpf.Windows;
using System.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    private void btnUser_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Pages.Users.UserPage());
    }
}