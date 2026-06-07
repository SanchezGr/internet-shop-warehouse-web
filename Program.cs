using InternetShopWarehouseWeb.Data;
using InternetShopWarehouseWeb.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any(u => u.Login == "admin"))
    {
        var admin = new User
        {
            FullName = "Адміністратор системи",
            Login = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin"
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.Use(async (context, next) =>
{
    var userLogin = context.Session.GetString("UserLogin");

    if (!string.IsNullOrEmpty(userLogin))
    {
        var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == userLogin);

        if (user == null)
        {
            context.Session.Clear();
            context.Response.Redirect("/Auth/Login");
            return;
        }

        context.Session.SetString("UserRole", user.Role);
        context.Session.SetString("UserFullName", user.FullName);
    }

    await next();
});
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");
app.Run();