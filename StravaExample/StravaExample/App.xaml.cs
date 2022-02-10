using StravaExample.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StravaExample
{
    public partial class App : Application
    {
        IDatabase database = DependencyService.Get<IDatabase>();
        public App()
        {
            InitializeComponent();
            Task.Run(async () => await database.Initialize()).Wait();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
