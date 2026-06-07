using System.ComponentModel.DataAnnotations;

namespace InternetShopWarehouseWeb.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву постачальника")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Введіть контактну особу")]
        public string ContactPerson { get; set; } = "";

        [Required(ErrorMessage = "Введіть телефон")]
        [Phone(ErrorMessage = "Некоректний формат телефону")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        public string Email { get; set; } = "";
    }
}