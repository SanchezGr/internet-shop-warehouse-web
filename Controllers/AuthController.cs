using InternetShopWarehouseWeb.Data;
using InternetShopWarehouseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InternetShopWarehouseWeb.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
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

            bool userExists = await _context.Users.AnyAsync(u => u.Login == login);

            if (userExists)
            {
                ViewBag.Error = "Користувач з таким логіном уже існує";
                return View();
            }

            var user = new User
            {
                FullName = fullName,
                Login = login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Guest"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserFullName", user.FullName);

            await AddLog(_context, "Зареєстровано нового користувача: " + user.Login);

            return RedirectToAction("AccessPending");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Введіть логін і пароль";
                return View();
            }

            login = login.Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Невірний логін або пароль";
                return View();
            }

            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserFullName", user.FullName);

            if (user.Role == "Guest")
            {
                return RedirectToAction("AccessPending");
            }

            return RedirectToAction("Index", "Products");
        }

        public async Task<IActionResult> AccessPending()
        {
            var userLogin = HttpContext.Session.GetString("UserLogin");

            if (string.IsNullOrEmpty(userLogin))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == userLogin);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserFullName", user.FullName);

            if (user.Role != "Guest")
            {
                return RedirectToAction("Index", "Products");
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}