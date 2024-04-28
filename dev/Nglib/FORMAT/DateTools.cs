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
        ///     Obtenir la différence entre 2 dates en chaine simplifiée ( 4 Hr , 26 Sec , ...)
        /// </summary>
        public static string ToStringDateDelay(DateTime? date1, DateTime? date2)
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

        /// <summary>
        /// Si jour même, affiche l'heure, sinon la date
        /// </summary>
        public static string ToStringDateOrTime(DateTime date1, string dateFormat = "dd/MM/yyyy")
        {
            if (date1.Date == DateTime.Now.Date)
                return date1.ToString("HH:mm:ss");
            return date1.ToString(dateFormat);
        }


        /// <summary>
        ///     Permet d'obtenir la prochaine date avec gestion des jours interdits (Jours fériés)
        /// </summary>
        public static DateTime AddDaysWithExcludes(DateTime startDate, int addDays = 1,
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
        public static DateTime TimeStampToDateTime(long unixTimeStamp, bool UseLocalTime = false)
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
                //if (retour.Year < 2000 && retour.Year > 2099) throw new Exception("date year invalide");
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
        /// Obtenir une Date en fonction de la valeur
        /// </summary>
        /// <param name="date"></param>
        /// <param name="valueOfDate"></param>
        /// <returns></returns>
        public static DateTime GetPartOfDate(DateTime date, ValueOfDateEnum valueOfDate)
        {
            switch (valueOfDate)
            {
                case ValueOfDateEnum.LastDayOfMonth:
                    return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                case ValueOfDateEnum.LastDayOfYear:
                    return new DateTime(date.Year, 12, 31);
                case ValueOfDateEnum.FirstSundayOfWeek:
                    return date.AddDays(-1 * Convert.ToInt32(date.DayOfWeek, CultureInfo.InvariantCulture));
                    case ValueOfDateEnum.FirstDayOfWeek:
                    return date.AddDays(-1 * (Convert.ToInt32(date.DayOfWeek, CultureInfo.InvariantCulture)-1));
                case ValueOfDateEnum.LastDayOfWeek:
                    return date.AddDays(7 - Convert.ToInt32(date.DayOfWeek, CultureInfo.InvariantCulture));
                case ValueOfDateEnum.LastSaturdayOfWeek:
                    return date.AddDays(6 - Convert.ToInt32(date.DayOfWeek, CultureInfo.InvariantCulture));
                case ValueOfDateEnum.FirstDayOfQuarter:
                    return new DateTime(date.Year, (GetQuarterOfDate(date) - 1) * 3 + 1, 1);
                case ValueOfDateEnum.LastDayOfQuarter:
                    return new DateTime(date.Year, (GetQuarterOfDate(date) - 1) * 3 + 3, 1).AddMonths(1).AddDays(-1);
                default:
                    return date;
            }
        }



        /// <summary>
        ///     Obtient le trimestre
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetQuarterOfDate(DateTime date) =>  ((date.Month + 2) / 3);
    


        public enum ValueOfDateEnum
        {
            /// <summary>
            /// Obtenir le premier jour de la semaine Française
            /// </summary>
            FirstDayOfWeek,

            /// <summary>
            /// Obtenir le premier jour de la semaine Américaine
            /// </summary>
            FirstSundayOfWeek,

            /// <summary>
            /// Obtenir le dernier jour de la semaine
            /// </summary>
            LastDayOfWeek,

            /// <summary>
            /// Obtenir le dernier jour de la semaine Américaine
            /// </summary>
            LastSaturdayOfWeek,

            /// <summary>
            /// Obtenir le dernier jour du mois
            /// </summary>
            LastDayOfMonth,

            /// <summary>
            /// Obtenir le dernier jour de l'année
            /// </summary>
            LastDayOfYear,

            /// <summary>
            /// Obtenir le premier jour du trimestre
            /// </summary>
            FirstDayOfQuarter,

            /// <summary>
            /// Obtenir le dernier jour du trimestre
            /// </summary>
            LastDayOfQuarter
        }
    }
}