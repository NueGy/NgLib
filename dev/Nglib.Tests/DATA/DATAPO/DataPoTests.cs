using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.DATAPO
{
    [TestClass]
    public class DataPoTests
    {


        [TestMethod]
        public async Task DevPOTest()
        {
            var connector = Nglib.DATA.CONNECTOR.ConnectorTests.GetDefaultConenctor();
            DataPoSampleProvider dataPoSampleProvider = new DataPoSampleProvider(connector);
            DataPoSample dataPoSample = dataPoSampleProvider.GetFirstPO(6);
            var test0 = dataPoSample.GetObject("jsonb", DataAccessorOptionEnum.None);
            var test = dataPoSample.GetObject("multicols1", DataAccessorOptionEnum.None);
            Console.WriteLine(test);
            string DnForTalosAdmins = dataPoSample.FluxXml.GetString("/param/DnForTalosAdmins");

            string flux = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<param>\r\n\t<empty true=\"False\" />\r\n\t<maval>e20a0115320b41679c8500e4e651690e</maval>\r\n</param>";
            await dataPoSampleProvider.UpdatePOAsync(new DataPoSample[] { dataPoSample }, "fluxxml", flux);
            Console.WriteLine("test");
        }

        /// <summary>
        /// Test insert, select one, save, delete
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FullOnePOTest()
        {
            Assert.IsNotNull(DataAnnotationsFactory.TableAttributeType); // les annotations doivent avoir été chargées
            var connector = Nglib.DATA.CONNECTOR.ConnectorTests.GetDefaultConenctor();
            DataPoSampleProvider dataPoSampleProvider = new DataPoSampleProvider(connector);

            // Insert
            DataPoSample dataPoSample = new DataPoSample();
            dataPoSample.Pseudo = "test";
            dataPoSample.FluxXml.SetObject("/param/sut1", "valeur1", DATA.ACCESSORS.DataAccessorOptionEnum.CreateIfNotExist);
            dataPoSample.FluxXml["/param/sut2"] = "valeur2";
            await dataPoSampleProvider.InsertPOAsync(dataPoSample);
            Assert.IsTrue(dataPoSample.TestId > 0);

            // Select
            dataPoSample = dataPoSampleProvider.GetFirstPO(dataPoSample.TestId);

            // save
            dataPoSample.FluxXml["/param/sut2"] = "valeurX2";
            dataPoSample.FluxXml["/param/sut3"] = "valeur2";
            dataPoSample.Pseudo = "test2";
            await dataPoSampleProvider.SavePOAsync(dataPoSample);

            // update

            // Delete
            dataPoSample = dataPoSampleProvider.GetFirstPO(dataPoSample.TestId);
            await dataPoSampleProvider.DeletePOAsync(dataPoSample);

            Assert.IsTrue(dataPoSample.TestId > 0);
        }



        /// <summary>
        /// Test de manipulation simple des flux
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FluxPOTest()
        {
            var connector = Nglib.DATA.CONNECTOR.ConnectorTests.GetDefaultConenctor();
            DataPoSampleProvider dataPoSampleProvider = new DataPoSampleProvider(connector);
            string maval = FORMAT.StringUtilities.GenerateGuid32();

            DataPoSample dataPoSample = dataPoSampleProvider.GetFirstPO(6);
            string fluxstr = DATAVALUES.DataValuesTools.ToFlux(dataPoSample.FluxXml);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(fluxstr) && fluxstr.Length > 6);
            DATAVALUES.DataValues dvtemp = new DATAVALUES.DataValues();
            DATAVALUES.DataValuesTools.FromFlux(dvtemp, fluxstr);
            dvtemp.SetObject("maval", maval, DATA.ACCESSORS.DataAccessorOptionEnum.CreateIfNotExist);
            fluxstr = DATAVALUES.DataValuesTools.ToFlux(dvtemp);
            dataPoSample.FluxXml.Fusion(dvtemp, true);
            fluxstr = DATAVALUES.DataValuesTools.ToFlux(dataPoSample.FluxXml);
            await dataPoSampleProvider.SavePOAsync(dataPoSample);
            string mavalafter = dataPoSample.FluxXml.GetString("maval");
            Assert.AreEqual(maval, mavalafter);
            Assert.AreEqual(maval, mavalafter);

        }

        //Datapo sans clefs
        [TestMethod]
        public async Task DevPONKTest()
        {
            var connector = Nglib.DATA.CONNECTOR.ConnectorTests.GetDefaultConenctor();
            DataPoSampleProviderNK dataPoSampleProvider = new DataPoSampleProviderNK(connector);

            // Insert
            DataPoSampleNK dataPoSample = new DataPoSampleNK();
            dataPoSample.Pseudo = "test";
            dataPoSample.FluxXml.SetObject("/param/sut1", "valeur1", DATA.ACCESSORS.DataAccessorOptionEnum.CreateIfNotExist);
            dataPoSample.FluxXml["/param/sut2"] = "valeur2";
            await dataPoSampleProvider.InsertPOAsync(dataPoSample);

            Console.WriteLine("ok");

        }



        [TestMethod]
        public async Task MultiPOTest()
        {
            var connector = Nglib.DATA.CONNECTOR.ConnectorTests.GetDefaultConenctor();
            DataPoSampleProvider dataPoSampleProvider = new DataPoSampleProvider(connector);

            CollectionPO<DataPoSample> samples = new CollectionPO<DataPoSample>();
            for (int i = 0; i < 50; i++)
                samples.Add(new DataPoSample() { Pseudo = "manysamples"+i.ToString() });

            await dataPoSampleProvider.InsertPOAsync(samples.ToArray());

            foreach (var item in samples)
            {
                item.Pseudo = "Oi" + item.Pseudo;
            }

            await dataPoSampleProvider.SavePOAsync(samples.ToArray());

    
            Console.WriteLine(samples.Count);
        }




    }
}
