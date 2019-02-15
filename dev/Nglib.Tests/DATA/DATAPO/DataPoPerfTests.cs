using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nglib.DATA.DATAPO
{

    /// <summary>
    /// Tests de performances des DataPo
    /// </summary>
    [TestClass]
    public class DataPoPerfTests
    {





        [TestMethod]
        public async Task PertTest1_Upd()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            int nb = 0;
            Nglib.DATA.CONNECTOR.IDataConnector connector = new Nglib.DATA.CONNECTOR.ConnectorGeneric();
            connector.SetConnectionString(Nglib.DATA.CONNECTOR.ConnectorTests.TestConnectionString, "Npgsql");
            try
            {
                connector.Open(true);
                System.Data.Common.DbConnection connect = (System.Data.Common.DbConnection)connector.GetDbConnection();

                watch.Start();

                DataPoSampleProvider provider = new DataPoSampleProvider(connector);


                CollectionPO<DataPoSample> listtest =  provider.GetListPO("SELECT * FROM tests");
                nb = listtest.Count;
                int i = 0;

                foreach (DataPoSample item in listtest.Take(100))
                {
                    //DataPoSample item2 = provider.GetFirst(6);
                    item.Pseudo = "tesperf_po_" + i.ToString();
                    provider.SavePO(item);
                    i++;
                }


                await provider.InsertPOAsync(new DataPoSample() { Pseudo = "totu" });

                watch.Stop();


            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                connector.Close();
            }
            Console.WriteLine(string.Format("elapsed:{0}ms - nb:{1}", watch.ElapsedMilliseconds, nb));
        }

         [TestMethod]
        public async Task PertTest1_pre()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            int nb = 0;
            Nglib.DATA.CONNECTOR.IDataConnector connector = new Nglib.DATA.CONNECTOR.ConnectorGeneric();
            connector.SetConnectionString(Nglib.DATA.CONNECTOR.ConnectorTests.TestConnectionString, "Npgsql");
            connector.QueryCompleted += Connector_QueryCompleted;

            try
            {
                connector.Open(true);
                System.Data.Common.DbConnection connect = (System.Data.Common.DbConnection)connector.GetDbConnection();

                watch.Start();

                DataPoSampleProvider provider = new DataPoSampleProvider(connector);

                Console.WriteLine(string.Format("elapsed:{0}ms - nb:{1}  executes={2}ms -{3}ms", watch.ElapsedMilliseconds, nb, totalelapsedExecute, totalelapsedConnector));
                CollectionPO<DataPoSample> listtest =  provider.GetListPO("SELECT * FROM tests");
                Console.WriteLine(string.Format("elapsed:{0}ms - nb:{1}  executes={2}ms -{3}ms", watch.ElapsedMilliseconds, nb, totalelapsedExecute, totalelapsedConnector));
                nb = listtest.Count;
   
                for (int i = 0; i < 100; i++)
                {
                    DataPoSample item = provider.GetFirstPO(6);
                    item.Pseudo = "tesperf_po_" + i.ToString();
                    provider.SavePO(item);
                    i++;
                }
                Console.WriteLine(string.Format("elapsed:{0}ms - nb:{1}  executes={2}ms -{3}ms", watch.ElapsedMilliseconds, nb, totalelapsedExecute, totalelapsedConnector));

                await provider.InsertPOAsync(new DataPoSample() { Pseudo = "totu" });

                watch.Stop();


            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                connector.Close();
            }
            Console.WriteLine(string.Format("elapsed:{0}ms - nb:{1}  executes={2}ms -{3}ms", watch.ElapsedMilliseconds, nb, totalelapsedExecute, totalelapsedConnector));
        }

        private static long totalelapsedExecute = 0;
        private static long totalelapsedConnector = 0;
        private void Connector_QueryCompleted(DATA.CONNECTOR.QueryContext queryContext)
        {
            totalelapsedExecute += queryContext.watchExecute.ElapsedMilliseconds;
            totalelapsedConnector += queryContext.watchAll.ElapsedMilliseconds;
        }
    }
}
