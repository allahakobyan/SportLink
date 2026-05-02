namespace SportLink.Mobile.Pages;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        Animate();
    }

    private async void Animate()
    {
        if (Logo == null || TitleLabel == null)
            return;

        // Logo fade
        await Logo.FadeTo(1, 800);

        await Task.Delay(300);

        //  text in center
        TitleLabel.Opacity = 1;

        string text = "SportLink";
        TitleLabel.Text = "";

        for (int i = 0; i < text.Length; i++)
        {
            TitleLabel.Text += text[i];
            await Task.Delay(80);
        }

        await Task.Delay(1000);

        // Go to app
        Application.Current.MainPage = new AppShell();
    }
}