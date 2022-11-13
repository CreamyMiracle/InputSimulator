using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputCollector.Helpers
{
    public static class TimeExtensions
    {
        public static string GetCurrentTimeStamp()
        {
            DateTime currTime = DateTime.UtcNow;
            string s = currTime.ToString("ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
            s += currTime.Kind.ToString();
            return s;
        }

        public static string GetCurrentTimeStampPrecise()
        {
            DateTime currTime = DateTime.UtcNow;
            string s = currTime.ToString("ddMMyyyy_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            s += currTime.Kind.ToString();
            return s;
        }
    }
}
