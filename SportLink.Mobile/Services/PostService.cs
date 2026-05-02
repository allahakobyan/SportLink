using System.Text.Json;
using SportLink.Mobile.Models;

namespace SportLink.Mobile.Services;

public static class PostService
{
    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "posts.json");

    public static List<PostItem> Posts { get; set; } = new();

    public static async Task LoadPostsAsync()
    {
        if (!File.Exists(FilePath))
        {
            Posts = new List<PostItem>();
            return;
        }

        string json = await File.ReadAllTextAsync(FilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Posts = new List<PostItem>();
            return;
        }

        try
        {
            Posts = JsonSerializer.Deserialize<List<PostItem>>(json) ?? new List<PostItem>();
        }
        catch
        {
            // fallback if old format breaks
            Posts = new List<PostItem>();
        }
    }

    public static async Task SavePostsAsync()
    {
        string json = JsonSerializer.Serialize(Posts);
        await File.WriteAllTextAsync(FilePath, json);
    }

    public static async Task AddPostAsync(PostItem post)
    {
        Posts.Insert(0, post);
        await SavePostsAsync();
    }

    public static async Task DeletePostAsync(PostItem post)
    {
        Posts.Remove(post);
        await SavePostsAsync();
    }
}
