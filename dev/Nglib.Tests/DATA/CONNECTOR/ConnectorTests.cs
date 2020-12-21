using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nglib.DATA.CONNECTOR
{
    [TestClass]
    public class ConnectorTests
    {
        public static string TestConnectionString = "";


        public static Nglib.DATA.CONNECTOR.IDataConnector GetDefaultConenctor()
        {
            Nglib.DATA.CONNECTOR.IDataConnector connector = new Nglib.DATA.CONNECTOR.ConnectorGeneric();
            connector.SetConnectionString(TestConnectionString, "Npgsql");
            return connector;
        }




        [TestMethod]
        public void InsertSerieBulkTest()
        {
            int max = 500000;
            Nglib.DATA.CONNECTOR.IDataConnector connector = GetDefaultConenctor();

            System.Data.DataTable table = new System.Data.DataTable("crawler_websites");
            table.Columns.Add(new System.Data.DataColumn("websitename", typeof(string)));
            table.Columns.Add(new System.Data.DataColumn("domainname", typeof(string)));
            table.Columns.Add(new System.Data.DataColumn("ixdomain", typeof(string)));
            table.Columns.Add(new System.Data.DataColumn("ixdomain1", typeof(string)));
            table.Columns.Add(new System.Data.DataColumn("scanneed", typeof(int)));


            string f = "x";

            for (int i = 0; i < max; i++)
            {
                System.Data.DataRow row = table.NewRow();
                //(websitename,domainname,ixdomain,ixdomain1,scanneed,ipcountrycode,urlmaster)
                string aleweb = Nglib.FORMAT.StringTools.GenerateString(7, "azertyuiopqsdfghjklmwxcvbn");
                row["websitename"]= string.Format("http://{0}{1}.{2}.com", f, i.ToString(), aleweb);
                row["domainname"] = string.Format("{0}.com", aleweb);
                row["ixdomain"] = aleweb.Substring(0, 3).ToUpper();
                row["ixdomain1"] = aleweb.Substring(0, 1).ToUpper();
                row["scanneed"]= 0;
                table.Rows.Add(row);


            }

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            connector.InsertTableAsync(table).GetAwaiter().GetResult();
            watch.Stop();
            Console.WriteLine(string.Format("{0}ms", watch.ElapsedMilliseconds));

        }




        [TestMethod]
        public void InsertSerieTest()
        {
            int max = 500;
            Nglib.DATA.CONNECTOR.IDataConnector connector = GetDefaultConenctor();
            connector.Open(true);
            //connector.BeginTransaction();
    
            for (int i = 0; i < max; i++)
            {
                Dictionary<string, object> ins = new Dictionary<string, object>();
                //(websitename,domainname,ixdomain,ixdomain1,scanneed,ipcountrycode,urlmaster)
                string aleweb = Nglib.FORMAT.StringTools.GenerateString(7, "azertyuiopqsdfghjklmwxcvbn");
                ins.Add("websitename", string.Format("http://{0}.com", aleweb));
                ins.Add("domainname", string.Format("{0}.com", aleweb));
                ins.Add("ixdomain", aleweb.Substring(0, 3).ToUpper());
                ins.Add("ixdomain1", aleweb.Substring(0, 1).ToUpper());
                ins.Add("scanneed", 0);

                connector.InsertAsync("crawler_websites", ins, null).GetAwaiter().GetResult();


            }
            //connector.CommitTransaction();
            connector.Close(true);

        }













        [TestMethod]
        public void FactoryTest()
        {
            //var ival = Nglib.DATA.CONNECTOR.ConnectorTools.ConnectionFactory("npgsql");
            //Console.WriteLine(string.Format("{0}", ival));


            //var ival2 = Nglib.DATA.CONNECTOR.ConnectorTools.DataAdapterFactory("ngpsql");
            //Console.WriteLine(string.Format("{0}", ival));
        }


        [TestMethod]
        public void TestMethod1()
        {

            Nglib.DATA.CONNECTOR.IDataConnector connector = new Nglib.DATA.CONNECTOR.ConnectorGeneric();
            connector.SetConnectionString(TestConnectionString, "Npgsql");
            connector.QueryCompleted += Connector_QueryCompleted;
            System.Data.DataTable ret = connector.QueryAsync("SELECT * FROM web_websites WHERE domainname=@p1;", "nuegy.net").GetAwaiter().GetResult();
            connector.Query("SELECT * FROM web_websites;");
            connector.QueryScalar("SELECT * FROM web_websites;");
            connector.Query("SELECT * FROM web_websites;");
        }

        private void Connector_QueryCompleted(QueryContext queryContext)
        {
            Console.WriteLine(queryContext.ToString());
        }
    }




}
