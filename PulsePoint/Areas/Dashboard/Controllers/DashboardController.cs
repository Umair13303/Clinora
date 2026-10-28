using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoint.Areas.Dashboard.Helpers;

namespace PulsePoint.Areas.Dashboard.Controllers;

[Area("Dashboard")]
[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardHelper _dashboardHelper;

    public DashboardController(IDashboardHelper dashboardHelper)
    {
        _dashboardHelper = dashboardHelper;
    }

    /// <summary>
    /// Displays the primary Hospital Clinical Dashboard with metrics and menu items
    /// GET: /dashboard or /Dashboard/Dashboard/Index
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        var currentPath = HttpContext.Request.Path.Value ?? "/dashboard";
        var model = await _dashboardHelper.GetDashboardViewModelAsync(User, currentPath);
        
        ViewBag.FullName = model.FullName;
        ViewBag.BranchName = model.BranchName;
        ViewBag.RoleId = model.RoleId;
        ViewBag.RoleName = model.RoleName;
        ViewBag.StandaloneMenuItems = model.StandaloneMenuItems;
        ViewBag.GroupedMenuItems = model.GroupedMenuItems;

        return View(model);
    }
}
