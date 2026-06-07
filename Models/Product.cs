using System.ComponentModel.DataAnnotations;

namespace InternetShopWarehouseWeb.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву товару")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Введіть категорію товару")]
        public string Category { get; set; } = "";

        [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути від’ємною")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна має бути більшою за 0")]
        public decimal Price { get; set; }
    }
}