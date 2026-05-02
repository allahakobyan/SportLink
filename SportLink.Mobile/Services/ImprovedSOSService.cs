using System.Text.Json;
using Microsoft.Maui.ApplicationModel.Communication;


namespace SportLink.Mobile.Services;

/// <summary>
/// SOS Service - Handles emergency location sharing and SMS notifications
/// </summary>
public static class SOSService
{
    private const string ContactsKey = "sos_contacts";
    private const string SOSSessionKey = "sos_active_session";

    public class EmergencyContact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; } // e.g., "Parent", "Friend", "Police"
        public bool NotifySms { get; set; } = true;
        public bool NotifyApp { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class SOSSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double LastLatitude { get; set; }
        public double LastLongitude { get; set; }
        public string LocationAddress { get; set; }
        public List<string> NotifiedContacts { get; set; } = new();
    }

    private static List<EmergencyContact> _contacts = new();
    private static SOSSession? _currentSession;
    public static bool IsSOSActive { get; set; }
    public static bool IsSessionActive { get; set; }
    public static DateTime? SOSStartTime { get; set; }
    public static List<EmergencyContact> Contacts => _contacts;

    // ============ Contact Management ============

    public static async Task AddContactAsync(string name, string phone, string relationship)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Name and phone are required.");

        if (!IsValidPhoneNumber(phone))
            throw new ArgumentException("Invalid phone number format.");

        var contact = new EmergencyContact
        {
            Name = name,
            Phone = phone,
            Relationship = relationship
        };

        _contacts.Add(contact);
        await SaveContactsAsync();
    }

    public static async Task DeleteContactAsync(string contactId)
    {
        _contacts.RemoveAll(c => c.Id == contactId);
        await SaveContactsAsync();
    }

    public static async Task UpdateContactAsync(string contactId, string name, string phone, string relationship)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact == null)
            throw new InvalidOperationException("Contact not found.");

        contact.Name = name;
        contact.Phone = phone;
        contact.Relationship = relationship;
        await SaveContactsAsync();
    }

    public static async Task LoadContactsAsync()
    {
        var json = Preferences.Get(ContactsKey, "[]");
        _contacts = JsonSerializer.Deserialize<List<EmergencyContact>>(json) ?? new List<EmergencyContact>();
        await Task.CompletedTask;
    }

    public static async Task SaveContactsAsync()
    {
        var json = JsonSerializer.Serialize(_contacts);
        Preferences.Set(ContactsKey, json);
        await Task.CompletedTask;
    }

    // ============ SOS Activation ============

    public static async Task<bool> ActivateSOSAsync()
    {
        if (_contacts.Count == 0)
            throw new InvalidOperationException("Add emergency contacts before activating SOS.");

        _currentSession = new SOSSession { StartTime = DateTime.UtcNow };
        IsSOSActive = true;
        IsSessionActive = true;
        SOSStartTime = DateTime.UtcNow;

        // Get current location
        try
        {
            var location = await GetCurrentLocationAsync();
            _currentSession.LastLatitude = location.latitude;
            _currentSession.LastLongitude = location.longitude;
            _currentSession.LocationAddress = await GetAddressFromCoordinatesAsync(location.latitude, location.longitude);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }

        // Send notifications to emergency contacts
        await NotifyEmergencyContactsAsync(_currentSession);

        return true;
    }

    public static async Task<bool> DeactivateSOSAsync()
    {
        if (_currentSession != null)
        {
            _currentSession.EndTime = DateTime.UtcNow;
        }

        IsSOSActive = false;
        IsSessionActive = false;
        SOSStartTime = null;

        return await Task.FromResult(true);
    }

    public static async Task<bool> UpdateLocationAsync()
    {
        if (!IsSOSActive || _currentSession == null)
            return false;

        try
        {
            var location = await GetCurrentLocationAsync();
            _currentSession.LastLatitude = location.latitude;
            _currentSession.LastLongitude = location.longitude;
            _currentSession.LocationAddress = await GetAddressFromCoordinatesAsync(location.latitude, location.longitude);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============ Notification System ============

    private static async Task NotifyEmergencyContactsAsync(SOSSession session)
    {
        var smsContacts = _contacts.Where(c => c.NotifySms).ToList();

        foreach (var contact in smsContacts)
        {
            try
            {
                await SendSOSMessageAsync(contact, session);
                session.NotifiedContacts.Add(contact.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to notify {contact.Name}: {ex.Message}");
            }
        }
    }

    private static async Task SendSOSMessageAsync(EmergencyContact contact, SOSSession session)
    {
        // Construct SOS message with location
        string message = $"🆘 EMERGENCY ALERT from SportLink\n" +
                        $"Location: {session.LocationAddress}\n" +
                        $"Coordinates: {session.LastLatitude:F4}, {session.LastLongitude:F4}\n" +
                        $"Time: {DateTime.Now:g}\n" +
                        $"Tap link to view: https://maps.google.com/?q={session.LastLatitude},{session.LastLongitude}";

        // In production, integrate with Twilio or similar SMS service
        // Example: await SendSmsViaTwilioAsync(contact.Phone, message);

        // For now, use device native SMS
        try
        {
            var sms = new SmsMessage(message, contact.Phone);
            await Sms.Default.ComposeAsync(sms);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SMS Send Error: {ex.Message}");
        }
    }

    // ============ Location Services ============

    private static async Task<(double latitude, double longitude)> GetCurrentLocationAsync()
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location == null)
                throw new Exception("Unable to get location.");

            return (location.Latitude, location.Longitude);
        }
        catch (Exception ex)
        {
            throw new Exception($"Location error: {ex.Message}");
        }
    }

    private static async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var location = new Location(latitude, longitude);
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                return $"{placemark.Thoroughfare}, {placemark.Locality}, {placemark.CountryName}";
            }

            return $"{latitude:F4}, {longitude:F4}";
        }
        catch
        {
            return $"{latitude:F4}, {longitude:F4}";
        }
    }

    // ============ Validation ============

    private static bool IsValidPhoneNumber(string phone)
    {
        // Remove common formatting characters
        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return cleaned.Length >= 10 && (cleaned.StartsWith("+") || cleaned.Length == 10 || cleaned.Length == 11);
    }
}
