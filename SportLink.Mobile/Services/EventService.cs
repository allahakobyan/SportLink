using SportLink.Mobile.Models;

namespace SportLink.Mobile.Services;

public static class EventService
{
    public static List<EventItem> Events { get; set; } = new();

    public static async Task SaveEventsAsync()
    {
        // TEMP: just simulate saving
        await Task.CompletedTask;
    }

    public static async Task LoadEventsAsync()
    {
        // TEMP: just simulate loading
        await Task.CompletedTask;
    }

    public static void AddEvent(EventItem ev)
    {
        Events.Add(ev);
    }

    public static bool IsUserSubscribed()
    {
        return false; // change later
    }
}