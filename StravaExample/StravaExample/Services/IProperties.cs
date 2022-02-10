using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Services
{
    public interface IProperties
    {
        void Save(string key, object value);
        bool Contains(string key);
        object Get(string key);
        void Remove(string key);
    }
}
