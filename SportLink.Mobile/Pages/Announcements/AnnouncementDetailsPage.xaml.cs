using SportLink.Mobile.Models;

namespace SportLink.Mobile.Pages.Announcements;

public partial class AnnouncementDetailsPage : ContentPage
{
    public static AnnouncementItem? SelectedAnnouncement;

    public AnnouncementDetailsPage()
    {
        InitializeComponent();

        if (SelectedAnnouncement != null)
        {
            TitleLabel.Text = SelectedAnnouncement.Title;
            CreatedAtLabel.Text = SelectedAnnouncement.CreatedAt;
            BodyLabel.Text = SelectedAnnouncement.Body;
        }
    }
}