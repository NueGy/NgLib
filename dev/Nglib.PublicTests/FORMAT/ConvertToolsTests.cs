using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.FORMAT;
using System;

namespace Nglib.PublicTests.FORMAT
{
    /// <summary>
    /// Tests unitaires pour ConvertTools - Une méthode unique teste toutes les fonctionnalités
    /// </summary>
    [TestClass]
    public class ConvertToolsTests
    {
        /// <summary>
        /// Test complet de toutes les méthodes ConvertTools
        /// </summary>
        [TestMethod]
        public void ConvertTools_AllMethods_FullyTest()
        {
            // === ToBoolean Tests ===
            
            // Tests bool direct
            Assert.IsTrue(ConvertTools.ToBoolean(true), "ToBoolean: bool true");
            Assert.IsFalse(ConvertTools.ToBoolean(false), "ToBoolean: bool false");
            
            // Tests int
            Assert.IsTrue(ConvertTools.ToBoolean(1), "ToBoolean: int 1");
            Assert.IsTrue(ConvertTools.ToBoolean(5), "ToBoolean: int positif");
            Assert.IsFalse(ConvertTools.ToBoolean(0), "ToBoolean: int 0");
            Assert.IsFalse(ConvertTools.ToBoolean(-1), "ToBoolean: int négatif");
            
            // Tests string standards
            Assert.IsTrue(ConvertTools.ToBoolean("true"), "ToBoolean: string true");
            Assert.IsFalse(ConvertTools.ToBoolean("false"), "ToBoolean: string false");
            Assert.IsTrue(ConvertTools.ToBoolean("TRUE"), "ToBoolean: string TRUE");
            Assert.IsFalse(ConvertTools.ToBoolean("FALSE"), "ToBoolean: string FALSE");
            
            // Tests string spéciaux
            Assert.IsTrue(ConvertTools.ToBoolean("on"), "ToBoolean: string on");
            Assert.IsTrue(ConvertTools.ToBoolean("ON"), "ToBoolean: string ON");
            Assert.IsTrue(ConvertTools.ToBoolean("yes"), "ToBoolean: string yes");
            Assert.IsTrue(ConvertTools.ToBoolean("YES"), "ToBoolean: string YES");
            Assert.IsTrue(ConvertTools.ToBoolean("1"), "ToBoolean: string 1");
            
            // Tests cas limites
            Assert.IsFalse(ConvertTools.ToBoolean(null), "ToBoolean: null");
            Assert.IsFalse(ConvertTools.ToBoolean(""), "ToBoolean: string vide");
            Assert.IsFalse(ConvertTools.ToBoolean("   "), "ToBoolean: string espaces");
            Assert.IsFalse(ConvertTools.ToBoolean("no"), "ToBoolean: string no");
            Assert.IsFalse(ConvertTools.ToBoolean("off"), "ToBoolean: string off");
            Assert.IsFalse(ConvertTools.ToBoolean("2"), "ToBoolean: string 2");
            
            // Tests safe mode
            Assert.IsFalse(ConvertTools.ToBoolean("invalid", true), "ToBoolean safe: valeur invalide");
            Assert.IsFalse(ConvertTools.ToBoolean(new object(), true), "ToBoolean safe: objet complexe");
            
            // Test exception en mode non-safe
            try
            {
                ConvertTools.ToBoolean(new object());
                Assert.Fail("ToBoolean: Exception attendue pour objet complexe");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("ToBoolean"), "ToBoolean: Message d'erreur avec nom méthode");
            }

            // === ToDateTime Tests ===
            
            // Tests DateTime direct
            var testDate = new DateTime(2023, 10, 15, 14, 30, 0);
            Assert.AreEqual(testDate, ConvertTools.ToDateTime(testDate), "ToDateTime: DateTime direct");
            
            // Tests string classique
            Assert.AreEqual(new DateTime(2023, 10, 15), ConvertTools.ToDateTime("2023-10-15"), "ToDateTime: string standard");
            Assert.AreEqual(new DateTime(2023, 10, 15), ConvertTools.ToDateTime("15/10/2023"), "ToDateTime: string français");
            
