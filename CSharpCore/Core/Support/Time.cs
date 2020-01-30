using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class Time
    {
        private static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        static Time()
        {
            stopwatch.Start();
        }

        /// <summary>
        /// 获取当前时间戳（微秒）
        /// </summary>
        /// <returns>返回当前的时间戳</returns>
        public static Int64 HighPrecision_GetTickCount()
        {
            //return DllImportAPI.HighPrecision_GetTickCount();
            var freq = System.Diagnostics.Stopwatch.Frequency / 1000000;
            var now = System.Diagnostics.Stopwatch.GetTimestamp();

            return now / freq;
        }

        //static long AppStartTime = HighPrecision_GetTickCount();
        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        /// <returns>返回当前的时间戳</returns>
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static Int64 GetTickCount()
        {
            return stopwatch.ElapsedMilliseconds;
            //var time = (HighPrecision_GetTickCount()- AppStartTime) / 1000;
            //return time;
        }

        public static int DaysOfYear = 365;

        public static int DiffDays(DateTime leftTime, DateTime rightTime)
        {
            int days = 0;
            var years = Math.Abs(leftTime.Year - rightTime.Year);
            days += (int)(years * DaysOfYear);
            if (leftTime < rightTime)
            {
                days -= leftTime.DayOfYear;
                days += rightTime.DayOfYear;
            }
            else if (leftTime > rightTime)
            {
                days -= rightTime.DayOfYear;
                days += leftTime.DayOfYear;
            }

            return days;
        }

        public static int DiffWeaks(DateTime leftTime, DateTime rightTime)
        {
            int days = DiffDays(leftTime, rightTime);
            int leftWeak = (leftTime.DayOfWeek == DayOfWeek.Sunday ? 7 : Convert.ToInt32(leftTime.DayOfWeek));
            int rightWeak = (rightTime.DayOfWeek == DayOfWeek.Sunday ? 7 : Convert.ToInt32(rightTime.DayOfWeek));
            int weak = 0;
            if (leftTime < rightTime)
            {
                if (rightWeak >= leftWeak)
                    weak = days / 7;
                else
                    weak = days / 7 + 1;
            }
            else
            {
                if (leftWeak >= rightWeak)
                    weak = days / 7;
                else
                    weak = days / 7 + 1;
            }
            return weak;
        }
    }
}
