//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Nglib.DATA.CONNECTOR;
//using Nglib.DATA.DATAPO;
//using Nglib.DATA.ACCESSORS;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Nglib.DATA.DATAPO.TESTS
//{
//    /// <summary>
//    /// Tests unitaires pour DataPOProviderSQL
//    /// Utilise SQLite et DataPOExample pour des tests rapides et isolés
//    /// </summary>
//    [TestClass]
//    public class DataPOProviderOldTests
//    {
//        private static string _tempDbPath;

//        /// <summary>
//        /// Initialise un provider de test avec SQLite
//        /// </summary>
//        private static async Task<DataPOProviderSQL<DataPOExample>> InitializeProviderAsync()
//        {
//            // Créer une base de données temporaire SQLite
//            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_datapo_{Guid.NewGuid()}.db");
//            string connectionString = $"Data Source={_tempDbPath};Version=3;";

//            var connector = new ConnectorGeneric();
//            connector.SetConnectionString(connectionString, "SQLITE");
//            connector.Open();

//            // Créer la table de test
//            await connector.QueryAsync(@"
//                CREATE TABLE IF NOT EXISTS demotable (
//                    monid INTEGER PRIMARY KEY AUTOINCREMENT,
//                    mavaleur TEXT,
//                    fluxjson TEXT
//                )");

//            return new DataPOProviderSQL<DataPOExample>(connector);
//        }

//        /// <summary>
//        /// Nettoie les ressources de test (ferme connexion et supprime DB temporaire)
//        /// </summary>
//        private static void CleanupTest()
//        {
//            if (!string.IsNullOrEmpty(_tempDbPath) && File.Exists(_tempDbPath))
//                File.Delete(_tempDbPath);
//        }

//        /// <summary>
//        /// Test complet des opérations CRUD et batch
//        /// </summary>
//        [TestMethod]
//        public async Task CrudAndBatchOperationsFullyTest()
//        {
//            var provider = await InitializeProviderAsync();

//            // === INSERT simple avec auto-increment ===
//            var item1 = new DataPOExample { MaValeur = "Item 1" };
//            await provider.InsertPOAsync(item1);
//            Assert.IsTrue(item1.MonId > 0, $"L'ID doit être auto-incrémenté (ID={item1.MonId})");

//            // === READ - GetPOAsync ===
//            var itemById = await provider.GetPOAsync(item1.MonId);
//            Assert.IsNotNull(itemById, "GetPOAsync doit retourner l'item");
//            Assert.AreEqual("Item 1", itemById.MaValeur);

//            // === QUERY avec QueryContext et paramètres ===
//            var queryParams = new Dictionary<string, object> { ["mavaleur"] = "Item 1" };
//            var queryContext = new QueryContext("SELECT * FROM demotable WHERE mavaleur = @mavaleur", queryParams);
//            var queryResult = await provider.QueryPOAsync(queryContext);
//            Assert.AreEqual(1, queryResult.Count, "La requête doit retourner 1 résultat");
//            Assert.AreEqual(item1.MonId, queryResult[0].MonId);

//            // === UPDATE via SavePOAsync (uniquement si modifié) ===
//            itemById.MaValeur = "Item 1 Updated";
//            Assert.IsTrue(itemById.IsChanges(), "L'objet doit être marqué comme modifié");
//            var saved = await provider.SavePOAsync(itemById);
//            Assert.IsTrue(saved, "SavePOAsync doit retourner true");

//            var itemVerified = await provider.GetPOAsync(item1.MonId);
//            Assert.AreEqual("Item 1 Updated", itemVerified.MaValeur);

//            // === BATCH INSERT - Insertion multiple ===
//            var batchItems = new[]
//            {
//                    new DataPOExample { MaValeur = "Item 2" },
//                    new DataPOExample { MaValeur = "Item 3" },
//                    new DataPOExample { MaValeur = "Item 4" }
//                };

//            await provider.InsertPOAsync(batchItems);

//            // Vérifier que tous les IDs sont affectés
//            Assert.IsTrue(batchItems.All(u => u.MonId > 0), "Tous les IDs doivent être affectés");
//            var distinctIds = batchItems.Select(u => u.MonId).Distinct().Count();
//            Assert.IsTrue(distinctIds == 3, $"Les IDs doivent être uniques (distinct={distinctIds})");

//            // === BATCH UPDATE - Mise à jour avec valeurs communes ===
//            var allItemsQuery = new QueryContext("SELECT * FROM demotable", null);
//            var allItems = await provider.QueryPOAsync(allItemsQuery);
//            Assert.AreEqual(4, allItems.Count, "Il doit y avoir 4 items au total");

//            var updates = new Dictionary<string, object> { ["mavaleur"] = "Updated Value" };
//            await provider.UpdatePOAsync(allItems.ToArray(), updates);

