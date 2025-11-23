// Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Calculateurs de formule
    /// </summary>
    public static class FormulaDateFunctions
    {

        public static string DefaultDateFormatConst = "dd/MM/yyyy HH:mm:ss";// Date française par default
        public static string GetDefaultDateFormat(string[] args, int argnum)  
            => (args.Length > argnum && !string.IsNullOrWhiteSpace(args[argnum])) ? args[argnum] : DefaultDateFormatConst;


        [Formula("now", 0, "Get today's DateTime")]
        public static object Now(string[] args)
            => DateTime.Now.ToString(GetDefaultDateFormat(args, 0));


        /// <summary>
        /// Formula calculators
        /// </summary>
 
        [Formula("month", 1, "Get the month of the date. ", UseExample = "Month(Now())=12")]
        public static object Month(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).Month.ToString();

        [Formula("year", 1, "Get the year of the date. ", UseExample = "Year(Now())=2024")]
        public static object Year(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).Year.ToString();

        [Formula("day", 1, "Get the day of the date. ", UseExample = "Day(Now())=3")]
        public static object Day(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).Day.ToString();

   
  
        [Formula("todate", 2, "Reformat a date. ", UseExample = "todate(now(),'dd/MM/yyyy HH:mm:ss')")]
        public static object todate(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).ToString(args[1]);


        [Formula("datediff", 2, "Diff between two dates")] 
        public static object DateDiff(string[] args)
            => (Nglib.FORMAT.ConvertTools.ToDateTime(args[0]) - Nglib.FORMAT.ConvertTools.ToDateTime(args[1])).TotalDays.ToString();


        [Formula("addday", 2, "Add days to a date")]
        public static object DateAdd(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).AddDays(Convert.ToDouble(args[1])).ToString(GetDefaultDateFormat(args,2));


        [Formula("addmonth", 2, "Add months to a date")]
        public static object AddMonth(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).AddMonths(Convert.ToInt32(args[1])).ToString(GetDefaultDateFormat(args, 2));

        [Formula("addyear", 2, "Add years to a date")]
        public static object AddYear(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).AddYears(Convert.ToInt32(args[1])).ToString(GetDefaultDateFormat(args, 2));

        [Formula("addhour", 2, "Add hours to a date")]
        public static object AddHour(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).AddHours(Convert.ToDouble(args[1])).ToString(GetDefaultDateFormat(args, 2));

        [Formula("addminute", 2, "Add minutes to a date")]
        public static object AddMinute(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).AddMinutes(Convert.ToDouble(args[1])).ToString(GetDefaultDateFormat(args, 2));

        [Formula("addsecond", 2, "Add seconds to a date")]
        public static object AddSecond(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).AddSeconds(Convert.ToDouble(args[1])).ToString(GetDefaultDateFormat(args, 2));

        [Formula("dateabs", 1, Description = "Date without hours ", UseExample = "DateAbs(now()) = 03/12/2024")]
        public static object DateAbs(string[] args)
            => Nglib.FORMAT.ConvertTools.ToDateTime(args[0]).ToString("dd/MM/yyyy");


        [Formula("PartOfDate", 1, Description = "Get date Part ", UseExample = "DatePart(Now(),'FirstDayOfQuarter/FirstDayOfWeek/FirstSundayOfWeek/LastDayOfMonth/LastDayOfWeek/LastDayOfYear/LastSaturdayOfWeek')")]
        public static object DatePart(string[] args)
            => Nglib.FORMAT.DateTools.GetPartOfDate(Nglib.FORMAT.ConvertTools.ToDateTime(args[0]), Enum.Parse<Nglib.FORMAT.DateTools.ValueOfDateEnum>(args[1]))
            .ToString(GetDefaultDateFormat(args, 2));


     
        [Formula("ToTimestamp", 1, Description = "Date in Linux numeric format.")]
        public static object ToTimestamp(string[] args)
             => Nglib.FORMAT.DateTools.DateTimeToTimeStamp(Nglib.FORMAT.ConvertTools.ToDateTime(args[0])).ToString();

        [Formula("FromTimestamp", 1, Description = "Date in Linux numeric format.")]
        public static object FromTimestamp(string[] args)
            => Nglib.FORMAT.DateTools.TimeStampToDateTime(Convert.ToInt64(args[0])).ToString(GetDefaultDateFormat(args, 1));


        //[Formula("DayPeriods", 1, Description = "Retourne un Array de tous les jours entre deux dates")]
        //public static object DayPeriods(string[] args)
        //     => Nglib.FORMAT.DateTools.GetPeriodDays(Nglib.FORMAT.ConvertPlus.ToDateTime(args[0])).ToString();


    }
}
