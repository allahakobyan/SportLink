using System.Text.Json;
using SportLink.Mobile.Models;

namespace SportLink.Mobile.Services;

public static class PollService
{
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "polls.json");

    public static List<PollItem> Polls { get; set; } = new();

    public static async Task LoadPollsAsync()
    {
        if (!File.Exists(FilePath))
        {
            Polls = new List<PollItem>();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(FilePath);
            Polls = JsonSerializer.Deserialize<List<PollItem>>(json) ?? new List<PollItem>();
        }
        catch
        {
            Polls = new List<PollItem>();
        }
    }

    public static async Task SavePollsAsync()
    {
        var json = JsonSerializer.Serialize(Polls);
        await File.WriteAllTextAsync(FilePath, json);
    }
}