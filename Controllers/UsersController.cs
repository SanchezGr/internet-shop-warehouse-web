using InternetShopWarehouseWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InternetShopWarehouseWeb.Controllers
{
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string fullName, string login, string password, string role)
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Заповніть усі поля";
                return View();
            }

            fullName = fullName.Trim();
            login = login.Trim();

            if (fullName.Length < 3 || !fullName.Any(char.IsLetter))
            {
                ViewBag.Error = "ПІБ має містити щонайменше 3 символи та хоча б одну літеру.";
                return View();
            }

            if (login.Length < 3)
            {
                ViewBag.Error = "Логін має містити щонайменше 3 символи.";
                return View();
            }

            if (!login.Any(char.IsLetter))
            {
                ViewBag.Error = "Логін має містити хоча б одну літеру.";
                return View();
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9_]+$"))
            {
                ViewBag.Error = "Логін може містити тільки латинські літери, цифри та символ _.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Пароль має містити щонайменше 6 символів";
                return View();
            }

            if (!password.Any(char.IsLetter))
            {
                ViewBag.Error = "Пароль має містити хоча б одну літеру";
                return View();
            }

            if (!password.Any(char.IsDigit))
            {
                ViewBag.Error = "Пароль має містити хоча б одну цифру";
                return View();
            }

            var allowedRoles = new[] { "Guest", "Seller", "Manager", "Admin" };

            if (!allowedRoles.Contains(role))
            {
                ViewBag.Error = "Обрано некоректну роль";
                return View();
            }

            bool userExists = await _context.Users.AnyAsync(u => u.Login == login);

            if (userExists)
            {
                ViewBag.Error = "Користувач з таким логіном уже існує";
                return View();
            }

            var user = new InternetShopWarehouseWeb.Models.User
            {
                FullName = fullName,
                Login = login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await AddLog(_context, "Адміністратор створив користувача: " +
                                   user.Login +
                                   " з роллю " +
                                   user.Role);

            TempData["Success"] = "Користувача успішно створено.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditRole(int id)
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            var currentLogin = HttpContext.Session.GetString("UserLogin");

            if (user.Login == currentLogin)
            {
                TempData["Error"] = "Ви не можете змінити роль власного облікового запису.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(int id, string role)
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            var allowedRoles = new[] { "Guest", "Seller", "Manager", "Admin" };

            if (!allowedRoles.Contains(role))
            {
                TempData["Error"] = "Обрано некоректну роль.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            var currentLogin = HttpContext.Session.GetString("UserLogin");

            if (user.Login == currentLogin)
            {
                TempData["Error"] = "Ви не можете змінити роль власного облікового запису.";
                return RedirectToAction(nameof(Index));
            }

            var oldRole = user.Role;

            user.Role = role;
            await _context.SaveChangesAsync();

            await AddLog(_context, "Змінено роль користувача " +
                                   user.Login +
                                   " з " +
                                   oldRole +
                                   " на " +
                                   role);

            TempData["Success"] = "Роль користувача успішно змінено.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            var currentLogin = HttpContext.Session.GetString("UserLogin");

            if (user.Login == currentLogin)
            {
                TempData["Error"] = "Ви не можете видалити власний обліковий запис.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var access = CheckAccess("Admin");
            if (access != null)
            {
                return access;
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            var currentLogin = HttpContext.Session.GetString("UserLogin");

            if (user.Login == currentLogin)
            {
                TempData["Error"] = "Ви не можете видалити власний обліковий запис.";
                return RedirectToAction(nameof(Index));
            }

            var deletedUserLogin = user.Login;
            var deletedUserRole = user.Role;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await AddLog(_context, "Видалено користувача: " +
                                   deletedUserLogin +
                                   " з роллю " +
                                   deletedUserRole);

            TempData["Success"] = "Користувача успішно видалено.";
            return RedirectToAction(nameof(Index));
        }
    }
}