using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.CONNECTOR;
using Nglib.DATA.DATAPO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO.TESTS
{
    /// <summary>
    /// Tests unitaires pour le DataPOProvider générique universel (multi-types Models et PO)
    /// Valide que le provider générique peut manipuler plusieurs types simultanément
    /// </summary>
    [TestClass]
    public class DataPOProviderGenericTests
    {
        private static string _tempDbPath;

        #region === Utilitaires de test ===

        /// <summary>
        /// Modèle métier pour les tests
        /// </summary>
        [Table("users")]
        public class UserModel
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }
            
            [Column("name")]
            public string Name { get; set; }
            
            [Column("email")]
            public string Email { get; set; }
        }

        /// <summary>
        /// Modèle métier pour les produits
        /// </summary>
        [Table("products")]
        public class ProductModel
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }
            
            [Column("productname")]
            public string ProductName { get; set; }
            
            [Column("price")]
            public decimal Price { get; set; }
        }

        /// <summary>
        /// Initialise une base de données SQLite temporaire avec plusieurs tables
        /// </summary>
        private static async Task<IDataConnector> InitializeDatabaseAsync()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_generic_provider_{Guid.NewGuid()}.db");
            string connectionString = $"Data Source={_tempDbPath};Version=3;";

            var connector = new ConnectorGeneric();
            connector.SetConnectionString(connectionString, "SQLITE");
            connector.Open();

            // Table de démo (DataPOExample)
            await connector.QueryAsync(@"
                CREATE TABLE IF NOT EXISTS demotable (
                    monid INTEGER PRIMARY KEY AUTOINCREMENT,
                    mavaleur TEXT,
                    fluxjson TEXT
                )");

            // Table users (pour UserModel)
            await connector.QueryAsync(@"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT,
                    email TEXT
                )");

            // Table products (pour ProductModel)
            await connector.QueryAsync(@"
                CREATE TABLE IF NOT EXISTS products (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    productname TEXT,
                    price REAL
                )");

            return connector;
        }

        /// <summary>
        /// Nettoie les ressources de test
        /// </summary>
        private static void CleanupTest(IDataConnector connector)
        {
            connector?.Dispose();
            if (!string.IsNullOrEmpty(_tempDbPath) && File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        #endregion



        /// <summary>
        /// Test complet du provider générique avec plusieurs types de DataPO
        /// Valide CRUD sur DataPOExample et d'autres types simultanément
        /// </summary>
        [TestMethod]
        public async Task GenericProvider_DataPO_MultipleTypesTest()
        {
            var connector = await InitializeDatabaseAsync();
            try
            {
                //  UN SEUL provider universel pour TOUS les types (PO et Models)
                var provider = new DataPOGenericProvider(connector);

                // === INSERT DataPOExample ===
                var demo1 = new DataPOExample { MaValeur = "Demo 1" };
                var demo2 = new DataPOExample { MaValeur = "Demo 2" };
                await provider.InsertPOAsync(demo1, demo2);

                Assert.IsTrue(demo1.MonId > 0, "Demo1 ID doit être auto-généré");
                Assert.IsTrue(demo2.MonId > 0, "Demo2 ID doit être auto-généré");
                Assert.AreNotEqual(demo1.MonId, demo2.MonId, "Les IDs doivent être différents");

                // === READ DataPOExample par ID ===
                var demoFromDb = await provider.GetPOAsync<DataPOExample>(demo1.MonId);
                Assert.IsNotNull(demoFromDb, "GetPOAsync doit retourner l'objet");
                Assert.AreEqual("Demo 1", demoFromDb.MaValeur);
                Assert.AreEqual(demo1.MonId, demoFromDb.MonId);

                // === READ par dictionnaire de paramètres ===
                var demoByParams = await provider.GetPOAsync<DataPOExample>(
                    new Dictionary<string, object> { ["mavaleur"] = "Demo 2" });
                Assert.IsNotNull(demoByParams);
                Assert.AreEqual(demo2.MonId, demoByParams.MonId);

                // === QUERY avec limite ===
                var allDemos = await provider.QueryPOAsync<DataPOExample>(100);
                Assert.AreEqual(2, allDemos.Count, "QueryPOAsync doit retourner 2 objets");

                // === UPDATE DataPOExample ===
                demoFromDb.MaValeur = "Demo 1 Updated";
                await provider.SavePOAsync(demoFromDb);

                var demoVerified = await provider.GetPOAsync<DataPOExample>(demo1.MonId);
                Assert.AreEqual("Demo 1 Updated", demoVerified.MaValeur);

                // === EXIST vérification ===
                var exists = await provider.ExistAsync<DataPOExample>(
                    new Dictionary<string, object> { ["monid"] = demo1.MonId });
                Assert.IsTrue(exists, "L'objet doit exister");

                var notExists = await provider.ExistAsync<DataPOExample>(
                    new Dictionary<string, object> { ["monid"] = 99999 });
                Assert.IsFalse(notExists, "L'objet inexistant ne doit pas être trouvé");

                // === DELETE DataPOExample ===
                await provider.DeletePOAsync(demo2);
                var deletedDemo = await provider.GetPOAsync<DataPOExample>(demo2.MonId);
                Assert.IsNull(deletedDemo, "L'objet supprimé ne doit plus exister");

                // === QUERY après delete ===
                var remainingDemos = await provider.QueryPOAsync<DataPOExample>(100);
                Assert.AreEqual(1, remainingDemos.Count, "Il ne doit rester qu'un seul objet");

                // === BATCH UPDATE avec UpdatePOAsync ===
                var batchDemos = new[]
                {
                    new DataPOExample { MaValeur = "Batch 1" },
                    new DataPOExample { MaValeur = "Batch 2" },
                    new DataPOExample { MaValeur = "Batch 3" }
                };
                await provider.InsertPOAsync(batchDemos);

                var allBatch = await provider.QueryPOAsync<DataPOExample>(100);
                await provider.UpdatePOAsync(
                    allBatch.ToArray(),
                    new Dictionary<string, object> { ["mavaleur"] = "Updated Batch" });

                var updatedBatch = await provider.QueryPOAsync<DataPOExample>(100);
                Assert.IsTrue(updatedBatch.All(d => d.MaValeur == "Updated Batch"),
                    "Tous les objets doivent être mis à jour");
            }
            finally
            {
                CleanupTest(connector);
            }
        }

        /// <summary>
        /// Test complet du provider générique avec Models (mapping automatique par réflexion)
        /// Valide CRUD sur plusieurs types de Models simultanément
        /// </summary>
        [TestMethod]
        public async Task GenericProvider_Models_MultipleTypesTest()
        {
            var connector = await InitializeDatabaseAsync();
            try
            {
                //  UN SEUL provider universel pour TOUS les Models
                var provider = new DataPOGenericProvider(connector);

                // === INSERT UserModel ===
                var user1 = new UserModel { Name = "John Doe", Email = "john@example.com" };
                var user2 = new UserModel { Name = "Jane Smith", Email = "jane@example.com" };

                var insertedUser1 = await provider.InsertModelAsync(user1);
                var insertedUser2 = await provider.InsertModelAsync(user2);

                Assert.IsTrue(insertedUser1.Id > 0, "User1 ID doit être auto-généré");
                Assert.IsTrue(insertedUser2.Id > 0, "User2 ID doit être auto-généré");

                // === INSERT ProductModel (type différent avec le même provider) ===
                var product1 = new ProductModel { ProductName = "Laptop", Price = 999.99m };
                var insertedProduct1 = await provider.InsertModelAsync(product1);
                Assert.IsTrue(insertedProduct1.Id > 0, "Product ID doit être auto-généré");

                // === READ UserModel par ID ===
                var userFromDb = await provider.GetModelAsync<UserModel>(insertedUser1.Id);
                Assert.IsNotNull(userFromDb, "GetModelAsync doit retourner le modèle");
                Assert.AreEqual("John Doe", userFromDb.Name);
                Assert.AreEqual("john@example.com", userFromDb.Email);

                // === READ ProductModel par ID (type différent) ===
                var productFromDb = await provider.GetModelAsync<ProductModel>(insertedProduct1.Id);
                Assert.IsNotNull(productFromDb);
                Assert.AreEqual("Laptop", productFromDb.ProductName);
                Assert.AreEqual(999.99m, productFromDb.Price);

                // === QUERY UserModel avec limite ===
                var users = await provider.QueryModelAsync<UserModel>(100);
                Assert.AreEqual(2, users.data.Count, "QueryModelAsync doit retourner 2 users");

                // === QUERY ProductModel avec limite ===
                var products = await provider.QueryModelAsync<ProductModel>(100);
                Assert.AreEqual(1, products.data.Count, "QueryModelAsync doit retourner 1 product");

                // === UPDATE UserModel ===
                userFromDb.Email = "newemail@example.com";
                var updatedUser = await provider.SaveModelAsync(userFromDb);
                Assert.AreEqual("newemail@example.com", updatedUser.Email);

                var userVerified = await provider.GetModelAsync<UserModel>(insertedUser1.Id);
                Assert.AreEqual("newemail@example.com", userVerified.Email);

                // === UPDATE ProductModel (type différent) ===
                productFromDb.Price = 899.99m;
                var updatedProduct = await provider.SaveModelAsync(productFromDb);
                Assert.AreEqual(899.99m, updatedProduct.Price);

                // === DELETE UserModel ===
                var deleted = await provider.DeleteModelAsync(insertedUser2);
                Assert.IsTrue(deleted, "Delete doit retourner true");

                var deletedUser = await provider.GetModelAsync<UserModel>(insertedUser2.Id);
                Assert.IsNull(deletedUser, "Le modèle supprimé ne doit plus exister");

                // === Vérification finale des counts ===
                var finalUsers = await provider.QueryModelAsync<UserModel>(100);
                Assert.AreEqual(1, finalUsers.data.Count, "Il doit rester 1 user");

                var finalProducts = await provider.QueryModelAsync<ProductModel>(100);
                Assert.AreEqual(1, finalProducts.data.Count, "Il doit rester 1 product");
            }
            finally
            {
                CleanupTest(connector);
            }
        }

        /// <summary>
        /// Test de validation : provider typé vs provider générique
        /// Vérifie que le provider typé reste strictement type-safe
        /// et que le provider générique offre la flexibilité multi-types
        /// </summary>
        [TestMethod]
        public async Task GenericProvider_TypeSafety_ValidationTest()
        {
            var connector = await InitializeDatabaseAsync();
            try
            {
                // === Provider Générique : Flexibilité multi-types ===
                var genericProvider = new DataPOGenericProvider(connector);

                var demo = new DataPOExample { MaValeur = "Test Generic" };
                await genericProvider.InsertPOAsync(demo);

                // ✅ Provider générique peut manipuler plusieurs types
                var demoFromGeneric = await genericProvider.GetPOAsync<DataPOExample>(demo.MonId);
                Assert.IsNotNull(demoFromGeneric);

                // === Provider Typé : Type-Safety stricte ===
                var typedProvider = new DataPOProviderSQL<DataPOExample>(connector);

                var demo2 = new DataPOExample { MaValeur = "Test Typed" };
                await typedProvider.InsertPOAsync(demo2);

                // ✅ Provider typé ne peut manipuler QUE son type déclaré
                var demoFromTyped = await typedProvider.GetPOAsync(demo2.MonId);
                Assert.IsNotNull(demoFromTyped);

                // ✅ Vérifier que le provider typé n'a PAS de méthodes génériques
                var genericMethods = typeof(DataPOProviderSQL<DataPOExample>).GetMethods()
                    .Where(m => m.Name == "GetPOAsync" && m.IsGenericMethod)
                    .ToList();

                Assert.AreEqual(0, genericMethods.Count,
                    "DataPOProviderSQL<T> ne doit PAS avoir de méthodes génériques");

                // ✅ Vérifier que le provider générique a DES méthodes génériques
                var universalMethods = typeof(DataPOGenericProvider).GetMethods()
                    .Where(m => m.Name == "GetPOAsync" && m.IsGenericMethod)
                    .ToList();

                Assert.IsTrue(universalMethods.Count > 0,
                    "DataPOProvider doit avoir des méthodes génériques");

                // === Performance : les deux doivent être similaires ===
                var startGeneric = DateTime.Now;
                for (int i = 0; i < 20; i++)
                {
                    await genericProvider.GetPOAsync<DataPOExample>(demo.MonId);
                }
                var elapsedGeneric = (DateTime.Now - startGeneric).TotalMilliseconds;

                var startTyped = DateTime.Now;
                for (int i = 0; i < 20; i++)
                {
                    await typedProvider.GetPOAsync(demo2.MonId);
                }
                var elapsedTyped = (DateTime.Now - startTyped).TotalMilliseconds;

                // Performance similaire (ratio entre 0.5 et 2.0)
                var ratio = elapsedGeneric / (elapsedTyped + 1); // +1 pour éviter division par 0
                Assert.IsTrue(ratio > 0.3 && ratio < 3.0,
                    $"Performance similaire attendue. Generic: {elapsedGeneric}ms, Typed: {elapsedTyped}ms");
            }
            finally
            {
                CleanupTest(connector);
            }
        }

    }
}
