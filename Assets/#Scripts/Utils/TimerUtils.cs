using System;

namespace Unpuzzle
{
    public static class TimerUtils
    {
        public static string GetTimeStr(TimeSpan timeLeft)
        {
            string timeStr;
            if (timeLeft.Days > 0)
            {
                timeStr = $"{timeLeft.Days}d";
                var hours = timeLeft.Hours;
                if (hours > 0 && timeLeft.Days < 2)
                {
                    timeStr = $"{timeStr} {hours}h";
                }
            }
            else if (timeLeft.Hours > 0)
            {
                timeStr = $"{timeLeft.Hours}h";
                var minutes = timeLeft.Minutes;
                if (minutes > 0)
                {
                    timeStr = $"{timeStr} {minutes}m";
                }
            }
            else if (timeLeft.Minutes > 0)
            {
                timeStr = $"{timeLeft.Minutes}m";
                var seconds = timeLeft.Seconds;
                if (seconds > 0)
                {
                    timeStr = $"{timeStr} {seconds}s";
                }
            }
            else if (timeLeft.Seconds >= 0)
            {
                timeStr = $"{timeLeft.Seconds}s";
            }
            else
            {
                timeStr = "0s";
            }

            return timeStr;
        }
    }
}
