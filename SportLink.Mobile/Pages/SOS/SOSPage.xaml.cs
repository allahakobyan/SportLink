using SportLink.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
namespace SportLink.Mobile.Pages.SOS;

public partial class SOSPage : ContentPage
{
    private bool _timerRunning = false;
    private CancellationTokenSource? _locationUpdateCts;

    public SOSPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadContacts();
        UpdateStatusDisplay();

        if (SOSService.IsSOSActive)
        {
            StartTimer();
        }
    }

    // ================= TIMER =================

    private void StartTimer()
    {
        if (_timerRunning)
            return;

        _timerRunning = true;

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!_timerRunning || !SOSService.IsSOSActive || SOSService.SOSStartTime == null)
                return false;

            var elapsed = DateTime.UtcNow - SOSService.SOSStartTime.Value;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text =
                    $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            });

            return true;
        });
    }

    private void StopTimer()
    {
        _timerRunning = false;
        TimerLabel.Text = "00:00:00";
    }

    // ================= CONTACTS =================

    private async Task LoadContacts()
    {
        await SOSService.LoadContactsAsync();
        ContactsList.ItemsSource = SOSService.Contacts;
    }

    // ================= UI =================

    private void UpdateStatusDisplay()
    {
        if (SOSService.IsSOSActive)
        {
            StatusLabel.Text = "SOS Active";
            InstructionLabel.Text = "Tap to stop";
            TimerLabel.IsVisible = true;
        }
        else
        {
            StatusLabel.Text = "No active session";
            InstructionLabel.Text = "Tap to activate SOS"; 
            TimerLabel.IsVisible = false;
        }
    }

    // ================= SOS =================

    private async Task ActivateSOS()
    {
        if (SOSService.Contacts.Count == 0)
        {
            await DisplayAlert("Error", "Add at least one contact", "OK");
            return;
        }

        var hasPermission = await CheckLocationPermission();

        if (!hasPermission)
        {
            await DisplayAlert("Permission Required", "Location permission is needed for SOS.", "OK");
            return;
        }

        await SOSService.ActivateSOSAsync();

        UpdateStatusDisplay();
        StartTimer();
        StartLocationUpdates();

        await DisplayAlert("SOS Activated", "Message ready to send", "OK");
    }

    private async Task<bool> CheckLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return status == PermissionStatus.Granted;
    }

    private async Task DeactivateSOS()
    {
        _locationUpdateCts?.Cancel();

        await SOSService.DeactivateSOSAsync();

        StopTimer();
        UpdateStatusDisplay();

        await DisplayAlert("Stopped", "SOS stopped", "OK");
    }

    private void StartLocationUpdates()
    {
        _locationUpdateCts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!_locationUpdateCts.Token.IsCancellationRequested && SOSService.IsSOSActive)
            {
                await SOSService.UpdateLocationAsync();
                await Task.Delay(30000);
            }
        });
    }

    // ================= TAP BUTTON =================

    private async void OnSOSTapped(object sender, EventArgs e)
    {
        if (SOSService.IsSOSActive)
        {
            await DeactivateSOS();
        }
        else
        {
            await ActivateSOS();
        }
    }

    // ================= NAV =================

    private async void OnAddContactClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddContactPage));
    }

    private async void OnDeleteContactClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var contact = btn?.BindingContext as SOSService.EmergencyContact;

        if (contact != null)
        {
            await SOSService.DeleteContactAsync(contact.Id);
            await LoadContacts();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _locationUpdateCts?.Cancel();
    }
}