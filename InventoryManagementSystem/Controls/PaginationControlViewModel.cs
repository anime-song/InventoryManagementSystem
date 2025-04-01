using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventoryManagementSystem.WPF.Controls
{
    public sealed class PaginationControlViewModel
    {
        public int FixedPageAreaWidth { get; }

        public ReactiveProperty<int> CurrentPage { get; } = new ReactiveProperty<int>(1);
        public ReactiveProperty<int> LastPage { get; } = new ReactiveProperty<int>(5);
        public ReactiveProperty<int> DisplayRange { get; } = new ReactiveProperty<int>(5);
        public ReactiveProperty<bool> IsLastPage { get; } = new ReactiveProperty<bool>(false);
        public ReactiveCollection<PageButtonModel> CenterPages { get; } = new ReactiveCollection<PageButtonModel>();

        public ReactiveProperty<bool> ShowLeftEllipsis { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> ShowRightEllipsis { get; } = new ReactiveProperty<bool>();

        public ReactiveCommand MovePrevCommand { get; }
        public ReactiveCommand MoveNextCommand { get; }
        public ReactiveCommand<int> MoveToPageCommand { get; }

        public ICommand? PageChangedCommand { get; set; }

        public PaginationControlViewModel()
        {
            MovePrevCommand = CurrentPage
                .Select(x => x > 1)
                .ToReactiveCommand()
                .WithSubscribe(() => MoveToPage(CurrentPage.Value - 1));
            MoveNextCommand = CurrentPage
                .Select(x => x < LastPage.Value)
                .ToReactiveCommand()
                .WithSubscribe(() => MoveToPage(CurrentPage.Value + 1));
            MoveToPageCommand = new ReactiveCommand<int>()
                .WithSubscribe(MoveToPage);

            new[]
            {
                CurrentPage.PropertyChangedAsObservable(),
                LastPage.PropertyChangedAsObservable()
            }
            .Merge()
            .Subscribe(_ =>
            {
                ShowLeftEllipsis.Value = CurrentPage.Value > (1 + DisplayRange.Value / 2 + 1);
                ShowRightEllipsis.Value = CurrentPage.Value < (LastPage.Value - DisplayRange.Value / 2 - 1);
            });

            LastPage.Subscribe(_ =>
            {
                UpdateCenterPages();
            });

            FixedPageAreaWidth = DisplayRange.Value * (40 + 4);

            UpdateCenterPages();
        }

        private void MoveToPage(int page)
        {
            if (page < 1 || page > LastPage.Value || page == CurrentPage.Value)
            {
                return;
            }

            CurrentPage.Value = page;
            IsLastPage.Value = page == LastPage.Value;
            UpdateCenterPages();
            CurrentPage.ForceNotify();

            PageChangedCommand?.Execute(page);
        }

        private void UpdateCenterPages()
        {
            CenterPages.Clear();

            int half = DisplayRange.Value / 2;
            int start = CurrentPage.Value - half;
            int end = CurrentPage.Value + half; // LastPageは表示しないため

            if (start < 2)
            {
                end += (2 - start);
                start = 2;
            }
            if (end > LastPage.Value - 1)
            {
                start -= (end - (LastPage.Value - 1));
                end = LastPage.Value - 1;
            }

            if (start < 2)
            {
                start = 2;
            }

            for (int i = start; i <= end; i++)
            {
                CenterPages.Add(new PageButtonModel(i, this));
            }
        }
    }
}