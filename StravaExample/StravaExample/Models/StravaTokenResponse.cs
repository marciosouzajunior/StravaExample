using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class StravaTokenResponse
    {
        public int expires_at { get; set; }
        public string refresh_token { get; set; }
        public string access_token { get; set; }
    }
}
