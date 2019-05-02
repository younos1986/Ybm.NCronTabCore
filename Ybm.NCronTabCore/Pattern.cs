using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ybm.NCronTabCore
{
    public enum EnumUnitType
    {
        Secondly = 0,
        Minutely = 1,
        Hourly = 2,
        Daily = 3,
        Weekly = 4,
        Monthly = 5,
        Yearly = 6
    }

    public class Pattern
    {

        public Pattern()
        {
            Days = new List<int>();
            Months = new List<int>();
        }

        public EnumUnitType UnitType { get; set; }
        public int Unit { get; set; }
        public int EveryNUnit{ get; set; }
        public List<int> Units { get; set; }
        

        public int Minute { get; set; }
        public int Hour { get; set; }

        public List<int> Days { get; set; }
        public List<int> Months { get; set; }


        public string PatternSecond { get; set; }
        public string PatternMinute { get; set; }
        public string PatternHour { get; set; }
        public string PatternDayOfMonth { get; set; }
        public string PatternMonth { get; set; }
        public string PatternDayOfWeek { get; set; }
    }
}
