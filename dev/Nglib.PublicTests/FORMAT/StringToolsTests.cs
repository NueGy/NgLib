using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.FORMAT;

namespace Nglib.Tests.FORMAT
{
    /// <summary>
    /// Tests complets pour StringTools - Une méthode unique teste toutes les fonctionnalités
    /// </summary>
    [TestClass]
    public class StringToolsTests
    {
        /// <summary>
        /// Test complet de toutes les méthodes StringTools
        /// </summary>
        [TestMethod]
        public void StringTools_AllMethods_ComprehensiveTest()
        {
            // === RandomString Tests ===
            var randomStr = StringTools.RandomString(10);
            Assert.AreEqual(10, randomStr.Length, "RandomString: Longueur incorrecte");
            Assert.IsTrue(randomStr.All(c => "abcdefghijklmnopqrstuvwxyz123456789".Contains(c)), "RandomString: Caractères invalides");
            
            var customRandom = StringTools.RandomString(5, "ABC");
            Assert.AreEqual(5, customRandom.Length, "RandomString custom: Longueur incorrecte");
            Assert.IsTrue(customRandom.All(c => "ABC".Contains(c)), "RandomString custom: Caractères invalides");

            // === RandomGuid32 Tests ===
            var guid1 = StringTools.RandomGuid32();
            var guid2 = StringTools.RandomGuid32();
            Assert.AreEqual(32, guid1?.Length, "RandomGuid32: Longueur incorrecte");
            Assert.AreNotEqual(guid1, guid2, "RandomGuid32: GUIDs identiques");

            // === IsAlphaNumeric Tests ===
            Assert.IsTrue(StringTools.IsAlphaNumeric("ABC123"), "IsAlphaNumeric: Doit accepter alphanumérique");
            Assert.IsFalse(StringTools.IsAlphaNumeric("ABC-123"), "IsAlphaNumeric: Ne doit pas accepter tiret");
            Assert.IsFalse(StringTools.IsAlphaNumeric(""), "IsAlphaNumeric: String vide doit retourner false");
            Assert.IsFalse(StringTools.IsAlphaNumeric(null), "IsAlphaNumeric: Null doit retourner false");

            // === SanitizeKey Tests ===
            Assert.AreEqual("HELLO123", KeyTools.SanitizeKey("héllo@123!"), "SanitizeKey: Accents et caractères spéciaux");
            Assert.AreEqual("TEST_KEY.VERSION", KeyTools.SanitizeKey("test_key.version"), "SanitizeKey: Points et underscores");
            Assert.AreEqual("", KeyTools.SanitizeKey("@#$%"), "SanitizeKey: Que des caractères interdits");
            Assert.IsNull(KeyTools.SanitizeKey(null), "SanitizeKey: Null input");

            // === FilterCharacters Tests ===
            Assert.AreEqual("abc123", StringTools.FilterCharacters("a!b@c#1$2%3", "abc123"), "FilterCharacters: Filtrage custom");
            Assert.IsNull(StringTools.FilterCharacters(null), "FilterCharacters: Null input");

            // === Limit Tests ===
            Assert.AreEqual("Hello", StringTools.Limit("Hello World", 5), "Limit: Troncature");
            Assert.AreEqual("Hi", StringTools.Limit("Hi", 10), "Limit: String plus courte");
            Assert.IsNull(StringTools.Limit(null, 5), "Limit: Null input");

            // === SubstringSafe Tests ===
            Assert.AreEqual("World", StringTools.SubstringSafe("Hello World", 6), "SubstringSafe: Position valide");
            Assert.AreEqual("", StringTools.SubstringSafe("Hello", 10), "SubstringSafe: Position trop grande");
            Assert.AreEqual("", StringTools.SubstringSafe(null, 0), "SubstringSafe: Null input");
            
            Assert.AreEqual("Wor", StringTools.SubstringSafe("Hello World", 6, 3), "SubstringSafe avec longueur");
            Assert.AreEqual("World", StringTools.SubstringSafe("Hello World", 6, 100), "SubstringSafe: Longueur trop grande");

            // === ReplaceDiacritics Tests ===
            Assert.AreEqual("aeiou", StringTools.ReplaceDiacritics("àéîôù"), "ReplaceDiacritics: Accents basiques");
            Assert.AreEqual("noel cafe", StringTools.ReplaceDiacritics("noël café"), "ReplaceDiacritics: Mélange");
            Assert.AreEqual("", StringTools.ReplaceDiacritics(""), "ReplaceDiacritics: String vide");
            Assert.IsNull(StringTools.ReplaceDiacritics(null), "ReplaceDiacritics: Null input");

            // === CleanString Tests ===
            Assert.AreEqual("Hello World", StringTools.CleanString("Hello\tWorld\r\n"), "CleanString: Whitespace");
            Assert.AreEqual("Hello World", StringTools.CleanString("Hello@#$World"), "CleanString: Caractères spéciaux");
            Assert.AreEqual("", StringTools.CleanString("   \t\r\n  "), "CleanString: Que des whitespaces");
            Assert.IsNull(StringTools.CleanString(null), "CleanString: Null input");

            // === ReplaceChar Tests ===
            Assert.AreEqual("HeLlo", "Hello".ReplaceChar(2, 'L'), "ReplaceChar: Remplacement");
            Assert.AreEqual("Hi X", "Hi".ReplaceChar(3, 'X'), "ReplaceChar: Extension avec espace");

            // === SplitTag Tests ===
            var tags1 = StringTools.SplitTag("tag1;tag2;tag3");
            Assert.AreEqual(3, tags1.Length, "SplitTag: Nombre de tags");
            Assert.AreEqual("TAG1", tags1[0], "SplitTag: Uppercase");
            
            var tags2 = StringTools.SplitTag("tag1;;tag3", false); // avec élément vide
            Assert.IsTrue(tags2.Length <= 3, "SplitTag: Gestion éléments vides");
            
            Assert.IsNull(StringTools.SplitTag(null), "SplitTag: Null input");

            // === SplitEncapsuled Tests ===
            var encap = StringTools.SplitEncapsuled("text{value1}more{value2}end", "{", "}");
            Assert.AreEqual(2, encap.Length, "SplitEncapsuled: Nombre d'éléments");
            Assert.AreEqual("{value1}", encap[0], "SplitEncapsuled: Premier élément");
            Assert.AreEqual("{value2}", encap[1], "SplitEncapsuled: Deuxième élément");
            
            var empty = StringTools.SplitEncapsuled("", "{", "}");
            Assert.AreEqual(0, empty.Length, "SplitEncapsuled: String vide");

        }
         
    }
}