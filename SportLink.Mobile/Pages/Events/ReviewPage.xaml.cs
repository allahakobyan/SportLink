using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Events;

public partial class ReviewPage : ContentPage
{
    public static EventItem? SelectedEventForReview;

    public ReviewPage()
    {
        InitializeComponent();
    }

    private async void OnSaveReviewClicked(object sender, EventArgs e)
    {
        if (SelectedEventForReview == null)
            return;

        if (!int.TryParse(RatingEntry.Text, out int rating) || rating < 1 || rating > 5)
        {
            await DisplayAlert("Invalid", "Rating must be between 1 and 5.", "OK");
            return;
        }

        var review = new ReviewItem
        {
            Rating = rating,
            Comment = CommentEditor.Text ?? ""
        };

        SelectedEventForReview.Reviews.Add(review);

        await EventService.SaveEventsAsync();

        await DisplayAlert("Saved", "Review saved.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}