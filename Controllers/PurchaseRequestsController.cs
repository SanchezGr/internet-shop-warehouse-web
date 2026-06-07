using InternetShopWarehouseWeb.Data;
using InternetShopWarehouseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InternetShopWarehouseWeb.Controllers
{
    public class PurchaseRequestsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public PurchaseRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var requests = await _context.PurchaseRequests
                .Include(r => r.Product)
                .Include(r => r.Supplier)
                .ToListAsync();

            return View(requests);
        }

        public IActionResult Create()
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseRequest request)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            if (ModelState.IsValid)
            {
                request.CreatedAt = DateTime.Now;
                request.Status = "Нова";
                request.IsApplied = false;

                _context.PurchaseRequests.Add(request);
                await _context.SaveChangesAsync();

                var product = await _context.Products.FindAsync(request.ProductId);
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId);

                await AddLog(_context, "Створено заявку на закупівлю: " +
                    (product?.Name ?? "товар не знайдено") +
                    ", постачальник: " +
                    (supplier?.Name ?? "постачальник не знайдений") +
                    ", кількість: " +
                    request.Quantity);

                TempData["Success"] = "Заявку на закупівлю успішно створено.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = new SelectList(_context.Products, "Id", "Name", request.ProductId);
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name", request.SupplierId);

            return View(request);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var request = await _context.PurchaseRequests.FindAsync(id);

            if (request == null)
            {
                TempData["Error"] = "Заявку не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            if (request.IsApplied || request.Status == "Виконано")
            {
                TempData["Error"] = "Виконану заявку не можна редагувати.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = new SelectList(_context.Products, "Id", "Name", request.ProductId);
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name", request.SupplierId);

            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PurchaseRequest request)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            if (ModelState.IsValid)
            {
                var existingRequest = await _context.PurchaseRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (existingRequest == null)
                {
                    TempData["Error"] = "Заявку не знайдено.";
                    return RedirectToAction(nameof(Index));
                }

                if (existingRequest.IsApplied || existingRequest.Status == "Виконано")
                {
                    TempData["Error"] = "Виконану заявку не можна редагувати.";
                    return RedirectToAction(nameof(Index));
                }

                request.CreatedAt = existingRequest.CreatedAt;
                request.Status = "Нова";
                request.IsApplied = false;

                _context.PurchaseRequests.Update(request);
                await _context.SaveChangesAsync();

                var product = await _context.Products.FindAsync(request.ProductId);
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId);

                await AddLog(_context, "Оновлено заявку на закупівлю: " +
                    (product?.Name ?? "товар не знайдено") +
                    ", постачальник: " +
                    (supplier?.Name ?? "постачальник не знайдений") +
                    ", кількість: " +
                    request.Quantity);

                TempData["Success"] = "Заявку успішно оновлено.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = new SelectList(_context.Products, "Id", "Name", request.ProductId);
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Name", request.SupplierId);

            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var request = await _context.PurchaseRequests
                .Include(r => r.Product)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Заявку не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status == "Нова" && !request.IsApplied)
            {
                var product = await _context.Products.FindAsync(request.ProductId);

                if (product == null)
                {
                    TempData["Error"] = "Товар для цієї заявки не знайдено.";
                    return RedirectToAction(nameof(Index));
                }

                product.Quantity += request.Quantity;

                request.Status = "Виконано";
                request.IsApplied = true;

                await _context.SaveChangesAsync();

                await AddLog(_context, "Виконано заявку на закупівлю: " +
                    product.Name +
                    ", кількість додано: " +
                    request.Quantity);

                TempData["Success"] = "Заявку виконано. Кількість товару на складі збільшено.";
            }
            else
            {
                TempData["Error"] = "Ця заявка вже була виконана.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var request = await _context.PurchaseRequests
                .Include(r => r.Product)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Заявку не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var request = await _context.PurchaseRequests
                .Include(r => r.Product)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Заявку не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            var productName = request.Product?.Name ?? "товар не знайдено";
            var supplierName = request.Supplier?.Name ?? "постачальник не знайдений";
            var quantity = request.Quantity;

            _context.PurchaseRequests.Remove(request);
            await _context.SaveChangesAsync();

            await AddLog(_context, "Видалено заявку на закупівлю: " +
                productName +
                ", постачальник: " +
                supplierName +
                ", кількість: " +
                quantity);

            TempData["Success"] = "Заявку успішно видалено.";

            return RedirectToAction(nameof(Index));
        }
    }
}