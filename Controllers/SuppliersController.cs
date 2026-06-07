using InternetShopWarehouseWeb.Data;
using InternetShopWarehouseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShopWarehouseWeb.Controllers
{
    public class SuppliersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
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

            return View(await _context.Suppliers.ToListAsync());
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
        public async Task<IActionResult> Create(Supplier supplier)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            if (ModelState.IsValid)
            {
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                await AddLog(_context, "Додано постачальника: " + supplier.Name);

                TempData["Success"] = "Постачальника успішно додано.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                TempData["Error"] = "Постачальника не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            if (ModelState.IsValid)
            {
                var existingSupplier = await _context.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == supplier.Id);

                if (existingSupplier == null)
                {
                    TempData["Error"] = "Постачальника не знайдено.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();

                await AddLog(_context, "Оновлено постачальника: " + supplier.Name);

                TempData["Success"] = "Дані постачальника успішно оновлено.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                TempData["Error"] = "Постачальника не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            bool hasRequests = await _context.PurchaseRequests
                .AnyAsync(r => r.SupplierId == id);

            if (hasRequests)
            {
                TempData["Error"] = "Неможливо видалити постачальника, оскільки він використовується в заявках.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var access = CheckAccess("Manager", "Admin");
            if (access != null)
            {
                return access;
            }

            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                TempData["Error"] = "Постачальника не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            bool hasRequests = await _context.PurchaseRequests
                .AnyAsync(r => r.SupplierId == id);

            if (hasRequests)
            {
                TempData["Error"] = "Неможливо видалити постачальника, оскільки він використовується в заявках.";
                return RedirectToAction(nameof(Index));
            }

            var supplierName = supplier.Name;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            await AddLog(_context, "Видалено постачальника: " + supplierName);

            TempData["Success"] = "Постачальника успішно видалено.";
            return RedirectToAction(nameof(Index));
        }
    }
}