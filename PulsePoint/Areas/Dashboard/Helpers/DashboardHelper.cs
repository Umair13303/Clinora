using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PulsePoint.Areas.Dashboard.Models;
using PulsePoint.Infrastructure.Data;

namespace PulsePoint.Areas.Dashboard.Helpers;

public class DashboardHelper : IDashboardHelper
{
    private readonly AppDbContext _db;

    public DashboardHelper(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal user, string currentPath = "/dashboard")
    {
        var fullName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Clinical Staff";
        var roleId = user.FindFirst(ClaimTypes.Role)?.Value ?? "1";
        var branchName = user.FindFirst("LocationName")?.Value ?? "Central Facility";

        // Query active facilities count from database
        var activeFacilitiesCount = await _db.Locations.CountAsync(l => l.IsActive);

        // Fetch dynamic left menu aligned with database AppRights table
        var (standalone, grouped) = await GetNavigationMenuAsync(roleId, currentPath);

        var model = new DashboardViewModel
        {
            FullName = fullName,
            BranchName = branchName,
            RoleId = roleId,
            RoleName = GetRoleName(roleId),
            
            // Clinical KPI Metrics
            ActiveAdmissions = 38,
            TotalBeds = 120,
            OccupiedBeds = 94,
            TodayConsultations = 64,
            EmergencyCases = 5,
            ActiveFacilitiesCount = activeFacilitiesCount > 0 ? activeFacilitiesCount : 1,

            // Navigation aligned with database AppRights
            StandaloneMenuItems = standalone,
            GroupedMenuItems = grouped,

            // Department Statuses
            DepartmentStatuses = new List<DepartmentStatusItem>
            {
                new()
                {
                    DepartmentName = "Intensive Care Unit (ICU)",
                    LeadDoctor = "Dr. Sarah Jenkins",
                    CurrentLoad = "10 Critical Patients",
                    OccupancyPercentage = 83,
                    StatusBadge = "83% Full",
                    StatusBadgeClass = "bg-danger-subtle text-danger"
                },
                new()
                {
                    DepartmentName = "General Medicine Ward A",
                    LeadDoctor = "Dr. Robert Hayes",
                    CurrentLoad = "32 Inpatients",
                    OccupancyPercentage = 64,
                    StatusBadge = "64% Occupied",
                    StatusBadgeClass = "bg-success-subtle text-success"
                },
                new()
                {
                    DepartmentName = "Emergency & Trauma Center",
                    LeadDoctor = "Dr. Marcus Vance",
                    CurrentLoad = "5 Triage Cases",
                    OccupancyPercentage = 50,
                    StatusBadge = "Priority Queue",
                    StatusBadgeClass = "bg-warning-subtle text-warning"
                }
            }
        };

        return model;
    }

    public async Task<(List<AppMenuItem> Standalone, List<AppMenuGroup> Grouped)> GetNavigationMenuAsync(string roleId, string currentPath = "/dashboard")
    {
        var normalizedPath = (currentPath ?? "/dashboard").Trim().ToLowerInvariant();

        // Query active left menu items from AppRights in HMS_DB
        var activeRights = await _db.AppRights
            .AsNoTracking()
            .Where(r => r.IsActive && r.IsLeftMenu)
            .OrderBy(r => r.Priority)
            .ToListAsync();

        // Filter by role if role restrictions are present
        var permittedRights = activeRights.Where(r =>
            string.IsNullOrEmpty(r.RoleIds) || 
            r.RoleIds.Split(',', StringSplitOptions.TrimEntries).Contains(roleId)
        ).ToList();

        // 1. Standalone Top-Level Menu Items (Menu == null / empty, e.g. /dashboard)
        var standalone = permittedRights
            .Where(r => string.IsNullOrEmpty(r.Menu))
            .Select(r => new AppMenuItem
            {
                Id = r.Id,
                Title = r.Display,
                RouteUrl = r.RouteUrl,
                Icon = r.RouteUrl.Contains("dashboard", StringComparison.OrdinalIgnoreCase) ? "📊" : "📌",
                IsActive = normalizedPath == r.RouteUrl.ToLowerInvariant() || 
                           (r.RouteUrl.Equals("/dashboard", StringComparison.OrdinalIgnoreCase) && (normalizedPath == "/" || normalizedPath == "/dashboard"))
            })
            .ToList();

        // If database had no standalone items, provide default /dashboard
        if (!standalone.Any())
        {
            standalone.Add(new AppMenuItem
            {
                Id = 1,
                Title = "Dashboard",
                RouteUrl = "/dashboard",
                Icon = "📊",
                IsActive = true
            });
        }

        // 2. Hierarchical Grouped Menu Items (e.g. IPD Management -> /ipd/admission, /ipd/bed-occupancy)
        var grouped = permittedRights
            .Where(r => !string.IsNullOrEmpty(r.Menu))
            .GroupBy(r => r.Menu!)
            .Select(g => new AppMenuGroup
            {
                GroupName = g.Key,
                Icon = g.Key.Contains("IPD", StringComparison.OrdinalIgnoreCase) ? "🏥" : "📁",
                IsOpen = true,
                Items = g.Select(sub => new AppMenuItem
                {
                    Id = sub.Id,
                    Title = sub.Display,
                    RouteUrl = sub.RouteUrl,
                    Badge = sub.Id == 2 ? "38 IPD" : sub.Id == 3 ? "78%" : null,
                    IsActive = normalizedPath.StartsWith(sub.RouteUrl.ToLowerInvariant())
                }).ToList()
            })
            .ToList();

        return (standalone, grouped);
    }

    private static string GetRoleName(string roleId) => roleId switch
    {
        "1" => "Administrator",
        "2" => "Physician / Doctor",
        "3" => "Nurse / Ward Staff",
        "4" => "Medical Officer",
        _ => "Clinical Staff"
    };
}
