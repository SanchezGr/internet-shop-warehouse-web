using InternetShopWarehouseWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace InternetShopWarehouseWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
    }
}