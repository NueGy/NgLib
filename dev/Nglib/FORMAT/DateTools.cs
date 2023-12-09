// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nglib.FORMAT
{
    /// <summary>
    ///     Outils pour manipulations des dates
    /// </summary>
    public static class DateTools
    {
        /// <summary>
        ///     Mois enum
        /// </summary>
        public enum MonthEnum
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


        /// <summary>
        ///     Obtenir la différence entre 2 dates en chaine simplifiée ( 4 Hr , 26 Sec , ...)
        /// </summary>
        public static string StringDateDelay(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue) return string.Empty;
            var next = (int)(date2.Value - date1.Value).TotalSeconds;
            var isneg = "";
            if (next < 0)
            {
                next = Math.Abs(next);
                isneg = "-";
            }

            if (next < 2) return "Now";
            if (next < 60) return isneg + next + " Sec";
            if (next < 3600) return isneg + next / 60 + " Min";
            if (next < 86000) return isneg + next / 3600 + " Hr";
            return isneg + next / 86000 + " Days";
        }

        public static string StringDateDay(DateTime date1, string dateFormat = "dd/MM/yyyy")
        {
            if (date1.Date == DateTime.Now.Date)
                return date1.ToString("HH:mm:ss");
            return date1.ToString(dateFormat);
        }


        /// <summary>
        ///     Permet d'obtenir la prochaine date avec gestion des jours interdits (Jours fériés)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="addDays"></param>
        /// <param name="excludesDates"></param>
        /// <param name="excludesDays"></param>
        /// <returns></returns>
        public static DateTime NextDateWithExcludes(DateTime startDate, int addDays = 1,
            List<DateTime> excludesDates = null, List<DayOfWeek> excludesDays = null)
        {
            var lastDate = startDate;
            var addedDays = 0;
            while (true)
            {
                lastDate = lastDate.AddDays(1);
                if (excludesDays != null && excludesDays.Contains(lastDate.DayOfWeek)) continue;
                if (excludesDates != null && excludesDates.Contains(lastDate)) continue;
                addedDays++;
                if (addedDays == addDays)
                    return lastDate;
            }
        }


        /// <summary>
        ///     UNIX TimeStamp To DateTime
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(double unixTimeStamp, bool UseLocalTime = false)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            if (UseLocalTime) dtDateTime = dtDateTime.ToLocalTime();
            return dtDateTime;
        }


        /// <summary>
        ///     DateTime To UNIX TimeStamp
        /// </summary>
        /// <param name="time"></param>
        /// <param name="UseLocalTime"></param>
        /// <returns></returns>
        public static long DateTimeToTimeStamp(DateTime time, bool UseLocalTime = false)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var span = time.Subtract(unixEpoch);

            return (long)span.TotalSeconds;
        }


        /// <summary>
        ///     Convertir une date sur 8cars : 20211231
        /// </summary>
        /// <param name="datestr"></param>
        /// <returns></returns>
        public static DateTime ConvertDateTime8(string datestr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datestr)) return DateTime.MinValue;
                datestr = datestr.Trim();
                if (datestr.Length != 8) throw new Exception("invalidString for 8Chars Date");
                return new DateTime(Convert.ToInt32(datestr.Substring(0, 4)), Convert.ToInt32(datestr.Substring(4, 2)),
                    Convert.ToInt32(datestr.Substring(6, 2)));
            }
            catch (Exception ex)
            {
                throw new Exception("ConvertDateTime8 " + ex.Message);
            }
        }


        /// <summary>
        ///     Try parse simplifié
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime? TryParse(string input)
        {
            var ret = DateTime.MinValue;
            if (DateTime.TryParse(input, out ret)) return ret;
            return null;
        }


        /// <summary>
        ///     Obtient le timestamp actuel
        /// </summary>
        /// <returns></returns>
        public static long Time()
        {
            return DateTimeToTimeStamp(DateTime.UtcNow);
        }


        /// <summary>
        ///     Obtient le trimestre
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetQuarterOfDate(DateTime date)
        {
            switch (date.Month)
            {
                case 1: return 1;
                case 2: return 1;
                case 3: return 1;
                case 4: return 2;
                case 5: return 2;
                case 6: return 2;
                case 7: return 3;
                case 8: return 3;
                case 9: return 3;
                case 10: return 4;
                case 11: return 4;
                case 12: return 4;
                default: return 0;
            }
        }


        /// <summary>
        ///     Permet d'obtenir toutes les jounrées dans une période
        /// </summary>
        /// <param name="dateStart">Date de début incluse</param>
        /// <param name="dateEnd">Date de fin, non incluse</param>
        /// <returns>periode range</returns>
        public static List<DateTime> GetPeriodDays(DateTime dateStart, DateTime dateEnd)
        {
            if (dateStart > dateEnd) return new List<DateTime>();
            var daysInPeriod = new List<DateTime>();
            var lastdate = dateStart.Date;
            while (true)
            {
                daysInPeriod.Add(lastdate);
                lastdate = lastdate.AddDays(1);
                if (lastdate >= dateEnd.Date) break;
            }

            return daysInPeriod;
        }


        /// <summary>
        ///     Gets the end of week.
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <returns>Returns end of week date</returns>
        public static DateTime GetEndOfWeek(DateTime input)
        {
            var dayOfWeek = Convert.ToInt32(input.DayOfWeek, CultureInfo.InvariantCulture);
            return input.AddDays(6 - dayOfWeek);
        }


        /// <summary>
        ///     Gets the start of week.
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <returns>Returns start of supplied week</returns>
        public static DateTime GetStartOfWeek(DateTime input)
        {
            var dayOfWeek = Convert.ToInt32(input.DayOfWeek, CultureInfo.InvariantCulture);
            return input.AddDays(-1 * dayOfWeek);
        }


        /// <summary>
        ///     Obtient le dernier jour du mois
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DateTime GetEndOfMounth(DateTime input)
        {
            return new DateTime(input.Year, input.Month, DateTime.DaysInMonth(input.Year, input.Month));
        }
    }
}