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
using System.Linq;

namespace Ybm.NCronTabCore
{
    public class CronTabScheduler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cron"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="convertToJalali"></param>
        /// <returns></returns>
        public List<string> Occurances(string cron, DateTime startDate, DateTime endDate, bool convertToJalali = false)
        {
            List<DateTime> occurances = new List<DateTime>();
            List<string> FinalOccurances = new List<string>();
            CronPattern cronPattern = new CronPattern();

            var pattern = cronPattern.Parse(cron);
            RetrieveOccurances(startDate, endDate, occurances, pattern);

            if (convertToJalali == false)
                return occurances.Cast<string>().ToList();


            foreach (var occurance in occurances)
            {
                //if (DateTime.Now <= occurance && endDate > occurance)
                FinalOccurances.Add(GregorianToJalali(occurance));
            }

            return FinalOccurances;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="occurances"></param>
        /// <param name="pattern"></param>
        private void RetrieveOccurances(DateTime startDate, DateTime endDate, List<DateTime> occurances, Pattern pattern)
        {
            if (pattern.UnitType == EnumUnitType.Secondly)
            {
                if (pattern.EveryNUnit > 0)
                    for (double i = TimeSpan.FromTicks(startDate.Ticks).TotalSeconds; i < TimeSpan.FromTicks(endDate.Ticks).TotalSeconds; i = i + pattern.EveryNUnit)
                    {
                        occurances.Add(new DateTime().AddSeconds(i));
                    }
            }

            if (pattern.UnitType == EnumUnitType.Minutely)
            {
                for (double i = TimeSpan.FromTicks(startDate.Ticks).TotalMinutes; i < TimeSpan.FromTicks(endDate.Ticks).TotalMinutes; i = i + pattern.EveryNUnit)
                {
                    occurances.Add(new DateTime().AddMinutes(i));
                }
            }

            if (pattern.UnitType == EnumUnitType.Hourly)
            {
                for (double i = TimeSpan.FromTicks(startDate.Ticks).TotalHours; i < TimeSpan.FromTicks(endDate.Ticks).TotalHours; i = i + pattern.EveryNUnit)
                {
                    occurances.Add(new DateTime().AddHours(i));
                }
            }

            if (pattern.UnitType == EnumUnitType.Daily)
            {
                for (double i = TimeSpan.FromTicks(startDate.Ticks).TotalHours; i < TimeSpan.FromTicks(endDate.Ticks).TotalHours; i = i + (pattern.Days[0] * 24))
                {
                    occurances.Add(new DateTime().AddHours(i + (pattern.Days[0] * 24)).Date.AddHours(pattern.Hour).AddMinutes(pattern.Minute));

                }
            }

            if (pattern.UnitType == EnumUnitType.Weekly)
            {
                for (double i = TimeSpan.FromTicks(startDate.Ticks).TotalDays; i < TimeSpan.FromTicks(endDate.Ticks).TotalDays; i = i + 1)
                {
                    if (pattern.Days.Count >= 1)
                    {
                        var theDay = new DateTime(1, 1, 1).AddDays(i);
                        var jc = new JalaliCalendar().GetPersianDateTime(theDay);
                        if (pattern.Days.Contains(jc.DayNumber))
                        {
                            occurances.Add(theDay.Date.AddHours(pattern.Hour).AddMinutes(pattern.Minute));
                        }
                    }
                }
            }

            if (pattern.UnitType == EnumUnitType.Monthly)
            {
                for (double i = TimeSpan.FromTicks(startDate.Ticks).TotalDays; i < TimeSpan.FromTicks(endDate.Ticks).TotalDays; i++)
                {
                    var theDay = new DateTime(1, 1, 1).AddDays(i);
                    var jc = new JalaliCalendar().GetPersianDateTime(theDay);//  GregorianToJalali2(theDay);

                    if (pattern.Days.Count == 1 && pattern.Days[0] == -1)
                    {
                        if (jc.MonthTotalDays == jc.Day)
                            occurances.Add(theDay.Date.AddHours(pattern.Hour).AddMinutes(pattern.Minute));
                    }
                    if (pattern.Days.Any() && pattern.Months.Any())
                    {
                        if (pattern.Days.Contains(jc.Day) && pattern.Months.Contains(jc.Month))
                        {
                            occurances.Add(theDay.Date.AddHours(pattern.Hour).AddMinutes(pattern.Minute));
                        }
                    }
                    else
                    if (pattern.Days.Contains(jc.Day))
                    {
                        occurances.Add(theDay.Date.AddHours(pattern.Hour).AddMinutes(pattern.Minute));
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTimeAsPersian"></param>
        /// <returns></returns>
        public DateTime JalaliToGregorian(string dateTimeAsPersian)
        {
            dateTimeAsPersian = dateTimeAsPersian.Replace("-", "/");
            List<string> datetimeParts = dateTimeAsPersian.Split(new List<char>() { ' ' }.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            if (datetimeParts.Count == 0) return default(DateTime);
            if (datetimeParts.Count == 1) datetimeParts.Add("00:00");

            List<string> dateSegments = datetimeParts[0].Split('/').ToList<string>();
            List<string> timeSegments = datetimeParts[1].Split(':').ToList<string>();
            if (dateSegments.Count < 3) return default(DateTime);
            int year = int.Parse(dateSegments[0]);
            int month = int.Parse(dateSegments[1]);
            int day = int.Parse(dateSegments[2]);
            int hour = int.Parse(timeSegments[0]);
            int minute = int.Parse(timeSegments[1]);
            PersianCalendar pcal = new PersianCalendar();
            return pcal.ToDateTime(year, month, day, hour, minute, 0, 0);
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTimeAsGregorian"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GregorianToJalali(DateTime dateTimeAsGregorian, string format = "yyyy/mm/dd")
        {
            if (dateTimeAsGregorian.Year < 1000) dateTimeAsGregorian = DateTime.Now;
            PersianCalendar pc = new PersianCalendar();
            string jalaliDateTime = format.ToLower();
            jalaliDateTime = jalaliDateTime.Replace("yyyy", pc.GetYear(dateTimeAsGregorian).ToString());
            jalaliDateTime = jalaliDateTime.Replace("mm", pc.GetMonth(dateTimeAsGregorian).ToString());
            jalaliDateTime = jalaliDateTime.Replace("dd", pc.GetDayOfMonth(dateTimeAsGregorian).ToString());
            jalaliDateTime = jalaliDateTime + " " + dateTimeAsGregorian.ToLongTimeString();
            return jalaliDateTime;
        }

    }

}
