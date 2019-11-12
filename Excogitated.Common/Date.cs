using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common
{
    public static class Extensions_Date
    {
        private static readonly TimeZoneInfo _cst = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        public static DateTimeOffset ToCST(this DateTimeOffset date) => TimeZoneInfo.ConvertTime(date, _cst);

        public static Date ToDate(this DateTimeOffset date) => date.DateTime;
        public static Date ToDate(this DateTime date) => date;
        public static Date ToDate(this int yearMonthDay) => yearMonthDay;
        public static Date ToDate(this string date) => date;
    }

    public struct MonthDayYear
    {
        public static implicit operator MonthDayYear(string mdy) => new MonthDayYear(mdy);
        public static implicit operator MonthDayYear(Date date) => new MonthDayYear(date);
        public static implicit operator string(MonthDayYear mdy) => mdy.ToString();
        public static implicit operator Date(MonthDayYear mdy) => mdy.Value;

        public Date Value { get; }

        public MonthDayYear(Date date)
        {
            Value = date;
        }

        public MonthDayYear(string dmy)
        {
            using var parts = dmy.GetNumericParts().GetEnumerator();
            var m = parts.MoveNext() ? parts.Current : 0;
            var d = parts.MoveNext() ? parts.Current : 0;
            var y = parts.MoveNext() ? parts.Current : 0;
            Value = new Date(y, m, d);
        }

        public override bool Equals(object obj) => obj is MonthDayYear d && d.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => ToCharSpan().ToString();

        public ReadOnlySpan<char> ToCharSpan()
        {
            var chars = new char[Date.DefaultYearLength + 6];
            var month = Value.Month;
            chars[1] = (month % 10).ToChar();
            chars[0] = (month / 10 % 10).ToChar();
            chars[2] = '-';

            var day = Value.Day;
            chars[4] = (day % 10).ToChar();
            chars[3] = (day / 10 % 10).ToChar();
            chars[5] = '-';

            var year = Value.Year;
            chars[^1] = (year % 10).ToChar();
            for (var i = Date.DefaultYearLength + 4; i > 5; i--)
                chars[i] = ((year /= 10) % 10).ToChar();
            return new ReadOnlySpan<char>(chars);
        }
    }

    public struct Date : IComparable<Date>, IEquatable<Date>
    {
        public static implicit operator DateTimeOffset(Date date) => date.DateTime;
        public static implicit operator DateTime(Date date) => date.DateTime;
        public static implicit operator string(Date date) => date.ToString();
        public static implicit operator int(Date date) => date.YearMonthDay;

        public static implicit operator Date(DateTimeOffset date) => new Date(date.Year, date.Month, date.Day);
        public static implicit operator Date(DateTime date) => new Date(date.Year, date.Month, date.Day);
        public static implicit operator Date(string date) => Parse(date);
        public static implicit operator Date(int ymd) => new Date(ymd);

        //public static bool operator >(SimpleDate left, SimpleDate right) => left.EpochSeconds > right.EpochSeconds;
        //public static bool operator <(SimpleDate left, SimpleDate right) => left.EpochSeconds < right.EpochSeconds;
        //public static bool operator >=(SimpleDate left, SimpleDate right) => left.EpochSeconds >= right.EpochSeconds;
        //public static bool operator <=(SimpleDate left, SimpleDate right) => left.EpochSeconds <= right.EpochSeconds;
        public static bool operator ==(Date left, Date right) => left.YearMonthDay == right.YearMonthDay;
        public static bool operator !=(Date left, Date right) => left.YearMonthDay != right.YearMonthDay;

        //public static bool operator ==(SimpleDate left, DateTimeOffset right) => left == right.ToSimple();
        //public static bool operator !=(SimpleDate left, DateTimeOffset right) => left != right.ToSimple();
        //public static bool operator ==(DateTimeOffset left, SimpleDate right) => left.ToSimple() == right;
        //public static bool operator !=(DateTimeOffset left, SimpleDate right) => left.ToSimple() != right;

        public static TimeSpan operator -(Date left, Date right) => left.DateTime - right.DateTime;

        public static Date Today => DateTime.Now.ToDate();

        public static bool TryParse(string value, out Date result)
        {
            var date = value.GetDigits();
            var day = date % 100;
            date /= 100;
            var month = date % 100;
            date /= 100;
            var year = date;
            var error = TryValidate(year, month, day);
            if (error is null)
            {
                result = new Date(year, month, day);
                return true;
            }
            result = default;
            return false;
        }

        public static Date Parse(string value)
        {
            try
            {
                var date = value.GetDigits();
                return new Date(date);
            }
            catch (Exception e)
            {
                throw new Exception($"Expected Date Format: yyyy-MM-dd, Value: {value}", e);
            }
        }

        public static string TryValidate(int year, int month, int day)
        {
            if (year == 0 && month == 0 && day == 0)
                return null;

            if (year < 0)
                return "Year < 0";
            if (month < 1)
                return "Month < 1";
            if (month > 12)
                return "Month > 12";
            if (day < 1)
                return "Day < 1";
            if (day > 31)
                return "Day > 31";
            if (day > 30 && month.EqualsAny(4, 6, 9, 11))
                return $"Day > 30 && Month == {month}";
            if (day > 28 && month == 2)
                if (year % 4 != 0)
                    return "Day > 28 && Month == 2 && Year % 4 != 0";
                else if (day > 29)
                    return "Day > 29 && Month == 2 && Year % 4 == 0";
            return null;
        }

        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public int YearMonthDay { get; }
        public DateTime DateTime => YearMonthDay == 0 ? DateTime.MinValue : new DateTime(Year, Month, Day);
        public DayOfWeek DayOfWeek => DateTime.DayOfWeek;

        public Date(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
            YearMonthDay = (Year * 100 + Month) * 100 + Day;
            Validate();
        }

        public Date(int yearMonthDay)
        {
            YearMonthDay = yearMonthDay;
            Day = yearMonthDay % 100;
            yearMonthDay /= 100;
            Month = yearMonthDay % 100;
            yearMonthDay /= 100;
            Year = yearMonthDay;
            Validate();
        }

        private void Validate()
        {
            if (TryValidate(Year, Month, Day) is string ex)
                throw new ArgumentException(ex);
        }

        public Date AddDays(int days) => DateTime.AddDays(days);
        public Date AddMonths(int months) => DateTime.AddMonths(months);
        public Date AddYears(int years) => DateTime.AddYears(years);
        public TimeSpan Subtract(Date updated, params DayOfWeek[] excluded)
        {
            var days = 0;
            var exList = excluded.ToList();
            var i = updated < this ? 1 : -1;
            while (updated != this)
            {
                updated = updated.AddDays(i);
                if (!exList.Contains(updated.DayOfWeek))
                    days++;
            }
            return TimeSpan.FromDays(days);
        }

        public bool Equals(Date other) => YearMonthDay == other;
        public int CompareTo(Date other) => YearMonthDay - other.YearMonthDay; // ? 1 : YearMonthDay < other ? -1 : 0;

        public override bool Equals(object obj) => obj is Date d && Equals(d);
        public override int GetHashCode() => YearMonthDay.GetHashCode();

        public override string ToString() => ToCharSpan().ToString();

        public static int DefaultYearLength = DateTime.Now.Year.ToString().Length;

        public ReadOnlySpan<char> ToCharSpan()
        {
            var year = Year;
            var dyl = DefaultYearLength;
            var chars = new char[dyl + 6];
            chars[dyl - 1] = (year % 10).ToChar();
            for (var i = dyl - 2; i >= 0; i--)
                chars[i] = ((year /= 10) % 10).ToChar();
            chars[dyl] = '-';

            var month = Month;
            chars[dyl + 2] = (month % 10).ToChar();
            chars[dyl + 1] = (month / 10 % 10).ToChar();
            chars[dyl + 3] = '-';

            var day = Day;
            chars[dyl + 5] = (day % 10).ToChar();
            chars[dyl + 4] = (day / 10 % 10).ToChar();
            return new ReadOnlySpan<char>(chars);
        }

        public bool IsMarketHoliday()
        {
            var dayOfWeek = DateTime.DayOfWeek;
            var isWeekDay = dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
            //Saturday or Sunday
            if (!isWeekDay)
                return true;

            //New Years Day
            if (Month == 1 && Day == 1 && isWeekDay)
                return true;
            if (Month == 1 && Day == 2 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //MLK Day
            if (Month == 1 && Day >= 15 && Day <= 21 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //Washington's Birthday
            if (Month == 2 && Day >= 15 && Day <= 21 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //Good Friday
            if (Month >= 3 && Month <= 4 && dayOfWeek == DayOfWeek.Friday)
            {
                var y = Year;
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
                var e = AddDays(2);
                if (m == e.Month && d == e.Day)
                    return true;
            }

            //Memorial Day
            if (Month == 5 && Day >= 25 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //Independence Day
            if (Month == 7 && Day == 3 && dayOfWeek == DayOfWeek.Friday)
                return true;
            if (Month == 7 && Day == 4 && isWeekDay)
                return true;
            if (Month == 7 && Day == 5 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //Labor Day
            if (Month == 9 && Day <= 7 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //Thanksgiving Day
            if (Month == 11 && Day >= 22 && Day <= 28 && dayOfWeek == DayOfWeek.Thursday)
                return true;

            //Christmas
            if (Month == 12 && Day == 24 && dayOfWeek == DayOfWeek.Friday)
                return true;
            if (Month == 12 && Day == 25 && isWeekDay)
                return true;
            if (Month == 12 && Day == 26 && dayOfWeek == DayOfWeek.Monday)
                return true;

            //special market closures
            if (Year == 2018 && Month == 12 && Day == 5) //Death of President Bush
                return true;
            return false;
        }

        public Date GetNextMarketDay() => GetMarketDays(1).FirstOrDefault();
        public Date GetPrevMarketDay() => GetMarketDays(-1).FirstOrDefault();
        public IEnumerable<Date> GetMarketDays(Date endInclusive) => GetDays(endInclusive).Where(d => !d.IsMarketHoliday());
        public IEnumerable<Date> GetMarketDays(int count) => GetDays(count > 0 ? 1 : -1).Skip(1).Where(d => !d.IsMarketHoliday()).Take(Math.Abs(count));

        public Date GetNextDay() => AddDays(1);
        public Date GetPrevDay() => AddDays(-1);

        public IEnumerable<Date> GetDays(Date endInclusive)
        {
            if (this > endInclusive)
                return GetDays(-1).TakeWhile(d => d >= endInclusive);
            if (this < endInclusive)
                return GetDays(1).TakeWhile(d => d <= endInclusive);
            return Enumerable.Repeat(endInclusive, 1);
        }

        public IEnumerable<Date> GetDays(int step)
        {
            var date = this;
            while (true)
            {
                yield return date;
                date = date.AddDays(step);
            }
        }
    }
}