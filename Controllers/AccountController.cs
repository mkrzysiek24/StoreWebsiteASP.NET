using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SneakersPlanet.Models;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly MongoDbContext _dbContext;

    public AccountController(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            if (user.AccountType == "admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            return View("Account", user);
        }

        return RedirectToAction("Index", "Login");
    }

    [HttpGet]
    public IActionResult Details()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            return View("Details", user);
        }

        return RedirectToAction("Index", "Login");
    }

    public async Task<IActionResult> Orders()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            var orders = await _dbContext.Orders.Find(o => o.UserId == user.Id).ToListAsync();

            return View("Orders", orders);
        }

        return RedirectToAction("Index", "Login");
    }

    [Route("account/orderdetails/{orderId}")]
    public async Task<IActionResult> OrderDetails(string orderId)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            var order = await _dbContext.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();

            if (order != null)
            {
                
                if (order.UserId != user.Id)
                {
                    return Unauthorized();
                }
                var orderUser = await _dbContext.Users.Find(u => u.Id == order.UserId).FirstOrDefaultAsync();

                var itemIds = order.Items.Select(i => i.SneakerId).ToList();
                var items = await _dbContext.Items.Find(i => itemIds.Contains(i.Id)).ToListAsync();

                var viewModel = new OrderDetailsViewModel
                {
                    OrderId = order.Id,
                    UserName = $"{orderUser.FirstName} {orderUser.LastName}",
                    UserAddress = orderUser.Address,
                    UserPostal = orderUser.PostalCode,
                    UserCity = orderUser.City,
                    OrderDate = order.OrderDate,
                    TotalValue = order.TotalValue,
                    Items = order.Items.Select(i => new OrderItemDetails
                    {
                        ProductName = items.FirstOrDefault(p => p.Id == i.SneakerId)?.Name ?? "Produkt nieznany",
                        ProductImage = items.FirstOrDefault(p => p.Id == i.SneakerId)?.ImageURL ?? "",
                        Size = i.Size,
                        Quantity = i.Quantity,
                        Price = items.FirstOrDefault(p => p.Id == i.SneakerId)?.Price ?? 0
                    }).ToList()
                };

                return View("OrderDetails", viewModel);
            }

            return NotFound("Nie znaleziono zamówienia");
        }

        return RedirectToAction("Index", "Login");
    }

    public IActionResult Adress()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            return View("Adress", user);
        }

        return RedirectToAction("Index", "Login");
    }

    [HttpPost]
    public async Task<IActionResult> Adress(User updatedUser)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            var dbUser = await _dbContext.Users.Find(u => u.Id == user.Id).FirstOrDefaultAsync();

            if (dbUser != null)
            {
                dbUser.FirstName = updatedUser.FirstName;
                dbUser.LastName = updatedUser.LastName;
                dbUser.PostalCode = updatedUser.PostalCode;
                dbUser.City = updatedUser.City;
                dbUser.Address = updatedUser.Address;

                await _dbContext.Users.ReplaceOneAsync(u => u.Id == user.Id, dbUser);

                HttpContext.Session.SetObjectAsJson("UserSession", dbUser);

                TempData["SuccessMessage"] = "Dane zostały zaktualizowane.";
                return RedirectToAction("Adress");
            }

            return NotFound("Nie znaleziono użytkownika.");
        }

        return RedirectToAction("Index", "Login");
    }


    [HttpPost]
    [Route("account/orders/delete/{orderId}")]
    public async Task<IActionResult> DeleteOrder(string orderId)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            var order = await _dbContext.Orders.Find(o => o.Id == orderId && o.UserId == user.Id).FirstOrDefaultAsync();

            if (order != null)
            {
                if (order.UserId != user.Id)
                {
                    return Unauthorized();
                }
                await _dbContext.Orders.DeleteOneAsync(o => o.Id == orderId);
                return RedirectToAction("Orders");
            }

            return NotFound("Nie znaleziono zamówienia");
        }

        return RedirectToAction("Index", "Login");
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user == null)
        {
            TempData["ErrorMessage"] = "Nie jesteś zalogowany.";
            return RedirectToAction("Index", "Login");
        }

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
        {
            TempData["ErrorMessage"] = "Błędne hasło.";
            return RedirectToAction("Details");
        }

        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "Nowe hasło i potwierdzenie hasła muszą być identyczne.";
            return RedirectToAction("Details");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _dbContext.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

        TempData["SuccessMessage"] = "Hasło zostało zmienione pomyślnie.";
        return RedirectToAction("Details");
    }
}
