namespace SportLink.Mobile.Pages.Events;

public partial class ParticipantsPage : ContentPage
{
    public static List<string> Participants = new();

    public ParticipantsPage()
    {
        InitializeComponent();
        ParticipantsList.ItemsSource = Participants;
    }
}