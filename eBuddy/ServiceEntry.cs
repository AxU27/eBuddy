using SQLite;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using eBuddy;

public class ServiceEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public int Mileage { get; set; }
    public string Description { get; set; }
    public string ServicedBy { get; set; }
    public int ServiceCost { get; set; }
    public ServiceType ServiceType { get; set; }
    public int VehicleId { get; set; } // Foreign key to Vehicle
}

public class ServiceFile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ServiceEntryId { get; set; } // Foreign key to ServiceEntry
    public string FilePath { get; set; } // Path to the file
    public string FileName { get; set; } // Name of the file
}

public enum ServiceType
{
    [Display(Name = "Testing")]
    [Icon("engine_oil_icon_accent.png")]
    GeneralMaintenance,

    [Display(Name = "Engine & Performance")]
    [Icon("engine_icon_accent.png")]
    EngineAndPerformance,

    [Display(Name = "Transmission & Drivetrain")]
    [Icon("transmission_icon_accent.png")]
    TransmissionAndDrivetrain,

    [Display(Name = "Brakes & Suspension")]
    [Icon("brakes_icon_accent.png")]
    BrakesAndSuspension,

    [Display(Name = "Low voltage electronics")]
    [Icon("battery_icon_accent.png")]
    LowVoltageElectronics,

    [Display(Name = "High voltage electronics")]
    [Icon("ev_icon_accent.png")]
    HighVoltageElectronics,

    [Display(Name = "Bodywork, Interior & Glass")]
    [Icon("car_icon_accent.png")]
    BodyInteriorAndGlass,

    [Display(Name = "Tires & Wheels")]
    [Icon("wheel_icon_accent.png")]
    TiresAndWheels,

    [Display(Name = "Heating, Cooling & A/C")]
    [Icon("ac_icon_accent.png")]
    HeatingAndCooling,

    [Display(Name = "Other Maintenance")]
    [Icon("gears_icon_accent.png")]
    OtherMaintenance
}

public static class  EnumHelper
{
    public static string GetLocalizedDisplayName(Enum value)
    {
        return AppResources.ResourceManager.GetString(value.ToString()) ?? value.ToString();
        /*return value.GetType().GetMember(value.ToString())
            .First()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? value.ToString();*/
    }

    public static string GetIcon(Enum value)
    {
        return value.GetType().GetMember(value.ToString())
            .First()?
            .GetCustomAttribute<IconAttribute>()?
            .GetIcon() ?? value.ToString();
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class IconAttribute : Attribute
{
    public string IconFile { get; }
    public IconAttribute(string iconFile)
    {
        IconFile = iconFile;
    }

    public string? GetIcon()
    {
        return IconFile;
    }
}