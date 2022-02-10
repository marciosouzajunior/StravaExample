using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StravaExample.Services
{
    public interface IDialog
    {
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
        Task DisplayAlert(string title, string message, string accept);
        Task<string> DisplayPrompt(string title, string message, string accept, string cancel, string placeholder, int maxLength, Keyboard keyboard, string initialValue);
        Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);
    }
}
