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
        {
            var sUri = new Uri("ms-windows-store://review/?PFN=" + Package.Current.Id.FamilyName);
            await Windows.System.Launcher.LaunchUriAsync(sUri);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.uiVers.Text = Package.Current.Id.Version.Major + "." + Package.Current.Id.Version.Minor + "." + Package.Current.Id.Version.Build;
        }
    }
}