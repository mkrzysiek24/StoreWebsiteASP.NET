using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SneakersPlanet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SneakersPlanet.Controllers
{
    public class SearchController : Controller
    {
        private readonly MongoDbContext _context;

        public SearchController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string query)
        {
            List<Item> items;

            if (!string.IsNullOrEmpty(query))
            {
                items = await _context.Items.Find(x =>
                    x.Name.ToLower().Contains(query.ToLower()) ||
                    x.Category.ToLower().Contains(query.ToLower()) ||
                    x.Brand.ToLower().Contains(query.ToLower())
                ).ToListAsync();
            }
            else
            {
                items = await _context.Items.Find(_ => true).ToListAsync();
            }

            ViewData["Query"] = query;
            return View(items);
        }
    }
}
