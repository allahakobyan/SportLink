using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Feed;

public partial class EditPostPage : ContentPage
{
    public static PostItem? SelectedPost { get; set; }

    public EditPostPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (SelectedPost == null)
            return;

        UsernameEntry.Text = SafeText(SelectedPost.Username, 100);
        ProfileImageUrlEntry.Text = SafeText(SelectedPost.ProfileImageUrl, 1000);
        CaptionEditor.Text = SafeText(SelectedPost.Caption, 2000);
        ImageUrlEntry.Text = SafeText(SelectedPost.ImageUrl, 1000);
    }

    private static string SafeText(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var cleaned = value.Replace("\r", " ").Replace("\n", " ");

        if (cleaned.Length <= maxLength)
            return cleaned;

        return cleaned.Substring(0, maxLength);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (SelectedPost == null)
        {
            await DisplayAlert("Error", "No post selected.", "OK");
            return;
        }

        SelectedPost.Username = UsernameEntry.Text ?? string.Empty;
        SelectedPost.ProfileImageUrl = ProfileImageUrlEntry.Text ?? string.Empty;
        SelectedPost.Caption = CaptionEditor.Text ?? string.Empty;
        SelectedPost.ImageUrl = ImageUrlEntry.Text ?? string.Empty;

        await PostService.SavePostsAsync();

        await DisplayAlert("Saved", "Post updated successfully.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}