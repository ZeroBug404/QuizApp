using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.Models.Auth;

namespace QuizApp.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<object> _hasher = new();

    public AccountController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var adminEmail = _config["Admin:Email"];
        var adminPassword = _config["Admin:Password"];
        var adminName = _config["Admin:DisplayName"] ?? "Admin";

        // Admin static account
        if (!string.IsNullOrWhiteSpace(adminEmail)
            && vm.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase)
            && vm.Password == adminPassword)
        {
            await SignInAsync("admin", adminName, adminEmail, role: "Admin");
            return RedirectAfterLogin(vm.ReturnUrl);
        }

        // Student lookup
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Email == vm.Email);
        if (student == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        var pwdResult = _hasher.VerifyHashedPassword(student, student.PasswordHash, vm.Password);
        if (pwdResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        await SignInAsync(student.Id.ToString(), student.FullName, student.Email, role: "Student");
        return RedirectAfterLogin(vm.ReturnUrl);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        bool emailExists = await _db.Students.AnyAsync(s => s.Email == vm.Email);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email already registered.");
            return View(vm);
        }

        var student = new Student
        {
            FullName = vm.FullName,
            Email = vm.Email,
        };
        student.PasswordHash = _hasher.HashPassword(student, vm.Password);

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        await SignInAsync(student.Id.ToString(), student.FullName, student.Email, role: "Student");
        return RedirectToAction("Index", "Quiz");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Quiz");
    }

    private async Task SignInAsync(string id, string name, string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
        return RedirectToAction("Index", "Quiz");
    }
}

