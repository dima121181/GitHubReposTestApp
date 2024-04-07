using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using GitHubReposTestApp.Views;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace GitHubReposTestApp.ViewModels {
    public class RepositoryListViewModel : INotifyPropertyChanged {
        private string RepositoriesJSON{
            get => Method.GetKeyValue("RepositoriesJSON").Result;
            set => Method.SetKeyValue("RepositoriesJSON", value);
        }
        private string RepositoryContentsJSON {
            get => Method.GetKeyValue("RepositoryContentsJSON").Result;
            set => Method.SetKeyValue("RepositoryContentsJSON", value);
        }
        private ObservableCollection<RepositoryViewModel> Repositories { get; set; }
        public ObservableCollection<RepositoryViewModel> RepositoriesForView { get; set; }
        private List<List<RepositoryContentViewModel>> RepositoryContents { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand GetRepositoriesCommand { protected set; get; }

        public ICommand BackCommand { protected set; get; }
        private RepositoryViewModel _selectedRepository;

        public INavigation Navigation { get; set; }

        public string LastUpdateUserLogin {
            get => Method.GetKeyValue("UserLogin").Result;
            set {
                Method.SetKeyValue("UserLogin", value);
                OnPropertyChanged("LastUpdateUserLogin");
            }
        }

        public string UserLogin { get; set; }

        public DateTime? LastUpdate {
            get => Method.GetDateTime(Method.GetKeyValue("LastUpdate").Result);
            set {
                Method.SetKeyValue("LastUpdate", value != null ? ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff") : null);
                OnPropertyChanged("LastUpdate");
            }
        }

        private string _SearchText { get; set; }
        public string SearchText {
            get { return _SearchText; }
            set {
                if (value != _SearchText) {
                    _SearchText = value;
                    UpdateRepView();
                }
            }
        }

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
        public bool IsBusy {
            get => _IsBusy;
            set {
                if (value != _IsBusy) {
                    _IsBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        public RepositoryListViewModel() {
            try {
                string RepJSON = RepositoriesJSON;
                string RepContJSON = RepositoryContentsJSON;
                Repositories = !string.IsNullOrWhiteSpace(RepJSON) ? (ObservableCollection<RepositoryViewModel>)JsonConvert.DeserializeObject(RepJSON, typeof(ObservableCollection<RepositoryViewModel>)) : new ObservableCollection<RepositoryViewModel>();
                RepositoryContents = !string.IsNullOrWhiteSpace(RepContJSON) ? (List<List<RepositoryContentViewModel>>)JsonConvert.DeserializeObject(RepContJSON, typeof(List<List<RepositoryContentViewModel>>)) : new List<List<RepositoryContentViewModel>>();
            }
            catch {
                Repositories = new ObservableCollection<RepositoryViewModel>();
                RepositoryContents = new List<List<RepositoryContentViewModel>>();
            }
            UpdateRepView();
            UserLogin = LastUpdateUserLogin;
            GetRepositoriesCommand = new Command(GetRepositories);
            BackCommand = new Command(Back);
        }

        public RepositoryViewModel SelectedRepository {
            get { return _selectedRepository; }
            set {
                if (_selectedRepository != value) {
                    IsBusy = true;
                    _selectedRepository = null;
                    OnPropertyChanged("SelectedRepository");
                    OpenRepositoryPage(value);
                }
            }
        }
        private async void OpenRepositoryPage(RepositoryViewModel tempRepository) {
            try {
                await Navigation.PushAsync(new RepositoryTabbedPage(tempRepository, RepositoryContents?.FirstOrDefault(rcl => !string.IsNullOrWhiteSpace(tempRepository?.name) && rcl.Any(rc => rc.RepositoryName == tempRepository.name))));
            }
            finally {
                IsBusy = false;
            }
        }
        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void UpdateRepView() {
            RepositoriesForView = new ObservableCollection<RepositoryViewModel>(Repositories?.Where(rp => string.IsNullOrWhiteSpace(SearchText) || Method.GetString(rp.name).ToLower().Contains(Method.GetString(SearchText).ToLower()))?.ToList() ?? new List<RepositoryViewModel>());
            OnPropertyChanged("RepositoriesForView");
        }

        private void GetRepositories() {
            IsBusy = true;

            _ = Task.Run(() => {
                string Url = $"https://api.github.com/users/{UserLogin}/repos";
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
                myRequest.Method = "GET";
                myRequest.ContentType = "application/json";
                myRequest.UserAgent = "request";
                if (!string.IsNullOrWhiteSpace(App.GitHubToken))
                    myRequest.Headers.Add("Authorization", "token " + App.GitHubToken);

                string Response = string.Empty;
                bool UpdateSuccess = false;
                try {
                    using (WebResponse myResponse = myRequest.GetResponse())
                    using (Stream stream = myResponse.GetResponseStream())
                    using (StreamReader sr = new StreamReader(stream))
                        Response = sr.ReadToEnd();

                    try {
                        if (!string.IsNullOrWhiteSpace(Method.GetString(Response)) && Method.GetString(Response) != "[]")
                            Repositories = (ObservableCollection<RepositoryViewModel>)JsonConvert.DeserializeObject(Response, typeof(ObservableCollection<RepositoryViewModel>));
                        else {
                            Repositories = new ObservableCollection<RepositoryViewModel>();
                            Device.BeginInvokeOnMainThread(async () => { await Method.ShowMessage("Публичные репозитории отсутствуют"); });
                        }
                        RepositoriesJSON = Response;
                        LastUpdateUserLogin = UserLogin;
                        LastUpdate = Method.MoscowDateTime;
                        UpdateSuccess = true;
                    }
                    catch (Exception exd) {
                        Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("Repositories Deserialize", exd); });
                    }
                    UpdateRepView();

                    if (UpdateSuccess && Repositories != null) {
                        List<List<RepositoryContentViewModel>> RepContents = new List<List<RepositoryContentViewModel>>();
                        List<string> RepContentsJSON = new List<string>();
                        foreach (var rep in Repositories) {
                            Url = rep.url+ "/contents";
                            myRequest = (HttpWebRequest)WebRequest.Create(Url);
                            myRequest.Method = "GET";
                            myRequest.ContentType = "application/json";
                            myRequest.UserAgent = "request";
                            if (!string.IsNullOrWhiteSpace(App.GitHubToken))
                                myRequest.Headers.Add("Authorization", "token " + App.GitHubToken);

                            Response = string.Empty;
                            using (WebResponse myResponse = myRequest.GetResponse())
                            using (Stream stream = myResponse.GetResponseStream())
                            using (StreamReader sr = new StreamReader(stream))
                                Response = sr.ReadToEnd();

                            try {
                                List<RepositoryContentViewModel> RepContent = null;
                                if (!string.IsNullOrWhiteSpace(Method.GetString(Response)) && Method.GetString(Response) != "[]")
                                    RepContent = (List<RepositoryContentViewModel>)JsonConvert.DeserializeObject(Response, typeof(List<RepositoryContentViewModel>));
                                if (RepContent != null) {
                                    RepContents.Add(RepContent);
                                    RepContentsJSON.Add(Response);
                                }
                            }
                            catch (Exception exd) {
                                Device.BeginInvokeOnMainThread(async () => { await Method.ShowError($"Repository \"{rep.name}\" Content Deserialize", exd); });
                            }
                        }
                        RepositoryContents = RepContents;
                        RepositoryContentsJSON = "[" + string.Join(", ", RepContentsJSON) + "]";
                    }
                }
                catch (WebException ex) {
                    if (ex.Response != null && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("GetRepositories WebException", (UpdateSuccess ? "Файлы" : "Данные") + $" по адресу \"{Url}\" не найдены"); });
                    else
                        Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("GetRepositories WebException", ex); });
                }
                catch (Exception ex) {
                    Device.BeginInvokeOnMainThread(async () => { await Method.ShowError("GetRepositories Exception", ex); });
                }
                finally {
                    IsBusy = false;
                }
            });
        }
        private void Back() {
            _ = Navigation.PopAsync();
        }
    }
}
