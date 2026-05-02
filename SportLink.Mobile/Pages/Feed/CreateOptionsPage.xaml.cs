using SportLink.Mobile.Pages.Announcements;
using SportLink.Mobile.Pages.Events;
using SportLink.Mobile.Pages.Polls;

namespace SportLink.Mobile.Pages.Feed;

public partial class CreateOptionsPage : ContentPage
{
    public CreateOptionsPage()
    {
        InitializeComponent();
    }

    private async void OnPostClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Main/CreatePostPage");
    }

    private async void OnPollClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Main/CreatePollPage");
    }

    private async void OnEventClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Main/CreateEventPage");
    }
}