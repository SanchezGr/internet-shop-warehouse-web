using System.ComponentModel.DataAnnotations;

namespace InternetShopWarehouseWeb.Models
{
    public class ActionLog
    {
        public int Id { get; set; }

        [Required]
        public string UserLogin { get; set; } = "";

        [Required]
        public string UserRole { get; set; } = "";

        [Required]
        public string Action { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}