using System.ComponentModel.DataAnnotations;

namespace InternetShopWarehouseWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть ПІБ")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Введіть логін")]
        public string Login { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public string Role { get; set; } = "Guest";
    }
}