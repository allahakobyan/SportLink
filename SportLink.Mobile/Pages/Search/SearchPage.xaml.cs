using System.Collections.ObjectModel;
using SportLink.Mobile.Models;
using SportLink.Mobile.Pages.Events;

namespace SportLink.Mobile.Pages.Search;

public partial class SearchPage : ContentPage
{
    private readonly List<SearchSuggestionItem> _allSuggestions = new();
    private readonly ObservableCollection<SearchSuggestionItem> _filteredSuggestions = new();

    public SearchPage()
    {
        InitializeComponent();
        LoadSuggestions();
        SuggestionsList.ItemsSource = _filteredSuggestions;
    }

    private void LoadSuggestions()
    {
        _allSuggestions.Clear();

        _allSuggestions.Add(new SearchSuggestionItem { Name = "Sam", Specialty = "Cycling Specialist", Rating = "4.9" });
        _allSuggestions.Add(new SearchSuggestionItem { Name = "Alex", Specialty = "Running Specialist", Rating = "4.8" });
        _allSuggestions.Add(new SearchSuggestionItem { Name = "Grace", Specialty = "Volleyball Specialist", Rating = "4.7" });
        _allSuggestions.Add(new SearchSuggestionItem { Name = "Frank", Specialty = "Fitness Specialist", Rating = "4.5" });
        _allSuggestions.Add(new SearchSuggestionItem { Name = "David", Specialty = "Climbing Specialist", Rating = "4.9" });
        _allSuggestions.Add(new SearchSuggestionItem { Name = "Ella", Specialty = "Swimming Specialist", Rating = "4.8" });

        RefreshSuggestions(_allSuggestions);
    }

    private void RefreshSuggestions(IEnumerable<SearchSuggestionItem> items)
    {
        _filteredSuggestions.Clear();

        foreach (var item in items)
            _filteredSuggestions.Add(item);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string text = e.NewTextValue?.Trim().ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            RefreshSuggestions(_allSuggestions);
            return;
        }

        var filtered = _allSuggestions.Where(x =>
            x.Name.ToLower().Contains(text) ||
            x.Specialty.ToLower().Contains(text));

        RefreshSuggestions(filtered);
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var item = button?.BindingContext as SearchSuggestionItem;

        if (item == null)
            return;

        item.IsConnected = !item.IsConnected;

        RefreshSuggestions(_filteredSuggestions.ToList());

        await DisplayAlert("Updated",
            item.IsConnected
                ? $"You connected with {item.Name}."
                : $"You disconnected from {item.Name}.",
            "OK");
    }

    private async void OnSuggestionTapped(object sender, TappedEventArgs e)
    {
        var frame = sender as Frame;
        var item = frame?.BindingContext as SearchSuggestionItem;

        if (item == null)
            return;

        PartnerProfilePage.SelectedPartner = item;
        await Shell.Current.GoToAsync(nameof(PartnerProfilePage));
    }

    private async void OnSeeDetailsClicked(object sender, EventArgs e)
    {
        EventDetailsPage.SelectedEvent = new EventItem
        {
            Title = "Tiffany’s Yoga Class",
            Type = "Yoga",
            Date = new DateTime(2026, 4, 20, 10, 0, 0), // ✅ FIXED
            Time = "10:00",
            Location = "Yoga Studio, st. Flowers",
            ParticipantsCount = 8,
            MaxParticipants = 10,
            Description = "Relaxing yoga session for all levels.",
            IsPaid = true,
            Price = 15
        };

        await Shell.Current.GoToAsync(nameof(EventDetailsPage));
    }
}

public class SearchSuggestionItem
{
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Rating { get; set; } = string.Empty;
    public bool IsConnected { get; set; } = false;

    public string Initial =>
        string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpper();

    public string ConnectButtonText => IsConnected ? "Connected" : "Connect";
}