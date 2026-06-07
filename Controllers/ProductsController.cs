using InternetShopWarehouseWeb.Data;
using InternetShopWarehouseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShopWarehouseWeb.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var access = CheckAccess("Seller", "Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var allProducts = await _context.Products.ToListAsync();

            ViewBag.TotalProducts = allProducts.Count;
            ViewBag.LowStockProducts = allProducts.Count(p => p.Quantity < 5);
            ViewBag.TotalWarehouseValue = allProducts.Sum(p => p.Quantity * p.Price);
            ViewBag.Search = search;

            var products = allProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return View(products.ToList());
        }

        public IActionResult Create()
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                await AddLog(_context, "Додано товар: " + product.Name);

                TempData["Success"] = "Товар успішно додано.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Товар не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct == null)
                {
                    TempData["Error"] = "Товар не знайдено.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                await AddLog(_context, "Оновлено товар: " + product.Name);

                TempData["Success"] = "Дані товару успішно оновлено.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Товар не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            bool hasRequests = await _context.PurchaseRequests
                .AnyAsync(r => r.ProductId == id);

            if (hasRequests)
            {
                TempData["Error"] = "Неможливо видалити товар, оскільки він використовується в заявках.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                TempData["Error"] = "Товар не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            bool hasRequests = await _context.PurchaseRequests
                .AnyAsync(r => r.ProductId == id);

            if (hasRequests)
            {
                TempData["Error"] = "Неможливо видалити товар, оскільки він використовується в заявках.";
                return RedirectToAction(nameof(Index));
            }

            var productName = product.Name;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await AddLog(_context, "Видалено товар: " + productName);

            TempData["Success"] = "Товар успішно видалено.";
            return RedirectToAction(nameof(Index));
        }
    }
}