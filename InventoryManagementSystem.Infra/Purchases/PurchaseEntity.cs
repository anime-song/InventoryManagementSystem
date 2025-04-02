using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Purchases
{
    public sealed class PurchaseEntity
    {
        [BsonId]
        public int Id { get; set; }

        public int Status { get; set; }
        public int InventoryId { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}