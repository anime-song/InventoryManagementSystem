using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Queries.Inventories
{
    public interface IInventoryQueryService
    {
        /// <summary>
        /// 親がいる在庫を除いてfetchCountだけの在庫を取得します
        /// </summary>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        public IEnumerable<InventoryTreeModel> GetLatest(int fetchCount);
    }
}