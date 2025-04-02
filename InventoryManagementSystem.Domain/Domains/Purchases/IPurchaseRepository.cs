using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Domain.Domains.Purchases
{
    public interface IPurchaseRepository
    {
        Purchase? FindById(int purchasedId);

        IEnumerable<Purchase> FindAll();

        Purchase Add(Purchase purchase);

        void Update(Purchase purchase);
    }
}