using InventoryManagementSystem.Domain.Applications.Inventories.Requests;
using InventoryManagementSystem.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Domains.Inventories
{
    public interface IInventoryTransactionRepository
    {
        IEnumerable<InventoryTransaction> FindAll();

        PageResult<InventoryTransaction> FindWithPaginate(FindTransactionRequest request, int page, int fetchCount);

        InventoryTransaction? FindById(int id);

        IEnumerable<InventoryTransaction> Find(FindTransactionRequest request);

        void Add(InventoryTransaction transaction);
    }
}