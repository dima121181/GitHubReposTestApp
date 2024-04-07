using GitHubReposTestApp.ViewModels;
using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GitHubReposTestApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepositoryListPage : ContentPage {
        public RepositoryListPage() {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            BindingContext = new RepositoryListViewModel() { Navigation = this.Navigation };

            StackLayout SLMain = new StackLayout { VerticalOptions = LayoutOptions.EndAndExpand };

            SearchBar SearchRep = new SearchBar { TextColor = Color.Black };
            SearchRep.SetBinding(SearchBar.TextProperty, "SearchText", BindingMode.TwoWay);
            Frame FrameSearchRep = new Frame { Content = SearchRep, BorderColor = Color.Black, CornerRadius = 2, Margin = 3, Padding = 3, VerticalOptions = LayoutOptions.End };

            Grid GridBottomPanel = Method.CreateGrid(3, 2);

            Label LabelUserName = new Label { Text = "Пользователь GitHub: ", FontSize = 19 * App.FontScale, TextColor = Color.Black, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };
            Entry EntryUserName = new Entry { FontSize = 19 * App.FontScale, TextColor = Color.Black, Placeholder = "Введите логин", ClearButtonVisibility = ClearButtonVisibility.WhileEditing, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
            EntryUserName.SetBinding(Entry.TextProperty, "UserLogin", BindingMode.TwoWay);

            GridBottomPanel.Children.Add(LabelUserName, 0, 0);
            GridBottomPanel.Children.Add(EntryUserName, 1, 0);

            Button ButtonAdd = new Button { Text = "Получить репозитории", FontSize = 19 * App.FontScale, VerticalOptions = LayoutOptions.End };
            ButtonAdd.SetBinding(Button.CommandProperty, "GetRepositoriesCommand");
            GridBottomPanel.Children.Add(ButtonAdd, 0, 1);
            Grid.SetColumnSpan(ButtonAdd, 2);

            Label LabelLastUpdate = new Label { FontSize = 16 * App.FontScale, TextColor = Color.Black, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
            LabelLastUpdate.SetBinding(Label.TextProperty, new Binding { Path = "LastUpdate", Converter = new LastUpdateValueConverter()});
            GridBottomPanel.Children.Add(LabelLastUpdate, 0, 2);
            Grid.SetColumnSpan(LabelLastUpdate, 2);

            ListView LVRepositoryList = new ListView { HasUnevenRows = true, SeparatorVisibility = SeparatorVisibility.None, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            LVRepositoryList.SetBinding(ListView.ItemsSourceProperty, "RepositoriesForView");
            LVRepositoryList.SetBinding(ListView.SelectedItemProperty, "SelectedRepository", BindingMode.TwoWay);
            LVRepositoryList.ItemTemplate = new DataTemplate(() => {
                StackLayout SLRepository = new StackLayout { Padding = 5, Spacing = 0 };
                Label LabName = new Label { FontSize = 19 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                LabName.SetBinding(Label.TextProperty, "name");
                Label LabHtml = new Label { FontSize = 14 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                LabHtml.SetBinding(Label.TextProperty, "html_url");
                Label LabDescription = new Label { FontSize = 17 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                LabDescription.SetBinding(Label.TextProperty, "description");

                SLRepository.Children.Add(LabName);
                SLRepository.Children.Add(LabHtml);
                SLRepository.Children.Add(LabDescription);

                Frame FrameSLRepository = new Frame { Content = SLRepository, BorderColor = Color.Black, CornerRadius = 5, Padding = 1, Margin = 3 };

                return new ViewCell { View = FrameSLRepository };
            });


            Frame FrameBottomPanel = new Frame { Content = GridBottomPanel, BorderColor = Color.Black, CornerRadius = 2, Margin = 3, Padding = 3, VerticalOptions = LayoutOptions.End };

            Label LAbelRepositoryList = new Label { FontSize = 21 * App.FontScale, TextColor = Color.Black, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
            LAbelRepositoryList.SetBinding(Label.TextProperty, new Binding { Path = "LastUpdateUserLogin", StringFormat = "Список репозиториев {0}" });

            SLMain.Children.Add(LAbelRepositoryList);
            SLMain.Children.Add(LVRepositoryList);
            SLMain.Children.Add(FrameSearchRep);
            SLMain.Children.Add(FrameBottomPanel);

            Content = Method.GetContentWithActivityIndicator(SLMain, null);
        }

        public class LastUpdateValueConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return "Последнее обновление: " + (value != null ? ((DateTime)value).ToString("dd.MM.yyyy HH:mm:ss") : "не выполнялось");
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotImplementedException();
            }
        }
    }
}