namespace PulsePoint.Core.Entities;

public class AppRight
{
    public int Id { get; set; }
    public string RouteUrl { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string? Menu { get; set; }
    public int Priority { get; set; } = 1;
    public string? LocationsTypeIds { get; set; }
    public string? LocationIds { get; set; }
    public string? RoleIds { get; set; }
    public string? UserIds { get; set; }
    public bool IsLeftMenu { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
