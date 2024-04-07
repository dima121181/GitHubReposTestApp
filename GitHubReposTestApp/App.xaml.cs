using GitHubReposTestApp.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GitHubReposTestApp {
    public partial class App : Application {
        public static double FontScale { get; set; } = 1;
        public static double ElementScale { get; set; } = 1;
        public App() {
            InitializeComponent();

            MainPage = new NavigationPage(new RepositoryListPage());
        }

        public static string GitHubToken { get; set; } = string.Empty;

        protected override void OnStart() {
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
