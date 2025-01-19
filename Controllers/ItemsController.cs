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
    }
}
