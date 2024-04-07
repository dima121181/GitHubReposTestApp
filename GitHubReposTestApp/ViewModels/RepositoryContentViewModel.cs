using System;
using System.Text;
using GitHubReposTestApp.Models;

namespace GitHubReposTestApp.ViewModels {
    public class RepositoryContentViewModel : RepositoryContent {
        public bool IsDir { get { return Method.GetString(type).ToLower() == "dir"; } }
        public bool IsFile { get { return Method.GetString(type).ToLower() == "file"; } }
        public string TypeName { get { return IsDir ? "Папка" : IsFile ? "Файл" : "Другое"; } }
        public string ContentString {
            get {
                string res = string.Empty;
                if (Method.GetString(encoding).ToLower() == "base64" && !string.IsNullOrWhiteSpace(content))
                    try {
                        res = Encoding.UTF8.GetString(Convert.FromBase64String(content));
                    }
                    catch { }
                return res;
            }
        }
        public string RepositoryName {
            get {
                int i0 = Method.GetString(url).ToLower().IndexOf("https://api.github.com/repos/");
                int i1 = i0 >= 0 ? Method.GetString(url).Substring(i0 + 29).IndexOf("/") : -1;
                int i2 = Method.GetString(url).ToLower().IndexOf("/contents/");
                return i1 >= 0 && i2 >= 0 && i2 > i1 ? Method.GetString(url).Substring(i1 + 30, i2 - i1 - 30) : string.Empty;
            }
        }
    }
}
