using System.Security.Claims;
using PulsePoint.Areas.Dashboard.Models;

namespace PulsePoint.Areas.Dashboard.Helpers;

public interface IDashboardHelper
{
    Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal user, string currentPath = "/dashboard");
    Task<(List<AppMenuItem> Standalone, List<AppMenuGroup> Grouped)> GetNavigationMenuAsync(string roleId, string currentPath = "/dashboard");
}
