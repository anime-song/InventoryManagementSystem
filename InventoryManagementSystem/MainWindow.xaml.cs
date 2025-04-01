using Reactive.Bindings;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace InventoryManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public MainWindow(INavigationService navigationService, ISnackbarService snackbarService)
        {
            InitializeComponent();

            navigationService.SetNavigationControl(RootNavigation);
            snackbarService.SetSnackbarPresenter(snackbarPresenter);
        }

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public bool Navigate(Type pageType, object? dataContext) => RootNavigation.Navigate(pageType, dataContext);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) =>
            RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}