//            // Vérifier que tous sont mis à jour
//            var updatedQuery = new QueryContext("SELECT * FROM demotable WHERE mavaleur = 'Updated Value'", null);
//            var updatedItems = await provider.QueryPOAsync(updatedQuery);
//            Assert.AreEqual(4, updatedItems.Count, "Tous les items doivent être mis à jour");

//            // === DELETE - Suppression simple ===
//            int idToDelete = itemById.MonId; // Sauvegarder l'ID avant delete
//            await provider.DeletePOAsync(itemById);
//            var deletedItem = await provider.GetPOAsync(idToDelete);
//            Assert.IsNull(deletedItem, "L'item supprimé ne doit plus exister");

//            // === BATCH DELETE - Suppression multiple ===
//            await provider.DeletePOAsync(batchItems);
//            var remainingQuery = new QueryContext("SELECT * FROM demotable", null);
//            var remainingItems = await provider.QueryPOAsync(remainingQuery);
//            Assert.AreEqual(0, remainingItems.Count, "Tous les items doivent être supprimés");


//            //fin
//            provider?.Connector?.Dispose();
//            CleanupTest();
//        }

//        /// <summary>
//        /// Test des transactions (COMMIT et ROLLBACK)
//        /// </summary>
//        [TestMethod]
//        public async Task TransactionsFullyTest()
//        {
//            var provider = await InitializeProviderAsync();
//            var connector = provider.Connector;

//            // === Test COMMIT - Les données doivent persister ===
//            connector.BeginTransaction();
//            try
//            {
//                var item1 = new DataPOExample { MaValeur = "Transactional 1" };
//                var item2 = new DataPOExample { MaValeur = "Transactional 2" };

//                await provider.InsertPOAsync(item1);
//                await provider.InsertPOAsync(item2);

//                connector.CommitTransaction();
//            }
//            catch
//            {
//                connector.RollBackTransaction(safe: true);
//                throw;
//            }

//            // Vérifier la persistance (hors transaction)
//            var itemsQuery = new QueryContext("SELECT * FROM demotable", null);
//            var items = await provider.QueryPOAsync(itemsQuery);
//            Assert.AreEqual(2, items.Count, "Les 2 items doivent être persistés après COMMIT");

//            // === Test ROLLBACK - Les données ne doivent PAS persister ===
//            connector.BeginTransaction();
//            var item3 = new DataPOExample { MaValeur = "Transactional 3" };

//            try
//            {
//                await provider.InsertPOAsync(item3);

//                // Visible dans la transaction
//                var inTxQuery = new QueryContext("SELECT * FROM demotable WHERE mavaleur = 'Transactional 3'", null);
//                var inTransaction = await provider.QueryPOAsync(inTxQuery);
//                Assert.AreEqual(1, inTransaction.Count, "Item 3 doit être visible dans la transaction");

//                connector.RollBackTransaction(safe: true);
//            }
//            catch
//            {
//                connector.RollBackTransaction(safe: true);
//                throw;
//            }

//            // Non visible après rollback (hors transaction)
//            var afterRbQuery = new QueryContext("SELECT * FROM demotable WHERE mavaleur = 'Transactional 3'", null);
//            var afterRollback = await provider.QueryPOAsync(afterRbQuery);
//            Assert.AreEqual(0, afterRollback.Count, "Item 3 ne doit PAS être persisté après ROLLBACK");

//            // Cleanup
//            provider?.Connector?.Dispose();
//            CleanupTest();
//        }

//        /// <summary>
//        /// Test du change tracking (IsChanges, GetChangedValues, AcceptChanges) et modifications des flux
//        /// </summary>
//        [TestMethod]
//        public async Task ChangeTrackingFullyTest()
//        {
//            var provider = await InitializeProviderAsync();

//            // Insérer un item de test avec un flux
//            var item = new DataPOExample
//            {
//                MaValeur = "Test Item"
//            };
//            item.Flux["key1"] = "value1";
//            item.Flux["key2"] = 123;
//            await provider.InsertPOAsync(item);

//            // Récupérer l'item
//            var itemFromDb = await provider.GetPOAsync(item.MonId);
//            Assert.IsNotNull(itemFromDb);

//            // === Test 1: Pas de changement initialement ===
//            Assert.IsFalse(itemFromDb.IsChanges(), "IsChanges() doit retourner false avant modification");

//            // === Test 2: Modification d'une propriété simple ===
//            itemFromDb.MaValeur = "Modified Value";
//            Assert.IsTrue(itemFromDb.IsChanges(), "IsChanges() doit retourner true après modification");

//            // GetChangedValues
//            var changes = itemFromDb.GetChangedValues();
//            Assert.IsTrue(changes.ContainsKey("mavaleur"), "GetChangedValues doit contenir 'mavaleur'");
//            Assert.AreEqual("Modified Value", changes["mavaleur"]);

//            // AcceptChanges reset les changements
//            itemFromDb.AcceptChanges();
//            Assert.IsFalse(itemFromDb.IsChanges(), "IsChanges() doit retourner false après AcceptChanges");

