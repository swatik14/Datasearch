namespace DataSearch.Models;

public class Record
{
    public long Id { get; set; }
    public string State { get; set; } = string.Empty;        // Karnataka / Andhra Pradesh
    public string PhoneNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AadhaarId { get; set; } = string.Empty;   // ID
    public string AlternativeNumber { get; set; } = string.Empty;
    public string Circle { get; set; } = string.Empty;      // airtel/jio etc.
}