using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class StravaActivityStream
    {
        public StravaActivityHR heartrate { get; set; }
        public StravaActivityTime time { get; set; }
    }
}
