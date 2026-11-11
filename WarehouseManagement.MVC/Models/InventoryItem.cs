namespace WarehouseManagement.MVC.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        public string Product { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int WarehouseId { get; set; }
    }
}