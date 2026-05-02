using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    public static string CurrentUserFullName = "";
    public static string CurrentUsername = "";
    public static int EventsAttended = 0;
    public static int Connections = 0;
    public static double Rating = 0;

    private string _profileImagePath = "";

    public ProfilePage()
    {
        InitializeComponent();
        SetPostsTabActive();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Get current user from auth service
        var currentUser = Task.Run(async () => await AuthenticationService.GetCurrentUserAsync()).Result;

        if (currentUser != null)
        {
            FullNameLabel.Text = currentUser.FullName;
            UsernameLabel.Text = "@" + currentUser.Username;
            _profileImagePath = currentUser.ProfileImageUrl;

            // Load profile image if exists
            if (!string.IsNullOrEmpty(_profileImagePath))
            {
                ProfileImage.Source = ImageSource.FromFile(_profileImagePath);
            }
        }

        EventsAttendedLabel.Text = EventsAttended.ToString();
        ConnectionsLabel.Text = Connections.ToString();
        RatingLabel.Text = $"{Rating:0.0}/5";

        if (currentUser != null)
        {
            var userPosts = PostService.Posts
                .Where(p => p.Username == currentUser.FullName)
                .ToList();

            UserPostsList.ItemsSource = userPosts;
        }
    }

    private void SetPostsTabActive()
    {
        PostsSection.IsVisible = true;
        PastActivitiesSection.IsVisible = false;
    }

    private void SetPastActivitiesTabActive()
    {
        PostsSection.IsVisible = false;
        PastActivitiesSection.IsVisible = true;
    }

    private void OnPostsTabClicked(object sender, EventArgs e)
    {
        SetPostsTabActive();
    }

    private void OnPastActivitiesTabClicked(object sender, EventArgs e)
    {
        SetPastActivitiesTabActive();
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Edit Name", "Enter name:");

        if (!string.IsNullOrWhiteSpace(name))
        {
            CurrentUserFullName = name;
            FullNameLabel.Text = name;

            // Update in auth service
            var result = await AuthenticationService.UpdateProfileAsync(name, "", _profileImagePath);
            if (result.Success)
            {
                await DisplayAlert("Success", "Profile updated!", "OK");
            }
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet(
            "Settings",
            "Cancel",
            null,
            "Change Password",
            "Privacy Settings",
            "Logout"
        );

        if (action == "Logout")
        {
            await AuthenticationService.LogoutAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    /// <summary>
    /// Handle profile image tap - Show camera/gallery options
    /// </summary>
    private async void OnChangeImageTapped(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet(
            "Choose Profile Picture",
            "Cancel",
            null,
            "Take Photo",
            "Choose from Gallery"
        );

        if (action == "Take Photo")
        {
            await TakePhotoAsync();
        }
        else if (action == "Choose from Gallery")
        {
            await PickPhotoFromGalleryAsync();
        }
    }

    /// <summary>
    /// Take photo with device camera
    /// </summary>
    private async Task TakePhotoAsync()
    {
        try
        {
            // Request camera permission
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Camera permission is required to take photos.", "OK");
                return;
            }

            // Take photo
            var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take Profile Picture"
            });

            if (photo != null)
            {
                var newFile = Path.Combine(FileSystem.CacheDirectory, $"profile_{DateTime.Now.Ticks}.jpg");
                using (var stream = await photo.OpenReadAsync())
                {
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                    }
                }

                // Update UI
                ProfileImage.Source = ImageSource.FromFile(newFile);
                _profileImagePath = newFile;

                // Save to auth service
                var result = await AuthenticationService.UpdateProfilePictureAsync(newFile);
                if (result.Success)
                {
                    await DisplayAlert("Success", "Profile picture updated!", "OK");
                }
            }
        }
        catch (FeatureNotSupportedException ex)
        {
            await DisplayAlert("Error", "Camera not supported on this device.", "OK");
        }
        catch (PermissionException ex)
        {
            await DisplayAlert("Permission Error", "Camera permission is required.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to take photo: {ex.Message}", "OK");
        }
    }

    /// <summary>
    ///  Pick photo from device
    /// </summary>
    private async Task PickPhotoFromGalleryAsync()
    {
        try
        {
            // Request storage permission
            var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (storageStatus != PermissionStatus.Granted)
            {
                storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            if (storageStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Storage permission is required to access photos.", "OK");
                return;
            }

            // Pick photo
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Choose Profile Picture"
            });

            if (photo != null)
            {
                var newFile = Path.Combine(FileSystem.CacheDirectory, $"profile_{DateTime.Now.Ticks}.jpg");
                using (var stream = await photo.OpenReadAsync())
                {
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                    }
                }

                // Update UI
                ProfileImage.Source = ImageSource.FromFile(newFile);
                _profileImagePath = newFile;

                // Save to auth service
                var result = await AuthenticationService.UpdateProfilePictureAsync(newFile);
                if (result.Success)
                {
                    await DisplayAlert("Success", "Profile picture updated!", "OK");
                }
            }
        }
        catch (PermissionException ex)
        {
            await DisplayAlert("Permission Error", "Storage permission is required.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick photo: {ex.Message}", "OK");
        }
    }
}
