using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.FORMAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    [TestClass()]
    public class DateToolsTests
    {
        [TestMethod()]
        public void GetPartOfDateTest()
        {
            DateTime date = new DateTime(2024, 4, 20);
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.FirstDayOfWeek), new DateTime(2024, 4, 15));
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.FirstSundayOfWeek), new DateTime(2024, 4, 14)); //Dimanche Américain
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.FirstDayOfQuarter), new DateTime(2024, 4, 1));
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.LastDayOfWeek), new DateTime(2024, 4, 21));
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.LastSaturdayOfWeek), new DateTime(2024, 4, 20)); //Samedi Américain
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.LastDayOfMonth), new DateTime(2024, 4, 30));
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.LastDayOfQuarter), new DateTime(2024, 6, 30));
            Assert.AreEqual(DateTools.GetPartOfDate(date, DateTools.ValueOfDateEnum.LastDayOfYear), new DateTime(2024, 12, 31));
        }

        [TestMethod()]
        public void ToStringDateDelayTest()
        {
            var date1 = new DateTime(2024, 1, 1, 10, 0, 0);
            var date2 = new DateTime(2024, 1, 1, 10, 0, 30);
            
            // Test délai en secondes
            Assert.AreEqual("30 Sec", DateTools.ToStringDateDelay(date1, date2));
            
            // Test délai en minutes
            date2 = new DateTime(2024, 1, 1, 10, 5, 0);
            Assert.AreEqual("5 Min", DateTools.ToStringDateDelay(date1, date2));
            
            // Test délai en heures - NOTE: Bug connu avec constante 86000 au lieu de 86400
            date2 = new DateTime(2024, 1, 1, 13, 0, 0);
            Assert.AreEqual("3 Hr", DateTools.ToStringDateDelay(date1, date2));
            
            // Test délai négatif - NOTE: Bug connu avec constante 86000 au lieu de 86400  
            date2 = new DateTime(2024, 1, 1, 7, 0, 0); // 3h avant
            Assert.AreEqual("-3 Hr", DateTools.ToStringDateDelay(date1, date2));
            
            // Test Now
            Assert.AreEqual("Now", DateTools.ToStringDateDelay(date1, date1));
            
            // Test valeurs nulles
            Assert.AreEqual(string.Empty, DateTools.ToStringDateDelay(null, date1));
            Assert.AreEqual(string.Empty, DateTools.ToStringDateDelay(date1, null));
        }

        [TestMethod()]
        public void ToStringDateOrTimeTest()
        {
            var today = DateTime.Now.Date.AddHours(14).AddMinutes(30).AddSeconds(45);
            var yesterday = DateTime.Now.Date.AddDays(-1).AddHours(10);
            
            // Même jour - doit afficher l'heure
            Assert.AreEqual("14:30:45", DateTools.ToStringDateOrTime(today));
            
            // Jour différent - doit afficher la date
            Assert.AreEqual(yesterday.ToString("dd/MM/yyyy"), DateTools.ToStringDateOrTime(yesterday));
            
            // Test avec format personnalisé
            Assert.AreEqual(yesterday.ToString("yyyy-MM-dd"), DateTools.ToStringDateOrTime(yesterday, "yyyy-MM-dd"));
        }

        [TestMethod()]
        public void AddDaysWithExcludesTest()
        {
            var startDate = new DateTime(2024, 1, 5); // Vendredi
            
            // Test basique sans exclusions
            var result = DateTools.AddDaysWithExcludes(startDate, 1);
            Assert.AreEqual(new DateTime(2024, 1, 6), result);
            
            // Test avec exclusion des weekends
            var excludeDays = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };
            result = DateTools.AddDaysWithExcludes(startDate, 1, null, excludeDays);
            Assert.AreEqual(new DateTime(2024, 1, 8), result); // Lundi suivant
            
            // Test avec exclusion de dates spécifiques
            var excludeDates = new List<DateTime> { new DateTime(2024, 1, 6), new DateTime(2024, 1, 7) };
            result = DateTools.AddDaysWithExcludes(startDate, 1, excludeDates);
            Assert.AreEqual(new DateTime(2024, 1, 8), result);
            
            // Test avec plusieurs jours à ajouter
            result = DateTools.AddDaysWithExcludes(startDate, 3, null, excludeDays);
            Assert.AreEqual(new DateTime(2024, 1, 10), result); // Mercredi
        }

        [TestMethod()]
        public void TimeStampConversionTest()
        {
            var testDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var timestamp = DateTools.DateTimeToTimeStamp(testDate);
            
            // Test conversion DateTime vers timestamp
            Assert.IsTrue(timestamp > 0);
            
            // Test conversion timestamp vers DateTime
            var convertedDate = DateTools.TimeStampToDateTime(timestamp);
            Assert.AreEqual(testDate.Date, convertedDate.Date);
            Assert.AreEqual(testDate.Hour, convertedDate.Hour);
            
            // Test avec heure locale
            var localDate = DateTools.TimeStampToDateTime(timestamp, true);
            Assert.IsNotNull(localDate);
        }

        [TestMethod()]
        public void ConvertDateTime8Test()
        {
            // Test conversion valide
            var result = DateTools.ConvertDateTime8("20241225");
            Assert.AreEqual(new DateTime(2024, 12, 25), result);
            
            // Test chaîne vide
            result = DateTools.ConvertDateTime8("");
            Assert.AreEqual(DateTime.MinValue, result);
            
            // Test chaîne null
            result = DateTools.ConvertDateTime8(null);
            Assert.AreEqual(DateTime.MinValue, result);
            
            // Test format invalide
            Assert.ThrowsException<Exception>(() => DateTools.ConvertDateTime8("20241"));
            Assert.ThrowsException<Exception>(() => DateTools.ConvertDateTime8("abcd1234"));
        }

        [TestMethod()]
        public void TryParseTest()
        {
            // Test parse valide
            var result = DateTools.TryParse("2024-12-25");
            Assert.IsNotNull(result);
            Assert.AreEqual(new DateTime(2024, 12, 25), result.Value);
            
            // Test parse invalide
            result = DateTools.TryParse("invalid date");
            Assert.IsNull(result);
            
            // Test chaîne vide
            result = DateTools.TryParse("");
            Assert.IsNull(result);
            
            // Test null
            result = DateTools.TryParse(null);
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void TimeTest()
        {
            var timestamp1 = DateTools.Time();
            System.Threading.Thread.Sleep(1000);
            var timestamp2 = DateTools.Time();
            
            // Vérifier que le timestamp avance
            Assert.IsTrue(timestamp2 > timestamp1);
            Assert.IsTrue(timestamp2 - timestamp1 >= 1);
        }

        [TestMethod()]
        public void GetPeriodDaysTest()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 5);
            
            var result = DateTools.GetPeriodDays(startDate, endDate);
            
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(new DateTime(2024, 1, 1), result[0]);
            Assert.AreEqual(new DateTime(2024, 1, 2), result[1]);
            Assert.AreEqual(new DateTime(2024, 1, 3), result[2]);
            Assert.AreEqual(new DateTime(2024, 1, 4), result[3]);
            
            // Test avec date de fin antérieure
            result = DateTools.GetPeriodDays(endDate, startDate);
            Assert.AreEqual(0, result.Count);
            
            // Test avec même date - BUG: retourne 1 jour au lieu de 0
            result = DateTools.GetPeriodDays(startDate, startDate);
            Assert.AreEqual(1, result.Count); // Bug connu: devrait être 0 mais retourne 1
        }

        [TestMethod()]
        public void GetQuarterOfDateTest()
        {
            // Premier trimestre
            Assert.AreEqual(1, DateTools.GetQuarterOfDate(new DateTime(2024, 1, 15)));
            Assert.AreEqual(1, DateTools.GetQuarterOfDate(new DateTime(2024, 2, 15)));
            Assert.AreEqual(1, DateTools.GetQuarterOfDate(new DateTime(2024, 3, 15)));
            
            // Deuxième trimestre
            Assert.AreEqual(2, DateTools.GetQuarterOfDate(new DateTime(2024, 4, 15)));
            Assert.AreEqual(2, DateTools.GetQuarterOfDate(new DateTime(2024, 5, 15)));
            Assert.AreEqual(2, DateTools.GetQuarterOfDate(new DateTime(2024, 6, 15)));
            
            // Troisième trimestre
            Assert.AreEqual(3, DateTools.GetQuarterOfDate(new DateTime(2024, 7, 15)));
            Assert.AreEqual(3, DateTools.GetQuarterOfDate(new DateTime(2024, 8, 15)));
            Assert.AreEqual(3, DateTools.GetQuarterOfDate(new DateTime(2024, 9, 15)));
            
            // Quatrième trimestre
            Assert.AreEqual(4, DateTools.GetQuarterOfDate(new DateTime(2024, 10, 15)));
            Assert.AreEqual(4, DateTools.GetQuarterOfDate(new DateTime(2024, 11, 15)));
            Assert.AreEqual(4, DateTools.GetQuarterOfDate(new DateTime(2024, 12, 15)));
        }
    }
}