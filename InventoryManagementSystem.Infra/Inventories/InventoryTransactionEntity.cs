using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Inventories
{
    public sealed class InventoryTransactionEntity
    {
        [BsonId]
        public int Id { get; set; }

        public int TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Quantity { get; set; }
        public int InventoryId { get; set; }

        public int? CanceledTransactionId { get; set; }
    }
}