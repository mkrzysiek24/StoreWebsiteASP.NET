using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;
using SneakersPlanet.Models;
using System.ComponentModel.DataAnnotations;

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

    [Route("admin/sneakers")]
    public async Task<IActionResult> Sneakers()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            try
            {
                var items = await _dbContext.Items.Find(i => i.Category == "Sneakers").ToListAsync();
                var viewModel = new CategoryItemsViewModel
                {
                    CategoryName = "Obuwie",
                    Items = items
                };
                return View("Sneakers", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching sneakers: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [Route("admin/clothes")]
    public async Task<IActionResult> Clothes()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            try
            {
                var items = await _dbContext.Items.Find(i => i.Category == "Ubrania").ToListAsync();
                var viewModel = new CategoryItemsViewModel
                {
                    CategoryName = "Ubrania",
                    Items = items
                };
                return View("Sneakers", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching clothes: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [Route("admin/accessories")]
    public async Task<IActionResult> Accessories()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            try
            {
                var items = await _dbContext.Items.Find(i => i.Category == "Akcesoria").ToListAsync();
                var viewModel = new CategoryItemsViewModel
                {
                    CategoryName = "Akcesoria",
                    Items = items
                };
                return View("Sneakers", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching accessories: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        return RedirectToAction("Index", "Home");
    }
    [Route("admin/items/edit/{itemId}")]
    public async Task<IActionResult> Edit(string itemId)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            try
            {
                var item = await _dbContext.Items.Find(i => i.Id == itemId).FirstOrDefaultAsync();
                if (item == null)
                {
                    return NotFound();
                }

                var viewModel = new EditItemViewModel
                {
                    Id = item.Id,
                    CategoryName = item.Category,
                    Brand = item.Brand,
                    Gender = item.Gender,
                    ImageURL = item.ImageURL,
                    Name = item.Name,
                    Price = item.Price,
                    Slug = item.Slug,
                    Sizes = item.Sizes.Select(s => new Size
                    {
                        SizeValue = s.Size,
                        Quantity = s.Quantity
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading item for edit: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Route("admin/items/edit/{itemId}")]
    public async Task<IActionResult> Edit(string itemId, EditItemViewModel model)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            try
            {
                var item = await _dbContext.Items.Find(i => i.Id == itemId).FirstOrDefaultAsync();
                if (item == null)
                {
                    return NotFound();
                }

                item.Brand = model.Brand;
                item.Gender = model.Gender;
                item.ImageURL = model.ImageURL;
                item.Name = model.Name;
                item.Price = model.Price;
                item.Slug = model.Slug;

                var sizes = model.Sizes.Select(s => new SizeValue
                {
                    Size = s.SizeValue,
                    Quantity = s.Quantity
                }).ToList();

                item.Sizes = sizes;

                var updateDefinition = Builders<Item>.Update
                    .Set(i => i.Brand, item.Brand)
                    .Set(i => i.Gender, item.Gender)
                    .Set(i => i.ImageURL, item.ImageURL)
                    .Set(i => i.Name, item.Name)
                    .Set(i => i.Price, item.Price)
                    .Set(i => i.Slug, item.Slug)
                    .Set(i => i.Sizes, item.Sizes);

                await _dbContext.Items.UpdateOneAsync(i => i.Id == itemId, updateDefinition);

                string categoryName = model.CategoryName;

                if (categoryName == "Sneakers")
                {
                    return RedirectToAction("Sneakers");
                }
                else if (categoryName == "Ubrania")
                {
                    return RedirectToAction("Clothes");
                }
                else if (categoryName == "Akcesoria")
                {
                    return RedirectToAction("Accessories");
                }
                else
                {
                    return RedirectToAction("Sneakers");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating item: " + ex.Message);
                return View(model);
            }
        }

        return RedirectToAction("Index", "Home");
    }
    [Route("admin/items/add/{category?}")]
    public IActionResult AddSneaker(string category = "Sneakers")
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            if (!string.IsNullOrEmpty(category) && category.Equals("Obuwie", StringComparison.OrdinalIgnoreCase))
            {
                category = "Sneakers";
            }
            ViewData["CategoryName"] = category;
            return View();
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Route("admin/items/add")]
    public async Task<IActionResult> AddSneaker(AddItemViewModel model)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            try
            {
                var sizes = GetPredefinedSizes(model.CategoryName);
                var newItem = new Item
                {
                    Brand = model.Brand,
                    Category = model.CategoryName,
                    Gender = model.Gender,
                    ImageURL = model.ImageURL,
                    Name = model.Name,
                    Price = model.Price,
                    Slug = model.Slug,
                    Sizes = sizes
                };

                await _dbContext.Items.InsertOneAsync(newItem);

                string redirectAction = model.CategoryName.ToLower() switch
                {
                    "ubrania" => "Clothes",
                    "akcesoria" => "Accessories",
                    _ => model.CategoryName
                };
                
                return RedirectToAction(redirectAction);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding new item: " + ex.Message);
                ModelState.AddModelError("", "Błąd podczas dodawania nowego przedmiotu. Spróbuj ponownie.");
                return View(model);
            }
        }

        return RedirectToAction("Index", "Home");
    }

    private List<SizeValue> GetPredefinedSizes(string category)
    {
        switch (category.ToLower())
        {
            case "sneakers":
                return Enumerable.Range(36, 11)
                    .Select(size => new SizeValue { Size = $"EU {size}", Quantity = 0 })
                    .ToList();

            case "ubrania":
                return new List<SizeValue>
                {
                    new SizeValue { Size = "XS", Quantity = 0 },
                    new SizeValue { Size = "S", Quantity = 0 },
                    new SizeValue { Size = "M", Quantity = 0 },
                    new SizeValue { Size = "L", Quantity = 0 },
                    new SizeValue { Size = "XL", Quantity = 0 }
                };

            case "akcesoria":
                return new List<SizeValue>
                {
                    new SizeValue { Size = "ONESIZE", Quantity = 0 }
                };

            default:
                throw new ArgumentException("Nieznana kategoria.");
        }
    }
    [HttpPost]
    [Route("admin/orders/delete/{orderId}")]
    public async Task<IActionResult> DeleteItem(string orderId)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            var order = await _dbContext.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();

            if (order != null)
            {
                await _dbContext.Orders.DeleteOneAsync(o => o.Id == orderId);
                return RedirectToAction("Orders");
            }

            return NotFound("Nie znaleziono zamówienia");
        }

        return RedirectToAction("Index", "Login");
    }
    [HttpPost]
    [Route("admin/items/delete/{itemId}")]
    public async Task<IActionResult> DeleteItem(string itemId, string CategoryName)
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null && user.AccountType == "admin")
        {
            var item = await _dbContext.Items.Find(o => o.Id == itemId).FirstOrDefaultAsync();

            if (item != null)
            {
                await _dbContext.Items.DeleteOneAsync(o => o.Id == itemId);
                if (CategoryName == "Ubrania")
                {
                    return RedirectToAction("Clothes");
                }
                else if (CategoryName == "Akcesoria")
                {
                    return RedirectToAction("Accessories");
                }
                else
                {
                    return RedirectToAction("Sneakers");
                }
            }

            return NotFound("Nie znaleziono przedmiotu");
        }

        return RedirectToAction("Index", "Login");
    }
    [Route("admin/users")]
    public async Task<IActionResult> Users()
    {
        var userSession = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (userSession != null && userSession.AccountType == "admin")
        {
            try
            {
                var users = await _dbContext.Users.Find(_ => true).ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching users: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Route("admin/users/{id}/delete")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var userSession = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (userSession != null && userSession.AccountType == "admin")
        {
            try
            {
                await _dbContext.Users.DeleteOneAsync(u => u.Id == id);
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting user: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Route("admin/ChangePassword/{userId}")]
    public async Task<IActionResult> ChangePassword(string userId)
    {
        var userSession = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (userSession != null && userSession.AccountType == "admin")
        {
            try
            {
                var user = await _dbContext.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound("User not found");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading user for password change: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Route("admin/ChangePassword/{userId}")]
    public async Task<IActionResult> ChangePassword(string userId, string newPassword, string confirmPassword)
    {
        var adminSession = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (adminSession == null || adminSession.AccountType != "admin")
        {
            TempData["ErrorMessage"] = "Nie masz uprawnień do zmiany hasła.";
            return RedirectToAction("Index", "Home");
        }

        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "Nowe hasło i potwierdzenie hasła muszą być identyczne.";
            return RedirectToAction("Users");
        }

        try
        {
            var user = await _dbContext.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "Użytkownik nie został znaleziony.";
                return RedirectToAction("Users");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _dbContext.Users.ReplaceOneAsync(u => u.Id == userId, user);

            TempData["SuccessMessage"] = "Hasło użytkownika zostało zmienione pomyślnie.";
            return RedirectToAction("Users");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error changing password: " + ex.Message);
            TempData["ErrorMessage"] = "Wystąpił błąd podczas zmiany hasła.";
            return RedirectToAction("Users");
        }
    }


}