//            // === Test 3: Modification du flux ===
//            itemFromDb.Flux["key1"] = "modified_value1";
//            itemFromDb.Flux["key3"] = "new_value";
//            Assert.IsTrue(itemFromDb.IsChanges(), "IsChanges() doit retourner true après modification du flux");

//            var fluxChanges = itemFromDb.GetChangedValues();
//            Assert.IsTrue(fluxChanges.ContainsKey("fluxjson"), "GetChangedValues doit contenir 'fluxjson'");

//            // Sauvegarder les changements
//            await provider.SavePOAsync(itemFromDb);

//            // Vérifier la persistance des changements de flux
//            var itemVerified = await provider.GetPOAsync(item.MonId);
//            Assert.IsNotNull(itemVerified.Flux, "Le flux ne doit pas être null");
//            Assert.AreEqual("modified_value1", itemVerified.Flux.GetString("key1"), "Le flux doit contenir la valeur modifiée");
//            Assert.AreEqual("new_value", itemVerified.Flux.GetString("key3"), "Le flux doit contenir la nouvelle clé");
//            Assert.AreEqual(123, itemVerified.Flux.GetInt("key2"), "Le flux doit conserver les valeurs non modifiées");

//            // === Test 4: Vider complètement le flux ===
//            itemVerified.Flux.Clear();
//            itemVerified.Flux["newkey"] = "newvalue";
//            Assert.IsTrue(itemVerified.IsChanges(), "IsChanges() doit retourner true après clear+ajout dans le flux");

//            await provider.SavePOAsync(itemVerified);

//            var itemFinal = await provider.GetPOAsync(item.MonId);
//            var finalKeys = itemFinal.Flux.ListFieldsKeys();
//            Assert.AreEqual(1, finalKeys.Length, "Le flux doit contenir 1 seule clé après clear+ajout");
//            Assert.AreEqual("newvalue", itemFinal.Flux.GetString("newkey"), "La nouvelle clé doit exister");

//            // Cleanup
//            provider?.Connector?.Dispose();
//            CleanupTest();
//        }

//        /// <summary>
//        /// Test des validations et opérations avancées
//        /// </summary>
//        [TestMethod]
//        public async Task ValidationAndAdvancedOperationsFullyTest()
//        {
//            var provider = await InitializeProviderAsync();


//            // === Préparer données de test ===
//            var items = new[]
//            {
//                    new DataPOExample { MaValeur = "Item A" },
//                    new DataPOExample { MaValeur = "Item B" },
//                    new DataPOExample { MaValeur = "Item C" }
//                };
//            await provider.InsertPOAsync(items);

//            // === Test GetPOByParamsAsync (clés primaires) ===
//            int itemAId = items[0].MonId;
//            var keys = new Dictionary<string, object> { ["monid"] = itemAId };
//            var itemByParams = await provider.GetPOByParamsAsync(keys);
//            Assert.IsNotNull(itemByParams, "GetPOByParamsAsync doit retourner l'item");
//            Assert.AreEqual("Item A", itemByParams.MaValeur);

//            // === Test QueryPOAsync avec limite et SqlParams ===
//            var limited = await provider.QueryPOAsync(limitResult: 2);
//            Assert.AreEqual(2, limited.Count, "QueryPOAsync avec limite doit retourner 2 résultats");

//            // === Test CollectionPO métadonnées ===
//            var allItemsQuery = new QueryContext("SELECT * FROM demotable", null);
//            var allItems = await provider.QueryPOAsync(allItemsQuery);
//            Assert.IsTrue(allItems.ExecuteTimeElapsed >= 0, "ExecuteTimeElapsed doit être défini");
//            Assert.IsTrue(allItems.Count == 3, $"Count doit être 3 (obtenu: {allItems.Count})");

//            // === Test exceptions typées ===
//            // Le provider lève InvalidOperationException pour les paramètres null
//            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
//                await provider.InsertPOAsync((DataPOExample)null),
//                "InsertPOAsync(null) doit lever InvalidOperationException");

//            // === Test QueryContext avec paramètres ===
//            var paramQuery = new QueryContext(
//                "SELECT * FROM demotable WHERE mavaleur = @val ORDER BY monid",
//                new Dictionary<string, object> { ["val"] = "Item A" }
//            );
//            var paramResult = await provider.QueryPOAsync(paramQuery);
//            Assert.AreEqual(1, paramResult.Count);
//            Assert.AreEqual("Item A", paramResult[0].MaValeur);

//            // === Test GetPOAsync avec ID inexistant ===
//            var nonExistent = await provider.GetPOAsync(99999);
//            Assert.IsNull(nonExistent, "GetPOAsync avec ID inexistant doit retourner null");

//            //fin
//            provider?.Connector?.Dispose();
//            CleanupTest();
//        }
//    }
//}
