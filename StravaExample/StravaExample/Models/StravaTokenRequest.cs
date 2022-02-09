using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class StravaTokenRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string code { get; set; }
        public string grant_type { get; set; }
    }
}
