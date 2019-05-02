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
using System.Linq;
using System.Text.RegularExpressions;

namespace Ybm.NCronTabCore
{
    internal class CronPattern
    {
        public CronPattern()
        {
            minutes = new HashSet<int>();
            hours = new HashSet<int>();
            days = new HashSet<int>();
            months = new HashSet<int>();
            years = new HashSet<int>();
            weekDays = new HashSet<int>();
        }
        HashSet<int> minutes { get; set; }
        HashSet<int> hours { get; set; }
        HashSet<int> days { get; set; }
        HashSet<int> months { get; set; }
        HashSet<int> years { get; set; }
        HashSet<int> weekDays { get; set; }



        public Pattern Parse(string cron)
        {
            var parts = cron.Split(' ');

            var pattern = new Pattern();

            pattern.PatternSecond = parts[0];
            pattern.PatternMinute = parts[1];
            pattern.PatternHour = parts[2];
            pattern.PatternDayOfMonth = parts[3];
            pattern.PatternMonth = parts[4];
            pattern.PatternDayOfWeek = parts[5];

            // secondly
            if (Regex.IsMatch(cron, @"((\*\/\d+|\d+) \* \* \* \* \*)+"))
            {
                pattern.UnitType = EnumUnitType.Secondly;
                ParsePattern(pattern.PatternSecond, pattern);
                return pattern;
            }

            // minutely
            if (Regex.IsMatch(cron, @"(\* (\*\/\d+|\d+) \* \* \* \*)+"))
            {
                pattern.UnitType = EnumUnitType.Minutely;
                ParsePattern(pattern.PatternMinute, pattern);
                return pattern;
            }

            // hourly
            if (Regex.IsMatch(cron, @"(\* \* (\*\/\d+|\d+) \* \* \*)+"))
            {
                pattern.UnitType = EnumUnitType.Hourly;
                ParsePattern(pattern.PatternHour, pattern);
                return pattern;
            }

            // Daily
            if (Regex.IsMatch(cron, @"(\* \d+ \d+ \*/\d+ \* \*)+"))
            {
                pattern.UnitType = EnumUnitType.Daily;

                var everNDays = pattern.PatternDayOfMonth.Split('/')[1].Split(',');
                pattern.Days.AddRange(Array.ConvertAll(everNDays, int.Parse));

                pattern.Hour = int.Parse(pattern.PatternHour);
                pattern.Minute = int.Parse(pattern.PatternMinute);
                return pattern;
            }



            // Days of months
            if (Regex.IsMatch(cron, @"(^\* \d+ \d+ \d+ \*\/(\d+|\,)+ \*)"))
            {
                pattern.UnitType = EnumUnitType.Monthly;
                var months = pattern.PatternMonth.Split('/')[1].Split(',');
                pattern.Months.AddRange(Array.ConvertAll(months, int.Parse));

                var daysOfmonth = pattern.PatternDayOfMonth.Split(',');
                pattern.Days.AddRange(Array.ConvertAll(daysOfmonth, int.Parse));

                pattern.Hour = int.Parse(pattern.PatternHour);
                pattern.Minute = int.Parse(pattern.PatternMinute);
                return pattern;
            }

            // Days of week
            if (Regex.IsMatch(cron, @"(^\* \d+ \d+ \* \* \*\/(\d+|\,)+)"))
            {
                pattern.UnitType = EnumUnitType.Weekly;

                var daysOfWeek = pattern.PatternDayOfWeek.Split('/')[1].Split(',');
                pattern.Days.AddRange(Array.ConvertAll(daysOfWeek, int.Parse));

                pattern.Hour = int.Parse(pattern.PatternHour);
                pattern.Minute = int.Parse(pattern.PatternMinute);
                return pattern;
            }

            //Last day or first day on months
            if (Regex.IsMatch(cron, @"(\* \d+ \d+ (\d+|L) ((\*)|(\*\/)(\d+|\,)+) \*)"))
            {
                pattern.UnitType = EnumUnitType.Monthly;

                int day = 0;
                if (int.TryParse(pattern.PatternDayOfMonth, out day))
                {
                    pattern.Days.Add(day);
                }
                else
                {
                    if (pattern.PatternDayOfMonth.ToLower() == "l")
                    {
                        pattern.Days.Add(-1);
                    }
                }

                if (pattern.PatternMonth != "*")
                {
                    var months = pattern.PatternMonth.Split('/')[1].Split(',');
                    pattern.Months.AddRange(Array.ConvertAll(months, int.Parse));
                }
                pattern.Hour = int.Parse(pattern.PatternHour);
                pattern.Minute = int.Parse(pattern.PatternMinute);
                return pattern;
            }

            return null;
        }

        private void ParsePattern(string patternSecond, Pattern pattern)
        {
            if (patternSecond.Contains("/"))
            {
                var parts = patternSecond.Split('/');
                if (parts[0] == "*")
                {
                    int unit;
                    if (int.TryParse(parts[1], out unit))
                    {
                        pattern.EveryNUnit = unit;
                    }
                    else
                    {
                        pattern.Units = Array.ConvertAll(parts[1].Split(','), int.Parse).ToList();

                    }
                }
            }
            else
                pattern.Unit = int.Parse(patternSecond);

        }

    }
}
