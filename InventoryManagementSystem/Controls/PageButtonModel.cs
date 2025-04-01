using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.WPF.Controls
{
    public sealed class PageButtonModel
    {
        public int PageNumber { get; }
        public PaginationControlViewModel Root { get; }

        public PageButtonModel(int pageNumber, PaginationControlViewModel root)
        {
            PageNumber = pageNumber;
            Root = root;
        }

        public bool IsCurrent => Root.CurrentPage.Value == PageNumber;
    }
}