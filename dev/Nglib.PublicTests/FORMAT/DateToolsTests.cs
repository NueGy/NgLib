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
    }
}