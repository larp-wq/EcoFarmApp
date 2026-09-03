using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcoFarmApp.Models
{
    public class Sale
    {
        [Key]
        [DisplayName("ID Продажи")]
        public int SaleID { get; set; }

        [DisplayName("ID продукта")]
        [Required]
        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        [DisplayName("Продукт")]
        [ValidateNever] // важно: не валидируем навигационное свойство
        public virtual Product? Product { get; set; }

        [DisplayName("Количество")]
        [Required]
        public decimal Quantity { get; set; }

        [DisplayName("Имя покупателя")]
        [Required]
        public string? CustomerName { get; set; } = string.Empty;

        [DisplayName("Дата продажи")]
        [DataType(DataType.DateTime)]
        public DateTime SaleDate { get; set; } = DateTime.Now;
    }
}