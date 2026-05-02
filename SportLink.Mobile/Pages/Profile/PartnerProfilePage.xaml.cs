namespace SportLink.Mobile.Pages.Search;

using SportLink.Mobile.Pages.Search;

public partial class PartnerProfilePage : ContentPage
{
    public static SearchSuggestionItem? SelectedPartner { get; set; }

    private bool _isConnected = false;

    public PartnerProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (SelectedPartner == null)
            return;

        InitialLabel.Text = SelectedPartner.Initial;
        NameLabel.Text = SelectedPartner.Name;
        SpecialtyLabel.Text = SelectedPartner.Specialty;
        RatingLabel.Text = SelectedPartner.Rating;

        _isConnected = SelectedPartner.IsConnected;
        UpdateConnectButton();
    }

    private void UpdateConnectButton()
    {
        if (_isConnected)
        {
            ConnectButton.Text = "Connected";
            ConnectButton.BackgroundColor = Color.FromArgb("#DDEFD8");
            ConnectButton.TextColor = Color.FromArgb("#355E2B");
        }
        else
        {
            ConnectButton.Text = "Connect";
            ConnectButton.BackgroundColor = Color.FromArgb("#6F63F6");
            ConnectButton.TextColor = Colors.White;
        }
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        if (SelectedPartner == null)
            return;

        _isConnected = !_isConnected;
        SelectedPartner.IsConnected = _isConnected;

        UpdateConnectButton();

        await DisplayAlert("Updated",
            _isConnected ? $"You connected with {SelectedPartner.Name}." : $"You disconnected from {SelectedPartner.Name}.",
            "OK");
    }
}