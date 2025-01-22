using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SneakersPlanet.Models;
using System.Threading.Tasks;

public class LoginController : Controller
{
    private readonly MongoDbContext _dbContext;

    public LoginController(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string email, string password)
    {
        var user = await _dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            ViewData["ErrorMessage"] = "Niepoprawne dane logowania";
            return View();
        }

        HttpContext.Session.SetObjectAsJson("UserSession", user);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string firstName, string lastName, string email, string password)
    {
        var existingUser = await _dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            ViewData["ErrorMessage"] = "Użytkownik z tym emailem już istnieje";
            return View();
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var newUser = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = hashedPassword,
            PostalCode = "",
            City = "",
            Address = "",
            AccountType = "user"
        };

        await _dbContext.Users.InsertOneAsync(newUser);

        HttpContext.Session.SetObjectAsJson("UserSession", newUser);

        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
