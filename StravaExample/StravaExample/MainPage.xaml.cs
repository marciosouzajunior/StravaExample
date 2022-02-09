using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StravaExample
{
    public partial class MainPage : ContentPage
    {

        StravaService StravaService;

        public MainPage()
        {
            InitializeComponent();
            StravaService = DependencyService.Get<StravaService>();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckConnected();
        }

        private async void Connect_Clicked(object sender, EventArgs e)
        {
            await StravaService.Connect();
            CheckConnected();
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            await StravaService.Disconnect();
            CheckConnected();
        }

        private void Sync_Clicked(object sender, EventArgs e)
        {
            StravaService.Sync();
        }

        private void CheckConnected()
        {
            if (StravaService.IsConnected())
            {
                connectButton.IsVisible = false;
                disconnectButton.IsVisible = true;
            }
            else
            {
                connectButton.IsVisible = true;
                disconnectButton.IsVisible = false;
            }
        }
    }
}
