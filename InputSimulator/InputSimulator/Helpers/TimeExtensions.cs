using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputSimulator.Helpers
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

        public static void NOP(double durationSeconds)
        {
            var durationTicks = Math.Round(durationSeconds * Stopwatch.Frequency);
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedTicks < durationTicks)
            {

            }
        }
    }
}