            // Tests format 8 caractères (YYYYMMDD) - format spécifique
            Assert.AreEqual(new DateTime(2023, 10, 15), ConvertTools.ToDateTime("20231015"), "ToDateTime: format 8 chars");
            Assert.AreEqual(new DateTime(2024, 1, 1), ConvertTools.ToDateTime("20240101"), "ToDateTime: format 8 chars début année");
            Assert.AreEqual(new DateTime(2024, 12, 31), ConvertTools.ToDateTime("20241231"), "ToDateTime: format 8 chars fin année");
            
            // Tests que le format 8 chars n'est pas utilisé avec séparateurs ou espaces
            var objstr8chars = "20231015";
            var objstrSeparators = "2023-10-15";
            var objstrSpaces = " 20231015 ";
            
            // Le format 8 avec espaces fonctionne car Trim() est appliqué et après Trim ça devient "20231015"
            Assert.AreEqual(new DateTime(2023, 10, 15), ConvertTools.ToDateTime(objstrSpaces), "ToDateTime: format 8 avec espaces (Trim appliqué)");
            
            // Test que les différents formats donnent la même date
            var date8 = ConvertTools.ToDateTime(objstr8chars);
            var dateSep = ConvertTools.ToDateTime(objstrSeparators);
            Assert.AreEqual(date8, dateSep, "ToDateTime: Les deux formats donnent la même date");
            
            // === ToInt Tests ===
            
            // Tests int direct
            Assert.AreEqual(42, ConvertTools.ToInt(42), "ToInt: int direct");
            Assert.AreEqual(-10, ConvertTools.ToInt(-10), "ToInt: int négatif");
            Assert.AreEqual(0, ConvertTools.ToInt(0), "ToInt: int zéro");
            
            // Tests bool
            Assert.AreEqual(1, ConvertTools.ToInt(true), "ToInt: bool true");
            Assert.AreEqual(0, ConvertTools.ToInt(false), "ToInt: bool false");
            
            // Tests string
            Assert.AreEqual(123, ConvertTools.ToInt("123"), "ToInt: string nombre");
            Assert.AreEqual(-456, ConvertTools.ToInt("-456"), "ToInt: string nombre négatif");
            Assert.AreEqual(1, ConvertTools.ToInt("true"), "ToInt: string true");
            Assert.AreEqual(0, ConvertTools.ToInt("false"), "ToInt: string false");
            Assert.AreEqual(1, ConvertTools.ToInt("TRUE"), "ToInt: string TRUE");
            Assert.AreEqual(0, ConvertTools.ToInt("FALSE"), "ToInt: string FALSE");
            Assert.AreEqual(1, ConvertTools.ToInt(" true "), "ToInt: string true avec espaces");
            
            // Tests avec defaultValue
            Assert.AreEqual(99, ConvertTools.ToInt(null, 99), "ToInt: null avec defaultValue");
            Assert.AreEqual(99, ConvertTools.ToInt(DBNull.Value, 99), "ToInt: DBNull avec defaultValue");
            Assert.AreEqual(99, ConvertTools.ToInt("", 99), "ToInt: string vide avec defaultValue");
            Assert.AreEqual(99, ConvertTools.ToInt("   ", 99), "ToInt: string espaces avec defaultValue");
            
            // Tests sans defaultValue (null -> 0 par défaut)
            Assert.AreEqual(0, ConvertTools.ToInt(null), "ToInt: null sans defaultValue");
            Assert.AreEqual(0, ConvertTools.ToInt(DBNull.Value), "ToInt: DBNull sans defaultValue");

            // === ChangeType Tests ===
            
            // Tests avec Type
            Assert.AreEqual(42, ConvertTools.ChangeType("42", typeof(int)), "ChangeType Type: string vers int");
            Assert.AreEqual(true, ConvertTools.ChangeType("yes", typeof(bool)), "ChangeType Type: string vers bool");
            Assert.AreEqual(new DateTime(2023, 10, 15), ConvertTools.ChangeType("20231015", typeof(DateTime)), "ChangeType Type: string vers DateTime");
            
