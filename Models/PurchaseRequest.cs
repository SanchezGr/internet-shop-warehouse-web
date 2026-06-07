using System.ComponentModel.DataAnnotations;

namespace InternetShopWarehouseWeb.Models
{
    public class PurchaseRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Оберіть товар")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть товар")]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required(ErrorMessage = "Оберіть постачальника")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть постачальника")]
        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Кількість має бути більшою за 0")]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Оберіть статус заявки")]
        public string Status { get; set; } = "Нова";

        public bool IsApplied { get; set; } = false;
    }
}