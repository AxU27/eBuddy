using SQLite;

public class Vehicle
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = "IX_Vehicle_Name", Unique = true)]
    public string Name { get; set; } = string.Empty;

    // Future-proof fields
    public string? Vin { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? ModelYear { get; set; }
    public string? CarBrand { get; set; }
    public string? Model { get; set; }
    public string? Notes { get; set; }
}