            // Tests avec DBNull
            Assert.AreEqual(0, ConvertTools.ChangeType(DBNull.Value, typeof(int)), "ChangeType: DBNull vers int retourne 0");
            Assert.AreEqual(false, ConvertTools.ChangeType(DBNull.Value, typeof(bool)), "ChangeType: DBNull vers bool retourne false");
            
            // Tests avec nullable
            Assert.IsNull(ConvertTools.ChangeType(null, typeof(int?)), "ChangeType: null vers int nullable");
            Assert.IsNull(ConvertTools.ChangeType(DBNull.Value, typeof(int?)), "ChangeType: DBNull vers int nullable");
            
            // Tests avec string typename
            Assert.AreEqual(42, ConvertTools.ChangeType("42", "int"), "ChangeType string: vers int");
            Assert.AreEqual(true, ConvertTools.ChangeType("on", "bool"), "ChangeType string: vers bool");
            Assert.AreEqual(new DateTime(2023, 10, 15), ConvertTools.ChangeType("20231015", "datetime"), "ChangeType string: vers datetime");
            
            // Test exception type null
            try
            {
                ConvertTools.ChangeType("test", (Type)null);
                Assert.Fail("ChangeType: Exception attendue pour Type null");
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsTrue(ex.Message.Contains("type"), "ChangeType: Message d'erreur correct");
            }

            // === ParseType Tests ===
            
            // Types supportés
            Assert.AreEqual(typeof(string), ConvertTools.ParseType("string"), "ParseType: string");
            Assert.AreEqual(typeof(int), ConvertTools.ParseType("int"), "ParseType: int");
            Assert.AreEqual(typeof(int), ConvertTools.ParseType("numeric"), "ParseType: numeric");
            Assert.AreEqual(typeof(DateTime), ConvertTools.ParseType("datetime"), "ParseType: datetime");
            Assert.AreEqual(typeof(DateTime), ConvertTools.ParseType("date"), "ParseType: date");
            Assert.AreEqual(typeof(double), ConvertTools.ParseType("double"), "ParseType: double");
            Assert.AreEqual(typeof(bool), ConvertTools.ParseType("bool"), "ParseType: bool");
            Assert.AreEqual(typeof(long), ConvertTools.ParseType("long"), "ParseType: long");
            Assert.AreEqual(typeof(char), ConvertTools.ParseType("char"), "ParseType: char");
            Assert.AreEqual(typeof(byte), ConvertTools.ParseType("byte"), "ParseType: byte");
            Assert.AreEqual(typeof(decimal), ConvertTools.ParseType("decimal"), "ParseType: decimal");
            
            // Tests case insensitive
            Assert.AreEqual(typeof(int), ConvertTools.ParseType("INT"), "ParseType: INT majuscule");
            Assert.AreEqual(typeof(bool), ConvertTools.ParseType("BOOL"), "ParseType: BOOL majuscule");
            Assert.AreEqual(typeof(string), ConvertTools.ParseType("STRING"), "ParseType: STRING majuscule");
            
            // Tests avec espaces
            Assert.AreEqual(typeof(int), ConvertTools.ParseType(" int "), "ParseType: int avec espaces");
            Assert.AreEqual(typeof(bool), ConvertTools.ParseType(" bool "), "ParseType: bool avec espaces");
            
            // Tests cas limites
            Assert.IsNull(ConvertTools.ParseType(null), "ParseType: null");
            Assert.IsNull(ConvertTools.ParseType(""), "ParseType: string vide");
            Assert.IsNull(ConvertTools.ParseType("   "), "ParseType: string espaces");
            Assert.IsNull(ConvertTools.ParseType("invalidtype"), "ParseType: type non supporté");
            Assert.IsNull(ConvertTools.ParseType("object"), "ParseType: type non supporté object");
        }
    }
}