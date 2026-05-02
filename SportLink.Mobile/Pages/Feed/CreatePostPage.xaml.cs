using SportLink.Mobile.Models;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Feed;

public partial class CreatePostPage : ContentPage
{
    private string _selectedImagePath = "";
    private bool _isLoading = false;

    public CreatePostPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Go back to previous page
    /// </summary>
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// ✅ CAMERA: Take photo with device camera
    /// </summary>
    private async void OnTakePhotoClicked(object sender, EventArgs e)
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
                Title = "Take Post Photo"
            });

            if (photo != null)
            {
                var newFile = Path.Combine(FileSystem.CacheDirectory, $"post_{DateTime.Now.Ticks}.jpg");
                using (var stream = await photo.OpenReadAsync())
                {
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                    }
                }

                // Store path and show preview
                _selectedImagePath = newFile;
                PostImage.Source = ImageSource.FromFile(newFile);
                ImagePreviewFrame.IsVisible = true;

                await DisplayAlert("Success", "Photo added to post!", "OK");
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
    /// ✅ PHOTO GALLERY: Pick photo from device
    /// </summary>
    private async void OnChoosePhotoClicked(object sender, EventArgs e)
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
                Title = "Choose Post Photo"
            });

            if (photo != null)
            {
                var newFile = Path.Combine(FileSystem.CacheDirectory, $"post_{DateTime.Now.Ticks}.jpg");
                using (var stream = await photo.OpenReadAsync())
                {
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                    }
                }

                // Store path and show preview
                _selectedImagePath = newFile;
                PostImage.Source = ImageSource.FromFile(newFile);
                ImagePreviewFrame.IsVisible = true;

                await DisplayAlert("Success", "Photo added to post!", "OK");
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

    /// <summary>
    /// Remove selected image from post
    /// </summary>
    private void OnRemoveImageClicked(object sender, EventArgs e)
    {
        _selectedImagePath = "";
        PostImage.Source = null;
        ImagePreviewFrame.IsVisible = false;
    }

    /// <summary>
    /// Publish the post
    /// </summary>
    private async void OnPublishClicked(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        // Validate caption
        if (string.IsNullOrWhiteSpace(CaptionEditor.Text))
        {
            await DisplayAlert("Error", "Please write something in your post.", "OK");
            return;
        }

        try
        {
            _isLoading = true;
            PublishButton.IsEnabled = false;
            PublishButton.Text = "Publishing...";

            // Get current user
            var currentUser = await AuthenticationService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                await DisplayAlert("Error", "No user logged in.", "OK");
                return;
            }

            // Create post
            var post = new PostItem
            {
                Username = currentUser.FullName,
                ProfileImageUrl = currentUser.ProfileImageUrl,
                Caption = CaptionEditor.Text.Trim(),
                ImageUrl = _selectedImagePath,
                CreatedAt = DateTime.UtcNow.ToString("g")
            };

            // Save post
            await PostService.AddPostAsync(post);

            await DisplayAlert("Success", "Post published!", "OK");

            // Clear form
            CaptionEditor.Text = "";
            _selectedImagePath = "";
            ImagePreviewFrame.IsVisible = false;

            // Go back
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to publish post: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false;
            PublishButton.IsEnabled = true;
            PublishButton.Text = "Publish Post";
        }
    }
}