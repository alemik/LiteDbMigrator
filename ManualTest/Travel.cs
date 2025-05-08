using System.Collections.ObjectModel;

namespace ManualTest;
public class Travel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Notes { get; set; }
    public Media DefaultMedia { get; set; }

    public string Companions { get; set; }
    public bool IsFavorite { get; set; }




    public GeoLocationData GeoLocation { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal Rate { get; set; }
    public bool IsActual { get; set; }




    public ObservableCollection<Media> Gallery { get; set; } = new();

    public ObservableCollection<Place> Places { get; set; } = new();



    void OnDefaultMediaChanged(Media oldValue, Media newValue)
    {
        if (newValue == null) return;
    }
}
