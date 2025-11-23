using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.CONNECTOR;
using Nglib.DATA.DATAPO;
using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO.TESTS
{
    /// <summary>
    /// Tests unitaires complets pour DataPOProviderSQL
    /// Inclut tests CRUD, transactions et fonctionnalités avancées
    /// Utilise SQLite et DataPOExample pour des tests rapides et isolés
    /// </summary>
    [TestClass]
    public class DataPOProviderTests
    {
        private static string _tempDbPath;

        #region === Utilitaires de test ===

        /// <summary>
        /// Initialise un provider de test avec SQLite
        /// </summary>
        private static async Task<DataPOProviderSQL<DataPOExample>> InitializeProviderAsync()
        {
            // Créer une base de données temporaire SQLite
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_datapo_{Guid.NewGuid()}.db");
            string connectionString = $"Data Source={_tempDbPath};Version=3;";

            var connector = new ConnectorGeneric();
            connector.SetConnectionString(connectionString, "SQLITE");
            connector.Open();

            // Créer la table de test avec support multi-tenant
            await connector.QueryAsync(@"
                CREATE TABLE IF NOT EXISTS demotable (
                    monid INTEGER PRIMARY KEY AUTOINCREMENT,
                    mavaleur TEXT,
                    fluxjson TEXT,
                    tenantid INTEGER DEFAULT 1
                )");

            return new DataPOProviderSQL<DataPOExample>(connector);
        }

        /// <summary>
        /// Nettoie les ressources de test (ferme connexion et supprime DB temporaire)
        /// </summary>
        private static void CleanupTest()
        {
            if (!string.IsNullOrEmpty(_tempDbPath) && File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        #endregion



        /// <summary>
        /// Test complet des opérations CRUD et batch
        /// </summary>
        [TestMethod]
        public async Task CrudAndBatchOperationsFullyTest()
        {
            var provider = await InitializeProviderAsync();
            try
            {
                // === INSERT simple avec auto-increment ===
                var item1 = new DataPOExample { MaValeur = "Item 1" };
                await provider.InsertPOAsync(item1);
                Assert.IsTrue(item1.MonId > 0, $"L'ID doit être auto-incrémenté (ID={item1.MonId})");

                // === READ - GetPOAsync ===
                var itemById = await provider.GetPOAsync(item1.MonId);
                Assert.IsNotNull(itemById, "GetPOAsync doit retourner l'item");
                Assert.AreEqual("Item 1", itemById.MaValeur);

                // === QUERY avec QueryContext et paramètres ===
                var queryParams = new Dictionary<string, object> { ["mavaleur"] = "Item 1" };
                var queryContext = new QueryContext("SELECT * FROM demotable WHERE mavaleur = @mavaleur", queryParams);
                var queryResult = await provider.QueryPOAsync(queryContext);
                Assert.AreEqual(1, queryResult.Count, "La requête doit retourner 1 résultat");
                Assert.AreEqual(item1.MonId, queryResult[0].MonId);
                Assert.IsFalse(itemById.IsChanges(), "L'objet doit être marqué comme NON modifié");

                // === UPDATE via SavePOAsync (uniquement si modifié) ===

                itemById.MaValeur = "Item 1 Updated";
                Assert.IsTrue(itemById.IsChanges(), "L'objet doit être marqué comme modifié");
                var saved = await provider.SavePOAsync(itemById);
                Assert.IsTrue(saved, "SavePOAsync doit retourner true");

                var itemVerified = await provider.GetPOAsync(item1.MonId);
                Assert.AreEqual("Item 1 Updated", itemVerified.MaValeur);

                // === BATCH INSERT - Insertion multiple ===
                var batchItems = new[]
                {
                    new DataPOExample { MaValeur = "Item 2" },
                    new DataPOExample { MaValeur = "Item 3" },
                    new DataPOExample { MaValeur = "Item 4" }
                };

                await provider.InsertPOAsync(batchItems);

                // Vérifier que tous les IDs sont affectés
                Assert.IsTrue(batchItems.All(u => u.MonId > 0), "Tous les IDs doivent être affectés");
                var distinctIds = batchItems.Select(u => u.MonId).Distinct().Count();
                Assert.AreEqual(3, distinctIds, $"Les IDs doivent être uniques (distinct={distinctIds})");

                // === BATCH UPDATE - Mise à jour avec valeurs communes ===
                var allItemsQuery = new QueryContext("SELECT * FROM demotable", null);
                var allItems = await provider.QueryPOAsync(allItemsQuery);
                Assert.AreEqual(4, allItems.Count, "Il doit y avoir 4 items au total");

                var updates = new Dictionary<string, object> { ["mavaleur"] = "Updated Value" };
                await provider.UpdatePOAsync(allItems.ToArray(), updates);

                // Vérifier que tous sont mis à jour
                var updatedQuery = new QueryContext("SELECT * FROM demotable WHERE mavaleur = 'Updated Value'", null);
                var updatedItems = await provider.QueryPOAsync(updatedQuery);
                Assert.AreEqual(4, updatedItems.Count, "Tous les items doivent être mis à jour");

                // === DELETE - Suppression simple ===
                int idToDelete = itemById.MonId; // Sauvegarder l'ID avant delete
                await provider.DeletePOAsync(itemById);
                var deletedItem = await provider.GetPOAsync(idToDelete);
                Assert.IsNull(deletedItem, "L'item supprimé ne doit plus exister");

                // === BATCH DELETE - Suppression multiple ===
                await provider.DeletePOAsync(batchItems);
                var remainingQuery = new QueryContext("SELECT * FROM demotable", null);
                var remainingItems = await provider.QueryPOAsync(remainingQuery);
                Assert.AreEqual(0, remainingItems.Count, "Tous les items doivent être supprimés");
            }
            finally
            {
                provider?.Connector?.Dispose();
                CleanupTest();
            }
        }

        /// <summary>
        /// Test des méthodes de recherche avec paramètres
        /// </summary>
        [TestMethod]
        public async Task SearchAndQueryMethodsTest()
        {
            var provider = await InitializeProviderAsync();
            try
            {
                // Préparer données de test
                var items = new[]
                {
                    new DataPOExample { MaValeur = "Item A" },
                    new DataPOExample { MaValeur = "Item B" },
                    new DataPOExample { MaValeur = "Item C" }
                };
                await provider.InsertPOAsync(items);

                // === Test GetPOAsync avec dictionnaire de paramètres ===
                var itemAId = items[0].MonId;
                var keys = new Dictionary<string, object> { ["monid"] = itemAId };
                var itemByParams = await provider.GetPOAsync(keys);
                Assert.IsNotNull(itemByParams, "GetPOAsync avec paramètres doit retourner l'item");
                Assert.AreEqual("Item A", itemByParams.MaValeur);

                // === Test QueryPOAsync avec limite ===
                var limited = await provider.QueryPOAsync(limitResult: 2);
                Assert.AreEqual(2, limited.Count, "QueryPOAsync avec limite doit retourner 2 résultats");

                // === Test QueryPOAsync avec SqlParams ===
                var sqlParams = new Dictionary<string, object> { ["mavaleur"] = "Item B" };
                var filtered = await provider.QueryPOAsync(limitResult: 1000, SqlParams: sqlParams);
                Assert.AreEqual(1, filtered.Count, "QueryPOAsync avec SqlParams doit filtrer correctement");
                Assert.AreEqual("Item B", filtered[0].MaValeur);

                // === Test GetPOAsync avec ID inexistant ===
                var nonExistent = await provider.GetPOAsync(99999);
                Assert.IsNull(nonExistent, "GetPOAsync avec ID inexistant doit retourner null");

                // === Test collection métadonnées ===
                var allItems = await provider.QueryPOAsync(new QueryContext("SELECT * FROM demotable", null));
                Assert.IsTrue(allItems.ExecuteTimeElapsed >= 0, "ExecuteTimeElapsed doit être défini");
                Assert.AreEqual(3, allItems.Count, $"Count doit être 3 (obtenu: {allItems.Count})");
            }
            finally
            {
                provider?.Connector?.Dispose();
                CleanupTest();
            }
        }



        /// <summary>
        /// Test des transactions (COMMIT et ROLLBACK)
        /// </summary>
        [TestMethod]
        public async Task TransactionsFullyTest()
        {
            var provider = await InitializeProviderAsync();
            var connector = provider.Connector;

            try
            {
                // === Test COMMIT - Les données doivent persister ===
                connector.BeginTransaction();
                try
                {
                    var item1 = new DataPOExample { MaValeur = "Transactional 1" };
                    var item2 = new DataPOExample { MaValeur = "Transactional 2" };

                    await provider.InsertPOAsync(item1);
                    await provider.InsertPOAsync(item2);

                    connector.CommitTransaction();
                }
                catch
                {
                    connector.RollBackTransaction();
                    throw;
                }

                // Vérifier la persistance (hors transaction)
                var itemsQuery = new QueryContext("SELECT * FROM demotable", null);
                var items = await provider.QueryPOAsync(itemsQuery);
                Assert.AreEqual(2, items.Count, "Les 2 items doivent être persistés après COMMIT");

                // === Test ROLLBACK - Les données ne doivent PAS persister ===
                connector.BeginTransaction();
                var item3 = new DataPOExample { MaValeur = "Transactional 3" };

                try
                {
                    await provider.InsertPOAsync(item3);

                    // Visible dans la transaction
                    var inTxQuery = new QueryContext("SELECT * FROM demotable WHERE mavaleur = 'Transactional 3'", null);
                    var inTransaction = await provider.QueryPOAsync(inTxQuery);
                    Assert.AreEqual(1, inTransaction.Count, "Item 3 doit être visible dans la transaction");

                    connector.RollBackTransaction();
                }
                catch
                {
                    connector.RollBackTransaction();
                    throw;
                }

                // Non visible après rollback (hors transaction)
                var afterRbQuery = new QueryContext("SELECT * FROM demotable WHERE mavaleur = 'Transactional 3'", null);
                var afterRollback = await provider.QueryPOAsync(afterRbQuery);
                Assert.AreEqual(0, afterRollback.Count, "Item 3 ne doit PAS être persisté après ROLLBACK");
            }
            finally
            {
                provider?.Connector?.Dispose();
                CleanupTest();
            }
        }




        /// <summary>
        /// Test du change tracking (IsChanges, GetChangedValues, AcceptChanges) et modifications des flux
        /// </summary>
        [TestMethod]
        public async Task ChangeTrackingFullyTest()
        {
            var provider = await InitializeProviderAsync();
            try
            {
                // Insérer un item de test avec un flux
                var item = new DataPOExample
                {
                    MaValeur = "Test Item"
                };
                item.Flux["key1"] = "value1";
                item.Flux["key2"] = 123;
                await provider.InsertPOAsync(item);

                // Récupérer l'item
                var itemFromDb = await provider.GetPOAsync(item.MonId);
                Assert.IsNotNull(itemFromDb);

                // === Test 1: Pas de changement initialement ===
                Assert.IsFalse(itemFromDb.IsChanges(), "IsChanges() doit retourner false avant modification");

                // === Test 2: Modification d'une propriété simple ===
                itemFromDb.MaValeur = "Modified Value";
                Assert.IsTrue(itemFromDb.IsChanges(), "IsChanges() doit retourner true après modification");

                // GetChangedValues
                var changes = itemFromDb.GetChangedValues();
                Assert.IsTrue(changes.ContainsKey("mavaleur"), "GetChangedValues doit contenir 'mavaleur'");
                Assert.AreEqual("Modified Value", changes["mavaleur"]);

                // AcceptChanges reset les changements
                itemFromDb.AcceptChanges();
                Assert.IsFalse(itemFromDb.IsChanges(), "IsChanges() doit retourner false après AcceptChanges");

                // === Test 3: Modification du flux ===
                itemFromDb.Flux["key1"] = "modified_value1";
                itemFromDb.Flux["key3"] = "new_value";
                Assert.IsTrue(itemFromDb.IsChanges(), "IsChanges() doit retourner true après modification du flux");

                var fluxChanges = itemFromDb.GetChangedValues();
                Assert.IsTrue(fluxChanges.ContainsKey("fluxjson"), "GetChangedValues doit contenir 'fluxjson'");

                // Sauvegarder les changements
                await provider.SavePOAsync(itemFromDb);

                // Vérifier la persistance des changements de flux
                var itemVerified = await provider.GetPOAsync(item.MonId);
                Assert.IsNotNull(itemVerified.Flux, "Le flux ne doit pas être null");
                Assert.AreEqual("modified_value1", itemVerified.Flux.GetString("key1"), "Le flux doit contenir la valeur modifiée");
                Assert.AreEqual("new_value", itemVerified.Flux.GetString("key3"), "Le flux doit contenir la nouvelle clé");
                Assert.AreEqual(123, itemVerified.Flux.GetInt("key2"), "Le flux doit conserver les valeurs non modifiées");

                // === Test 4: Vider complètement le flux ===
                itemVerified.Flux.Clear();
                itemVerified.Flux["newkey"] = "newvalue";
                Assert.IsTrue(itemVerified.IsChanges(), "IsChanges() doit retourner true après clear+ajout dans le flux");

                await provider.SavePOAsync(itemVerified);

                var itemFinal = await provider.GetPOAsync(item.MonId);
                var finalKeys = itemFinal.Flux.ListFieldsKeys();
                Assert.AreEqual(1, finalKeys.Length, "Le flux doit contenir 1 seule clé après clear+ajout");
                Assert.AreEqual("newvalue", itemFinal.Flux.GetString("newkey"), "La nouvelle clé doit exister");
            }
            finally
            {
                provider?.Connector?.Dispose();
                CleanupTest();
            }
        }




        /// <summary>
        /// Test des nouvelles méthodes avec TenantId (legacy)
        /// </summary>
        [TestMethod]
        public async Task TenantFilteringLegacyTest()
        {
            var provider = await InitializeProviderAsync();
            try
            {
                // Créer des données pour différents tenants
                var item1 = new DataPOExample { MaValeur = "Tenant1 Item" };
                item1["tenantid"] = 1;
                await provider.InsertPOAsync(item1);

                var item2 = new DataPOExample { MaValeur = "Tenant2 Item" };
                item2["tenantid"] = 2;
                await provider.InsertPOAsync(item2);

                // === Test GetPOAsync avec filtrage tenant ===
                var itemWithTenant = await provider.GetPOAsync(item1.MonId, TenantId: 1);
                Assert.IsNotNull(itemWithTenant, "GetPOAsync avec bon tenant doit retourner l'item");
                Assert.AreEqual("Tenant1 Item", itemWithTenant.MaValeur);

                // Test avec mauvais tenant
                var itemWrongTenant = await provider.GetPOAsync(item1.MonId, TenantId: 2);
                Assert.IsNull(itemWrongTenant, "GetPOAsync avec mauvais tenant doit retourner null");

                // Test sans filtrage tenant
                var itemNoFilter = await provider.GetPOAsync(item1.MonId, TenantId: 0);
                Assert.IsNotNull(itemNoFilter, "GetPOAsync sans filtrage tenant doit retourner l'item");
            }
            finally
            {
                provider?.Connector?.Dispose();
                CleanupTest();
            }
        }

        /// <summary>
        /// Test des performances et limites
        /// </summary>
        [TestMethod]
        public async Task PerformanceAndLimitsTest()
        {
            var provider = await InitializeProviderAsync();
            try
            {
                // === Test insertion batch importante ===
                var batchSize = 100;
                var largeBatch = new DataPOExample[batchSize];
                for (int i = 0; i < batchSize; i++)
                {
                    largeBatch[i] = new DataPOExample { MaValeur = $"Batch Item {i}" };
                }

                var start = DateTime.Now;
                await provider.InsertPOAsync(largeBatch);
                var elapsed = DateTime.Now - start;

                Assert.IsTrue(largeBatch.All(item => item.MonId > 0), "Tous les items du batch doivent avoir un ID");
                Assert.IsTrue(elapsed.TotalSeconds < 10, $"L'insertion de {batchSize} items doit prendre moins de 10 secondes (actuel: {elapsed.TotalSeconds}s)");

                // === Test requête avec limite ===
                var limitedResults = await provider.QueryPOAsync(limitResult: 50);
                Assert.AreEqual(50, limitedResults.Count, "La limite doit être respectée");
                Assert.IsTrue(limitedResults.ExecuteTimeElapsed >= 0, "Le temps d'exécution doit être mesuré");

                // === Test récupération complète ===
                var allResults = await provider.QueryPOAsync(limitResult: 1000);
                Assert.AreEqual(batchSize, allResults.Count, $"Tous les {batchSize} items doivent être récupérés");
            }
            finally
            {
                provider?.Connector?.Dispose();
                CleanupTest();
            }
        }
         
    }
}