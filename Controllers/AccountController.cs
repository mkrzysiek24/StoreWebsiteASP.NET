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

    public IActionResult Details()
    {
        var user = HttpContext.Session.GetObjectFromJson<User>("UserSession");

        if (user != null)
        {
            return View("Details", user);
        }

        return RedirectToAction("Index", "Login");
    }
}
