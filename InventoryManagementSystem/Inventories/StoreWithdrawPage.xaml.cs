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

namespace InventoryManagementSystem.WPF.Inventories
{
    /// <summary>
    /// StoreWithdrawPage.xaml の相互作用ロジック
    /// </summary>
    public partial class StoreWithdrawPage : INavigableView<StoreWithdrawViewModel>
    {
        public StoreWithdrawViewModel ViewModel { get; }

        public StoreWithdrawPage(StoreWithdrawViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}