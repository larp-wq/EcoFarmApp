using System.ComponentModel;

namespace EcoFarmApp.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        [DisplayName("Название")]
        public string ProductName { get; set; } = string.Empty;

        [DisplayName("Цена")]
        public decimal Price { get; set; }

        [DisplayName("Остаток")]
        public decimal StockQuantity { get; set; }

        public int? InventoryItemId { get; set; }
    }
}