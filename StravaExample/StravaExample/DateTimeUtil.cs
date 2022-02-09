using System;
using System.Collections.Generic;
using System.Text;

namespace StravaExample
{
    public class DateTimeUtil
    {
        public static DateTime ConvertEpochToDateTime(long seconds)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(seconds);
        }
        public static long ConvertDateTimeToEpoch(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((dateTime - epoch).TotalSeconds);
        }
        public static string ConvertDateTimeToISO8601(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
