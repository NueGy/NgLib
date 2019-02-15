using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Nglib.DATAPO
{

    public class NpgSqlConfiguration : DbConfiguration
    {
        public NpgSqlConfiguration()
        {
            var name = "Npgsql";

            SetProviderFactory(providerInvariantName: name,
            providerFactory: NpgsqlFactory.Instance);

            SetProviderServices(providerInvariantName: name,
            provider: NpgsqlServices.Instance);

            SetDefaultConnectionFactory(connectionFactory: new NpgsqlConnectionFactory());
        }
    }

    [TestClass]
    public class EntityFrameworkPerfTests
    {






        [TestMethod]
        public async Task PertTest1_Upd()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            int nb = 0;
            Nglib.CONNECTOR.IDataConnector connector = new Nglib.CONNECTOR.DataConnectorDefault();
            connector.SetConnectionString(Nglib.CONNECTOR.ConnectorTests.TestConnectionString, "Npgsql");
            try
            {
                connector.Open(true);
                System.Data.Common.DbConnection connect = (System.Data.Common.DbConnection)connector.GetDbConnection();

                watch.Start();
                
                TestEfContext db = new TestEfContext(connect);
                //var tlist = db.Tests.ToListAsync();
       

                List<TestClass> listtest = await db.Tests.ToListAsync<TestClass>();
                nb = listtest.Count;
                int i = 0;

                foreach (TestClass item in listtest.Take(100))
                {
                    TestClass item2 = await db.Tests.FirstAsync(g => g.testid == 6);
                    item.pseudo = "tesperf_ef_" + i.ToString();
                    //db.SaveChanges();
                    i++;
                }


                db.Tests.Add(new TestClass() { pseudo = "toti" });
                db.SaveChanges();

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
            Console.WriteLine(string.Format("elapsed:{0}ms - nb:{1}",watch.ElapsedMilliseconds,nb));
        }


        [System.ComponentModel.DataAnnotations.Schema.Table("tests", Schema ="public")]
        public class TestClass
        {
            [Key]
            public int testid { get; set; }

            [Column]
            public string pseudo { get; set; }
            //public string FirstMidName { get; set; }
            //public DateTime EnrollmentDate { get; set; }

            //public virtual ICollection<Enrollment> Enrollments { get; set; }
        }


        public class TestEfContext : DbContext
        {
            public TestEfContext(System.Data.Common.DbConnection connection) : base( connection,false )
            {
            }
            public TestEfContext(string connectorstring) : base(connectorstring)
            {
            }

            public DbSet<TestClass> Tests { get; set; }
            //public DbSet<Course> Courses { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
               // modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            }
        }


    }
}
