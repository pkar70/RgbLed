using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RgbLed
{
    public sealed partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }

        private void uiInfoOk_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void uiRate_Click(object sender, RoutedEventArgs e)
        {   // pod Android tu nie wejdzie, bo jest zabezpieczenie w XAML, ale i tak...
#if !__ANDROID__
            var sUri = new Uri("ms-windows-store://review/?PFN=" + Package.Current.Id.FamilyName);
            await Windows.System.Launcher.LaunchUriAsync(sUri);
#endif 
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            uiVers.Text = p.k.GetAppVers();
        }
    }
}