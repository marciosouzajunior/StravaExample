using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample.Models
{
    public class AppActivity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; internal set; }
        public DateTime StartDate { get; set; }
        public int ElapsedTime { get; internal set; }
    }
}
