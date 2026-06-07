using InternetShopWarehouseWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShopWarehouseWeb.Controllers
{
    public class LogsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category)
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            var logs = _context.ActionLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                if (category == "register")
                {
                    logs = logs.Where(l => l.Action.Contains("Зареєстровано"));
                }
                else if (category == "products")
                {
                    logs = logs.Where(l => l.Action.Contains("товар") || l.Action.Contains("Товар"));
                }
                else if (category == "suppliers")
                {
                    logs = logs.Where(l => l.Action.Contains("постачальник") || l.Action.Contains("Постачальник"));
                }
                else if (category == "requests")
                {
                    logs = logs.Where(l => l.Action.Contains("заявк") || l.Action.Contains("Заявк"));
                }
                else if (category == "users")
                {
                    logs = logs.Where(l =>
                        l.Action.Contains("користувач") ||
                        l.Action.Contains("Користувач") ||
                        l.Action.Contains("роль") ||
                        l.Action.Contains("Роль"));
                }
            }

            ViewBag.SelectedCategory = category;

            var result = await logs
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(result);
        }
    }
}