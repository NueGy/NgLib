using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Tests du QueryBuilder avec support multi-moteurs SQL (PostgreSQL, SQL Server, SQLite)
    /// Configuré pour tester un moteur spécifique via la propriété TestEngine
    /// </summary>
    [TestClass]
    public class QueryBuilderTests
    {
        /// <summary>
        /// Moteur SQL à tester : "postgresql", "mssql", "sqlite"
        /// Changer cette valeur pour tester un autre moteur
        /// </summary>
        private static readonly string TestEngine = "postgresql";

        /// <summary>
        /// Factory pour créer une instance de QueryBuilder selon le moteur configuré
        /// </summary>
        private static IQueryBuilder CreateQueryBuilder() 
            => QueryBuilderTools.CreateQueryBuilder(TestEngine);

        /// <summary>
        /// Test simple pour vérifier le fonctionnement de base du QueryBuilder
        /// </summary>
        [TestMethod]
        public void SimpleTest()
        {
            // SELECT basique avec quelques conditions
            var result = CreateQueryBuilder()
                .Select("id", "nom", "email")
                .From("utilisateurs")
                .Where("actif", "=", true)
                .OrderBy("nom ASC")
                .Limit(10)
                .Build();

            // Assertions de base
            Assert.IsNotNull(result.Item1);
            Assert.IsTrue(result.Item1.Contains("SELECT"));
            Assert.IsTrue(result.Item1.Contains("utilisateurs"));
            Assert.IsTrue(result.Item1.Contains("WHERE"));
            Assert.AreEqual(1, result.Item2.Count);
            Assert.IsTrue(result.Item2.ContainsKey("actif"));

            // Validation de la factory
            var builder = CreateQueryBuilder();
            Assert.IsNotNull(builder);
            Assert.AreEqual(TestEngine, builder.EngineName.ToLower());
        }

        /// <summary>
        /// Tests de requêtes SQL logiques avec StringBuilder pour génération de script exécutable
        /// Génère un script SQL complet (CREATE TABLE + INSERT + SELECT + UPDATE + DELETE)
        /// qui peut être copié et exécuté dans un SGBD pour validation
        /// </summary>
        [TestMethod]
        public void QueriesTest()
        {
            var sqlScript = new StringBuilder();

            // ===== CREATE TABLE pour la base de test =====
            sqlScript.AppendLine("-- =====================================================");
            sqlScript.AppendLine("-- Script de test QueryBuilder");
            sqlScript.AppendLine("-- Moteur : " + TestEngine.ToUpper());
            sqlScript.AppendLine("-- Généré automatiquement pour tests");
            sqlScript.AppendLine("-- =====================================================");
            sqlScript.AppendLine();

            sqlScript.AppendLine("-- Table 1 : utilisateurs");
            sqlScript.AppendLine("CREATE TABLE IF NOT EXISTS utilisateurs (");
            sqlScript.AppendLine("    id SERIAL PRIMARY KEY,");
            sqlScript.AppendLine("    nom VARCHAR(100) NOT NULL,");
            sqlScript.AppendLine("    email VARCHAR(150) UNIQUE NOT NULL,");
            sqlScript.AppendLine("    age INTEGER,");
            sqlScript.AppendLine("    actif BOOLEAN DEFAULT TRUE,");
            sqlScript.AppendLine("    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,");
            sqlScript.AppendLine("    deleted_at TIMESTAMP NULL");
            sqlScript.AppendLine(");");
            sqlScript.AppendLine();

            sqlScript.AppendLine("-- Table 2 : commandes");
            sqlScript.AppendLine("CREATE TABLE IF NOT EXISTS commandes (");
            sqlScript.AppendLine("    id SERIAL PRIMARY KEY,");
            sqlScript.AppendLine("    user_id INTEGER NOT NULL,");
            sqlScript.AppendLine("    montant DECIMAL(10,2) NOT NULL,");
            sqlScript.AppendLine("    statut VARCHAR(50) DEFAULT 'pending',");
            sqlScript.AppendLine("    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,");
            sqlScript.AppendLine("    FOREIGN KEY (user_id) REFERENCES utilisateurs(id)");
            sqlScript.AppendLine(");");
            sqlScript.AppendLine();
            sqlScript.AppendLine("-- =====================================================");
            sqlScript.AppendLine();

            // ===== INSERT - Insertion de données =====
            sqlScript.AppendLine("-- INSERT : Ajout d'utilisateurs");
            
            var insertQuery1 = CreateQueryBuilder()
                .Insert(new Dictionary<string, object>
                {
                    { "nom", "Dupont" },
                    { "email", "dupont@test.com" },
                    { "age", 25 },
                    { "actif", true }
                })
                .From("utilisateurs")
                .Build();

            sqlScript.AppendLine($"-- Query: {insertQuery1.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", insertQuery1.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.IsTrue(insertQuery1.Item1.Contains("INSERT INTO"));
            Assert.AreEqual(4, insertQuery1.Item2.Count);
            sqlScript.AppendLine();

            var insertQuery2 = CreateQueryBuilder()
                .Insert(new Dictionary<string, object>
                {
                    { "nom", "Martin" },
                    { "email", "martin@test.com" },
                    { "age", 30 }
                })
                .From("utilisateurs")
                .Build();

            sqlScript.AppendLine($"-- Query: {insertQuery2.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", insertQuery2.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.AreEqual(3, insertQuery2.Item2.Count);
            sqlScript.AppendLine();

            var insertQuery3 = CreateQueryBuilder()
                .Insert(new Dictionary<string, object>
                {
                    { "nom", "Bernard" },
                    { "email", "bernard@test.com" },
                    { "age", 45 },
                    { "actif", false }
                })
                .From("utilisateurs")
                .Build();

            sqlScript.AppendLine($"-- Query: {insertQuery3.Item1}");
            sqlScript.AppendLine();

            // ===== SELECT - Lecture simple =====
            sqlScript.AppendLine("-- SELECT : Lecture simple avec filtres");

            var selectQuery1 = CreateQueryBuilder()
                .Select("id", "nom", "email", "age")
                .From("utilisateurs")
                .Where("actif", "=", true)
                .OrderBy("nom ASC")
                .Build();

            sqlScript.AppendLine($"-- Query: {selectQuery1.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", selectQuery1.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.IsTrue(selectQuery1.Item1.Contains("SELECT"));
            Assert.IsTrue(selectQuery1.Item1.Contains("WHERE"));
            sqlScript.AppendLine();

            // SELECT avec AND multiple
            var selectQuery2 = CreateQueryBuilder()
                .Select("nom", "age")
                .From("utilisateurs")
                .Where("actif", "=", true)
                .Where("age", ">=", 18)
                .Where("age", "<=", 65)
                .OrderBy("age DESC")
                .Build();

            sqlScript.AppendLine($"-- Query: {selectQuery2.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", selectQuery2.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.AreEqual(3, selectQuery2.Item2.Count);
            sqlScript.AppendLine();

            // ===== SELECT avec BETWEEN =====
            sqlScript.AppendLine("-- SELECT : BETWEEN");

            var betweenQuery = CreateQueryBuilder()
                .Select("nom", "age")
                .From("utilisateurs")
                .WhereBetween("age", 20, 40)
                .Build();

            sqlScript.AppendLine($"-- Query: {betweenQuery.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", betweenQuery.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.IsTrue(betweenQuery.Item1.Contains("BETWEEN"));
            Assert.AreEqual(2, betweenQuery.Item2.Count);
            sqlScript.AppendLine();

            // ===== SELECT avec pagination =====
            sqlScript.AppendLine("-- SELECT : Pagination (LIMIT/OFFSET)");

            var paginationQuery = CreateQueryBuilder()
                .Select("*")
                .From("utilisateurs")
                .OrderBy("created_at DESC")
                .Limit(10, 0)
                .Build();

            sqlScript.AppendLine($"-- Query: {paginationQuery.Item1}");
            Assert.IsTrue(paginationQuery.Item1.Contains("LIMIT"));
            sqlScript.AppendLine();

            // ===== UPDATE - Modification =====
            sqlScript.AppendLine("-- UPDATE : Modification d'un utilisateur");

            var updateQuery = CreateQueryBuilder()
                .Update(new Dictionary<string, object>
                {
                    { "email", "nouveau.dupont@test.com" },
                    { "age", 26 }
                })
                .From("utilisateurs")
                .Where("nom", "=", "Dupont")
                .Build();

            sqlScript.AppendLine($"-- Query: {updateQuery.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", updateQuery.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.IsTrue(updateQuery.Item1.Contains("UPDATE"));
            Assert.IsTrue(updateQuery.Item1.Contains("SET"));
            Assert.AreEqual(3, updateQuery.Item2.Count);
            sqlScript.AppendLine();

            // ===== INSERT commande =====
            sqlScript.AppendLine("-- INSERT : Ajout de commandes");

            var insertCommandeQuery = CreateQueryBuilder()
                .Insert(new Dictionary<string, object>
                {
                    { "user_id", 1 },
                    { "montant", 150.50 },
                    { "statut", "completed" }
                })
                .From("commandes")
                .Build();

            sqlScript.AppendLine($"-- Query: {insertCommandeQuery.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", insertCommandeQuery.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            sqlScript.AppendLine();

            // ===== SELECT avec JOIN =====
            sqlScript.AppendLine("-- SELECT : JOIN entre tables");

            var joinQuery = CreateQueryBuilder()
                .Select("u.nom", "u.email", "c.montant", "c.statut")
                .From("utilisateurs u")
                .Join("commandes c", "c.user_id = u.id", QueryBuilderModels.JoinTypeEnum.Inner)
                .Where("u.actif", "=", true)
                .OrderBy("c.created_at DESC")
                .Build();

            sqlScript.AppendLine($"-- Query: {joinQuery.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", joinQuery.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.IsTrue(joinQuery.Item1.Contains("JOIN"));
            sqlScript.AppendLine();

            // ===== DELETE - Suppression =====
            sqlScript.AppendLine("-- DELETE : Suppression d'un utilisateur");

            var deleteQuery = CreateQueryBuilder()
                .Delete()
                .From("utilisateurs")
                .Where("email", "=", "bernard@test.com")
                .Build();

            sqlScript.AppendLine($"-- Query: {deleteQuery.Item1}");
            sqlScript.AppendLine($"-- Params: {string.Join(", ", deleteQuery.Item2.Select(p => $"{p.Key}={p.Value}"))}");
            Assert.IsTrue(deleteQuery.Item1.Contains("DELETE"));
            Assert.AreEqual(1, deleteQuery.Item2.Count);
            sqlScript.AppendLine();

            sqlScript.AppendLine("-- =====================================================");
            sqlScript.AppendLine("-- FIN DU SCRIPT");
            sqlScript.AppendLine("-- =====================================================");

            // Assertions globales
            Assert.IsTrue(sqlScript.Length > 500);
            
            // Output du script complet pour debug/copie
            Console.WriteLine(sqlScript.ToString());
        }

        /// <summary>
        /// Tests des cas complexes et fonctionnalités avancées du QueryBuilder
        /// (Subqueries, EXISTS, NULL, Extensions, Clone, Reset, Validation)
        /// </summary>
        [TestMethod]
        public void ComplexQueriesTest()
        {
            // ===== JOIN multiples =====
            var multiJoinQuery = CreateQueryBuilder()
                .Select("u.nom", "u.email", "c.montant", "p.nom_produit")
                .From("utilisateurs u")
                .Join("commandes c", "c.user_id = u.id", QueryBuilderModels.JoinTypeEnum.Inner)
                .Join("produits p", "p.id = c.produit_id", QueryBuilderModels.JoinTypeEnum.Left)
                .Where("u.actif", "=", true)
                .Where("c.montant", ">", 100)
                .OrderBy("c.montant DESC")
                .Build();

            Assert.IsTrue(multiJoinQuery.Item1.Contains("INNER JOIN") || multiJoinQuery.Item1.Contains("JOIN"));
            Assert.IsTrue(multiJoinQuery.Item1.Contains("LEFT JOIN"));
            Assert.AreEqual(2, multiJoinQuery.Item2.Count);

            // ===== Subquery avec IN =====
            var subQuery = CreateQueryBuilder()
                .Select("id")
                .From("categories")
                .Where("active", "=", true);

            var mainQueryWithSub = CreateQueryBuilder()
                .Select("*")
                .From("produits")
                .WhereSubquery("category_id", "IN", subQuery)
                .Where("stock", ">", 0)
                .Build();

            Assert.IsTrue(mainQueryWithSub.Item1.Contains("IN"));
            Assert.IsTrue(mainQueryWithSub.Item1.Contains("SELECT"));

            // ===== EXISTS =====
            var existsSubQuery = CreateQueryBuilder()
                .Select("1")
                .From("commandes")
                .WhereRaw("commandes.user_id = users.id", null);

            var existsQuery = CreateQueryBuilder()
                .Select("*")
                .From("users")
                .WhereExists(existsSubQuery, false)
                .Build();

            Assert.IsTrue(existsQuery.Item1.Contains("EXISTS"));

            // ===== NOT EXISTS =====
            var notExistsQuery = CreateQueryBuilder()
                .Select("*")
                .From("users")
                .WhereExists(existsSubQuery, true)
                .Build();

            Assert.IsTrue(notExistsQuery.Item1.Contains("NOT EXISTS"));

            // ===== WHERE NULL et NOT NULL =====
            var nullQuery = CreateQueryBuilder()
                .Select("*")
                .From("utilisateurs")
                .WhereNull("deleted_at", false)
                .WhereNull("verified_at", true)
                .Build();

            Assert.IsTrue(nullQuery.Item1.Contains("IS NULL"));
            Assert.IsTrue(nullQuery.Item1.Contains("IS NOT NULL"));

            // ===== WHERE clause personnalisée =====
            var customWhereQuery = CreateQueryBuilder()
                .Select("*")
                .From("products")
                .WhereRaw("(price > @custom1 OR discount > @custom2)", new Dictionary<string, object>
                {
                    { "@custom1", 100 },
                    { "@custom2", 0.2 }
                })
                .Build();

            Assert.IsNotNull(customWhereQuery.Item1);

            // ===== Extensions methods =====
            var extensionsQuery = CreateQueryBuilder()
                .SelectAll()
                .From("users")
                .WhereEqual("status", "active")
                .WhereNotEqual("role", "guest")
                .WhereGreater("score", 100)
                .WhereLess("age", 65)
                .WhereLike("name", "%John%")
                .WhereNotNull("email")
                .Build();

            Assert.IsTrue(extensionsQuery.Item1.Contains("SELECT") && extensionsQuery.Item1.Contains("*"));

            // ===== Clone - Vérification indépendance =====
            var original = CreateQueryBuilder()
                .Select("id", "nom")
                .From("users")
                .Where("actif", "=", true);

            var cloned = original.Clone();
            cloned.Where("age", ">", 18);

            var originalBuild = original.Build();
            var clonedBuild = cloned.Build();

            Assert.AreEqual(1, originalBuild.Item2.Count);
            Assert.AreEqual(2, clonedBuild.Item2.Count);
            Assert.AreNotSame(originalBuild, clonedBuild);

            // ===== Reset =====
            var resetQuery = CreateQueryBuilder()
                .Select("id")
                .From("test")
                .Where("active", "=", true);

            resetQuery.Build();
            resetQuery.Reset();
            
            var afterReset = resetQuery.Select("name").From("other").Build();
            Assert.IsTrue(afterReset.Item1.Contains("other"));

            // ===== ValidateQuery =====
            var validQuery = CreateQueryBuilder()
                .Select("id")
                .From("products");
            Assert.IsTrue(validQuery.ValidateQuery());

            // ===== Validation des paramètres =====
            Assert.ThrowsException<ArgumentException>(() => CreateQueryBuilder().Where("id", "INVALID_OP", 1));

            // ===== GROUP BY et HAVING =====
            var groupQuery = CreateQueryBuilder()
                .Select("category", "COUNT(*) as total")
                .From("products")
                .GroupBy("category")
                .Having("COUNT(*) > 5")
                .OrderBy("total DESC")
                .Build();

            Assert.IsTrue(groupQuery.Item1.Contains("GROUP BY"));
            Assert.IsTrue(groupQuery.Item1.Contains("HAVING"));

            // ===== Tests spécifiques SQL Server =====
            var sqlServerBuilder = new QueryBuilderMssql();
            var sqlServerQuery = sqlServerBuilder
                .Select("id", "name")
                .From("users")
                .Where("active", "=", true)
                .Limit(10, 5)
                .Build();

            Assert.IsNotNull(sqlServerQuery.Item1);
            Assert.AreEqual("mssql", sqlServerBuilder.EngineName);

            // ===== Tests spécifiques SQLite =====
            var sqliteBuilder = new QueryBuilderSqlite();
            var sqliteQuery = sqliteBuilder
                .Select("*")
                .From("test")
                .Limit(20)
                .Build();

            Assert.IsNotNull(sqliteQuery.Item1);
            Assert.AreEqual("sqlite", sqliteBuilder.EngineName);

            Console.WriteLine("✅ Tous les tests complexes sont passés avec succès");
        }

        /// <summary>
        /// Tests pour les opérations DataTable/DataRow : UpdateWithDataRow et InsertWithDataTable
        /// </summary>
        [TestMethod]
        public void QueryDatasetTests()
        {
            // ==================== PARTIE 1 : UpdateWithDataRow ====================
            Console.WriteLine("\n=== Tests UpdateWithDataRow ===");
            
            // Préparation : Créer une DataTable avec un schéma
            var updateTable = new System.Data.DataTable("utilisateurs");
            updateTable.Columns.Add("id", typeof(int));
            updateTable.Columns.Add("nom", typeof(string));
            updateTable.Columns.Add("email", typeof(string));
            updateTable.Columns.Add("age", typeof(int));
            updateTable.Columns.Add("actif", typeof(bool));
            
            // Définir la clé primaire
            updateTable.PrimaryKey = new[] { updateTable.Columns["id"] };

            // Ajouter une ligne
            var row = updateTable.NewRow();
            row["id"] = 1;
            row["nom"] = "Dupont";
            row["email"] = "dupont@test.com";
            row["age"] = 25;
            row["actif"] = true;
            updateTable.Rows.Add(row);
            updateTable.AcceptChanges();

            // Modifier la ligne
            row["nom"] = "Dupont Modifié";
            row["email"] = "nouveau@test.com";

            // Test 1.1 : UpdateWithDataRow avec allColumns = false (colonnes modifiées uniquement)
            var updateResult1 = CreateQueryBuilder()
                .UpdateWithDataRow(row, allColumns: false)
                .From("utilisateurs")
                .Build();

            Assert.IsNotNull(updateResult1.Item1);
            Assert.IsTrue(updateResult1.Item1.Contains("UPDATE"));
            Assert.IsTrue(updateResult1.Item1.Contains("utilisateurs"));
            Assert.IsTrue(updateResult1.Item1.Contains("WHERE"));
            Assert.IsTrue(updateResult1.Item2.ContainsKey("id"), "WHERE doit contenir id");
            Assert.IsTrue(updateResult1.Item2.ContainsKey("nom"), "SET doit contenir nom");
            Assert.IsTrue(updateResult1.Item2.ContainsKey("email"), "SET doit contenir email");
            Assert.IsFalse(updateResult1.Item2.ContainsKey("age"), "Ne doit pas contenir age (non modifié)");

            Console.WriteLine($"UpdateWithDataRow (modifiées) : {updateResult1.Item1}");

            // Test 1.2 : UpdateWithDataRow avec allColumns = true (toutes colonnes sauf PK)
            var updateResult2 = CreateQueryBuilder()
                .UpdateWithDataRow(row, allColumns: true)
                .From("utilisateurs")
                .Build();

            Assert.IsNotNull(updateResult2.Item1);
            Assert.IsTrue(updateResult2.Item1.Contains("UPDATE"));
            Assert.IsTrue(updateResult2.Item2.ContainsKey("id"), "WHERE doit contenir id");
            Assert.IsTrue(updateResult2.Item2.ContainsKey("nom"));
            Assert.IsTrue(updateResult2.Item2.ContainsKey("email"));
            Assert.IsTrue(updateResult2.Item2.ContainsKey("age"), "Doit contenir age (allColumns=true)");
            Assert.IsTrue(updateResult2.Item2.ContainsKey("actif"));

            Console.WriteLine($"UpdateWithDataRow (toutes) : {updateResult2.Item1}");

            // Test 1.3 : UpdateWithDataRow avec explicitWhereColumns
            var updateResult3 = CreateQueryBuilder()
                .UpdateWithDataRow(row, allColumns: false, explicitWhereColumns: new[] { "email" })
                .From("utilisateurs")
                .Build();

            Assert.IsNotNull(updateResult3.Item1);
            Assert.IsTrue(updateResult3.Item1.Contains("WHERE"));
            Assert.IsTrue(updateResult3.Item2.ContainsKey("email"), "WHERE doit contenir email (explicite)");
            
            Console.WriteLine($"UpdateWithDataRow (WHERE explicite) : {updateResult3.Item1}");
            Console.WriteLine("✅ Tests UpdateWithDataRow réussis");

            // ==================== PARTIE 2 : InsertWithDataTable ====================
            Console.WriteLine("\n=== Tests InsertWithDataTable ===");
            
            // Préparation : Créer une DataTable avec plusieurs lignes
            var insertTable = new System.Data.DataTable("produits");
            var insertIdColumn = insertTable.Columns.Add("id", typeof(int));
            insertIdColumn.AutoIncrement = true;
            insertIdColumn.AutoIncrementSeed = 1;
            insertTable.Columns.Add("nom", typeof(string));
            insertTable.Columns.Add("prix", typeof(decimal));
            insertTable.Columns.Add("stock", typeof(int));
            insertTable.Columns.Add("description", typeof(string));

            // Ajouter plusieurs lignes
            insertTable.Rows.Add(null, "Produit A", 19.99m, 100, "Description A");
            insertTable.Rows.Add(null, "Produit B", 29.99m, 50, "Description B");
            insertTable.Rows.Add(null, "Produit C", 39.99m, 25, "Description C");

            // Test 2.1 : InsertWithDataTable sans exclusions (id auto-incrémenté doit être exclu)
            var insertResult1 = CreateQueryBuilder()
                .InsertWithDataTable(insertTable)
                .From("produits")
                .Build();

            Assert.IsNotNull(insertResult1.Item1);
            Assert.IsTrue(insertResult1.Item1.Contains("INSERT INTO"));
            Assert.IsTrue(insertResult1.Item1.Contains("produits"));
            Assert.IsTrue(insertResult1.Item1.Contains("VALUES"));
            Assert.IsFalse(insertResult1.Item2.Any(p => p.Key.Contains("id")), "id (AutoIncrement) doit être exclu");
            Assert.IsTrue(insertResult1.Item2.Count >= 12, $"Devrait avoir au moins 12 paramètres, trouvé : {insertResult1.Item2.Count}");
            Assert.IsTrue(insertResult1.Item2.ContainsKey("nom_1"), "Doit contenir nom_1");
            Assert.IsTrue(insertResult1.Item2.ContainsKey("nom_2"), "Doit contenir nom_2");
            Assert.IsTrue(insertResult1.Item2.ContainsKey("nom_3"), "Doit contenir nom_3");

            Console.WriteLine($"InsertWithDataTable (3 lignes) : {insertResult1.Item1}");
            Console.WriteLine($"Paramètres : {insertResult1.Item2.Count} paramètres générés");

            // Test 2.2 : InsertWithDataTable avec excludeColumns
            var insertResult2 = CreateQueryBuilder()
                .InsertWithDataTable(insertTable, excludeColumns: new[] { "description" })
                .From("produits")
                .Build();

            Assert.IsNotNull(insertResult2.Item1);
            Assert.IsFalse(insertResult2.Item2.Any(p => p.Key.Contains("description")), "description doit être exclue");
            Assert.IsTrue(insertResult2.Item2.Count >= 9, $"Devrait avoir au moins 9 paramètres, trouvé : {insertResult2.Item2.Count}");

            Console.WriteLine($"InsertWithDataTable (avec exclusion) : {insertResult2.Item1}");

            // Test 2.3 : InsertWithDataTable avec une seule ligne
            var singleRowTable = new System.Data.DataTable("test");
            singleRowTable.Columns.Add("nom", typeof(string));
            singleRowTable.Columns.Add("valeur", typeof(int));
            singleRowTable.Rows.Add("Test", 42);

            var insertResult3 = CreateQueryBuilder()
                .InsertWithDataTable(singleRowTable)
                .From("test")
                .Build();

            Assert.IsNotNull(insertResult3.Item1);
            Assert.IsTrue(insertResult3.Item1.Contains("INSERT INTO"));
            Assert.AreEqual(2, insertResult3.Item2.Count, "Une ligne = 2 paramètres");

            Console.WriteLine($"InsertWithDataTable (1 ligne) : {insertResult3.Item1}");
            Console.WriteLine("✅ Tests InsertWithDataTable réussis");
            
            Console.WriteLine("\n✅ Tous les tests QueryDataset réussis");
        }

        /// <summary>
        /// Test spécifique PostgreSQL pour vérifier le casting ::jsonb
        /// </summary>
        [TestMethod]
        public void PostgreSQL_JsonbCast_Test()
        {
            // Ce test ne fonctionne que pour PostgreSQL
            if (TestEngine != "postgresql")
            {
                Assert.Inconclusive("Ce test est spécifique à PostgreSQL");
                return;
            }

            // Créer une DataTable avec une colonne JSON
            var table = new System.Data.DataTable("documents");
            var idColumn = table.Columns.Add("id", typeof(int));
            idColumn.AutoIncrement = true;
            table.Columns.Add("titre", typeof(string));
            table.Columns.Add("metadata_json", typeof(string));
            table.Columns.Add("contenu", typeof(string));

            // Ajouter des données avec du JSON
            table.Rows.Add(null, "Doc 1", "{\"tags\": [\"test\", \"demo\"]}", "Contenu 1");
            table.Rows.Add(null, "Doc 2", "{\"author\": \"John\"}", "Contenu 2");

            var result = CreateQueryBuilder()
                .InsertWithDataTable(table)
                .From("documents")
                .Build();

            Assert.IsNotNull(result.Item1);
            
            // Vérifier que ::jsonb est présent pour la colonne metadata_json
            Assert.IsTrue(result.Item1.Contains("::jsonb"), 
                "PostgreSQL doit ajouter ::jsonb pour les colonnes JSON");

            Console.WriteLine($"PostgreSQL JSON : {result.Item1}");
            Console.WriteLine("✅ Test PostgreSQL ::jsonb réussi");
            Console.WriteLine("\n✅ Tous les tests QueryDataset réussis");
        }

        /// <summary>
        /// Tests des nouvelles fonctionnalités : CTE (WITH), Subqueries, et méthodes Raw
        /// </summary>
        [TestMethod]
        public void AdvancedFeatures_Tests()
        {
            Console.WriteLine("\n=== Tests des Fonctionnalités Avancées ===");

            // ==================== PARTIE 1 : CTE (WITH Clause) ====================
            Console.WriteLine("\n--- Test 1: CTE Simple ---");
            
            // Créer une sous-requête CTE pour calculer le prix moyen
            var avgPriceQuery = CreateQueryBuilder()
                .Select("AVG(prix) as prix_moyen")
                .From("produits")
                .Where("actif", "=", true);

            // Utiliser la CTE dans la requête principale
            var cteResult = CreateQueryBuilder()
                .With("prix_moyens", avgPriceQuery)
                .Select("p.nom", "p.prix", "pm.prix_moyen")
                .From("produits p")
                .JoinRaw("CROSS JOIN prix_moyens pm")
                .WhereRaw("p.prix > pm.prix_moyen")
                .Build();

            Console.WriteLine($"SQL CTE: {cteResult.Item1}");
            Assert.IsTrue(cteResult.Item1.Contains("WITH"));
            Assert.IsTrue(cteResult.Item1.Contains("prix_moyens AS"));
            Assert.IsTrue(cteResult.Item1.Contains("AVG(prix)"));
            Assert.IsTrue(cteResult.Item1.Contains("CROSS JOIN prix_moyens"));

            // ==================== PARTIE 2 : WHERE avec Subquery ====================
            Console.WriteLine("\n--- Test 2: WHERE Subquery ---");
            
            // Sous-requête pour trouver le prix maximum
            var maxPriceSubquery = CreateQueryBuilder()
                .Select("MAX(prix)")
                .From("produits")
                .Where("categorie", "=", "electronique");

            // Requête principale utilisant la sous-requête
            var subqueryResult = CreateQueryBuilder()
                .Select()
                .From("produits")
                .WhereSubquery("prix", ">=", maxPriceSubquery)
                .Build();

            Console.WriteLine($"SQL Subquery: {subqueryResult.Item1}");
            Assert.IsTrue(subqueryResult.Item1.Contains("WHERE"));
            Assert.IsTrue(subqueryResult.Item1.Contains("prix >="));
            Assert.IsTrue(subqueryResult.Item1.Contains("(SELECT MAX(prix)"));
            Assert.IsTrue(subqueryResult.Item1.Contains("categorie"));
            
            // Vérifier que les paramètres de la sous-requête sont préfixés
            Assert.IsTrue(subqueryResult.Item2.Any(p => p.Key.StartsWith("sub_")));

            // ==================== PARTIE 3 : WHERE EXISTS ====================
            Console.WriteLine("\n--- Test 3: WHERE EXISTS ---");
            
            // Sous-requête EXISTS
            var existsSubquery = CreateQueryBuilder()
                .Select("1")
                .From("commandes c")
                .WhereRaw("c.utilisateur_id = u.id")
                .Where("c.total", ">", 1000);

            // Requête principale avec EXISTS
            var existsResult = CreateQueryBuilder()
                .Select()
                .From("utilisateurs u")
                .WhereExists(existsSubquery)
                .Build();

            Console.WriteLine($"SQL EXISTS: {existsResult.Item1}");
            Assert.IsTrue(existsResult.Item1.Contains("EXISTS"));
            Assert.IsTrue(existsResult.Item1.Contains("c.utilisateur_id = u.id"));

            // ==================== PARTIE 4 : Méthodes Raw ====================
            Console.WriteLine("\n--- Test 4: SelectRaw ---");
            
            var selectRawResult = CreateQueryBuilder()
                .Select("nom", "email")
                .SelectRaw("COUNT(*) OVER() as total_lignes")
                .SelectRaw("ROW_NUMBER() OVER(ORDER BY id) as rang")
                .From("utilisateurs")
                .Build();

            Console.WriteLine($"SQL SelectRaw: {selectRawResult.Item1}");
            Assert.IsTrue(selectRawResult.Item1.Contains("COUNT(*) OVER()"));
            Assert.IsTrue(selectRawResult.Item1.Contains("ROW_NUMBER()"));

            Console.WriteLine("\n--- Test 5: WhereRaw ---");
            
            var whereRawParams = new Dictionary<string, object> 
            { 
                ["annee"] = 2024,
                ["mois"] = 10
            };
            
            var whereRawResult = CreateQueryBuilder()
                .Select()
                .From("commandes")
                .WhereRaw("YEAR(date_creation) = @annee", whereRawParams)
                .WhereRaw("MONTH(date_creation) = @mois", whereRawParams)
                .Build();

            Console.WriteLine($"SQL WhereRaw: {whereRawResult.Item1}");
            Assert.IsTrue(whereRawResult.Item1.Contains("YEAR(date_creation)"));
            Assert.IsTrue(whereRawResult.Item1.Contains("MONTH(date_creation)"));
            Assert.IsTrue(whereRawResult.Item2.ContainsKey("annee"));
            Assert.IsTrue(whereRawResult.Item2.ContainsKey("mois"));
            Assert.AreEqual(2024, whereRawResult.Item2["annee"]);

            Console.WriteLine("\n--- Test 6: JoinRaw ---");
            
            var joinRawParams = new Dictionary<string, object> { ["min_total"] = 500 };
            var joinRawResult = CreateQueryBuilder()
                .Select("u.nom", "COUNT(c.id) as nb_commandes")
                .From("utilisateurs u")
                .JoinRaw("LEFT JOIN commandes c ON c.utilisateur_id = u.id AND c.total > @min_total", joinRawParams)
                .GroupBy("u.nom")
                .Build();

            Console.WriteLine($"SQL JoinRaw: {joinRawResult.Item1}");
            Assert.IsTrue(joinRawResult.Item1.Contains("LEFT JOIN commandes"));
            Assert.IsTrue(joinRawResult.Item1.Contains("c.total > @min_total"));
            Assert.IsTrue(joinRawResult.Item2.ContainsKey("min_total"));

            Console.WriteLine("\n--- Test 7: HavingRaw ---");
            
            var havingRawParams = new Dictionary<string, object> { ["min_count"] = 5 };
            var havingRawResult = CreateQueryBuilder()
                .Select("categorie", "COUNT(*) as nb_produits")
                .From("produits")
                .GroupBy("categorie")
                .HavingRaw("COUNT(*) > @min_count", havingRawParams)
                .Build();

            Console.WriteLine($"SQL HavingRaw: {havingRawResult.Item1}");
            Assert.IsTrue(havingRawResult.Item1.Contains("HAVING"));
            Assert.IsTrue(havingRawResult.Item1.Contains("COUNT(*) > @min_count"));
            Assert.IsTrue(havingRawResult.Item2.ContainsKey("min_count"));

            // ==================== PARTIE 5 : CTE Multiple ====================
            Console.WriteLine("\n--- Test 8: Multiple CTEs ---");
            
            var cte1 = CreateQueryBuilder()
                .Select("categorie", "AVG(prix) as prix_moyen")
                .From("produits")
                .GroupBy("categorie");

            var cte2 = CreateQueryBuilder()
                .Select("categorie", "COUNT(*) as nb_produits")
                .From("produits")
                .GroupBy("categorie");

            var multipleCteResult = CreateQueryBuilder()
                .With("prix_par_categorie", cte1)
                .With("count_par_categorie", cte2)
                .Select("ppc.categorie", "ppc.prix_moyen", "cpc.nb_produits")
                .From("prix_par_categorie ppc")
                .JoinRaw("INNER JOIN count_par_categorie cpc ON cpc.categorie = ppc.categorie")
                .Build();

            Console.WriteLine($"SQL Multiple CTEs: {multipleCteResult.Item1}");
            Assert.IsTrue(multipleCteResult.Item1.Contains("WITH prix_par_categorie AS"));
            Assert.IsTrue(multipleCteResult.Item1.Contains("count_par_categorie AS"));

            Console.WriteLine("\n✅ Tous les tests AdvancedFeatures réussis");
        }

        /// <summary>
        /// Tests des nouvelles fonctionnalités : SelectDistinct et WindowFunction
        /// </summary>
        [TestMethod]
        public void NewFeaturesTest()
        {
            Console.WriteLine("========== TESTS NOUVELLES FONCTIONNALITÉS ==========\n");

            // ==================== PARTIE 1 : SELECT DISTINCT ====================
            Console.WriteLine("--- Test 1: SelectDistinct simple ---");
            
            var distinctSimple = CreateQueryBuilder()
                .SelectDistinct("categorie")
                .From("produits")
                .Build();

            Console.WriteLine($"SQL: {distinctSimple.Item1}");
            Assert.IsTrue(distinctSimple.Item1.Contains("SELECT DISTINCT"));
            Assert.IsTrue(distinctSimple.Item1.Contains("categorie"));

            Console.WriteLine("\n--- Test 2: SelectDistinct avec plusieurs colonnes ---");
            
            var distinctMultiple = CreateQueryBuilder()
                .SelectDistinct("categorie", "marque")
                .From("produits")
                .Where("actif", "=", true)
                .OrderBy("categorie ASC", "marque ASC")
                .Build();

            Console.WriteLine($"SQL: {distinctMultiple.Item1}");
            Assert.IsTrue(distinctMultiple.Item1.Contains("SELECT DISTINCT"));
            Assert.IsTrue(distinctMultiple.Item1.Contains("categorie"));
            Assert.IsTrue(distinctMultiple.Item1.Contains("marque"));
            Assert.IsTrue(distinctMultiple.Item1.Contains("WHERE"));
            Assert.AreEqual(1, distinctMultiple.Item2.Count);

            Console.WriteLine("\n--- Test 3: SelectDistinct avec WHERE IN ---");
            
            var distinctWithIn = CreateQueryBuilder()
                .SelectDistinct("email")
                .From("utilisateurs")
                .WhereIn("role", new List<object> { "admin", "manager", "user" })
                .Build();

            Console.WriteLine($"SQL: {distinctWithIn.Item1}");
            Assert.IsTrue(distinctWithIn.Item1.Contains("SELECT DISTINCT"));
            Assert.IsTrue(distinctWithIn.Item1.Contains("email"));
            Assert.IsTrue(distinctWithIn.Item1.Contains("IN"));

            // ==================== PARTIE 2 : WINDOW FUNCTIONS ====================
            Console.WriteLine("\n--- Test 4: WindowFunction - ROW_NUMBER() PARTITION BY ---");
            
            var windowRowNumber = CreateQueryBuilder()
                .Select("nom", "categorie", "prix")
                .SelectWindowFunction("ROW_NUMBER()", "rang", "categorie", "prix DESC")
                .From("produits")
                .Build();

            Console.WriteLine($"SQL: {windowRowNumber.Item1}");
            Assert.IsTrue(windowRowNumber.Item1.Contains("ROW_NUMBER()"));
            Assert.IsTrue(windowRowNumber.Item1.Contains("OVER"));
            Assert.IsTrue(windowRowNumber.Item1.Contains("PARTITION BY categorie"));
            Assert.IsTrue(windowRowNumber.Item1.Contains("ORDER BY prix DESC"));
            Assert.IsTrue(windowRowNumber.Item1.Contains("AS rang"));

            Console.WriteLine("\n--- Test 5: WindowFunction - RANK() avec ORDER BY seulement ---");
            
            var windowRank = CreateQueryBuilder()
                .Select("nom", "score")
                .SelectWindowFunction("RANK()", "classement", null, "score DESC")
                .From("joueurs")
                .Build();

            Console.WriteLine($"SQL: {windowRank.Item1}");
            Assert.IsTrue(windowRank.Item1.Contains("RANK()"));
            Assert.IsTrue(windowRank.Item1.Contains("OVER"));
            Assert.IsTrue(windowRank.Item1.Contains("ORDER BY score DESC"));
            Assert.IsTrue(windowRank.Item1.Contains("AS classement"));
            Assert.IsFalse(windowRank.Item1.Contains("PARTITION BY"));

            Console.WriteLine("\n--- Test 6: WindowFunction - SUM() PARTITION BY ---");
            
            var windowSum = CreateQueryBuilder()
                .Select("utilisateur_id", "montant", "date_commande")
                .SelectWindowFunction("SUM(montant)", "total_cumul", "utilisateur_id", "date_commande ASC")
                .From("commandes")
                .Where("statut", "=", "completed")
                .OrderBy("utilisateur_id", "date_commande")
                .Build();

            Console.WriteLine($"SQL: {windowSum.Item1}");
            Assert.IsTrue(windowSum.Item1.Contains("SUM(montant)"));
            Assert.IsTrue(windowSum.Item1.Contains("OVER"));
            Assert.IsTrue(windowSum.Item1.Contains("PARTITION BY utilisateur_id"));
            Assert.IsTrue(windowSum.Item1.Contains("ORDER BY date_commande ASC"));
            Assert.IsTrue(windowSum.Item1.Contains("AS total_cumul"));

            Console.WriteLine("\n--- Test 7: WindowFunction - AVG() ---");
            
            var windowAvg = CreateQueryBuilder()
                .Select("nom", "salaire", "departement")
                .SelectWindowFunction("AVG(salaire)", "salaire_moyen_dept", "departement", null)
                .From("employes")
                .Build();

            Console.WriteLine($"SQL: {windowAvg.Item1}");
            Assert.IsTrue(windowAvg.Item1.Contains("AVG(salaire)"));
            Assert.IsTrue(windowAvg.Item1.Contains("OVER"));
            Assert.IsTrue(windowAvg.Item1.Contains("PARTITION BY departement"));
            Assert.IsTrue(windowAvg.Item1.Contains("AS salaire_moyen_dept"));

            Console.WriteLine("\n--- Test 8: Combinaison SELECT + WindowFunction + DISTINCT ---");
            
            var complexQuery = CreateQueryBuilder()
                .SelectDistinct("categorie")
                .SelectWindowFunction("COUNT(*)", "nb_total", null, null)
                .From("produits")
                .Where("actif", "=", true)
                .Build();

            Console.WriteLine($"SQL: {complexQuery.Item1}");
            Assert.IsTrue(complexQuery.Item1.Contains("SELECT DISTINCT"));
            Assert.IsTrue(complexQuery.Item1.Contains("COUNT(*)"));
            Assert.IsTrue(complexQuery.Item1.Contains("OVER"));

            Console.WriteLine("\n--- Test 9: WindowFunction - DENSE_RANK() ---");
            
            var windowDenseRank = CreateQueryBuilder()
                .Select("nom", "points", "saison")
                .SelectWindowFunction("DENSE_RANK()", "position", "saison", "points DESC, nom ASC")
                .From("classement")
                .Build();

            Console.WriteLine($"SQL: {windowDenseRank.Item1}");
            Assert.IsTrue(windowDenseRank.Item1.Contains("DENSE_RANK()"));
            Assert.IsTrue(windowDenseRank.Item1.Contains("PARTITION BY saison"));
            Assert.IsTrue(windowDenseRank.Item1.Contains("ORDER BY points DESC, nom ASC"));

            Console.WriteLine("\n--- Test 10: WindowFunction - LAG() ---");
            
            var windowLag = CreateQueryBuilder()
                .Select("date_vente", "montant")
                .SelectWindowFunction("LAG(montant, 1, 0)", "montant_precedent", null, "date_vente ASC")
                .From("ventes")
                .Build();


            Console.WriteLine($"SQL: {windowLag.Item1}");
            Assert.IsTrue(windowLag.Item1.Contains("LAG(montant, 1, 0)"));
            Assert.IsTrue(windowLag.Item1.Contains("OVER"));
            Assert.IsTrue(windowLag.Item1.Contains("ORDER BY date_vente ASC"));

            Console.WriteLine("\n✅ Tous les tests NewFeatures réussis");
        }

        /// <summary>
        /// Test de la méthode ApplySearchForm et WhereEquals
        /// </summary>
        [TestMethod]
        public void ApplySearchFormTest()
        {
            Console.WriteLine("\n=== Test ApplySearchForm ===");

            // Classe de test implémentant ISearchForm
            var searchForm = new TestSearchForm
            {
                CurrentPage = 2,
                LimitResults = 25,
                ShowOrderBy = "nom ASC, created_at DESC"
            };

            // Test ApplySearchForm
            var query = CreateQueryBuilder()
                .Select("id", "nom", "email")
                .From("utilisateurs")
                .Where("actif", "=", true)
                .ApplySearchForm(searchForm)
                .Build();

            Console.WriteLine($"SQL: {query.Item1}");
            Assert.IsTrue(query.Item1.Contains("LIMIT"));
            Assert.IsTrue(query.Item1.Contains("OFFSET"));
            Assert.IsTrue(query.Item1.Contains("ORDER BY"));
            Assert.IsTrue(query.Item1.Contains("nom ASC"));

            // Test avec l'extension WithSearchForm
            var query2 = CreateQueryBuilder()
                .Select()
                .From("produits")
                .ApplySearchForm(searchForm)
                .Build();

            Console.WriteLine($"SQL: {query2.Item1}");
            Assert.IsTrue(query2.Item1.Contains("LIMIT"));

            // Test WhereEquals avec dictionnaire
            Console.WriteLine("\n=== Test WhereEquals ===");
            var whereValues = new Dictionary<string, object>
            {
                { "nom", "Dupont" },
                { "actif", true },
                { "age", 30 }
            };

            var query3 = CreateQueryBuilder()
                .Select()
                .From("utilisateurs")
                .WhereEquals(whereValues)
                .Build();

            Console.WriteLine($"SQL: {query3.Item1}");
            Assert.IsTrue(query3.Item1.Contains("WHERE"));
            Assert.AreEqual(3, query3.Item2.Count);
            Assert.IsTrue(query3.Item2.ContainsKey("nom"));
            Assert.IsTrue(query3.Item2.ContainsKey("actif"));
            Assert.IsTrue(query3.Item2.ContainsKey("age"));

            Console.WriteLine("\n✅ Tests ApplySearchForm et WhereEquals réussis");
        }

        /// <summary>
        /// Classe de test implémentant ISearchForm
        /// </summary>
        private class TestSearchForm : DATA.BASICS.ISearchForm
        {
            public int CurrentPage { get; set; }
            public int LimitResults { get; set; }
            public string ShowOrderBy { get; set; }
        }
    }
}

