namespace ManualTest;
public class GeoLocationData
{
    public string UserLocationName { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;


    public double Latitude { get; set; }


    public double Longitude { get; set; }


    public double Altitude { get; set; }

    public bool HasPosition { get; set; }
    public bool HasAltitude { get; set; }
}
