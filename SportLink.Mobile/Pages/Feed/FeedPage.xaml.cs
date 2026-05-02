using SportLink.Mobile.Models;
using SportLink.Mobile.Pages.Announcements;
using SportLink.Mobile.Pages.Events;
using SportLink.Mobile.Pages.Polls;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Feed;

public partial class FeedPage : ContentPage
{
    private List<FeedItem> MixedFeed = new();

    public FeedPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFeedAsync();
    }

    private async Task LoadFeedAsync()
    {
        await EventService.LoadEventsAsync();
        await AnnouncementService.LoadAnnouncementsAsync();
        await PollService.LoadPollsAsync();
        await PostService.LoadPostsAsync();

        MixedFeed = new List<FeedItem>();

        foreach (var post in PostService.Posts)
        {
            DateTime.TryParse(post.CreatedAt, out DateTime postDate);

            MixedFeed.Add(new FeedItem
            {
                ItemType = "Post",
                Post = post,
                SortDate = postDate
            });
        }

        foreach (var ev in EventService.Events)
        {
            MixedFeed.Add(new FeedItem
            {
                ItemType = "Event",
                Event = ev,
                SortDate = ev.Date
            });
        }

        foreach (var announcement in AnnouncementService.Announcements)
        {
            DateTime.TryParse(announcement.CreatedAt, out DateTime announcementDate);

            MixedFeed.Add(new FeedItem
            {
                ItemType = "Announcement",
                Announcement = announcement,
                SortDate = announcementDate
            });
        }

        foreach (var poll in PollService.Polls)
        {
            DateTime.TryParse(poll.CreatedAt, out DateTime pollDate);

            MixedFeed.Add(new FeedItem
            {
                ItemType = "Poll",
                Poll = poll,
                SortDate = pollDate
            });
        }

        MixedFeed = MixedFeed
            .OrderByDescending(x => x.SortDate)
            .ToList();

        FeedList.ItemsSource = null;
        FeedList.ItemsSource = MixedFeed;
    }

    private async Task TogglePostLikeAsync(FeedItem? feedItem)
    {
        var post = feedItem?.Post;

        if (post == null)
            return;

        post.IsLiked = !post.IsLiked;

        if (post.IsLiked)
            post.LikesCount++;
        else if (post.LikesCount > 0)
            post.LikesCount--;

        await PostService.SavePostsAsync();
        await LoadFeedAsync();
    }

    private async Task AddCommentToPostAsync(FeedItem? feedItem)
    {
        var post = feedItem?.Post;

        if (post == null)
            return;

        string comment = await DisplayPromptAsync("Add Comment", "Write your comment:");

        if (string.IsNullOrWhiteSpace(comment))
            return;

        post.Comments.Add(new CommentItem
        {
            Username = "You",
            Text = comment
        });

        await PostService.SavePostsAsync();
        await LoadFeedAsync();
    }

    private async void OnLikePostClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        await TogglePostLikeAsync(feedItem);
    }

    private async void OnLikePostTapped(object sender, TappedEventArgs e)
    {
        var view = sender as View;
        var feedItem = view?.BindingContext as FeedItem;
        await TogglePostLikeAsync(feedItem);
    }

    private async void OnPostTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var feedItem = frame?.BindingContext as FeedItem;
        await TogglePostLikeAsync(feedItem);
    }

    private async void OnCommentPostClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        await AddCommentToPostAsync(feedItem);
    }

    private async void OnCommentPostTapped(object sender, TappedEventArgs e)
    {
        var view = sender as View;
        var feedItem = view?.BindingContext as FeedItem;
        await AddCommentToPostAsync(feedItem);
    }

    private async void OnViewDetailsTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var feedItem = frame?.BindingContext as FeedItem;
        var eventItem = feedItem?.Event;

        if (eventItem != null)
        {
            EventDetailsPage.SelectedEvent = eventItem;
            await Shell.Current.GoToAsync(nameof(EventDetailsPage));
        }
    }

    private async void OnJoinClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        var eventItem = feedItem?.Event;

        if (eventItem == null)
            return;

        if (eventItem.IsJoined)
        {
            eventItem.IsJoined = false;

            if (eventItem.ParticipantsCount > 0)
                eventItem.ParticipantsCount--;

            await EventService.SaveEventsAsync();
            await LoadFeedAsync();

            await DisplayAlert("Left", "You left the event.", "OK");
            return;
        }

        if (eventItem.ParticipantsCount >= eventItem.MaxParticipants)
        {
            await DisplayAlert("Full", "This event is full.", "OK");
            return;
        }

        if (eventItem.IsPaid)
        {
            bool pay = await DisplayAlert("Payment Required",
                                          $"This event costs {eventItem.Price}. Do you want to pay and join?",
                                          "Pay",
                                          "Cancel");

            if (!pay)
                return;
        }

        eventItem.ParticipantsCount++;
        eventItem.IsJoined = true;

        await EventService.SaveEventsAsync();
        await LoadFeedAsync();

        await DisplayAlert("Joined", "You joined the event.", "OK");
    }

    private void OnButtonLoaded(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        var eventItem = feedItem?.Event;

        if (eventItem == null)
            return;

        if (eventItem.IsJoined)
        {
            button.Text = "Joined";
            button.BackgroundColor = Color.FromArgb("#DCECCB");
            button.TextColor = Color.FromArgb("#355E2B");
            button.IsEnabled = true;
        }
        else if (eventItem.ParticipantsCount >= eventItem.MaxParticipants)
        {
            button.Text = "Full";
            button.BackgroundColor = Color.FromArgb("#E5E7EB");
            button.TextColor = Color.FromArgb("#666666");
            button.IsEnabled = false;
        }
        else
        {
            button.Text = "Join";
            button.BackgroundColor = Color.FromArgb("#DCECCB");
            button.TextColor = Color.FromArgb("#355E2B");
            button.IsEnabled = true;
        }
    }

    private async void OnAnnouncementTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var feedItem = frame?.BindingContext as FeedItem;
        var announcement = feedItem?.Announcement;

        if (announcement != null)
        {
            AnnouncementDetailsPage.SelectedAnnouncement = announcement;
            await Shell.Current.GoToAsync(nameof(AnnouncementDetailsPage));
        }
    }

    private async void OnVoteClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        var poll = feedItem?.Poll;

        if (poll == null)
            return;

        var selectedText = button.Text;
        int index = poll.Options.IndexOf(selectedText);

        if (index == -1)
            return;

        poll.SelectedOptionIndex = index;

        await PollService.SavePollsAsync();
        await DisplayAlert("Voted", $"You selected: {selectedText}", "OK");
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateOptionsPage));
    }

    private async void OnPostOptionsClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        var post = feedItem?.Post;

        if (post == null)
            return;

        string action = await DisplayActionSheet(
            "Post Options",
            "Cancel",
            null,
            "Edit",
            "Delete");

        if (action == "Edit")
        {
            EditPostPage.SelectedPost = post;
            await Shell.Current.GoToAsync(nameof(EditPostPage));
        }
        else if (action == "Delete")
        {
            bool confirm = await DisplayAlert("Delete", "Are you sure you want to delete this post?", "Yes", "No");

            if (!confirm)
                return;

            await PostService.DeletePostAsync(post);
            await LoadFeedAsync();
        }
    }

    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var feedItem = button?.BindingContext as FeedItem;
        var eventItem = feedItem?.Event;

        if (eventItem == null)
            return;

        EventDetailsPage.SelectedEvent = eventItem;
        await Shell.Current.GoToAsync(nameof(EventDetailsPage));
    }
}