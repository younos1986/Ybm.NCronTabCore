/*
┌───────────── minute (0 - 59)
│ ┌───────────── hour (0 - 23)
│ │ ┌───────────── day of month (1 - 31)
│ │ │ ┌───────────── month (1 - 12)
│ │ │ │ ┌───────────── day of week (0 - 6) (Sunday to Saturday;
│ │ │ │ │                                       7 is also Sunday)
│ │ │ │ │
│ │ │ │ │
* * * * *  command to execute
* */

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ybm.NCronTabCore
{
    internal class JalaliCalendar
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int DayNumber { get; set; }
        public string DayName { get; set; }
        public int DayOfYear { get; set; }

        public int MonthTotalDays{ get; set; }
        public int DaysInYears { get; set; }
        public int MonthsInYears { get; set; }

        public int Hour { get; set; }
        public int Minute { get; set; }
        public int MinuteOfYear { get; set; }
        public int Second { get; set; }
        public double Milisecond { get; set; }
        public int Era { get; set; }

        private static Lazy<PersianCalendar> CachedClient = new Lazy<PersianCalendar>(() => new PersianCalendar());
        private static Func<PersianCalendar> DefaultFactory = () => CachedClient.Value;
        private static Func<PersianCalendar> _pc;
        private static object pcFactoryLock = new object();

        internal static Func<PersianCalendar> MyPersianCalendar
        {
            get
            {
                lock (pcFactoryLock)
                {
                    return _pc ?? DefaultFactory;
                }
            }
            set
            {
                lock (pcFactoryLock)
                {
                    _pc = value;
                }
            }
        }


        public JalaliCalendar GetPersianDateTime(DateTime helper, string format = "yyyy/mm/dd")
        {
            var pc = MyPersianCalendar();
            if (helper.Year < 1000) helper = DateTime.Now;
            var persianCalendarPart = new JalaliCalendar();
            persianCalendarPart.Year = pc.GetYear(helper);
            persianCalendarPart.Month = pc.GetMonth(helper);
            persianCalendarPart.Day = pc.GetDayOfMonth(helper);
            persianCalendarPart.DayOfYear = pc.GetDayOfYear(helper);
            persianCalendarPart.DaysInYears = GetDayInYears(persianCalendarPart);
            persianCalendarPart.MonthsInYears = GetMonthsInYears(persianCalendarPart);
            persianCalendarPart.MonthTotalDays= pc.GetDaysInMonth(persianCalendarPart.Year, persianCalendarPart.Month);

            persianCalendarPart.Hour = pc.GetHour(helper);
            persianCalendarPart.Minute = pc.GetMinute(helper);
            persianCalendarPart.MinuteOfYear = (persianCalendarPart.DayOfYear * 24 * 60) + (persianCalendarPart.Hour * 60) + persianCalendarPart.Minute;
            persianCalendarPart.Second = pc.GetSecond(helper);
            persianCalendarPart.Milisecond = pc.GetMilliseconds(helper);
            persianCalendarPart.DayNumber = GetDayOfWeekNumber(helper);
            return persianCalendarPart;
        }

        private int DayOfWeek(int dayOfYear)
        {
            var remain = dayOfYear % 7;
            return remain;
        }

        public void MinuteOfYearToDayAndMonth(int minuteOfYear, out int minute, out int hour, out int day, out int month)
        {
            var pc = MyPersianCalendar();
            DateTime dt = new DateTime(1391, 1, 1, pc);
            dt = dt.AddMinutes(minuteOfYear);
            var pers = new JalaliCalendar().GetPersianDateTime(dt);
            month = pers.Month;
            day = pers.Day;
            hour = pers.Hour;
            minute = pers.Minute;
        }

        public void SecondToDate(long seconds)
        {
            var pc = MyPersianCalendar();
            DateTime dt = new DateTime(1, 1, 1,1,1,1,1, pc, DateTimeKind.Local);
            dt = dt.AddSeconds(seconds);
            var pers = new JalaliCalendar().GetPersianDateTime(dt);
            //month = pers.Month;
            //day = pers.Day;
            //hour = pers.Hour;
            //minute = pers.Minute;
        }

        //0 0 * * SAT,MON,WED,FRI,SUN,TUE,THU
        public Dictionary<string, int> WeekMapping
        {
            get
            {
                Dictionary<string, int> tempWeekMapping = new Dictionary<string, int>();
                tempWeekMapping.Add("SAT", 0);
                tempWeekMapping.Add("SUN", 1);
                tempWeekMapping.Add("MON", 2);
                tempWeekMapping.Add("TUE", 3);
                tempWeekMapping.Add("WED", 4);
                tempWeekMapping.Add("THU", 5);
                tempWeekMapping.Add("FRI", 6);
                return tempWeekMapping;
            }
        }


        public int GetDayInYears(JalaliCalendar j)
        {
            DateTime theDate = new DateTime(1, 1, 1);
            var pc = MyPersianCalendar();

            int totdalDays = 509147;
            for (int i = 1395; i < j.Year; i++)
            {
                totdalDays += pc.GetDaysInYear(i);
            }
            return totdalDays + j.DayOfYear;
        }
        public int GetMonthsInYears(JalaliCalendar j)
        {
            DateTime theDate = new DateTime(1, 1, 1);
            return (j.Year * 12) + j.Month;
        }
        public void DaysInYearsToDateParts(int daysInYears, out int year, out int month, out int day)
        {
            var pc = MyPersianCalendar();
            year = 0;
            int daysInYearsToMines = daysInYears - 509147;
            for (int i = 1396; i < 2500; i++)
            {
                if (daysInYearsToMines <= pc.GetDaysInYear(i))
                {
                    year = i;
                    break;
                }
                daysInYearsToMines = daysInYearsToMines - pc.GetDaysInYear(i);
            }
            //year++;

            day = DayOfYearToDayOfMonth(daysInYearsToMines, year);
            month = DayOfYearToMonthOfYear(daysInYearsToMines, year);
        }
        public int DaysInYearsToDayNum(int daysInYears)
        {
            int year;
            int month;
            int day;
            DaysInYearsToDateParts(daysInYears, out  year, out  month, out  day);
            var TheDate = new CronTabScheduler().JalaliToGregorian(year + "/" + month + "/" + day);
            var pers = new JalaliCalendar().GetPersianDateTime(TheDate);
            return pers.DayNumber;
        }
        public int DayOfYearToDayOfMonth(int dayOfYear, int year)
        {
            var pc = MyPersianCalendar();
            DateTime dt = new DateTime(year, 1, 1, pc);
            dt = dt.AddDays(dayOfYear - 1);
            var pers = new JalaliCalendar().GetPersianDateTime(dt);
            return pers.Day;
        }
        public int DayOfYearToDayNum(int dayOfYear)
        {
            var pc = MyPersianCalendar();
            DateTime dt = new DateTime(1391, 1, 1, pc);
            dt = dt.AddDays(dayOfYear - 1);
            var pers = new JalaliCalendar().GetPersianDateTime(dt);
            return pers.DayNumber;
        }

        public int DayOfYearToMonthOfYear(int dayOfYear, int year)
        {
            var pc = MyPersianCalendar();
            DateTime dt = new DateTime(year, 1, 1, pc);
            dt = dt.AddDays(dayOfYear - 1);
            var pers = new JalaliCalendar().GetPersianDateTime(dt);
            return pers.Month;
        }
        public override string ToString()
        {
            return string.Format("{0}/{1}/{2} {3}:{4}", Year, Month, Day, Hour, Minute);
        }
        public string GetDayOfWeekName(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case System.DayOfWeek.Saturday: return "شنبه";
                case System.DayOfWeek.Sunday: return "يکشنبه";
                case System.DayOfWeek.Monday: return "دوشنبه";
                case System.DayOfWeek.Tuesday: return "سه‏ شنبه";
                case System.DayOfWeek.Wednesday: return "چهارشنبه";
                case System.DayOfWeek.Thursday: return "پنجشنبه";
                case System.DayOfWeek.Friday: return "جمعه";
                default: return "";
            }
        }
        public int GetDayOfWeekNumber(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case System.DayOfWeek.Saturday: return 0;
                case System.DayOfWeek.Sunday: return 1;
                case System.DayOfWeek.Monday: return 2;
                case System.DayOfWeek.Tuesday: return 3;
                case System.DayOfWeek.Wednesday: return 4;
                case System.DayOfWeek.Thursday: return 5;
                case System.DayOfWeek.Friday: return 6;
                default: return -1;
            }
        }
        public string GetMonthName(DateTime date)
        {
            var pc = MyPersianCalendar();
            string pdate = string.Format("{0:0000}/{1:00}/{2:00}", pc.GetYear(date), pc.GetMonth(date), pc.GetDayOfMonth(date));

            string[] dates = pdate.Split('/');
            int month = Convert.ToInt32(dates[1]);

            switch (month)
            {
                case 1: return "فررودين";
                case 2: return "ارديبهشت";
                case 3: return "خرداد";
                case 4: return "تير‏";
                case 5: return "مرداد";
                case 6: return "شهريور";
                case 7: return "مهر";
                case 8: return "آبان";
                case 9: return "آذر";
                case 10: return "دي";
                case 11: return "بهمن";
                case 12: return "اسفند";
                default: return "";
            }

        }
    }
}
