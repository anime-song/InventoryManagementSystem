using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Abstractions.Controls;

namespace InventoryManagementSystem.WPF.Purchases
{
    /// <summary>
    /// PurchaseRegisterPage.xaml の相互作用ロジック
    /// </summary>
    public partial class PurchaseRegisterPage : INavigableView<PurchaseRegisterViewModel>
    {
        public PurchaseRegisterViewModel ViewModel { get; }

        public PurchaseRegisterPage(PurchaseRegisterViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}