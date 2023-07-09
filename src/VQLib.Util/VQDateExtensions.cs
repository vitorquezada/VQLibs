namespace VQLib.Util
{
    public static class VQDateExtensions
    {
        public static void Deconstruct(this DateOnly date, out int year, out int month, out int day)
        {
            year = date.Year;
            month = date.Month;
            day = date.Day;
        }

        public static DateOnly GetNextBusinessDay(
            this DateOnly date,
            int count = 0,
            bool saturdayIsBusinessDay = false,
            bool sundayIsBusinessDay = false,
            HashSet<DateOnly>? holidays = null)
        {
            var auxDate = date;

            while (auxDate.IsNotBusinessDay(saturdayIsBusinessDay, sundayIsBusinessDay, holidays))
                auxDate = auxDate.AddDays(1);

            while (count > 0)
            {
                auxDate = auxDate.AddDays(1);
                count--;
                while (auxDate.IsNotBusinessDay(saturdayIsBusinessDay, sundayIsBusinessDay, holidays))
                    auxDate = auxDate.AddDays(1);
            }

            return auxDate;
        }

        public static DateOnly GetPreviousBusinessDay(
            this DateOnly date,
            int count = 0,
            bool saturdayIsBusinessDay = false,
            bool sundayIsBusinessDay = false,
            HashSet<DateOnly>? holidays = null)
        {
            var auxDate = date;

            while (auxDate.IsNotBusinessDay(saturdayIsBusinessDay, sundayIsBusinessDay, holidays))
                auxDate = auxDate.AddDays(-1);

            while (count > 0)
            {
                auxDate = auxDate.AddDays(-1);
                count--;

                while (auxDate.IsNotBusinessDay(saturdayIsBusinessDay, sundayIsBusinessDay, holidays))
                    auxDate = auxDate.AddDays(-1);
            }

            return auxDate;
        }

        public static DateOnly GetStartDateConsideringHolidaysWeekend(
            this DateOnly data,
            bool saturdayIsBusinessDay = false,
            bool sundayIsBusinessDay = false,
            HashSet<DateOnly>? holidays = null)
        {
            var startDate = data;
            var previousDate = startDate.AddDays(-1);

            while (previousDate.IsNotBusinessDay(saturdayIsBusinessDay, sundayIsBusinessDay, holidays))
            {
                startDate = previousDate;
                previousDate = previousDate.AddDays(-1);
            }

            return startDate;
        }

        public static bool IsBusinessDay(this DateOnly date,
            bool saturdayIsBusinessDay = false,
            bool sundayIsBusinessDay = false,
            HashSet<DateOnly>? holidays = null)
        {
            return (date.DayOfWeek != DayOfWeek.Sunday || sundayIsBusinessDay)
                && (date.DayOfWeek != DayOfWeek.Saturday || saturdayIsBusinessDay)
                && (holidays == null || !holidays.Contains(date));
        }

        public static bool IsNotBusinessDay(this DateOnly date,
            bool saturdayIsBusinessDay = false,
            bool sundayIsBusinessDay = false,
            HashSet<DateOnly>? holidays = null)
        {
            return !IsBusinessDay(date, saturdayIsBusinessDay, sundayIsBusinessDay, holidays);
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