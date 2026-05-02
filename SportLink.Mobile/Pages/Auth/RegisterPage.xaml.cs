using System.Security.Cryptography;
using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Auth;

public partial class RegisterPage : ContentPage
{
    private bool _isLoading = false;

    public RegisterPage()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        string password = e.NewTextValue ?? "";

        UpdateRequirement(Req8Chars, password.Length >= 8);
        UpdateRequirement(ReqUppercase, password.Any(char.IsUpper));
        UpdateRequirement(ReqLowercase, password.Any(char.IsLower));
        UpdateRequirement(ReqNumber, password.Any(char.IsDigit));

        string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        UpdateRequirement(ReqSymbol, password.Any(c => symbols.Contains(c)));
    }

    private void UpdateRequirement(Label label, bool isMet)
    {
        label.Text = isMet ? "✓" : "✕";
        label.TextColor = isMet ? Color.FromArgb("#27AE60") : Color.FromArgb("#E74C3C");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = "";

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError("Please enter your full name.");
            return;
        }

        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            ShowError("Please choose a username.");
            return;
        }

        if (UsernameEntry.Text.Length < 3)
        {
            ShowError("Username must be at least 3 characters.");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ShowError("Please enter your email address.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("Please create a password.");
            return;
        }

        try
        {
            _isLoading = true;
            CreateButton.IsEnabled = false;
            CreateButton.Text = "Creating account...";

            var result = await AuthenticationService.RegisterAsync(
                NameEntry.Text.Trim(),
                UsernameEntry.Text.Trim(),
                EmailEntry.Text.Trim(),
                PasswordEntry.Text
            );

            if (result.Success)
            {
                await DisplayAlert(
                    "Verification Email Sent",
                    $"We've sent a verification code to {EmailEntry.Text}",
                    "OK"
                );

                // ✅ FIXED NAVIGATION
                await Shell.Current.GoToAsync(
                    nameof(VerifyEmailPage) + $"?email={EmailEntry.Text.Trim()}"
                );
            }
            else
            {
                ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Registration error: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            CreateButton.IsEnabled = true;
            CreateButton.Text = "Create Account";
        }
    }

    private async void OnLoginTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}