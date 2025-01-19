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
