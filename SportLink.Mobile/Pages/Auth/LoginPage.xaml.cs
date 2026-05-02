using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Auth;

public partial class LoginPage : ContentPage
{
    private bool _isLoading = false;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;

            ErrorLabel.IsVisible = false;
            ErrorLabel.Text = "";

            if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
            {
                ShowError("Please enter your email or username.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ShowError("Please enter your password.");
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Text = "Logging in...";

            var result = await AuthenticationService.LoginAsync(
                UsernameEntry.Text.Trim(),
                PasswordEntry.Text
            );

            if (result == null)
            {
                ShowError("Login error (null result)");
                return;
            }

            if (result.Success)
            {
                if (RememberMeCheckBox.IsChecked)
                {
                    Preferences.Set("remember_email", UsernameEntry.Text.Trim());
                }

                
                await Shell.Current.GoToAsync("//Main");
            }
            else
            {
                ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Crash: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            LoginButton.IsEnabled = true;
            LoginButton.Text = "Log In";
        }
    }

    private async void OnSignUpTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert(
            "Password Reset",
            "Please contact our support team for password reset assistance.",
            "OK"
        );
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}