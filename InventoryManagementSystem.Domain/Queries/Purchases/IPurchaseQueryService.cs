using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Queries.Purchases
{
    public interface IPurchaseQueryService
    {
        IEnumerable<PurchaseInventoryDTO> GetLatest(int fetchCount);
    }
}