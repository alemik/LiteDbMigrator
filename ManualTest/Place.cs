using System.Collections.ObjectModel;

namespace ManualTest;
public class Place
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Media DefaultMedia { get; set; }

    public string Companions { get; set; }
    public bool IsFavorite { get; set; }


    //TO REFACTOR


    public DateTime Arrival { get; set; }



    public DateTime Departure { get; set; }


    public int Rating { get; set; }



    //-------------------------

    public Guid TravelId { get; set; }


    public ObservableCollection<Media> Gallery { get; set; } = new();



    private TimeSpan _arrivalTime;
    public TimeSpan ArrivalTime
    {
        get => _arrivalTime;
        set
        {
            _arrivalTime = value;
            Arrival = new DateTime(Arrival.Year, Arrival.Month, Arrival.Day, value.Hours, value.Minutes, value.Seconds);
        }
    }

    private TimeSpan _departureTime;
    public TimeSpan DepartureTime
    {
        get => _departureTime;
        set
        {
            _departureTime = value;
            Departure = new DateTime(Departure.Year, Departure.Month, Departure.Day, value.Hours, value.Minutes, value.Seconds);
        }
    }



    void OnDefaultMediaChanged(Media oldValue, Media newValue)
    {

    }
}
