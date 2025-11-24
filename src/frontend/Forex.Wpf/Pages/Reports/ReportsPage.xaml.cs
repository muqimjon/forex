namespace Forex.Wpf.Pages.Reports
{
    using Forex.Wpf.Pages.Home;
    using Forex.Wpf.Pages.Reports.ViewModels;
    using Forex.Wpf.Windows;
    using Microsoft.Extensions.DependencyInjection;
    using System.Windows;
    using System.Windows.Controls;

    public partial class ReportsPage : Page
    {
        private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
        private bool _isInitialized = false;

        public ReportsPage()
        {
            InitializeComponent();

            DataContext = App.AppHost!.Services.GetRequiredService<ReportsPageViewModel>();

            // Boshlang'ich flag
            _isInitialized = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                Main.NavigateTo(new HomePage());
        }

        private bool _finishedStockLoaded = false;



        //private async void ReportsTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count == 0 || !_isInitialized) return;

        //    if (DataContext is not ReportsPageViewModel vm) return;

        //    var selectedIndex = ReportsTab.SelectedIndex;

        //    // Faqat bir marta yuklash!
        //    switch (selectedIndex)
        //    {
        //        case 0 when !vm.SalesHistoryVM.IsDataLoaded:
        //            await vm.SalesHistoryVM.LoadAsync();
        //            vm.SalesHistoryVM.IsDataLoaded = true;
        //            break;
        //        case 1 when !vm.FinishedStockVM.IsDataLoaded:
        //            await vm.FinishedStockVM.LoadAsync();
        //            vm.FinishedStockVM.IsDataLoaded = true;
        //            break;
        //            // qolganlari ham shunday
        //    }
        //}
    }
}
