using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoint.Areas.Dashboard.Helpers;

namespace PulsePoint.Areas.IPD.Controllers;

[Area("IPD")]
[Authorize]
public class AdmissionController : Controller
{
    private readonly IDashboardHelper _dashboardHelper;

    public AdmissionController(IDashboardHelper dashboardHelper)
    {
        _dashboardHelper = dashboardHelper;
    }

    /// <summary>
    /// Displays Inpatient Admission management portal
    /// GET: /ipd/admission or /IPD/Admission/Index
    /// </summary>
    [HttpGet]
    [Route("ipd/admission")]
    public async Task<IActionResult> Index()
    {
        var currentPath = HttpContext.Request.Path.Value ?? "/ipd/admission";
        var roleId = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "1";
        var (standalone, grouped) = await _dashboardHelper.GetNavigationMenuAsync(roleId, currentPath);

        ViewBag.FullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Clinical Staff";
        ViewBag.BranchName = User.FindFirst("LocationName")?.Value ?? "Central Facility";
        ViewBag.StandaloneMenuItems = standalone;
        ViewBag.GroupedMenuItems = grouped;

        return View();
    }
}
