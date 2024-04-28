using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.COLLECTIONS
{
    [TestClass()]
    public class DataResultsTests
    {
        [TestMethod(), TestCategory("unit")]
        public void ListResultTest()
        {
            ListResult<string>  listResult = new  ListResult<string>();
            listResult.AddRange(new string[] { "test1", "test2", "test3" });
            listResult.info = new BASICS.ResultInfoModel() { TotalCount = listResult.Count};

            string json = System.Text.Json.JsonSerializer.Serialize(listResult);



            Assert.IsTrue(json.Contains("test1"));

            // todo !!!
            //var liste2 = System.Text.Json.JsonSerializer.Deserialize<ListResult<string>>(json); 
            //Assert.AreEqual(liste2.Count, 3);
            //Assert.AreEqual(liste2.info.TotalResult, 3);

        }
         
    }
}