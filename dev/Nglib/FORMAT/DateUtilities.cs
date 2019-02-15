// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Outils pour manipulations des dates
    /// </summary>
    public static class DateUtilities
    {



        /// <summary>
        /// Common DateTime Methods.
        /// </summary>
        public enum Quarter : int
        {
            First = 1,
            Second = 2,
            Third = 3,
            Fourth = 4
        }

        public enum Month : int
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }


        #region Obsolete


        /// <summary>
        /// Obtenir le temps restant entre 2 dates en string
        /// </summary>
        public static string stringdepuisdate(DateTime date1, DateTime date2)
        {
            int next = (int)(date2 - date1).TotalSeconds;
            string isneg = "";
            if (next < 0) { next = Math.Abs(next); isneg = "-"; }
            if (next == 0) return "Now";
            else if (next < 60) return isneg + next.ToString() + " Sec";
            else if (next < 3600) return isneg + (next / 60).ToString() + " Min";
            else if (next < 86000) return isneg + (next / 3600).ToString() + " Hr";
            else return isneg + (next / 86000).ToString() + " Jr";

        }



        /// <summary>
        /// Obtient le timestamp actuel
        /// </summary>
        /// <returns></returns>
        public static long time()
        {
            return DateTimeToTimeStamp(DateTime.UtcNow, false);
        }




        /// <summary>
        /// UNIX TimeStamp To DateTime
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(double unixTimeStamp, bool UseLocalTime = false)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            if (UseLocalTime) dtDateTime = dtDateTime.ToLocalTime();
            return dtDateTime;
        }


        /// <summary>
        /// DateTime To UNIX TimeStamp
        /// </summary>
        /// <param name="time"></param>
        /// <param name="UseLocalTime"></param>
        /// <returns></returns>
        public static long DateTimeToTimeStamp(DateTime time, bool UseLocalTime = false)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = time.Subtract(unixEpoch);

            return (long)span.TotalSeconds;
        }



        public static DateTime Ago(this TimeSpan value)
        {
            return DateTime.Now.Add(value.Negate());
        }

        public static DateTime FromNow(this TimeSpan value)
        {
            return DateTime.Now.Add(value);
        }









        [Obsolete]
        public static long time(DateTime time)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan span = time.Subtract(unixEpoch);

            return (long)span.TotalSeconds;
        }


        [Obsolete]
        public static DateTime fromPHPTime(long ticks)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            return unixEpoch.Add(new TimeSpan(0, 0, (int)ticks));
        }

        //public static string FormatDate(DateTime date, string format)
        //{
        //    string retour = date.ToString(format);
        //    if (retour.Contains("QQQ"))
        //        retour=retour.Replace("QQQ", DATA.FORMAT.StringUtilities.Complete(date.DayOfYear.ToString(), 3, true));
            
        //    return retour;
        //}


        #endregion







        public static DateTime GetStartOfQuarter( int Year, Quarter Qtr )
        {
            if( Qtr == Quarter.First )    // 1st Quarter = January 1 to March 31
                return new DateTime( Year, 1, 1, 0, 0, 0, 0 );
            else if( Qtr == Quarter.Second ) // 2nd Quarter = April 1 to June 30
                return new DateTime( Year, 4, 1, 0, 0, 0, 0 );
            else if( Qtr == Quarter.Third ) // 3rd Quarter = July 1 to September 30
                return new DateTime( Year, 7, 1, 0, 0, 0, 0 );
            else // 4th Quarter = October 1 to December 31
                return new DateTime( Year, 10, 1, 0, 0, 0, 0 );
        }

        public static DateTime GetEndOfQuarter( int Year, Quarter Qtr )
        {
            if( Qtr == Quarter.First )    // 1st Quarter = January 1 to March 31
                return new DateTime( Year, 3, 
                       DateTime.DaysInMonth( Year, 3 ), 23, 59, 59, 999 );
            else if( Qtr == Quarter.Second ) // 2nd Quarter = April 1 to June 30
                return new DateTime( Year, 6, 
                       DateTime.DaysInMonth( Year, 6 ), 23, 59, 59, 999 );
            else if( Qtr == Quarter.Third ) // 3rd Quarter = July 1 to September 30
                return new DateTime( Year, 9, 
                       DateTime.DaysInMonth( Year, 9 ), 23, 59, 59, 999 );
            else // 4th Quarter = October 1 to December 31
                return new DateTime( Year, 12, 
                       DateTime.DaysInMonth( Year, 12 ), 23, 59, 59, 999 );
        }

        public static Quarter GetQuarter( Month Month )
        {
            if( Month <= Month.March )
            // 1st Quarter = January 1 to March 31
                return Quarter.First;
            else if( ( Month >= Month.April ) && ( Month <= Month.June ) )
            // 2nd Quarter = April 1 to June 30
                return Quarter.Second;
            else if( ( Month >= Month.July ) && ( Month <= Month.September ) )
            // 3rd Quarter = July 1 to September 30
                return Quarter.Third;
            else // 4th Quarter = October 1 to December 31
                return Quarter.Fourth;
        }

        public static DateTime GetEndOfLastQuarter()
        {                 
            if( (Month)DateTime.Now.Month <= Month.March )
            //go to last quarter of previous year
                return GetEndOfQuarter( DateTime.Now.Year - 1, Quarter.Fourth);
            else //return last quarter of current year
                return GetEndOfQuarter( DateTime.Now.Year, 
                  GetQuarter( (Month)DateTime.Now.Month));
        }

        public static DateTime GetStartOfLastQuarter()
        {
            if( (Month)DateTime.Now.Month <= Month.March )
            //go to last quarter of previous year
                return GetStartOfQuarter( DateTime.Now.Year - 1, Quarter.Fourth);
            else //return last quarter of current year
                return GetStartOfQuarter( DateTime.Now.Year, 
                  GetQuarter( (Month)DateTime.Now.Month));
        }

        public static DateTime GetStartOfCurrentQuarter()
        {
            return GetStartOfQuarter( DateTime.Now.Year, 
                   GetQuarter( (Month)DateTime.Now.Month ));
        }

        public static DateTime GetEndOfCurrentQuarter()
        {
            return GetEndOfQuarter( DateTime.Now.Year, 
                   GetQuarter( (Month)DateTime.Now.Month ));
        }
 








        public static DateTime GetStartOfLastWeek()
        {
            int DaysToSubtract = (int)DateTime.Now.DayOfWeek + 7;
            DateTime dt = 
              DateTime.Now.Subtract(System.TimeSpan.FromDays( DaysToSubtract ) );
            return new DateTime( dt.Year, dt.Month, dt.Day, 0, 0, 0, 0 );
        }

        public static DateTime GetEndOfLastWeek()
        {
            DateTime dt = GetStartOfLastWeek().AddDays(6);
            return new DateTime( dt.Year, dt.Month, dt.Day, 23, 59, 59, 999 );
        }

        public static DateTime GetStartOfCurrentWeek()
        {
            int DaysToSubtract = (int)DateTime.Now.DayOfWeek ;
            DateTime dt = 
              DateTime.Now.Subtract( System.TimeSpan.FromDays( DaysToSubtract ) );
            return new DateTime( dt.Year, dt.Month, dt.Day, 0, 0, 0, 0 );
        }

        public static DateTime GetEndOfCurrentWeek()
        {
            DateTime dt = GetStartOfCurrentWeek().AddDays(6);
            return new DateTime( dt.Year, dt.Month, dt.Day, 23, 59, 59, 999 );
        }
 









        public static DateTime GetStartOfMonth( Month Month, int Year )
        {
             return new DateTime( Year, (int)Month, 1, 0, 0, 0, 0 );
        }

        public static DateTime GetEndOfMonth( Month Month, int Year )
        {
            return new DateTime( Year, (int)Month, 
               DateTime.DaysInMonth( Year, (int)Month ), 23, 59, 59, 999 );
        }

        public static DateTime GetStartOfLastMonth()
        {
            if( DateTime.Now.Month == 1 )
                return GetStartOfMonth( (Month)12, DateTime.Now.Year - 1);
            else
                return GetStartOfMonth( (Month)DateTime.Now.Month -1, DateTime.Now.Year );
        }

        public static DateTime GetEndOfLastMonth()
        {
            if( DateTime.Now.Month == 1 )
                return GetEndOfMonth( (Month)12, DateTime.Now.Year - 1);
            else
                return GetEndOfMonth( (Month)DateTime.Now.Month -1, DateTime.Now.Year );
        }

        public static DateTime GetStartOfCurrentMonth()
        {
            return GetStartOfMonth( (Month)DateTime.Now.Month, DateTime.Now.Year );
        }

        public static DateTime GetEndOfCurrentMonth()
        {
            return GetEndOfMonth( (Month)DateTime.Now.Month, DateTime.Now.Year );
        }




        public static DateTime GetStartOfYear( int Year )
        {
            return new DateTime( Year, 1, 1, 0, 0, 0, 0 );
        }

        public static DateTime GetEndOfYear( int Year )
        {
            return new DateTime( Year, 12, 
              DateTime.DaysInMonth( Year, 12 ), 23, 59, 59, 999 );
        }

        public static DateTime GetStartOfLastYear()
        {
            return GetStartOfYear( DateTime.Now.Year - 1 );
        }

        public static DateTime GetEndOfLastYear()
        {
            return GetEndOfYear( DateTime.Now.Year - 1 );
        }

        public static DateTime GetStartOfCurrentYear()
        {
            return GetStartOfYear( DateTime.Now.Year );
        }

        public static DateTime GetEndOfCurrentYear()
        {
            return GetEndOfYear( DateTime.Now.Year );
        }





        public static DateTime GetStartOfDay( DateTime date )
        {
            return new DateTime( date.Year, date.Month, date.Day, 0, 0, 0, 0 );
        }

        public static DateTime GetEndOfDay( DateTime date )
        {
            return new DateTime( date.Year, date.Month, 
                                 date.Day, 23, 59, 59, 999 );
        }



        public static int GetDayCountInMonth(int month, int year)
        {
            if (month < 1)
            {
                throw new ArgumentException("month must be in range 1-12");
            }

            if (month > 12)
            {
                throw new ArgumentException("month must be in range 1-12");
            }

            if (year > 9999)
            {
                throw new ArgumentException("year must be in range 1-9999");
            }

            var dt1 = new DateTime(year, month, 1);
            if (month == 12)
            {
                month = 1;
                year++;
            }
            else
            {
                month++;
            }

            var dt2 = new DateTime(year, month, 1);
            var days = (dt2 - dt1).Days;
            return days;
        }



        public static string[] GetDayNamesInMonth(int month, int year)
        {
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentException(month + " must be in range 1-12");
            }

            var dt = new DateTime(year, month, 1);
            var numberOfDays = GetDayCountInMonth(month, year);
            var days = new string[numberOfDays];
            for (var i = 0; i < numberOfDays; i++)
            {
                days[i] = Enum.GetName(typeof(DayOfWeek), dt.DayOfWeek);
                dt = dt.AddDays(Convert.ToDouble(1));
            }

            return days;
        }



        public static List<DateTime> GetDaysInPeriod(DateTime dateStart, DateTime dateEnd, DayOfWeek dayOfWeek)
        {
            var daysInPeriod = new List<DateTime>();
            var date = dateStart;
            var dayFound = false;
            for (var i = 0; date <= dateEnd; i++)
            {
                if (date.DayOfWeek == dayOfWeek)
                {
                    daysInPeriod.Add(date);
                    dayFound = true;
                }

                if (dayFound)
                {
                    date = date.AddDays(7);
                    continue;
                }

                date = date.AddDays(1);
            }

            return daysInPeriod;
        }



        public static List<DateTime> GetDaysInYear(DateTime dateStart, DayOfWeek dayOfWeek)
        {
            var dateEnd = dateStart.AddMonths(12);
            return GetDaysInPeriod(dateStart, dateEnd, dayOfWeek);
        }


        public static DateTime GetEndOfWeek()
        {
            return GetEndOfWeek(DateTime.Today);
        }

        /// <summary>
        /// Gets the end of week.
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <returns>Returns end of week date</returns>
        public static DateTime GetEndOfWeek(DateTime input)
        {
            var dayOfWeek = Convert.ToInt32(input.DayOfWeek, System.Globalization.CultureInfo.InvariantCulture);
            return input.AddDays(6 - dayOfWeek);
        }



        public static DateTime GetStartOfWeek()
        {
            return GetStartOfWeek(DateTime.Today);
        }

        /// <summary>
        /// Gets the start of week.
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <returns>Returns start of supplied week</returns>
        public static DateTime GetStartOfWeek(DateTime input)
        {
            var dayOfWeek = Convert.ToInt32(input.DayOfWeek, System.Globalization.CultureInfo.InvariantCulture);
            return input.AddDays(-1 * dayOfWeek);
        }




      }
}

