using System;
using Xamarin.Forms;
using XamarinForms.Views;
using Xamarin.Forms.Xaml;
using XamarinForms.Services;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace XamarinForms
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();
            
            DataStoreContainer = new DataStoreContainer(new FirebaseAuthService());

            MainPage = new MainPage();
		}

        public static DataStoreContainer DataStoreContainer { get; private set; }

		protected override void OnStart ()
		{
            // Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
