namespace InventoryManagementSystem.Domain.Applications.Inventories.Requests
{
    public sealed class SplitInventoryRequest
    {
        public sealed class SplitInventoryItemRequest
        {
            public SplitInventoryItemRequest(string itemName, int quantity, int locationId)
            {
                ItemName = itemName;
                Quantity = quantity;
                LocationId = locationId;
            }

            public string ItemName { get; }
            public int Quantity { get; }
            public int LocationId { get; }
        }

        public int SourceInventoryId { get; }
        public IReadOnlyList<SplitInventoryItemRequest> Items { get; }

        public SplitInventoryRequest(int sourceInventoryId, IEnumerable<SplitInventoryItemRequest> items)
        {
            SourceInventoryId = sourceInventoryId;
            Items = items.ToList();
        }
    }
}