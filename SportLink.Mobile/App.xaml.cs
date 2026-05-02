using Microsoft.Extensions.DependencyInjection;
using SportLink.Mobile.Pages; // IMPORTANT

namespace SportLink.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Starting with SplashPage 
            return new Window(new NavigationPage(new SplashPage()));
        }
    }
}