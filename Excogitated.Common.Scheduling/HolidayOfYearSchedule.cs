using System;

namespace Excogitated.Common.Scheduling
{
    public enum HolidayOfYear
    {
        NewYearsDay,
        MartinLutherKingJrDay,
        PresidentsDay,
        GoodFriday,
        EasterDay,
        MemorialDay,
        IndependenceDay,
        LaborDay,
        ColumbusDay,
        VeteransDay,
        ThanksgivingDay,
        ChristmasDay,
    }

    internal class HolidayOfYearSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HolidayOfYear[] _holidaysOfYear;

        public HolidayOfYearSchedule(ISchedule schedule, HolidayOfYear[] holidaysOfYear)
        {
            _schedule = schedule ?? NullSchedule.Instance;
            _holidaysOfYear = holidaysOfYear ?? Enum.GetValues<HolidayOfYear>();
            if (_holidaysOfYear.Length == 0)
                _holidaysOfYear = Enum.GetValues<HolidayOfYear>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            while (!IsHoliday(next))
                next = GetNextEventPrivate(next);
            return next;
        }

        private bool IsHoliday(DateTimeOffset next)
        {
            foreach (var holidayOfYear in _holidaysOfYear)
            {
                var isHoliday = holidayOfYear switch
                {
                    HolidayOfYear.NewYearsDay => next.IsNewYearsDay(),
                    HolidayOfYear.MartinLutherKingJrDay => next.IsMartinLutherKingJrDay(),
                    HolidayOfYear.PresidentsDay => next.IsPresidentsDay(),
                    HolidayOfYear.GoodFriday => next.IsGoodFriday(),
                    HolidayOfYear.EasterDay => next.IsEaster(),
                    HolidayOfYear.MemorialDay => next.IsMemorialDay(),
                    HolidayOfYear.IndependenceDay => next.IsIndependenceDay(),
                    HolidayOfYear.LaborDay => next.IsLaborDay(),
                    HolidayOfYear.ColumbusDay => next.IsColumbusDay(),
                    HolidayOfYear.VeteransDay => next.IsVeteransDay(),
                    HolidayOfYear.ThanksgivingDay => next.IsThanksgivingDay(),
                    HolidayOfYear.ChristmasDay => next.IsChristmasDay(),
                    _ => throw new ArgumentException($"{holidayOfYear} is not a supported value.", nameof(holidayOfYear))
                };
                if (isHoliday)
                    return true;
            }
            return false;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddDays(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnHolidayOfYear(this ISchedule schedule, params HolidayOfYear[] holidaysOfYear)
        {
            return new HolidayOfYearSchedule(schedule, holidaysOfYear);
        }

        public static bool IsWeekday(this DateTimeOffset date) => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        public static bool IsWeekend(this DateTimeOffset date) => !date.IsWeekday();
        public static bool IsFirstWeek(this DateTimeOffset date) => date.Day >= 1 && date.Day <= 7;
        public static bool IsSecondWeek(this DateTimeOffset date) => date.Day >= 8 && date.Day <= 14;
        public static bool IsThirdWeek(this DateTimeOffset date) => date.Day >= 15 && date.Day <= 21;
        public static bool IsFourthWeek(this DateTimeOffset date) => date.Day >= 22 && date.Day <= 28;
        public static bool IsFifthWeek(this DateTimeOffset date) => date.Day >= 29 && date.Day <= 31;

        public static bool IsNewYearsDay(this DateTimeOffset date) => date.Month == 1 && date.Day == 1;
        public static bool IsNewYearsDayOff(this DateTimeOffset date)
        {
            if (date.IsNewYearsDay() && date.IsWeekday())
                return true;
            if (date.Month == 1 && date.Day == 2 && date.DayOfWeek == DayOfWeek.Monday)
                return true;
            return false;
        }

        public static bool IsMartinLutherKingJrDay(this DateTimeOffset date) => date.Month == 1 && date.DayOfWeek == DayOfWeek.Monday && date.IsThirdWeek();

        public static bool IsPresidentsDay(this DateTimeOffset date) => date.Month == 2 && date.DayOfWeek == DayOfWeek.Monday && date.IsThirdWeek();

        public static bool IsGoodFriday(this DateTimeOffset date) => date.AddDays(2).IsEaster();

        public static bool IsEaster(this DateTimeOffset date)
        {
            if (date.Month >= 3 && date.Month <= 4 && date.DayOfWeek == DayOfWeek.Sunday)
            {
                var y = date.Year;
                var c = y / 100;
                var n = y - 19 * (y / 19);
                var k = (c - 17) / 25;
                var i = c - c / 4 - (c - k) / 3 + 19 * n + 15;
                i -= 30 * (i / 30);
                i -= (i / 28) * (1 - (i / 28) * (29 / (i + 1)) * ((21 - n) / 11));
                var j = y + y / 4 + i + 2 - c + c / 4;
                j -= 7 * (j / 7);
                var l = i - j;
                var m = 3 + (l + 40) / 44;
                var d = l + 28 - 31 * (m / 4);
                if (m == date.Month && d == date.Day)
                    return true;
            }
            return false;
        }

        public static bool IsMemorialDay(this DateTimeOffset date) => date.Month == 5 && date.Day >= 25 && date.DayOfWeek == DayOfWeek.Monday;

        public static bool IsIndependenceDay(this DateTimeOffset date) => date.Month == 7 && date.Day == 4;

        public static bool IsIndependenceDayOff(this DateTimeOffset date)
        {
            if (date.IsIndependenceDay() && date.IsWeekday())
                return true;
            if (date.Month == 7 && date.Day == 3 && date.DayOfWeek == DayOfWeek.Friday)
                return true;
            if (date.Month == 7 && date.Day == 5 && date.DayOfWeek == DayOfWeek.Monday)
                return true;
            return false;
        }

        public static bool IsLaborDay(this DateTimeOffset date) => date.Month == 9 && date.IsFirstWeek() && date.DayOfWeek == DayOfWeek.Monday;

        public static bool IsColumbusDay(this DateTimeOffset date) => date.Month == 10 && date.IsSecondWeek() && date.DayOfWeek == DayOfWeek.Monday;

        public static bool IsVeteransDay(this DateTimeOffset date) => date.Month == 9 && date.IsFirstWeek() && date.DayOfWeek == DayOfWeek.Monday;

        public static bool IsThanksgivingDay(this DateTimeOffset date) => date.Month == 11 && date.IsThirdWeek() && date.DayOfWeek == DayOfWeek.Thursday;

        public static bool IsChristmasDay(this DateTimeOffset date) => date.Month == 12 && date.Day == 25;

        public static bool IsChristmasDayOff(this DateTimeOffset date)
        {
            if (date.IsChristmasDay() && date.IsWeekday())
                return true;
            if (date.Month == 12 && date.Day == 24 && date.DayOfWeek == DayOfWeek.Friday)
                return true;
            if (date.Month == 12 && date.Day == 26 && date.DayOfWeek == DayOfWeek.Monday)
                return true;
            return false;
        }
    }
}