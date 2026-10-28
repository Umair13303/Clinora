namespace PulsePoint.Core.Entities;

public class Location
{
    public int Id { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int LocationTypeId { get; set; } // 1: Main Hospital, 2: City Clinic, 3: Lab/Diagnostics
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
