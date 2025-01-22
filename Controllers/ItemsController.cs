using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SneakersPlanet.Models;
using System.Threading.Tasks;

namespace SneakersPlanet.Controllers
{
    public class ItemsController : Controller
    {
        private readonly MongoDbContext _context;

        public ItemsController(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category)
        {
            var items = new List<Item>();

            if (string.IsNullOrEmpty(category))
            {
                items = await _context.Items.Find(x => true).ToListAsync();
                ViewData["Category"] = "Wszystkie Produkty"; 
            }
            else
            {
                items = await _context.Items.Find(x => x.Category == category).ToListAsync();
                ViewData["Category"] = category; 
            }

            return View(items); 
        }

        public async Task<IActionResult> Details(string slug)
        {
            var item = await _context.Items.Find(x => x.Slug == slug).FirstOrDefaultAsync();
            if (item == null)
                return NotFound();
            
            return View(item);
        }

        [HttpPost]
        [Route("item/add-to-cart")]
        public async Task<IActionResult> AddToCart(string sneakerId, string size, string slug)
        {
            var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var item = await _context.Items.Find(x => x.Id == sneakerId).FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
            var selectedSize = item.Sizes.FirstOrDefault(s => s.Size == size);
            var remainingStock = selectedSize?.Quantity ?? 0;

            var cartItem = new CartItem
            {
                SneakerId = sneakerId,
                Name = item.Name,
                Size = size,
                Price = item.Price,
                ImageURL = item.ImageURL,
                RemainingStock = remainingStock
            };

            cart.Items.Add(cartItem);

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Details", new { slug });
        }

        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
            var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

            var totalValue = cart.Items.Sum(item => item.Price);

            if (user == null || string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) ||
                string.IsNullOrEmpty(user.PostalCode) || string.IsNullOrEmpty(user.City) || string.IsNullOrEmpty(user.Address))
            {
                ViewBag.ErrorMessage = "Przed zakupem wypełnij wszystkie dane w panelu użytkownika.";
            }

            var itemCount = cart.Items.Count;
            return View(new { Cart = cart, User = user, TotalValue = totalValue, ItemCount = itemCount });
        }

        [HttpPost]
        [Route("Items/ConfirmOrder")]
        public async Task<IActionResult> ConfirmOrder()
        {
            var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();

            var groupedCart = cart.Items
                .GroupBy(item => new { item.SneakerId, item.Size })
                .Select(group => new
                {
                    SneakerId = group.Key.SneakerId,
                    Size = group.Key.Size,
                    Quantity = group.Count()
                }).ToList();

            foreach (var itemGroup in groupedCart)
            {
                var item = await _context.Items.Find(x => x.Id == itemGroup.SneakerId).FirstOrDefaultAsync();
                var sizeInfo = item?.Sizes.FirstOrDefault(s => s.Size == itemGroup.Size);
                if (sizeInfo == null || sizeInfo.Quantity < itemGroup.Quantity)
                {
                    TempData["ErrorMessage"] = "Brak wystarczającej ilości przedmiotów w magazynie.";
                    return RedirectToAction("Cart");
                }
            }

            foreach (var itemGroup in groupedCart)
            {
                var item = await _context.Items.Find(x => x.Id == itemGroup.SneakerId).FirstOrDefaultAsync();
                var sizeInfo = item?.Sizes.FirstOrDefault(s => s.Size == itemGroup.Size);

                if (sizeInfo != null)
                {
                    sizeInfo.Quantity -= itemGroup.Quantity;

                    var filter = Builders<Item>.Filter.Eq(x => x.Id, itemGroup.SneakerId);
                    var update = Builders<Item>.Update.Set(x => x.Sizes, item.Sizes);
                    await _context.Items.UpdateOneAsync(filter, update);
                }
            }

            var order = new Order
            {
                UserId = user.Id,
                Items = groupedCart.Select(c => new OrderItem
                {
                    SneakerId = c.SneakerId,
                    Size = c.Size,
                    Quantity = c.Quantity
                }).ToList(),
                TotalValue = cart.Items.Sum(i => i.Price),
                OrderDate = DateTime.UtcNow
            };

            await _context.Orders.InsertOneAsync(order);

            HttpContext.Session.SetObjectAsJson("Cart", new Cart());

            return View("OrderConfirm", new { Cart = cart.Items, User = user, TotalValue = cart.Items.Sum(i => i.Price) });
        }

        [HttpPost]
        [Route("items/remove")]
        public IActionResult RemoveFromCart(string sneakerId)
        {
            var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();

            var itemToRemove = cart.Items.FirstOrDefault(item => item.SneakerId == sneakerId);
            
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Cart");
        }

    }
}
