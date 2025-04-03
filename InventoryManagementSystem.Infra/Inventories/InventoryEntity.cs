using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Inventories
{
    public sealed class InventoryEntity
    {
        [BsonId]
        public int Id { get; set; }

        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int LocationId { get; set; }
        public DateTime RegisteredDate { get; set; }

        public int Status { get; set; }
        public int? ParentInventoryId { get; set; }
    }
}