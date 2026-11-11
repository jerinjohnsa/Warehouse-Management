namespace WarehouseManagement
{
    public class TransferRequest
    {
        public string Product { get; set; } = string.Empty;
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int Quantity { get; set; }
    }
}