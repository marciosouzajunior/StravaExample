using StravaExample.Models;
using StravaExample.Services;
using StravaExample.Services.Impl;
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

        public ImageSource StravaLogo { get; set; }
        IStravaService stravaService;
        IDialog dialog;

        public MainPage()
        {
            InitializeComponent();
            StravaLogo = ImageSource.FromResource("StravaExample.strava_logo.jpg");
            stravaService = DependencyService.Get<IStravaService>();
            dialog = DependencyService.Get<IDialog>();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckConnected();

            stravaService.Connected += OnConnected;
            stravaService.Disconnected += OnDisconnected;
            stravaService.ActivityAdded += OnActivityAdded;
            stravaService.SyncCompleted += OnSyncCompleted;
        }

        protected override void OnDisappearing()
        {
            stravaService.Connected -= OnConnected;
            stravaService.Disconnected -= OnDisconnected;
            stravaService.ActivityAdded -= OnActivityAdded;
            stravaService.SyncCompleted -= OnSyncCompleted;
        }

        private async void Connect_Clicked(object sender, EventArgs e)
        {
            await stravaService.Connect();
            CheckConnected();
        }

        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            await stravaService.Disconnect();
            CheckConnected();
        }

        private async void AddActivity_Clicked(object sender, EventArgs e)
        {
            IDatabase database = DependencyService.Get<IDatabase>();
            Random rnd = new Random();

            // Activity mock
            AppActivity appActivity = new AppActivity();
            appActivity.Name = "Test Activity " + DateTime.Now.ToString("G");
            appActivity.StartDate = DateTime.Now;
            appActivity.ElapsedTime = rnd.Next(1200, 1800);
            await database.Insert(appActivity);

            for (int i = 0; i < appActivity.ElapsedTime; i += 5)
            {
                AppActivityHR appActivityHR = new AppActivityHR();
                appActivityHR.AppActivityId = appActivity.Id;
                appActivityHR.SecondsMeasure = i;
                appActivityHR.HR = rnd.Next(150, 190);
                await database.Insert(appActivityHR);
            }

            await stravaService.NewActivity(appActivity);
        }

        private void Sync_Clicked(object sender, EventArgs e)
        {
            stravaService.Sync();     
        }

        private async void OnConnected(object sender, EventArgs e)
        {
            await dialog.DisplayAlert("Connect", "Connected to Strava.", "OK");
        }

        private async void OnDisconnected(object sender, EventArgs e)
        {
            await dialog.DisplayAlert("Disconnect", "Disconnected from Strava.", "OK");
        }

        private async void OnActivityAdded(object sender, EventArgs e)
        {
            await dialog.DisplayAlert("NewActivity", "Activity inserted and ready to sync.", "OK");
        }

        private async void OnSyncCompleted(object sender, EventArgs e)
        {
            SyncEventArgs syncEventArgs = e as SyncEventArgs;

            if (syncEventArgs.IsSuccessResult)
            {
                await dialog.DisplayAlert("Sync", "Sync completed successfully." +
                    "\nDownloads: " + syncEventArgs.DownloadCount +
                    "\nUploads: " + syncEventArgs.UploadCount, 
                    "OK");
            }
            else
            {
                await dialog.DisplayAlert("Sync", "Failed to sync. Check logs for error.", "OK");
            }
        }

        private void CheckConnected()
        {
            if (stravaService.IsConnected())
            {
                connectButton.IsEnabled = false;
                disconnectButton.IsEnabled = true;
                addActivityButton.IsEnabled = true;
                syncButton.IsEnabled = true;
            }
            else
            {
                connectButton.IsEnabled = true;
                disconnectButton.IsEnabled = false;
                addActivityButton.IsEnabled = false;
                syncButton.IsEnabled = false;
            }
        }
    }
}
