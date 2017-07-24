using System;

namespace Infrastructure.Extensions
{
    public static partial class Extension
    {
        public static bool Between(this DateTime @this, DateTime minValue, DateTime maxValue)
        {
            return @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;
        }

        public static bool BetweenExclusive(this DateTime @this, DateTime minValue, DateTime maxValue)
        {
            return minValue.CompareTo(@this) == -1 && @this.CompareTo(maxValue) == -1;
        }

        public static bool In(this DateTime @this, params DateTime[] values)
        {
            return Array.IndexOf(values, @this) != -1;
        }

        public static bool NotIn(this DateTime @this, params DateTime[] values)
        {
            return Array.IndexOf(values, @this) == -1;
        }

        public static int Age(this DateTime @this)
        {
            if (DateTime.Today.Month < @this.Month ||
                DateTime.Today.Month == @this.Month &&
                DateTime.Today.Day < @this.Day)
            {
                return DateTime.Today.Year - @this.Year - 1;
            }
            return DateTime.Today.Year - @this.Year;
        }

        #region Moment

        public static DateTime StartOfDay(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day);
        }

        public static DateTime EndOfDay(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }

        public static DateTime StartOfMonth(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startDayOfWeek = DayOfWeek.Sunday)
        {
            var start = new DateTime(dt.Year, dt.Month, dt.Day);

            if (start.DayOfWeek != startDayOfWeek)
            {
                int d = startDayOfWeek - start.DayOfWeek;
                if (startDayOfWeek <= start.DayOfWeek)
                {
                    return start.AddDays(d);
                }
                return start.AddDays(-7 + d);
            }

            return start;
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startDayOfWeek = DayOfWeek.Sunday)
        {
            DateTime end = dt;
            DayOfWeek endDayOfWeek = startDayOfWeek - 1;
            if (endDayOfWeek < 0)
            {
                endDayOfWeek = DayOfWeek.Saturday;
            }

            if (end.DayOfWeek != endDayOfWeek)
            {
                if (endDayOfWeek < end.DayOfWeek)
                {
                    end = end.AddDays(7 - (end.DayOfWeek - endDayOfWeek));
                }
                else
                {
                    end = end.AddDays(endDayOfWeek - end.DayOfWeek);
                }
            }

            return new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 999);
        }

        public static DateTime StartOfYear(this DateTime @this)
        {
            return new DateTime(@this.Year, 1, 1);
        }

        public static DateTime EndOfYear(this DateTime @this)
        {
            return new DateTime(@this.Year, 1, 1).AddYears(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
        }

        public static DateTime FirstDayOfWeek(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(-( int )@this.DayOfWeek);
        }

        public static DateTime LastDayOfWeek(this DateTime @this)
        {
            return new DateTime(@this.Year, @this.Month, @this.Day).AddDays(6 - ( int )@this.DayOfWeek);
        }

        public static bool IsMorning(this DateTime @this)
        {
            return @this.TimeOfDay < new DateTime(2000, 1, 1, 12, 0, 0).TimeOfDay;
        }

        public static bool IsAfternoon(this DateTime @this)
        {
            return @this.TimeOfDay >= new DateTime(2000, 1, 1, 12, 0, 0).TimeOfDay;
        }

        public static bool IsWeekDay(this DateTime @this)
        {
            return !(@this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday);
        }

        public static bool IsWeekend(this DateTime @this)
        {
            return (@this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday);
        }

        #endregion Moment

        #region Set Time

        public static DateTime SetTime(this DateTime current, int hour)
        {
            return SetTime(current, hour, 0, 0, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute)
        {
            return SetTime(current, hour, minute, 0, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute, int second)
        {
            return SetTime(current, hour, minute, second, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute, int second, int millisecond)
        {
            return new DateTime(current.Year, current.Month, current.Day, hour, minute, second, millisecond);
        }

        #endregion Set Time
    }
}