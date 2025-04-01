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

namespace InventoryManagementSystem.WPF.Controls
{
    /// <summary>
    /// PaginationControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PaginationControl : UserControl
    {
        #region VM依存関係プロパティ

        public PaginationControlViewModel VM
        {
            get => (PaginationControlViewModel)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register(nameof(VM), typeof(PaginationControlViewModel), typeof(PaginationControl),
                new FrameworkPropertyMetadata()
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is PaginationControl control && e.NewValue is PaginationControlViewModel vm)
                        {
                            control.DataContext = vm;
                        }
                    }
                });

        #endregion VM依存関係プロパティ

        public PaginationControl()
        {
            InitializeComponent();

            DataContext = VM;
        }
    }
}