using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Events;

public partial class CreateEventPage : ContentPage
{
    public CreateEventPage()
    {
        InitializeComponent();
    }

    private void OnPaidToggled(object sender, ToggledEventArgs e)
    {
        PriceEntry.IsVisible = e.Value;

        if (!e.Value)
            PriceEntry.Text = "";
    }

    private async void OnCreateEventClicked(object sender, EventArgs e)
    {
        bool isPaid = PaidSwitch.IsToggled;

        //  NOT SUBSCRIBED → BLOCK
        if (isPaid && !EventService.IsUserSubscribed())
        {
            await DisplayAlert(
                "Subscription required",
                "To create a paid event, you need to subscribe to Sparkling Plus.",
                "OK"
            );
            return;
        }

        var ev = new EventItem
        {
            Title = TitleEntry.Text ?? "",
            Date = DatePicker.Date ?? DateTime.Now,
            Time = TimePickerControl.Time.ToString(),
            Description = DescriptionEditor.Text ?? "",
            MaxParticipants = int.TryParse(MaxParticipantsEntry.Text, out int max) ? max : 10,
            ParticipantsCount = 0,
            IsPaid = isPaid,
            Price = isPaid && double.TryParse(PriceEntry.Text, out double price) ? price : 0
        };

        EventService.Events.Add(ev);
        await EventService.SaveEventsAsync();

        try
        {
            var startDateTime = ev.Date.Date + TimeSpan.Parse(ev.Time);

            await Launcher.Default.OpenAsync($"content://com.android.calendar/time/{startDateTime.Ticks}");
        }
        catch { }

        await DisplayAlert("Success", "Event created!", "OK");
        await Shell.Current.GoToAsync("..");


    }
}