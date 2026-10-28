using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PulsePoint.Areas.Security.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please select your hospital branch.")]
    public int? LocationId { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public List<SelectListItem> Locations { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public record LoginApiRequest(string Username, string Password, int LocationId);
