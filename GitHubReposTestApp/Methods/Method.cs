using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitHubReposTestApp {
    public class Method {
        private static NavigationPage _mainPage { get { return App.Current.MainPage as NavigationPage; } }
        async public static Task<string> GetKeyValue(string token) {
            return GetString(await SecureStorage.GetAsync(token));
        }
        async public static void SetKeyValue(string token, string value) {
            try {
                if (value == null)
                    SecureStorage.Remove(token);
                else
                    await SecureStorage.SetAsync(token, value);
            }
            catch (Exception ex) {
                await ShowError(ex);
            }
        }
        public static string GetString(object value) {
            return GetString(value, "");
        }
        public static string GetString(object value, string DefValue) {
            try {
                if (value != DBNull.Value && value != null)
                    return value.ToString().Trim();
                return DefValue;
            }
            catch {
                return DefValue;
            }
        }
        public static DateTime MoscowDateTimeConverter(DateTime dt) { return TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(dt, DateTimeKind.Local), TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow")); }
        public static DateTime MoscowDateTime { get { return MoscowDateTimeConverter(DateTime.Now); } }
        public static DateTime? GetDateTime(object value) {
            try {
                if (value != DBNull.Value && value != null) {
                    DateTime? res = Convert.ToDateTime(value);
                    return res > DateTime.MinValue ? res : null;
                }
                return null;
            }
            catch {
                return null;
            }
        }
        public static async Task ShowMessage(string text) {
            await ShowMessage("Информация", text);
        }
        public static async Task ShowMessage(string typ, string text) {
            await _mainPage.DisplayAlert(typ, text, "OK");
        }
        public static async Task ShowError(Exception ex) {
            await ShowError("Ошибка", ex);
        }
        public static async Task ShowError(string caption, Exception ex) {
            await ShowError(caption, "Ответ: " + ex.Message + "\nStackTrace: " + ex.StackTrace);
        }
        public static async Task ShowError(string caption, WebException ex) {
            string weberror = "Статус: " + ex.Status+ "\nСообщение: " + ex.Message;
            if (ex.Response != null) {
                weberror += "\nКод статуса: " + (ex.Response as HttpWebResponse).StatusCode + "\nОписание: " + (ex.Response as HttpWebResponse).StatusDescription;
                using (Stream data = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(data))
                    weberror += "\nОтвет: " + reader.ReadToEnd();
            }
            await ShowError(caption, weberror);
        }
        public static async Task ShowError(string caption, string text) {
            await _mainPage.DisplayAlert((GetString(caption).ToLower().Contains("ошибка") ? "" : "Ошибка ") + caption, text, "OK");
        }
        public static Grid CreateGrid() {
            return CreateGrid(1, 1, 0, 0, 0, 0);
        }
        public static Grid CreateGrid(int rowcount, int columncount) {
            return CreateGrid(rowcount, columncount, 0, 0, 0, 0);
        }
        public static Grid CreateGrid(int rowcount, int columncount, int padLR, int padTB, int marLR, int marTB) {
            return CreateGrid(rowcount, columncount, new Thickness(padLR, padTB), new Thickness(marLR, marTB));
        }
        public static Grid CreateGrid(int rowcount, int columncount, Thickness ThickP, Thickness ThickM) {
            Grid CommonGrid = new Grid {
                BackgroundColor = Color.Transparent,
                Padding = ThickP,
                Margin = ThickM,
                ColumnSpacing = 0,
                RowSpacing = 0
            };
            for (int i = 1; i <= rowcount; i++)
                CommonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            for (int i = 1; i <= columncount; i++)
                CommonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return CommonGrid;
        }

        public static Grid GetContentWithActivityIndicator(StackLayout SLMain, Page SourcePage) {
            Grid gridProgress = new Grid { BackgroundColor = new Color(1, 0.827, 0.65, 0.7), Padding = new Thickness(50) };
            if (SourcePage != null)
                gridProgress.SetBinding(VisualElement.IsVisibleProperty, new Binding { Source = SourcePage, Path = "IsBusy" });
            else
                gridProgress.SetBinding(VisualElement.IsVisibleProperty, "IsBusy");

            ActivityIndicator activity = new ActivityIndicator { IsEnabled = true, IsVisible = false, HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand, IsRunning = false };
            if (SourcePage != null)
                activity.SetBinding(VisualElement.IsVisibleProperty, new Binding { Source = SourcePage, Path = "IsBusy" });
            else
                activity.SetBinding(VisualElement.IsVisibleProperty, "IsBusy");
            if (SourcePage != null)
                activity.SetBinding(ActivityIndicator.IsRunningProperty, new Binding { Source = SourcePage, Path = "IsBusy" });
            else
                activity.SetBinding(ActivityIndicator.IsRunningProperty, "IsBusy");

            Label label = new Label { FontSize = 19 * App.FontScale, TextColor = Color.Black, BackgroundColor = Color.Transparent, Padding = 0, Margin = 0, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand, FontAttributes = FontAttributes.Bold };
            if (SourcePage != null)
                label.SetBinding(Label.TextProperty, new Binding { Source = SourcePage, Path = "ProgressDescription", StringFormat = "{0}..." });
            else
                label.SetBinding(Label.TextProperty, new Binding { Path = "ProgressDescription", StringFormat = "{0}..." });

            StackLayout activitySL = new StackLayout { Children = { label, activity }, Orientation = StackOrientation.Vertical, Padding = 0, Margin = 0, Spacing = 10, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

            Frame activityFrame = new Frame { Content = activitySL, BorderColor = Color.Black, BackgroundColor = Color.FromHex("#f5f5f5"), Padding = 10, Margin = 0, HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand, CornerRadius = 5, HasShadow = true };
            if (SourcePage != null)
                activityFrame.SetBinding(VisualElement.IsVisibleProperty, new Binding { Source = SourcePage, Path = "IsBusy" });
            else
                activityFrame.SetBinding(VisualElement.IsVisibleProperty, "IsBusy");

            gridProgress.Children.Add(activityFrame);

            Grid GridContent = new Grid();
            GridContent.Children.Add(SLMain);
            GridContent.Children.Add(gridProgress);

            return GridContent;
        }
    }
}
