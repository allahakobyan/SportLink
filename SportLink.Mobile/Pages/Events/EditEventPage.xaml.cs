using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Events;

public partial class EditEventPage : ContentPage
{
    public static EventItem? SelectedEventForEdit;

    public EditEventPage()
    {
        InitializeComponent();

        if (SelectedEventForEdit != null)
        {
            LoadData();
        }
    }

    private void LoadData()
    {
        if (SelectedEventForEdit == null)
            return;

        TitleEntry.Text = SelectedEventForEdit.Title;
        DatePicker.Date = SelectedEventForEdit.Date;
        DescriptionEditor.Text = SelectedEventForEdit.Description;
        MaxParticipantsEntry.Text = SelectedEventForEdit.MaxParticipants.ToString();

        PaidSwitch.IsToggled = SelectedEventForEdit.IsPaid;
        PriceEntry.Text = SelectedEventForEdit.Price.ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (SelectedEventForEdit == null)
            return;

        SelectedEventForEdit.Title = TitleEntry.Text ?? "";
        SelectedEventForEdit.Date = DatePicker.Date ?? DateTime.Now;
        SelectedEventForEdit.Description = DescriptionEditor.Text ?? "";

        SelectedEventForEdit.MaxParticipants =
            int.TryParse(MaxParticipantsEntry.Text, out int max) ? max : 10;

        SelectedEventForEdit.IsPaid = PaidSwitch.IsToggled;

        SelectedEventForEdit.Price =
            double.TryParse(PriceEntry.Text, out double price) ? price : 0;

        await EventService.SaveEventsAsync();

        await DisplayAlert("Saved", "Event updated successfully.", "OK");
        await Shell.Current.GoToAsync("..");
    }
    private async void OnPaidToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value) // user tries to enable paid event
        {
            if (!EventService.IsUserSubscribed())
            {
                //  NOT SUBSCRIBED
                await DisplayAlert(
                    "Subscription required",
                    "To create a paid event, you need to subscribe to Sparkling Plus.",
                    "OK"
                );

                PaidSwitch.IsToggled = false;
                PriceEntry.IsEnabled = false;
                return;
            }

            //  subscribed
            PriceEntry.IsEnabled = true;
        }
        else
        {
            PriceEntry.IsEnabled = false;
            PriceEntry.Text = "";
        }
    }
}