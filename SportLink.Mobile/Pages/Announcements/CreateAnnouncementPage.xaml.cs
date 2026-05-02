using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Announcements;

public partial class CreateAnnouncementPage : ContentPage
{
    public CreateAnnouncementPage()
    {
        InitializeComponent();
    }

    private async void OnSaveAnnouncementClicked(object sender, EventArgs e)
    {
        var announcement = new AnnouncementItem
        {
            Title = TitleEntry.Text ?? string.Empty,
            Body = BodyEditor.Text ?? string.Empty,
            CreatedAt = DateTime.Now.ToString("g")
        };

        if (string.IsNullOrWhiteSpace(announcement.Title) || string.IsNullOrWhiteSpace(announcement.Body))
        {
            await DisplayAlertAsync("Invalid", "Please enter title and body.", "OK");
            return;
        }

        AnnouncementService.Announcements.Add(announcement);
        await AnnouncementService.SaveAnnouncementsAsync();

        await DisplayAlertAsync("Saved", "Announcement created.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}