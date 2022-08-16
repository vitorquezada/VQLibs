namespace VQLib.Util
{
    public static class VQDateExtensions
    {
        public static DateOnly GetNextUtilDay(DateOnly date, int count = 0)
        {
            var utilDate = date;

            while (utilDate.DayOfWeek == DayOfWeek.Sunday || utilDate.DayOfWeek == DayOfWeek.Saturday)
                utilDate = utilDate.AddDays(1);

            while (count > 0)
            {
                utilDate = utilDate.AddDays(1);
                count--;
                while (utilDate.DayOfWeek == DayOfWeek.Sunday || utilDate.DayOfWeek == DayOfWeek.Saturday)
                    utilDate = utilDate.AddDays(1);
            }

            return utilDate;
        }

        public static DateOnly GetPreviousUtilDay(DateOnly date, int count = 0)
        {
            var utilDate = date;

            while (utilDate.DayOfWeek == DayOfWeek.Sunday || utilDate.DayOfWeek == DayOfWeek.Saturday)
                utilDate = utilDate.AddDays(-1);

            while (count > 0)
            {
                utilDate = utilDate.AddDays(-1);
                count--;

                while (utilDate.DayOfWeek == DayOfWeek.Sunday || utilDate.DayOfWeek == DayOfWeek.Saturday)
                    utilDate = utilDate.AddDays(-1);
            }

            return utilDate;
        }

        public static DateOnly GetStartDateConsideringHolidaysWeekend(DateOnly data)
        {
            var startDate = data;
            var previousDate = startDate.AddDays(-1);

            while (previousDate.DayOfWeek == DayOfWeek.Sunday || previousDate.DayOfWeek == DayOfWeek.Saturday /* || Feriado*/)
            {
                startDate = previousDate;
                previousDate = previousDate.AddDays(-1);
            }

            return startDate;
        }

        public static DateOnly ToDateOnly(this DateTime date)
        {
            return new DateOnly(date.Year, date.Month, date.Day);
        }

        public static DateOnly ToDateOnly(this DateTimeOffset date)
        {
            return new DateOnly(date.Year, date.Month, date.Day);
        }

        public static DateTime ToDateTime(this DateOnly date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, DateTimeKind.Unspecified);
        }
    }
}