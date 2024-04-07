using GitHubReposTestApp.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GitHubReposTestApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepositoryContentPage : ContentPage {
        public List<RepositoryContentViewModel> ViewModel { get; private set; }
        private string _ProgressDescription = "Подождите";
        public string ProgressDescription {
            get => _ProgressDescription;
            set {
                if (value != _ProgressDescription) {
                    _ProgressDescription = value;
                    OnPropertyChanged("ProgressDescription");
                }
            }
        }

        private bool _IsBusy = false;
        public new bool IsBusy {
            get => _IsBusy;
            set {
                if (value != _IsBusy) {
                    _IsBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }
        public RepositoryContentPage(RepositoryViewModel _rvm, List<RepositoryContentViewModel> _rcvm, string SubDirectory, bool IsFile) {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            string URL = Method.GetString(_rvm?.url) + (!string.IsNullOrWhiteSpace(_rvm?.url) ? "/contents/" + SubDirectory : "");

            StackLayout SLRepositoryFiles = new StackLayout { Spacing = 0 };
            Label LabelHeader = new Label { Text = "Репозиторий " + (_rvm != null ? !string.IsNullOrWhiteSpace(_rvm.name) ? _rvm.name : "без наименования" : "не передан  на страницу"), FontSize = 21 * App.FontScale, TextColor = Color.Black, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
            Label LabelURL = new Label { Text = "URL: " + URL, FontSize = 18 * App.FontScale, TextColor = Color.Black, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };

            Grid GridHeaders = Method.CreateGrid(1, 3);
            GridHeaders.ColumnDefinitions[1].Width = new GridLength(4, GridUnitType.Star);

            Label LabTypeHeader = new Label { Text = "Тип", FontSize = 15 * App.FontScale, FontAttributes = FontAttributes.Bold, TextColor = Color.Black, BackgroundColor = Color.Transparent };
            Frame FrameTypeHeader = new Frame { Content = LabTypeHeader, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

            Label LabNameHeader = new Label { Text = "Наименование", FontSize = 15 * App.FontScale, FontAttributes = FontAttributes.Bold, TextColor = Color.Black, BackgroundColor = Color.Transparent };
            Frame FrameNameHeader = new Frame { Content = LabNameHeader, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

            Label LabSizeHeader = new Label { Text = "Размер", FontSize = 15 * App.FontScale, FontAttributes = FontAttributes.Bold, TextColor = Color.Black, BackgroundColor = Color.Transparent };
            Frame FrameSizeHeader = new Frame { Content = LabSizeHeader, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

            GridHeaders.Children.Add(FrameTypeHeader, 0, 0);
            GridHeaders.Children.Add(FrameNameHeader, 1, 0);
            GridHeaders.Children.Add(FrameSizeHeader, 2, 0);

            ListView LVFilesList = new ListView { HasUnevenRows = true, SeparatorVisibility = SeparatorVisibility.None, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

            ViewModel = _rcvm;

            if (ViewModel == null && !string.IsNullOrWhiteSpace(URL) && !string.IsNullOrWhiteSpace(SubDirectory)) {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(URL);
                myRequest.Method = "GET";
                myRequest.ContentType = "application/json";
                myRequest.UserAgent = "request";
                if (!string.IsNullOrWhiteSpace(App.GitHubToken))
                    myRequest.Headers.Add("Authorization", "token " + App.GitHubToken);

                string Response = string.Empty;
                try {
                    using (WebResponse myResponse = myRequest.GetResponse())
                    using (Stream stream = myResponse.GetResponseStream())
                    using (StreamReader sr = new StreamReader(stream))
                        Response = sr.ReadToEnd();

                    try {
                        if (!string.IsNullOrWhiteSpace(Method.GetString(Response)) && Method.GetString(Response) != "[]" && Method.GetString(Response) != "{}") {
                            if (IsFile) {
                                ViewModel = new List<RepositoryContentViewModel>();
                                RepositoryContentViewModel FileContent = (RepositoryContentViewModel)JsonConvert.DeserializeObject(Response, typeof(RepositoryContentViewModel));
                                if (FileContent != null)
                                    ViewModel.Add(FileContent);
                            }
                            else
                                ViewModel = (List<RepositoryContentViewModel>)JsonConvert.DeserializeObject(Response, typeof(List<RepositoryContentViewModel>));
                        }
                        else
                            ViewModel = new List<RepositoryContentViewModel>();
                    }
                    catch (Exception exd) {
                        Device.BeginInvokeOnMainThread(async () => { await Method.ShowError($"Repository Content from \"{URL}\" Deserialize", exd); });
                    }
                }
                catch (WebException ex) {
                    if (ex.Response != null && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("GetRepositories Content WebException", $"Данные по адресу \"{URL}\" не найдены"); });
                    else
                        Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("GetRepositories Content WebException", ex); });
                }
                catch (Exception ex) {
                    Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("GetRepositories Content Exception", ex); });
                }
            }

            if (ViewModel != null && ViewModel.Count > 0)
                ViewModel = ViewModel?.OrderBy(rc => rc.IsDir ? 1 : 2)?.ThenBy(rc => rc.name)?.ToList();

            BindingContext = ViewModel;

            LVFilesList.SetBinding(ListView.ItemsSourceProperty, ".");
            LVFilesList.ItemTemplate = new DataTemplate(() => {
                Grid GridRow = Method.CreateGrid(IsFile? 2 : 1, 3);
                GridRow.ColumnDefinitions[1].Width = new GridLength(4, GridUnitType.Star);
                TapGestureRecognizer TapGridRow = new TapGestureRecognizer();
                TapGridRow.Tapped += async (sender, e) => {
                    try {
                        IsBusy = true;
                        if (!IsFile && (sender as Frame).BindingContext is RepositoryContentViewModel rcvm)
                            await Navigation.PushAsync(new RepositoryContentPage(_rvm, null, (Method.GetString(SubDirectory) + "/" + rcvm.name).Trim('/'), rcvm.IsFile));
                    }
                    catch (Exception tex) {
                        await Method.ShowError("TapGridRow", tex);
                    }
                    finally {
                        IsBusy = false;
                    }
                };

                Label LabType = new Label { FontSize = 15 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                LabType.SetBinding(Label.TextProperty, "TypeName");
                Frame FrameType = new Frame { Content = LabType, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                FrameType.GestureRecognizers.Add(TapGridRow);

                Label LabName = new Label { FontSize = 15 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                LabName.SetBinding(Label.TextProperty, "name");
                Frame FrameName = new Frame { Content = LabName, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                FrameName.GestureRecognizers.Add(TapGridRow);

                Label LabSize = new Label { FontSize = 15 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                LabSize.SetBinding(Label.TextProperty, "size");
                Frame FrameSize = new Frame { Content = LabSize, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                FrameSize.GestureRecognizers.Add(TapGridRow);

                GridRow.Children.Add(FrameType, 0, 0);
                GridRow.Children.Add(FrameName, 1, 0);
                GridRow.Children.Add(FrameSize, 2, 0);
                if (IsFile) {
                    Label LabContent = new Label { FontSize = 15 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent };
                    LabContent.SetBinding(Label.TextProperty, "ContentString");
                    Frame FrameContent = new Frame { Content = LabContent, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                    GridRow.Children.Add(FrameContent, 0, 1);
                    Grid.SetColumnSpan(FrameContent, 3);
                }

                return new ViewCell { View = GridRow };
            });
            Content = Method.GetContentWithActivityIndicator(new StackLayout { Children = { LabelHeader, LabelURL, GridHeaders, LVFilesList } }, this);
        }

        public class FileContentVisibleConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return !string.IsNullOrWhiteSpace(Method.GetString(value));
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotImplementedException();
            }
        }
        public class FileContentRowHeightConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return !string.IsNullOrWhiteSpace(Method.GetString(value)) ? new GridLength(1, GridUnitType.Auto) : new GridLength(0, GridUnitType.Absolute);
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotImplementedException();
            }
        }
    }
}