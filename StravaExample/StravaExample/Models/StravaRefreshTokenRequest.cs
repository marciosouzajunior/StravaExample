using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class StravaRefreshTokenRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
        public string refresh_token { get; set; }
    }
}
