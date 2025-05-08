namespace ManualTest;
public class Media
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Path { get; set; }
    public string FullPath { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public GeoLocationData GeoLocation { get; set; } = new();
    public DateTime Date { get; set; }
    public bool IsFavorite { get; set; }
    public MediaType MediaType { get; set; }
}

