using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class StravaSync
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int AppActivityId { get; set; }
    }
}
