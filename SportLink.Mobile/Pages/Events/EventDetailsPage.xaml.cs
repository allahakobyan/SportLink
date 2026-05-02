using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Events;

public partial class EventDetailsPage : ContentPage
{
    public static EventItem? SelectedEvent;

    public EventDetailsPage()
    {
        InitializeComponent();

        if (SelectedEvent != null)
        {
            LoadEventData();
        }
    }

    private void LoadEventData()
    {
        if (SelectedEvent == null)
            return;

        TitleLabel.Text = SelectedEvent.Title;
        TitleLabel.Text = SelectedEvent.Title;

        DateLabel.Text = $"Date: {SelectedEvent.Date:dddd, MMM dd HH:mm}";

        PriceLabel.Text = SelectedEvent.IsPaid
            ? $"Price: {SelectedEvent.Price}$"
            : "Price: Free";

        ParticipantsLabel.Text =
            $"Participants: {SelectedEvent.ParticipantsCount}/{SelectedEvent.MaxParticipants}";

        DescriptionLabel.Text = SelectedEvent.Description;
        DescriptionLabel.Text = SelectedEvent.Description;
        DeleteButton.IsVisible = SelectedEvent.IsCreatedByCurrentUser;
        EditButton.IsVisible = SelectedEvent.IsCreatedByCurrentUser;
        ReviewsList.ItemsSource = null;
        ReviewsList.ItemsSource = SelectedEvent.Reviews;

        if (SelectedEvent.IsJoined)
        {
            JoinButton.Text = "Leave";
            JoinButton.BackgroundColor = Colors.LightGreen;
        }
        else
        {
            JoinButton.Text = "Join";
            JoinButton.BackgroundColor = Color.FromArgb("#B39DDB");
        }

        if (SelectedEvent.ParticipantsCount >= SelectedEvent.MaxParticipants && !SelectedEvent.IsJoined)
        {
            JoinButton.IsEnabled = false;
            JoinButton.Text = "Full";
            JoinButton.BackgroundColor = Colors.Gray;
        }
        else
        {
            JoinButton.IsEnabled = true;
        }

        PriceLabel.Text = SelectedEvent.IsPaid
            ? $"Price: {SelectedEvent.Price}"
            : "Free";
    }

    private async void OnJoinClicked(object sender, EventArgs e)
    {
        if (SelectedEvent == null)
            return;

        // FIRST: if already joined → leave
        if (SelectedEvent.IsJoined)
        {
            SelectedEvent.IsJoined = false;

            if (SelectedEvent.ParticipantsCount > 0)
                SelectedEvent.ParticipantsCount--;

            await EventService.SaveEventsAsync();
            await DisplayAlertAsync("Left", "You left the event.", "OK");

            LoadEventData();
            return;
        }

        // THEN: joining logic
        if (SelectedEvent.ParticipantsCount >= SelectedEvent.MaxParticipants)
        {
            await DisplayAlertAsync("Full", "This event is full.", "OK");
            return;
        }

        if (SelectedEvent.IsPaid)
        {
            bool pay = await DisplayAlertAsync("Payment Required",
                                               $"This event costs {SelectedEvent.Price}. Do you want to pay and join?",
                                               "Pay",
                                               "Cancel");

            if (!pay)
                return;
        }

        SelectedEvent.ParticipantsCount++;
        SelectedEvent.IsJoined = true;

        await EventService.SaveEventsAsync();
        await DisplayAlertAsync("Joined", "You joined the event.", "OK");

        LoadEventData();
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (SelectedEvent == null)
            return;

        bool confirm = await DisplayAlertAsync("Delete Event",
                                               "Are you sure you want to delete this event?",
                                               "Yes",
                                               "No");

        if (!confirm)
            return;

        EventService.Events.Remove(SelectedEvent);
        await EventService.SaveEventsAsync();

        SelectedEvent = null;

        await DisplayAlertAsync("Deleted", "Event deleted successfully.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (SelectedEvent == null)
            return;

        EditEventPage.SelectedEventForEdit = SelectedEvent;
        await Shell.Current.GoToAsync(nameof(EditEventPage));
    }

    private async void OnViewParticipantsClicked(object sender, EventArgs e)
    {
        if (SelectedEvent == null)
            return;

        ParticipantsPage.Participants = new List<string>();

        for (int i = 1; i <= SelectedEvent.ParticipantsCount; i++)
        {
            ParticipantsPage.Participants.Add($"Participant {i}");
        }

        await Shell.Current.GoToAsync(nameof(ParticipantsPage));
    }

    private async void OnLeaveReviewClicked(object sender, EventArgs e)
    {
        if (SelectedEvent == null)
            return;

        ReviewPage.SelectedEventForReview = SelectedEvent;
        await Shell.Current.GoToAsync(nameof(ReviewPage));
    }
}