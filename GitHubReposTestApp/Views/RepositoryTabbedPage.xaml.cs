using GitHubReposTestApp.ViewModels;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace GitHubReposTestApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepositoryTabbedPage : Xamarin.Forms.TabbedPage {
        public RepositoryTabbedPage(RepositoryViewModel _rvm, List<RepositoryContentViewModel> _rcvm) {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            BarBackgroundColor = Color.Gray;
            _ = On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            BarTextColor = Color.White;
            UnselectedTabColor = Color.White;
            SelectedTabColor = Color.Orange;

            Children.Add(new NavigationPage(new RepositoryContentPage(_rvm, _rcvm, string.Empty, false)) { Title = "Файлы", IconImageSource = "files" });
            Children.Add(new NavigationPage(new RepositoryPage(_rvm)) { Title = "Свойства", IconImageSource = "properties" });
        }
    }
}