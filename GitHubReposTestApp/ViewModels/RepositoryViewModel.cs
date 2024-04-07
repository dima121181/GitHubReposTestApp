using GitHubReposTestApp.Models;
using System;
using System.Reflection;

namespace GitHubReposTestApp.ViewModels {
    public class RepositoryViewModel : Repository {
        public object this[string propertyName] {
            get {
                Type myType = typeof(Repository);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(Repository);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
    }
}
