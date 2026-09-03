using System;
using System.ComponentModel; // Эту строку обязательно добавить для DisplayName
using System.ComponentModel.DataAnnotations; // Эту строку обязательно добавить для Key

namespace EcoFarmApp.Models
{
    public class Animal
    {
        [Key]
        [DisplayName("ID Животного")] // Добавил для консистентности
        public int AnimalID { get; set; }

        [DisplayName("Вид")]
        [Required]
        public string Species { get; set; } = null!;

        [DisplayName("Возраст")]
        public int Age { get; set; }

        [DisplayName("Состояние здоровья")]
        public string? HealthStatus { get; set; }

        [DisplayName("Последняя вакцинация")]
        public DateTime? LastVaccination { get; set; }
    }
}