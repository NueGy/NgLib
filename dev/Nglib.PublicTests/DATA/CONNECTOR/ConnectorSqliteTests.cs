using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR.TESTS
{
    /// <summary>
    /// Tests condensés des connecteurs de base de données avec SQLite
    /// Teste CRUD, transactions et cache de réflexion dans 3 méthodes unifiées
    /// </summary>
    [TestClass]
    public class ConnectorSqliteTests
    {
        // Constantes de test
        private const string TEST_USER_NAME = "John Doe";
        private const string TEST_USER_EMAIL = "john@test.com";
        private const int TEST_USER_AGE = 30;

        /// <summary>
        /// Initialise la connexion et la base de test avec cleanup automatique
        /// </summary>
        private async Task<(IDataConnector connector, string tempDbPath)> InitializeConnectorAsync()
        {
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"nglib_test_{Guid.NewGuid()}.db");
            string connectionString = $"Data Source={tempDbPath};Version=3;";

            var connector = new ConnectorGeneric();
            connector.SetConnectionString(connectionString, "SQLITE");
            connector.Open();

            // Créer table de test
            await connector.QueryAsync("CREATE TABLE IF NOT EXISTS TestUsers (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Email TEXT, Age INTEGER, IsActive INTEGER DEFAULT 1, CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP)");
            
            return (connector, tempDbPath);
        }

        /// <summary>
        /// Test complet CRUD : INSERT, SELECT, UPDATE, DELETE
        /// Utilise les extensions IDataConnectorExtends pour Query/QueryAsync
        /// Valide aussi le cache de réflexion via multiples appels factory
        /// </summary>   
        [TestMethod]
        public async Task CrudOperationsFullyTest()
        {
            var (connector, tempDbPath) = await InitializeConnectorAsync();
            try
            {
                // CREATE - Insertion combinée avec récupération d'ID (SQLite autocommit)
                var dtInsert = await connector.QueryAsync($"INSERT INTO TestUsers (Name, Email, Age) VALUES ('{TEST_USER_NAME}', '{TEST_USER_EMAIL}', {TEST_USER_AGE}); SELECT last_insert_rowid() as Id");
                Assert.IsNotNull(dtInsert, "DataTable null après INSERT");
                Assert.AreEqual(1, dtInsert.Rows.Count, "Aucune ligne retournée après INSERT");
                long userId = Convert.ToInt64(dtInsert.Rows[0]["Id"]);

                // READ - Lecture avec extension Query (teste le cache de réflexion)
                var dtUser = await connector.QueryAsync($"SELECT * FROM TestUsers WHERE Id = {userId}");
                Assert.IsNotNull(dtUser);
                Assert.AreEqual(1, dtUser.Rows.Count);
                Assert.AreEqual(TEST_USER_NAME, dtUser.Rows[0]["Name"].ToString());
                Assert.AreEqual(TEST_USER_EMAIL, dtUser.Rows[0]["Email"].ToString());
                Assert.AreEqual(TEST_USER_AGE, Convert.ToInt32(dtUser.Rows[0]["Age"]));

                // UPDATE - Modification
                const string updatedEmail = "john.updated@test.com";
                const int updatedAge = 31;
                await connector.QueryAsync($"UPDATE TestUsers SET Email = '{updatedEmail}', Age = {updatedAge} WHERE Id = {userId}");

                var dtUpdated = await connector.QueryAsync($"SELECT Email, Age FROM TestUsers WHERE Id = {userId}");
                Assert.AreEqual(updatedEmail, dtUpdated.Rows[0]["Email"].ToString());
                Assert.AreEqual(updatedAge, Convert.ToInt32(dtUpdated.Rows[0]["Age"]));

                // DELETE - Suppression et vérification
                await connector.QueryAsync($"DELETE FROM TestUsers WHERE Id = {userId}");
                Assert.AreEqual(0, await CountUsersAsync(connector, $"Id = {userId}"), "L'utilisateur devrait être supprimé");
            }
            finally
            {
                connector?.Close();
                connector?.Dispose();
                if (File.Exists(tempDbPath))
                    File.Delete(tempDbPath);
            }
        }
        
        /// <summary>
        /// TEST SIMPLE pour diagnostic
        /// </summary>
        [TestMethod]
        public void SimpleConnectionTest()
        {
            string testDb = Path.Combine(Path.GetTempPath(), $"nglib_simple_{Guid.NewGuid()}.db");
            try
            {
                var conn = new ConnectorGeneric();
                conn.SetConnectionString($"Data Source={testDb};Version=3;", "SQLITE");
                conn.Open();
                
                // Test simple sans extension
                var qctx = new QueryContext("SELECT 1 as TestValue", null);
                var ds = conn.QueryDataSetAsync(qctx).GetAwaiter().GetResult();
                
                Assert.IsNotNull(ds);
                Assert.AreEqual(1, ds.Tables.Count);
                
                conn.Close();
                conn.Dispose();
            }
            finally
            {
                if (File.Exists(testDb))
                    File.Delete(testDb);
            }
        }

        /// <summary>
        /// Test transactions : COMMIT et ROLLBACK avec isolation
        /// Utilise les extensions IDataConnectorExtends
        /// </summary>
        [TestMethod]
        public async Task TransactionsFullyTest()
        {
            var (connector, tempDbPath) = await InitializeConnectorAsync();
            try
            {
                // Test COMMIT - Les données doivent persister
                connector.BeginTransaction();
                try
                {
                    await connector.QueryAsync("INSERT INTO TestUsers (Name, Email, Age) VALUES ('Alice', 'alice@test.com', 25)");
                    await connector.QueryAsync("INSERT INTO TestUsers (Name, Email, Age) VALUES ('Bob', 'bob@test.com', 28)");
                    connector.CommitTransaction();

                    Assert.AreEqual(2, await CountUsersAsync(connector, "Name IN ('Alice', 'Bob')"), "Les 2 utilisateurs devraient être persistés après COMMIT");
                }
                catch
                {
                    connector.RollBackTransaction();
                    throw;
                }

                // Test ROLLBACK - Les données ne doivent PAS persister
                connector.BeginTransaction();
                try
                {
                    await connector.QueryAsync("INSERT INTO TestUsers (Name, Email) VALUES ('Charlie', 'charlie@test.com')");

                    // Visible dans la transaction
                    Assert.AreEqual(1, await CountUsersAsync(connector, "Name = 'Charlie'"), "Charlie devrait être visible dans la transaction");

                    connector.RollBackTransaction();

                    // Non visible après rollback
                    Assert.AreEqual(0, await CountUsersAsync(connector, "Name = 'Charlie'"), "Charlie ne devrait PAS être persisté après ROLLBACK");
                }
                catch
                {
                    connector.RollBackTransaction();
                    throw;
                }
            }
            finally
            {
                connector?.Close();
                connector?.Dispose();
                if (File.Exists(tempDbPath))
                    File.Delete(tempDbPath);
            }
        }

        /// <summary>
        /// Test gestion d'erreurs : exceptions SQL et tables/colonnes inexistantes
        /// Utilise Assert.ThrowsException pour validation des exceptions
        /// </summary>
        [TestMethod]
        public async Task ErrorHandlingFullyTest()
        {
            var (connector, tempDbPath) = await InitializeConnectorAsync();
            try
            {
                // Test table inexistante
                await Assert.ThrowsExceptionAsync<ConnectorException>(async () =>
                    await connector.QueryAsync("SELECT * FROM TableInexistante"),
                    "Une exception devrait être levée pour table inexistante");

                // Test colonne invalide
                await Assert.ThrowsExceptionAsync<ConnectorException>(async () =>
                    await connector.QueryAsync("INSERT INTO TestUsers (InvalidColumn) VALUES ('test')"),
                    "Une exception devrait être levée pour colonne invalide");
            }
            finally
            {
                connector?.Close();
                connector?.Dispose();
                if (File.Exists(tempDbPath))
                    File.Delete(tempDbPath);
            }
        }


        /// <summary>
        /// Compte le nombre d'utilisateurs selon un critère WHERE
        /// </summary>
        private async Task<int> CountUsersAsync(IDataConnector connector, string whereClause)
        {
            var dt = await connector.QueryAsync($"SELECT COUNT(*) as Total FROM TestUsers WHERE {whereClause}");
            return Convert.ToInt32(dt.Rows[0]["Total"]);
        }

        /// <summary>
        /// Test condensé QueryBuilder : CreateQueryBuilder + Extensions IDataConnectorExtends
        /// Teste : Query, QueryAsync, QueryScalar, QueryScalarAsync avec IQueryBuilder
        /// </summary>
        [TestMethod]
        public async Task QueryBuilderExtensionsTest()
        {
            var (connector, tempDbPath) = await InitializeConnectorAsync();
            try
            {
                // CreateQueryBuilder doit retourner un builder SQLite
                var qb = connector.CreateQueryBuilder();
                Assert.IsNotNull(qb, "CreateQueryBuilder doit retourner un builder pour SQLite");

                // INSERT classique pour préparer les données
                await connector.QueryAsync("INSERT INTO TestUsers (Name, Email, Age) VALUES ('QueryTest', 'qb@test.com', 40)");
                
                // SELECT via QueryBuilder + extension QueryAsync
                var dt = await connector.QueryAsync(connector.CreateQueryBuilder().From("TestUsers").Select("*").Where("Name", "=", "QueryTest"));
                Assert.AreEqual(1, dt.Rows.Count, "SELECT avec QueryBuilder doit retourner 1 ligne");
                Assert.AreEqual("QueryTest", dt.Rows[0]["Name"].ToString());

                // COUNT via QueryBuilder + QueryScalarAsync
                var count = await connector.QueryScalarAsync(connector.CreateQueryBuilder().From("TestUsers").Select("COUNT(*)").Where("Age", ">=", 40));
                Assert.AreEqual(1L, Convert.ToInt64(count), "COUNT avec QueryBuilder doit retourner 1");

                // QueryScalar synchrone + Query synchrone
                var name = connector.QueryScalar(connector.CreateQueryBuilder().From("TestUsers").Select("Name").Where("Email", "=", "qb@test.com"));
                Assert.AreEqual("QueryTest", name?.ToString(), "QueryScalar doit retourner le nom");
                
                var dtSync = connector.Query(connector.CreateQueryBuilder().From("TestUsers").Select("Age").Where("Name", "=", "QueryTest"));
                Assert.AreEqual(40, Convert.ToInt32(dtSync.Rows[0]["Age"]), "Query synchrone doit retourner l'âge");
            }
            finally
            {
                connector?.Close();
                connector?.Dispose();
                if (File.Exists(tempDbPath))
                    File.Delete(tempDbPath);
            }
        }

    }
}