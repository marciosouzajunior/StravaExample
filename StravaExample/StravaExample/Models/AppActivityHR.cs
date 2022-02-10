using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class AppActivityHR
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int AppActivityId { get; set; }
        public int SecondsMeasure { get; set; }
        public int HR { get; set; }
    }
}
