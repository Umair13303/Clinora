using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoint.Areas.Dashboard.Helpers;

namespace PulsePoint.Areas.IPD.Controllers;

[Area("IPD")]
[Authorize]
public class BedOccupancyController : Controller
{
    private readonly IDashboardHelper _dashboardHelper;

    public BedOccupancyController(IDashboardHelper dashboardHelper)
    {
        _dashboardHelper = dashboardHelper;
    }

    /// <summary>
    /// Displays Ward Bed Occupancy overview
    /// GET: /ipd/bed-occupancy or /IPD/BedOccupancy/Index
    /// </summary>
    [HttpGet]
    [Route("ipd/bed-occupancy")]
    public async Task<IActionResult> Index()
    {
        var currentPath = HttpContext.Request.Path.Value ?? "/ipd/bed-occupancy";
        var roleId = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "1";
        var (standalone, grouped) = await _dashboardHelper.GetNavigationMenuAsync(roleId, currentPath);

        ViewBag.FullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Clinical Staff";
        ViewBag.BranchName = User.FindFirst("LocationName")?.Value ?? "Central Facility";
        ViewBag.StandaloneMenuItems = standalone;
        ViewBag.GroupedMenuItems = grouped;

        return View();
    }
}
