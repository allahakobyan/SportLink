using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Polls;

public partial class CreatePollPage : ContentPage
{
    public CreatePollPage()
    {
        InitializeComponent();
    }

    private async void OnSavePollClicked(object sender, EventArgs e)
    {
        var options = new List<string>();

        if (!string.IsNullOrWhiteSpace(Option1Entry.Text))
            options.Add(Option1Entry.Text);

        if (!string.IsNullOrWhiteSpace(Option2Entry.Text))
            options.Add(Option2Entry.Text);

        if (!string.IsNullOrWhiteSpace(Option3Entry.Text))
            options.Add(Option3Entry.Text);

        if (string.IsNullOrWhiteSpace(QuestionEntry.Text) || options.Count < 2)
        {
            await DisplayAlertAsync("Invalid", "Enter a question and at least 2 options.", "OK");
            return;
        }

        var poll = new PollItem
        {
            Question = QuestionEntry.Text,
            Options = options,
            CreatedAt = DateTime.Now.ToString("g")
        };

        PollService.Polls.Add(poll);
        await PollService.SavePollsAsync();

        await DisplayAlert("Saved", "Poll created.", "OK");
        await Shell.Current.GoToAsync("..");

    }
}