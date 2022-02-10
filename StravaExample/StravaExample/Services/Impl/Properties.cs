using StravaExample.Services.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(Properties))]
namespace StravaExample.Services.Impl
{
    public class Properties : IProperties
    {
        public void Save(string key, object value)
        {
            Application.Current.Properties[key] = value;
            Application.Current.SavePropertiesAsync();
        }

        public bool Contains(string key)
        {
            return Application.Current.Properties.ContainsKey(key);
        }

        public object Get(string key)
        {
            return Application.Current.Properties[key];
        }

        public void Remove(string key)
        {
            Application.Current.Properties.Remove(key);
            Application.Current.SavePropertiesAsync();
        }
    }
}
