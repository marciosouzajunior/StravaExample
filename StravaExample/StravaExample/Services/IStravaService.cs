using StravaExample.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StravaExample.Services
{
    public interface IStravaService
    {
        event EventHandler Connected;
        event EventHandler Disconnected;
        event EventHandler ActivityAdded;
        event EventHandler SyncCompleted;
        Task Connect();
        Task Disconnect();
        bool IsConnected();
        Task NewActivity(AppActivity appActivity);
        void Sync();
    }
}
