using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PulsePoint.Areas.Security.Helpers;
using PulsePoint.Areas.Security.Models;
using PulsePoint.Infrastructure.Data;

namespace PulsePoint.Areas.Security.Controllers;

[Area("Security")]
public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IJwtHelper _jwtHelper;

    public AuthController(AppDbContext db, IJwtHelper jwtHelper)
    {
        _db = db;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Displays the visual Hospital Login screen with branch selection
    /// GET: /Security/Auth/Login
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Dashboard" });
        }

        var model = new LoginViewModel
        {
            Locations = await GetActiveLocationSelectList()
        };
        return View(model);
    }

    /// <summary>
    /// Handles visual Form Login submission with BCrypt verification and Facility claims
    /// POST: /Security/Auth/Login
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Locations = await GetActiveLocationSelectList();
            return View(model);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

        if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
        {
            model.ErrorMessage = "Invalid username or password.";
            model.Locations = await GetActiveLocationSelectList();
            return View(model);
        }

        if (user.PasswordExpiry.HasValue && user.PasswordExpiry.Value <= DateTime.UtcNow)
        {
            model.ErrorMessage = "Your account password has expired. Please contact your system administrator.";
            model.Locations = await GetActiveLocationSelectList();
            return View(model);
        }

        var location = await _db.Locations
            .FirstOrDefaultAsync(l => l.Id == model.LocationId && l.IsActive);

        if (location == null)
        {
            model.ErrorMessage = "The selected hospital facility branch is inactive or invalid.";
            model.Locations = await GetActiveLocationSelectList();
            return View(model);
        }

        // 1. Generate JWT Token
        var token = _jwtHelper.GenerateJwtToken(user, location);

        // 2. Build Claims Principal for MVC session
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.RoleId.ToString()),
            new("LocationId", location.Id.ToString()),
            new("LocationTypeId", location.LocationTypeId.ToString()),
            new("LocationName", location.LocationName),
            new("JwtToken", token)
        };

        var identity = new ClaimsIdentity(claims, "PulsePointCookieAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("PulsePointCookieAuth", principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

        // 3. Set JWT and metadata cookies for client-side scripts & Angular
        Response.Cookies.Append("hms_token", token, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        Response.Cookies.Append("hms_branch", location.LocationName, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        return RedirectToAction("Index", "Dashboard", new { area = "Dashboard" });
    }

    /// <summary>
    /// JSON Web API Login endpoint for Angular/SPA clients
    /// POST: /api/auth/login
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [Route("api/auth/login")]
    public async Task<IActionResult> ApiLogin([FromBody] LoginApiRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var location = await _db.Locations
            .FirstOrDefaultAsync(l => l.Id == request.LocationId && l.IsActive);

        if (location == null)
        {
            return BadRequest(new { message = "Invalid or inactive hospital branch." });
        }

        var token = _jwtHelper.GenerateJwtToken(user, location);

        return Ok(new
        {
            token,
            tokenType = "Bearer",
            expiresIn = 28800,
            user = new
            {
                id = user.Id,
                username = user.Username,
                fullName = user.FullName,
                roleId = user.RoleId
            },
            location = new
            {
                id = location.Id,
                name = location.LocationName,
                typeId = location.LocationTypeId
            }
        });
    }

    /// <summary>
    /// Returns current authenticated user and session info from JWT or Cookie
    /// GET: /api/auth/me
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("api/auth/me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            fullName = User.FindFirst(ClaimTypes.Name)?.Value,
            username = User.FindFirst("Username")?.Value ?? User.Identity?.Name,
            roleId = User.FindFirst(ClaimTypes.Role)?.Value,
            locationId = User.FindFirst("LocationId")?.Value,
            locationName = User.FindFirst("LocationName")?.Value,
            isAuthenticated = User.Identity?.IsAuthenticated == true
        });
    }

    /// <summary>
    /// Sign out action
    /// GET: /Security/Auth/Logout
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("PulsePointCookieAuth");
        Response.Cookies.Delete("hms_token");
        Response.Cookies.Delete("hms_branch");
        return RedirectToAction("Login", "Auth", new { area = "Security" });
    }

    private async Task<List<SelectListItem>> GetActiveLocationSelectList()
    {
        return await _db.Locations
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.LocationName
            })
            .ToListAsync();
    }

    /// <summary>
    /// Safely verifies password against BCrypt hash or plain-text (e.g. '123' during development)
    /// </summary>
    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(inputPassword))
        {
            return false;
        }

        if (storedHash.StartsWith("$2"))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return inputPassword == storedHash;
            }
        }

        // Plain text match for manually added development passwords (e.g., "123")
        return inputPassword == storedHash;
    }
}
