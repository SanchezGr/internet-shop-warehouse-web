using InternetShopWarehouseWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShopWarehouseWeb.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> StockReport()
        {
            var access = CheckAccess("Seller", "Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var products = await _context.Products.ToListAsync();

            ViewBag.TotalProducts = products.Count;
            ViewBag.LowStockProducts = products.Count(p => p.Quantity < 5);
            ViewBag.TotalWarehouseValue = products.Sum(p => p.Quantity * p.Price);

            return View(products);
        }
    }
}