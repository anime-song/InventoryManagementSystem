using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Helpers
{
    public sealed class PageResult<T>
    {
        /// <summary>
        /// 現在のページインデックス
        /// </summary>
        public int CurrentPageIndex { get; }

        /// <summary>
        /// ページ数
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// 1ページ当たりのデータ数
        /// </summary>
        public int PageInDataCount { get; }

        /// <summary>
        /// 取得されたデータ
        /// </summary>
        public IEnumerable<T> PageData { get; }

        public PageResult(int currentPageIndex, int totalDataCount, int pageInDataCount, IEnumerable<T> pageData)
        {
            if (currentPageIndex < 0)
            {
                throw new ArgumentException("ページインデックスは0以上である必要があります");
            }
            if (pageInDataCount < 0)
            {
                throw new ArgumentException("データ取得数は0以上である必要があります");
            }

            CurrentPageIndex = currentPageIndex;
            TotalPages = (int)Math.Ceiling(totalDataCount / (float)pageInDataCount);
            PageInDataCount = pageInDataCount;
            PageData = pageData;
        }
    }
}