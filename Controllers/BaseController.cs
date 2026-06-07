using InternetShopWarehouseWeb.Data;
using InternetShopWarehouseWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternetShopWarehouseWeb.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult? CheckAccess(params string[] allowedRoles)
        {
            var userLogin = HttpContext.Session.GetString("UserLogin");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userLogin))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (userRole == "Guest")
            {
                return RedirectToAction("AccessPending", "Auth");
            }

            if (!allowedRoles.Contains(userRole))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return null;
        }

        protected async Task AddLog(ApplicationDbContext context, string action)
        {
            var userLogin = HttpContext.Session.GetString("UserLogin") ?? "Невідомо";
            var userRole = HttpContext.Session.GetString("UserRole") ?? "Невідомо";

            var log = new ActionLog
            {
                UserLogin = userLogin,
                UserRole = userRole,
                Action = action,
                CreatedAt = DateTime.Now
            };

            context.ActionLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}