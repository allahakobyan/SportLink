using SportLink.Mobile.Services;

namespace SportLink.Mobile.Pages.Auth;

[QueryProperty(nameof(Email), "email")]
public partial class VerifyEmailPage : ContentPage
{
    private string _email;
    private string _password;
    private bool _isLoading = false;

    public string Email
    {
        get => _email;
        set => _email = Uri.UnescapeDataString(value ?? "");
    }

    public VerifyEmailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailLabel.Text = $"Verification code sent to {Email}";
    }

    /// <summary>
    /// Verify email code and complete registration
    /// </summary>
    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        // Clear previous error
        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = "";

        // Validate code
        if (string.IsNullOrWhiteSpace(VerificationCodeEntry.Text))
        {
            ShowError("Please enter the verification code.");
            return;
        }

        if (VerificationCodeEntry.Text.Length != 6)
        {
            ShowError("Verification code must be 6 digits.");
            return;
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(PasswordConfirmEntry.Text))
        {
            ShowError("Please confirm your password.");
            return;
        }

        try
        {
            _isLoading = true;
            VerifyButton.IsEnabled = false;
            VerifyButton.Text = "Verifying...";

            //  VERIFY EMAIL WITH CODE
            var result = await AuthenticationService.VerifyEmailAsync(
                Email,
                VerificationCodeEntry.Text.Trim(),
                PasswordConfirmEntry.Text
            );

            if (result.Success)
            {
                //  REGISTRATION COMPLETE
                await DisplayAlert(
                    "Success!",
                    "Your account has been created and verified. Welcome to SportLink!",
                    "OK"
                );

                // Navigate to main app
                await Shell.Current.GoToAsync("//Main/FeedPage");
            }
            else
            {
                ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Verification error: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            VerifyButton.IsEnabled = true;
            VerifyButton.Text = "Verify & Complete Registration";
        }
    }

    /// <summary>
    /// Resend verification code
    /// </summary>
    private async void OnResendClicked(object sender, TappedEventArgs e)
    {
        await DisplayAlert(
            "Code Resent",
            $"A new verification code has been sent to {Email}",
            "OK"
        );

        // In production, call API to resend code
    }

    /// <summary>
    /// Show error message
    /// </summary>
    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}