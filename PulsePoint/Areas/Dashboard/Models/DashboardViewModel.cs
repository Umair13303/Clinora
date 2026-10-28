namespace PulsePoint.Areas.Dashboard.Models;

public class DashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = "Clinical Staff";
    
    // KPI Metrics
    public int ActiveAdmissions { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds => Math.Max(0, TotalBeds - OccupiedBeds);
    public int TodayConsultations { get; set; }
    public int EmergencyCases { get; set; }
    public int ActiveFacilitiesCount { get; set; }

    public double BedOccupancyPercentage => TotalBeds > 0 
        ? Math.Round((double)OccupiedBeds / TotalBeds * 100, 1) 
        : 0;

    // Left Navigation Items mapped from AppRights in database
    public List<AppMenuItem> StandaloneMenuItems { get; set; } = new();
    public List<AppMenuGroup> GroupedMenuItems { get; set; } = new();

    // Departmental Statuses
    public List<DepartmentStatusItem> DepartmentStatuses { get; set; } = new();
}

public class AppMenuItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RouteUrl { get; set; } = string.Empty;
    public string Icon { get; set; } = "●";
    public string? Badge { get; set; }
    public bool IsActive { get; set; }
}

public class AppMenuGroup
{
    public string GroupName { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏥";
    public bool IsOpen { get; set; } = true;
    public List<AppMenuItem> Items { get; set; } = new();
}

public class DepartmentStatusItem
{
    public string DepartmentName { get; set; } = string.Empty;
    public string LeadDoctor { get; set; } = string.Empty;
    public string CurrentLoad { get; set; } = string.Empty;
    public int OccupancyPercentage { get; set; }
    public string StatusBadge { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
}
