using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.CONNECTOR.QUERYBUILDER;
using System;

namespace Nglib.PublicTests.DATA.CONNECTOR
{
    /// <summary>
    /// Tests pour les validations améliorées du QueryBuilder
    /// </summary>
    [TestClass]
    public class QueryBuilderValidationTests
    {
        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQuery_SelectBasic_ShouldReturnTrue()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Select("id", "name");

            // Act
            bool isValid = qb.ValidateQuery();

            // Assert
            Assert.IsTrue(isValid, "SELECT simple devrait être valide");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_DeleteWithoutWhere_ShouldThrow()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Delete();

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("DELETE requires WHERE clause"), 
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_UpdateWithoutWhere_ShouldThrow()
        {
            // Arrange
            var values = new System.Collections.Generic.Dictionary<string, object>
            {
                { "name", "John" }
            };

            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Update(values);

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("UPDATE requires WHERE clause"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_UpdateWithoutValues_ShouldThrow()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Update(new System.Collections.Generic.Dictionary<string, object>())
                .Where("id", "=", 1);

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("UPDATE requires values"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_InsertWithoutValues_ShouldThrow()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Insert(new System.Collections.Generic.Dictionary<string, object>());

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("INSERT requires values"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_HavingWithoutGroupBy_ShouldThrow()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("orders")
                .Select("customer_id", "SUM(amount) as total")
                .Having("SUM(amount) > 1000");

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("HAVING clause requires GROUP BY"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_DistinctInUpdate_ShouldThrow()
        {
            // Arrange
            var values = new System.Collections.Generic.Dictionary<string, object>
            {
                { "name", "John" }
            };

            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .SelectDistinct("name")  // Incorrect : DISTINCT avec UPDATE
                .Update(values)
                .Where("id", "=", 1);

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("DISTINCT is only valid for SELECT"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_LimitInDelete_ShouldThrow()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Delete()
                .Where("status", "=", "inactive")
                .Limit(10); // LIMIT dans DELETE n'est pas standard

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("LIMIT/OFFSET is only valid for SELECT"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQueryOrThrow_WithoutTableName_ShouldThrow()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .Select("id", "name");
            // Pas de From()

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => qb.ValidateQueryOrThrow());
            Assert.IsTrue(ex.Message.Contains("TableName is required"),
                $"Message d'erreur inattendu: {ex.Message}");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQuery_ValidSelectWithGroupByAndHaving_ShouldReturnTrue()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("orders")
                .Select("customer_id", "SUM(amount) as total")
                .GroupBy("customer_id")
                .Having("SUM(amount) > 1000");

            // Act
            bool isValid = qb.ValidateQuery();

            // Assert
            Assert.IsTrue(isValid, "SELECT avec GROUP BY et HAVING devrait être valide");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQuery_ValidUpdateWithWhere_ShouldReturnTrue()
        {
            // Arrange
            var values = new System.Collections.Generic.Dictionary<string, object>
            {
                { "name", "John" },
                { "age", 30 }
            };

            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Update(values)
                .Where("id", "=", 1);

            // Act
            bool isValid = qb.ValidateQuery();

            // Assert
            Assert.IsTrue(isValid, "UPDATE avec WHERE devrait être valide");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQuery_ValidDeleteWithWhere_ShouldReturnTrue()
        {
            // Arrange
            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Delete()
                .Where("status", "=", "deleted");

            // Act
            bool isValid = qb.ValidateQuery();

            // Assert
            Assert.IsTrue(isValid, "DELETE avec WHERE devrait être valide");
        }

        [TestMethod]
        [TestCategory("QueryBuilder")]
        public void ValidateQuery_ValidInsertWithValues_ShouldReturnTrue()
        {
            // Arrange
            var values = new System.Collections.Generic.Dictionary<string, object>
            {
                { "name", "John" },
                { "email", "john@example.com" }
            };

            var qb = QueryBuilderTools.CreateQueryBuilder("postgresql")
                .From("users")
                .Insert(values);

            // Act
            bool isValid = qb.ValidateQuery();

            // Assert
            Assert.IsTrue(isValid, "INSERT avec valeurs devrait être valide");
        }
    }
}
