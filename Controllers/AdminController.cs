using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SneakersPlanet.Models;

public class AdminController : Controller
{
    private readonly MongoDbContext _dbContext;

    public AdminController(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");
        if (user != null && user.AccountType == "admin")
        {
            return View("Index", user);
        }
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Orders()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            var orders = await _dbContext.Orders.Find(_ => true).ToListAsync();
            return View("Orders", orders);
        }

        return RedirectToAction("Index", "Login");
    }

    [Route("admin/orderdetails/{orderId}")]
    public async Task<IActionResult> OrderDetails(string orderId)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            var order = await _dbContext.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();

            if (order != null)
            {
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


    public IActionResult Users()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");
        if (user != null && user.AccountType == "admin")
        {
            var users = _dbContext.Users.Find(_ => true).ToList();
            return View("Users", users); 
        }
        return RedirectToAction("Index", "Home");
    }
}
