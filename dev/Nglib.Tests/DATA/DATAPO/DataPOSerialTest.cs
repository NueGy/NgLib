using System;
using Nglib.DATA.DATAPO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Nglib.DATA.DATAPO
{
    [TestClass]
    public class DataPOSerialTest
    {


        [TestMethod]
        public void TestwithPOSerial() // note : les PO sont 20% plus long que les POCO en serial et deserial
        {

            DataPoSample po = new DataPoSample();
            po.TestId = 5;
            po.Pseudo = "totoLtoto";
            po.FluxXml["/param/maval5"] = "totop";
            po.FluxJson["/param/mavalj8"] = "totopttttt";

            for (int i = 0; i < 1000; i++)
            {
                po.TestId++;

                Dictionary<string,object> valspo = po.GetValues();
                string jsonpo = Newtonsoft.Json.JsonConvert.SerializeObject(valspo);
                Dictionary<string, object> valspo2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonpo);
                DataPoSample po2 = new DataPoSample();
                po2.SetValues(valspo2);
                Assert.AreEqual(po2.Pseudo, po.Pseudo);
            }

        }


        [TestMethod]
        public void TestwithPOSimple() // note : les PO sont 20% plus long que les POCO en serial et deserial
        {

            DataPoSample po = new DataPoSample();
            po.TestId = 5;
            po.Pseudo = "totoLtoto";
            po.FluxXml["/param/maval5"] = "totop";

            for (int i = 0; i < 1000; i++)
            {
                po.TestId++;
                string jsonpo = Newtonsoft.Json.JsonConvert.SerializeObject(po);
                DataPoSample po2 = Newtonsoft.Json.JsonConvert.DeserializeObject<DataPoSample>(jsonpo);
                Assert.AreEqual(po2.Pseudo, po.Pseudo);
            }

        }



        [TestMethod]
        public void TestwithPoco()
        {

            DataModelSample po = new DataModelSample();
            po.TestId = 5;
            po.Pseudo = "totoLtoto";


            for (int i = 0; i < 1000; i++)
            {
                po.TestId++;
                string jsonpo = Newtonsoft.Json.JsonConvert.SerializeObject(po);
                DataModelSample po2 = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModelSample>(jsonpo);
                Assert.AreEqual(po2.Pseudo, po.Pseudo);
            }

        }


    }
}
