using GitHubReposTestApp.Models;
using GitHubReposTestApp.ViewModels;
using System;
using System.Globalization;
using System.Reflection;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GitHubReposTestApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepositoryPage : ContentPage {
        public RepositoryViewModel ViewModel { get; private set; }
        public RepositoryPage(RepositoryViewModel _rvm) {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ViewModel = _rvm;
            BindingContext = ViewModel;

            FillProperties();
        }

        public async void FillProperties() {
            StackLayout SLRepositoryProperties = new StackLayout { Spacing = 0 };
            PropertyInfo[] Properties = typeof(Repository).GetProperties();
            if (Properties != null)
                foreach (PropertyInfo Property in Properties) {
                    try {
                        Grid GridProperty = new Grid {
                            BackgroundColor = Color.Transparent,
                            ColumnSpacing = 0,
                            RowSpacing = 0,
                            RowDefinitions = {
                                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
                            },
                            ColumnDefinitions = {
                                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }
                            },
                            Margin = 0
                        };

                        Label LabName = new Label { Text = Property.Name, FontSize = 15 * App.FontScale, TextColor = Color.Black, Padding = new Thickness(5, 0, 0, 0), HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                        Frame FrameName = new Frame { Content = LabName, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                        Label LabValue = new Label { FontSize = 15 * App.FontScale, TextColor = Color.Black, Padding = new Thickness(5, 0, 0, 0), HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                        Frame FrameValue = new Frame { Content = LabValue, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

                        GridProperty.Children.Add(FrameName, 0, 0);
                        GridProperty.Children.Add(FrameValue, 1, 0);

                        SLRepositoryProperties.Children.Add(GridProperty);

                        PropertyInfo[] OtherProperties = Property.PropertyType == typeof(RepositoryOwner) ? typeof(RepositoryOwner).GetProperties() : Property.PropertyType == typeof(RepositoryLicense) ? typeof(RepositoryLicense).GetProperties() : null;
                        if (OtherProperties != null) {
                            foreach (PropertyInfo OtherProperty in OtherProperties) {
                                try {
                                    Grid GridOtherProperty = new Grid {
                                        BackgroundColor = Color.Transparent,
                                        ColumnSpacing = 0,
                                        RowSpacing = 0,
                                        RowDefinitions = {
                                            new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
                                        },
                                        ColumnDefinitions = {
                                            new ColumnDefinition { Width = new GridLength(0.2, GridUnitType.Star) },
                                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                            new ColumnDefinition { Width = new GridLength(3.8, GridUnitType.Star) }
                                        },
                                        Margin = 0
                                    };

                                    Label LabOthPropName = new Label { Text = OtherProperty.Name, FontSize = 15 * App.FontScale, TextColor = Color.Black, Padding = new Thickness(5, 0, 0, 0), HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                                    Frame FrameOthPropName = new Frame { Content = LabOthPropName, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                                    Label LabOthPropValue = new Label { FontSize = 15 * App.FontScale, TextColor = Color.Black, Padding = new Thickness(5, 0, 0, 0), HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                                    LabOthPropValue.SetBinding(Label.TextProperty, new Binding { Path = Property.Name + "." + OtherProperty.Name, Converter = new PropertyValueConverter() });
                                    Frame FrameOthPropValue = new Frame { Content = LabOthPropValue, BorderColor = Color.Gray, CornerRadius = 0, Padding = 1, Margin = 0, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

                                    GridOtherProperty.Children.Add(FrameOthPropName, 1, 0);
                                    GridOtherProperty.Children.Add(FrameOthPropValue, 2, 0);

                                    SLRepositoryProperties.Children.Add(GridOtherProperty);
                                }
                                catch (Exception ex) {
                                    await Method.ShowError("Заполнение RepositoryPage SubProperties", ex);
                                }
                            }
                        }
                        else
                            LabValue.SetBinding(Label.TextProperty, new Binding { Path = Property.Name, Converter = new PropertyValueConverter() });
                    }
                    catch (Exception ex) {
                        await Method.ShowError("Заполнение RepositoryPage", ex);
                    }
                }

            if (SLRepositoryProperties.Children.Count == 0)
                SLRepositoryProperties.Children.Add(new Label { Text = "Не получены данные репозитория" + (ViewModel != null && !string.IsNullOrWhiteSpace(Method.GetString(ViewModel.name)) ? " " + ViewModel.name : ""), FontSize = 13 * App.FontScale });

            Content = new StackLayout { Children = { new Label { Text = "Репозиторий " + (ViewModel != null ? !string.IsNullOrWhiteSpace(Method.GetString(ViewModel.name)) ? ViewModel.name : "без наименования" : "не передан  на страницу"), FontSize = 21 * App.FontScale, TextColor = Color.Black, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center }, new ScrollView { Content = SLRepositoryProperties, VerticalOptions = LayoutOptions.FillAndExpand, VerticalScrollBarVisibility = ScrollBarVisibility.Never } } };
        }

        public class PropertyValueConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return value != null && value is string[] sa ? string.Join(", ", sa) : Method.GetString(value);
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotImplementedException();
            }
        }
    }
}