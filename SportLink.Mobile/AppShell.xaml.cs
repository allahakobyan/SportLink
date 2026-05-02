using SportLink.Mobile.Pages.Announcements;
using SportLink.Mobile.Pages.Auth;
using SportLink.Mobile.Pages.Events;
using SportLink.Mobile.Pages.Feed;
using SportLink.Mobile.Pages.Polls;
using SportLink.Mobile.Pages.Search;
using SportLink.Mobile.Pages.SOS;

namespace SportLink.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // MAIN ROUTES
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(VerifyEmailPage), typeof(VerifyEmailPage));
        Routing.RegisterRoute(nameof(FeedPage), typeof(FeedPage));

        // EVENTS
        Routing.RegisterRoute(nameof(EventDetailsPage), typeof(EventDetailsPage));
        Routing.RegisterRoute(nameof(CreateEventPage), typeof(CreateEventPage));
        Routing.RegisterRoute(nameof(EditEventPage), typeof(EditEventPage));
        Routing.RegisterRoute(nameof(ParticipantsPage), typeof(ParticipantsPage));
        Routing.RegisterRoute(nameof(ReviewPage), typeof(ReviewPage));

        // SOS
        Routing.RegisterRoute(nameof(AddContactPage), typeof(AddContactPage));

        // ANNOUNCEMENTS
        Routing.RegisterRoute(nameof(CreateAnnouncementPage), typeof(CreateAnnouncementPage));
        Routing.RegisterRoute(nameof(AnnouncementDetailsPage), typeof(AnnouncementDetailsPage));

        // POLLS & POSTS
        Routing.RegisterRoute(nameof(CreatePollPage), typeof(CreatePollPage));
        Routing.RegisterRoute(nameof(CreatePostPage), typeof(CreatePostPage));
        Routing.RegisterRoute(nameof(CreateOptionsPage), typeof(CreateOptionsPage));
        Routing.RegisterRoute(nameof(EditPostPage), typeof(EditPostPage));
        

        // SEARCH
        Routing.RegisterRoute(nameof(PartnerProfilePage), typeof(PartnerProfilePage));
    }
}