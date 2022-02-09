using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class StravaActivity
    {
        public long id { get; set; }
        public string external_id { get; set; }
        public string name { get; set; }
        public DateTime start_date_local { get; set; }
        public int elapsed_time { get; set; }
        public double average_heartrate { get; set; }
        public double max_heartrate { get; set; }
        public StravaActivityStream activityStream { get; set; }
    }
}
