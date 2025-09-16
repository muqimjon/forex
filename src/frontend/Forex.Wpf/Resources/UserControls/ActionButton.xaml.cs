namespace Forex.Wpf.Resources.UserControls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

/// <summary>
/// Interaction logic for ActionButton.xaml
/// </summary>
public partial class ActionButton : UserControl
{
    public ActionButton()
    {
        InitializeComponent();
        ActionPopup.Closed += (_, _) =>
        {
            DotsButton.Visibility = Visibility.Visible;
        };
    }

    private void DotsButton_Click(object sender, RoutedEventArgs e)
    {

        ActionPopup.IsOpen = true;
        DotsButton.Visibility = Visibility.Hidden;

        // Scale animatsiya
        var scaleAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        stackScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        stackScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

        // Translate (chapga surish) animatsiya
        var moveAnim = new DoubleAnimation
        {
            From = 20,   // Boshlanish joyi (20px o‘ngda)
            To = 0,      // Asl joyiga qaytadi
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        stackTranslate.BeginAnimation(TranslateTransform.XProperty, moveAnim);
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Edit bosildi");
        ActionPopup.IsOpen = false;
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Delete bosildi");
        ActionPopup.IsOpen = false;
    }
}
