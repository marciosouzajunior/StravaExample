using StravaExample.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StravaExample.Services
{
    public interface IStravaService
    {
        Task Connect();
        Task Disconnect();
        bool IsConnected();
        Task NewActivity(AppActivity appActivity);
        void Sync();
        event EventHandler OnSyncCompleted;
    }
}
