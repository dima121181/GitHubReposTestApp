using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GitHubReposTestApp.Models {
    public class RepositoryContent : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public string name { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string encoding { get; set; }
        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
