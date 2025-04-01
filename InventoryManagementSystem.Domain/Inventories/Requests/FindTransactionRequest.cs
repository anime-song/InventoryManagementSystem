namespace InventoryManagementSystem.Domain.Inventories.Requests
{
    public sealed class FindTransactionRequest
    {
        public int? InventoryId { get; set; }
        public DateTime? TransactionPeriodStart { get; set; }
        public DateTime? TransactionPeriodEnd { get; set; }
    }
}