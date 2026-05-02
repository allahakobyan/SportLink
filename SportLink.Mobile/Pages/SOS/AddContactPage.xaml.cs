using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.SOS;

public partial class AddContactPage : ContentPage
{
    public AddContactPage()
    {
        InitializeComponent();
    }

    private async void OnSaveContactClicked(object sender, EventArgs e)
    {
        var contact = new SOSService.EmergencyContact
        {
            Name = NameEntry.Text,
            Phone = PhoneEntry.Text
        }; 

        if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Phone))
        {
            await DisplayAlert("Invalid", "Please enter name and phone number.", "OK");
            return;
        }

        SOSService.Contacts.Add(contact);
        await SOSService.SaveContactsAsync();

        await DisplayAlertAsync("Saved", "Emergency contact added.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}