using System.Text.Json;
using SportLink.Mobile.Models;



namespace SportLink.Mobile.Services;

public static class AnnouncementService
{
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "announcements.json");

    public static List<AnnouncementItem> Announcements { get; set; } = new();

    public static async Task LoadAnnouncementsAsync()
    {
        if (!File.Exists(FilePath))
        {
            Announcements = new List<AnnouncementItem>();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(FilePath);
            Announcements = JsonSerializer.Deserialize<List<AnnouncementItem>>(json) ?? new List<AnnouncementItem>();
        }
        catch
        {
            Announcements = new List<AnnouncementItem>();
        }
    }

    public static async Task SaveAnnouncementsAsync()
    {
        var json = JsonSerializer.Serialize(Announcements);
        await File.WriteAllTextAsync(FilePath, json);
    }
}
