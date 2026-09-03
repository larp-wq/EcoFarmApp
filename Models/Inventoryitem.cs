using System.ComponentModel; // Эту строку обязательно добавить
using System.ComponentModel.DataAnnotations; // Эту строку обязательно добавить
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFarmApp.Models
{
    public class InventoryItem
    {
        [Key]
        [DisplayName("ID Склада")]
        public int ItemID { get; set; }

        [DisplayName("Наименование")]
        [Required]
        public string ItemName { get; set; } = null!;

        [DisplayName("Количество")]
        public decimal Quantity { get; set; } = 0m;

        [DisplayName("Единица измерения")]
        [Required]
        public string Unit { get; set; } = null!;
    }
}