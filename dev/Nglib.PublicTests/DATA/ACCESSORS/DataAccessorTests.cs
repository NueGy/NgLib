using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.ACCESSORS
{
    [TestClass()]
    public class DataAccessorTests
    {


        [TestMethod()]
        public void DataAccessorMultiTest()
        {
            var demo = new Nglib.DATA.COLLECTIONS.DictionaryData();
            demo.SetData("test", "test", DataAccessorOptionEnum.Default); //Ajout d'une valeur standard
            demo.SetObject("test2", 123); //Ajout simplifié d'une valeur (méthode d'extension)
            demo.SetObject("testdate", "20/04/2024");  // Format français dd/MM/yyyy
            demo.SetObject("testbool", true);  

            Assert.AreEqual(demo.GetData("test", DataAccessorOptionEnum.Default), "test"); //Obtention de la valeur standard
            Assert.AreEqual(demo.GetString("test"), "test");//Obtention simplifié de la valeur (méthode d'extension)
            Assert.AreEqual(demo.GetObject("test2"), 123); //Obtention simplifié de la valeur (méthode d'extension)
            Assert.AreEqual(demo.GetInt("test2"),123); //Obtention simplifié de la valeur (méthode d'extension)
            Assert.AreEqual(demo.GetDateTime("testdate"), new DateTime(2024, 4, 20));  
            Assert.AreEqual(demo.GetBoolean("testbool"), true);

            Assert.IsTrue(demo.ListFieldsKeys().Length>3); //Liste des champs
        }


        [TestMethod()]
        public void DataAccessorToolsCopyToTest()
        {
            var demo1 = new Nglib.DATA.COLLECTIONS.DictionaryData();
            demo1.SetObject("test2", 123);
            demo1.SetObject("testdate", "2024-04-20"); // Format ISO universel

            var demo2 = new Nglib.DATA.COLLECTIONS.DictionaryData();
            DataAccessorTools.CopyTo(demo1, demo2);
            Assert.AreEqual(demo2.GetInt("test2"), 123);
            Assert.AreEqual(demo2.GetDateTime("testdate"), new DateTime(2024, 4, 20));
        }












        [TestMethod()]
        public void GetEnumDefaultValueTest()
        {
            // Test GetEnumDefaultValue method
            var defaultValue = DataAccessorTools.GetEnumDefaultValue<DataAccessorOptionEnum>();
            Assert.AreEqual(DataAccessorOptionEnum.None, defaultValue);
        }

        [TestMethod()]
        public void DataAccessorExtensionsFullyTest()
        {
            // Test exhaustif de toutes les méthodes d'extension avec DictionaryData
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            
            // === Tests types de base ===
            // String
            data.SetObject("testString", "Hello World");
            data.SetObject("testStringNull", null);
            data.SetObject("testStringEmpty", "");
            Assert.AreEqual("Hello World", data.GetString("testString"));
            Assert.AreEqual("", data.GetString("testStringNull")); // Safe mode, jamais null
            Assert.AreEqual("", data.GetString("testStringEmpty"));
            Assert.AreEqual("Hello World", data.GetValue<string>("testString", DataAccessorOptionEnum.Safe));
            Assert.IsNull(data.GetValue<string>("testStringNull"));
            
            // Int
            data.SetObject("testInt", 123);
            data.SetObject("testIntString", "456");
            data.SetObject("testIntNull", null);
            Assert.AreEqual(123, data.GetInt("testInt"));
            Assert.AreEqual(0, data.GetInt("testIntNull")); // Default value
            Assert.AreEqual(999, data.GetValue<int>("testIntMissing", 999, DataAccessorOptionEnum.Safe)); // Custom default
            Assert.IsNull(data.GetValue<int?>("testIntNull"));
            
            // Long
            data.SetObject("testLong", 123456789L);
            Assert.AreEqual(123456789L, data.GetValue<long>("testLong"));
            Assert.AreEqual(0L, data.GetValue<long>("testLongNull"));
            
            // Double
            data.SetObject("testDouble", 123.45);
            data.SetObject("testDoubleString", "123.45"); // InvariantCulture utilise le point comme séparateur décimal
            Assert.AreEqual(123.45, data.GetDouble("testDouble"), 0.01);
            Assert.AreEqual(123.45, data.GetDouble("testDoubleString"), 0.01);
            Assert.AreEqual(0.0, data.GetDouble("testDoubleNull"), 0.01);
            
            // Boolean
            data.SetObject("testBoolTrue", true);
            data.SetObject("testBoolFalse", false);
            data.SetObject("testBoolString", "true");
            data.SetObject("testBoolString2", "false");
            Assert.AreEqual(true, data.GetBoolean("testBoolTrue"));
            Assert.AreEqual(false, data.GetBoolean("testBoolFalse"));
            Assert.AreEqual(true, data.GetBoolean("testBoolString"));
            Assert.AreEqual(false, data.GetBoolean("testBoolString2"));
            Assert.AreEqual(false, data.GetBoolean("testBoolNull"));
            
            // DateTime
            var testDate = new DateTime(2024, 4, 20, 15, 30, 45);
            data.SetObject("testDateTime", testDate);
            data.SetObject("testDateTimeString", "2024-04-20 15:30:45");
            Assert.AreEqual(testDate, data.GetDateTime("testDateTime"));
            Assert.AreEqual(new DateTime(2024, 4, 20, 15, 30, 45), data.GetDateTime("testDateTimeString"));
            Assert.AreEqual(new DateTime(), data.GetDateTime("testDateTimeNull"));
            
            // Enum
            data.SetObject("testEnum", DataAccessorOptionEnum.Safe);
            data.SetObject("testEnumString", "Required"); // CORRECTION: Required au lieu de Nullable
            data.SetObject("testEnumInt", 2); // Safe = 2
            Assert.AreEqual(DataAccessorOptionEnum.Safe, data.GetEnum<DataAccessorOptionEnum>("testEnum", DataAccessorOptionEnum.None));
            Assert.AreEqual(DataAccessorOptionEnum.Required, data.GetEnum<DataAccessorOptionEnum>("testEnumString", DataAccessorOptionEnum.None));
            Assert.AreEqual(DataAccessorOptionEnum.Safe, data.GetEnum<DataAccessorOptionEnum>("testEnumInt", DataAccessorOptionEnum.None));
            Assert.AreEqual(DataAccessorOptionEnum.None, data.GetEnum<DataAccessorOptionEnum>("testEnumNull", DataAccessorOptionEnum.None));
            
            // === Tests arrays ===
            data.SetObject("testStringArray", new string[] { "a", "b", "c" });
            data.SetObject("testIntArray", new int[] { 1, 2, 3 });
            data.SetObject("testLongArray", new long[] { 100L, 200L, 300L });
            data.SetObject("testDoubleArray", new double[] { 1.1, 2.2, 3.3 });
            data.SetObject("testBoolArray", new bool[] { true, false, true });
            
            var stringArray = data.GetValue<string[]>("testStringArray");
            var intArray = data.GetValue<int[]>("testIntArray");
            var longArray = data.GetValue<long[]>("testLongArray");
            var doubleArray = data.GetValue<double[]>("testDoubleArray");
            var boolArray = data.GetValue<bool[]>("testBoolArray");
            
            Assert.IsNotNull(stringArray);
            Assert.AreEqual(3, stringArray.Length);
            Assert.AreEqual("a", stringArray[0]);
            Assert.IsNotNull(intArray);
            Assert.AreEqual(3, intArray.Length);
            Assert.AreEqual(1, intArray[0]);
            Assert.IsNotNull(boolArray);
            Assert.AreEqual(3, boolArray.Length);
            Assert.AreEqual(true, boolArray[0]);
            
            // === Tests options ===
            // Test Safe mode - pas d'exception
            Assert.AreEqual("", data.GetString("nonexistent"));
            Assert.AreEqual(0, data.GetInt("nonexistent"));
            
 
            
            // Test NotReplace - Maintenant corrigé dans DictionaryData
            data.SetObject("testNotReplace", "original");
            data.SetData("testNotReplace", "modified", DataAccessorOptionEnum.NotReplace);
            Assert.AreEqual("original", data.GetString("testNotReplace")); // Pas modifié grâce à NotReplace
            
            // === Tests dictionnaires ===
            var dictValues = data.ToDictionaryValues();
            var dictStrings = data.ToDictionaryString();
            Assert.IsTrue(dictValues.Count > 10);
            Assert.IsTrue(dictStrings.Count > 10);
            Assert.AreEqual("Hello World", dictStrings["testString"]);
            
            var newData = new Nglib.DATA.COLLECTIONS.DictionaryData();
            newData.FromDictionaryValues(dictValues);
            Assert.AreEqual("Hello World", newData.GetString("testString"));
            
            // === Tests mapping objets ===
            var testObj = new { Name = "Test", Value = 42, Active = true };
            var objData = new Nglib.DATA.COLLECTIONS.DictionaryData();
            objData.FromReflectionProperties(testObj);
            Assert.AreEqual("Test", objData.GetString("Name"));
            Assert.AreEqual(42, objData.GetInt("Value"));
            Assert.AreEqual(true, objData.GetBoolean("Active"));
            
            // === Tests case-insensitive ===
            data.SetObject("CaseSensitive", "test");
            Assert.AreEqual("test", data.GetString("casesensitive"));
            Assert.AreEqual("test", data.GetString("CASESENSITIVE"));
            Assert.AreEqual("test", data.GetString("CaseSensitive"));
            
            // === Tests ListFieldsKeys ===
            var keys = data.ListFieldsKeys();
            Assert.IsTrue(keys.Length > 15);
            Assert.IsTrue(keys.Contains("testString"));
        }

        [TestMethod()]
        public void DataAccessorToolsAndUtilsFullyTest()
        {
            // Test exhaustif des outils et méthodes utilitaires
            var sourceData = new Nglib.DATA.COLLECTIONS.DictionaryData();
            var targetData = new Nglib.DATA.COLLECTIONS.DictionaryData();
            
            // === Tests DataAccessorTools.CopyTo ===
            sourceData.SetObject("field1", "value1");
            sourceData.SetObject("field2", 123);
            sourceData.SetObject("field3", true);
            sourceData.SetObject("field4", new DateTime(2024, 1, 1));
            sourceData.SetObject("field5", DataAccessorOptionEnum.Safe);
            
            DataAccessorTools.CopyTo(sourceData, targetData);
            
            Assert.AreEqual("value1", targetData.GetString("field1"));
            Assert.AreEqual(123, targetData.GetInt("field2"));
            Assert.AreEqual(true, targetData.GetBoolean("field3"));
            Assert.AreEqual(new DateTime(2024, 1, 1), targetData.GetDateTime("field4"));
            Assert.AreEqual(DataAccessorOptionEnum.Safe, targetData.GetEnum<DataAccessorOptionEnum>("field5", DataAccessorOptionEnum.None));
            
            // === Tests GetEnumDefaultValue ===
            var defaultEnum = DataAccessorTools.GetEnumDefaultValue<DataAccessorOptionEnum>();
            Assert.AreEqual(DataAccessorOptionEnum.None, defaultEnum);
            
            // === Tests ConvertoArrayString ===
            var intArray = new int[] { 1, 2, 3, 4, 5 };
            var stringArray = DataAccessorTools.ConvertoArrayString(intArray);
            Assert.IsNotNull(stringArray);
            Assert.AreEqual(5, stringArray.Length);
            Assert.AreEqual("1", stringArray[0]);
            Assert.AreEqual("5", stringArray[4]);
            
            var nullArray = DataAccessorTools.ConvertoArrayString(null);
            Assert.IsNull(nullArray);
            
            var nonArray = DataAccessorTools.ConvertoArrayString("not an array");
            Assert.IsNull(nonArray);
            
            var alreadyStringArray = new string[] { "a", "b", "c" };
            var resultArray = DataAccessorTools.ConvertoArrayString(alreadyStringArray);
            Assert.AreSame(alreadyStringArray, resultArray);
            
            // === Tests DictionaryData spécifiques ===
            var dict = new Nglib.DATA.COLLECTIONS.DictionaryData();
            
            // Test constructeurs
            var dictFromStringDict = new Nglib.DATA.COLLECTIONS.DictionaryData(
                new Dictionary<string, string> { ["key1"] = "value1", ["key2"] = "value2" });
            Assert.AreEqual("value1", dictFromStringDict.GetString("key1"));
            
            var dictFromObjectDict = new Nglib.DATA.COLLECTIONS.DictionaryData(
                new Dictionary<string, object> { ["key1"] = 123, ["key2"] = true });
            Assert.AreEqual(123, dictFromObjectDict.GetInt("key1"));
            Assert.AreEqual(true, dictFromObjectDict.GetBoolean("key2"));
            
            // Test SetValueMinMax
            Assert.IsTrue(dict.SetValueMinMax("price", 10.5, 99.9));
            Assert.AreEqual(10.5, dict.GetDouble("priceMin"), 0.01);
            Assert.AreEqual(99.9, dict.GetDouble("priceMax"), 0.01);
            
            // Test ContainsKey case-insensitive
            dict.SetObject("TestKey", "value");
            Assert.IsTrue(dict.ContainsKey("testkey"));
            Assert.IsTrue(dict.ContainsKey("TESTKEY"));
            Assert.IsTrue(dict.ContainsKey("TestKey"));
            Assert.IsFalse(dict.ContainsKey("nonexistent"));
            
            // Test Remove case-insensitive
            dict.SetObject("ToRemove", "value");
            Assert.IsTrue(dict.ContainsKey("ToRemove"));
            Assert.IsTrue(dict.Remove("toremove"));
            Assert.IsFalse(dict.ContainsKey("ToRemove"));
            
            // Test IsEmpty
            dict.SetObject("emptyString", "");
            dict.SetObject("nullValue", null);
            dict.SetObject("normalValue", "test");
            Assert.IsTrue(dict.IsEmpty("emptyString"));
            Assert.IsTrue(dict.IsEmpty("nullValue"));
            Assert.IsFalse(dict.IsEmpty("normalValue"));
            Assert.IsTrue(dict.IsEmpty("nonexistent"));
            
            // Test Clone
            dict.SetObject("cloneTest", "original");
            var cloned = dict.Clone();
            Assert.AreEqual("original", cloned.GetString("cloneTest"));
            cloned.SetObject("cloneTest", "modified");
            Assert.AreEqual("original", dict.GetString("cloneTest")); // Original inchangé
            Assert.AreEqual("modified", cloned.GetString("cloneTest"));
            
            // === Tests gestion d'erreurs ===
            try
            {
                DataAccessorTools.CopyTo(null, targetData);
                Assert.Fail("Should throw ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            
            try
            {
                DataAccessorTools.CopyTo(sourceData, null);
                Assert.Fail("Should throw ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            
            // === Tests edge cases ===
            // Test avec valeurs DBNull
            dict.SetObject("dbNullValue", DBNull.Value);
            Assert.AreEqual("", dict.GetString("dbNullValue"));
            Assert.AreEqual(0, dict.GetInt("dbNullValue"));
            Assert.AreEqual(false, dict.GetBoolean("dbNullValue"));
            
            // Test conversion types complexes
            dict.SetObject("complexConversion", 123.45f); // float vers double
            Assert.AreEqual(123.45, dict.GetDouble("complexConversion"), 0.01);
            
            // Test ListFieldsKeys complet
            dict.Clear();
            dict.SetObject("key1", "value1");
            dict.SetObject("key2", "value2");
            dict.SetObject("KEY3", "value3"); // Test case
            var allKeys = dict.ListFieldsKeys();
            Assert.AreEqual(3, allKeys.Length);
            Assert.IsTrue(allKeys.Contains("key1"));
            Assert.IsTrue(allKeys.Contains("key2"));
            Assert.IsTrue(allKeys.Contains("KEY3"));
        }

        [TestMethod()]
        public void BenchmarkSpecializedVsGenericPerformance()
        {
            // Benchmark: Comparer GetString/GetInt VS GetValue<string>/GetValue<int>
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            data.SetObject("testString", "Hello World");
            data.SetObject("testInt", 42);
            
            const int iterations = 100000; // 100K iterations pour des résultats significatifs
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"Benchmark Iterations: {iterations:N0}");

            // === TEST 1: Méthodes spécialisées GetString + GetInt ===
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var str = data.GetString("testString");
                var num = data.GetInt("testInt");
            }
            sw.Stop();
            Console.WriteLine($"Specialized (GetString + GetInt): {sw.ElapsedMilliseconds} ms");

            // === TEST 2: Méthodes génériques GetValue<T> ===
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var str = data.GetValue<string>("testString");
                var num = data.GetValue<int>("testInt");
            }
            sw.Stop();
            Console.WriteLine($"Generic (GetValue<T>):            {sw.ElapsedMilliseconds} ms");
  
             
        }

        [TestMethod()]
        public void GetValueArrayTypesTest()
        {
            // Test GetValue<T[]> pour vérifier le support des tableaux
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            
            // === Test int[] ===
            var intArray = new int[] { 1, 2, 3, 4, 5 };
            data.SetObject("intArray", intArray);
            
            // GetValue<int[]> devrait fonctionner avec le pattern matching "is T"
            var retrievedIntArray = data.GetValue<int[]>("intArray");
            Assert.IsNotNull(retrievedIntArray);
            Assert.AreEqual(5, retrievedIntArray.Length);
            Assert.AreEqual(1, retrievedIntArray[0]);
            Assert.AreEqual(5, retrievedIntArray[4]);
            
            // === Test string[] ===
            var stringArray = new string[] { "a", "b", "c" };
            data.SetObject("stringArray", stringArray);
            
            var retrievedStringArray = data.GetValue<string[]>("stringArray");
            Assert.IsNotNull(retrievedStringArray);
            Assert.AreEqual(3, retrievedStringArray.Length);
            Assert.AreEqual("a", retrievedStringArray[0]);
            Assert.AreEqual("c", retrievedStringArray[2]);
            
            // === Test double[] ===
            var doubleArray = new double[] { 1.1, 2.2, 3.3 };
            data.SetObject("doubleArray", doubleArray);
            
            var retrievedDoubleArray = data.GetValue<double[]>("doubleArray");
            Assert.IsNotNull(retrievedDoubleArray);
            Assert.AreEqual(3, retrievedDoubleArray.Length);
            Assert.AreEqual(1.1, retrievedDoubleArray[0], 0.01);
            
            // === Test bool[] ===
            var boolArray = new bool[] { true, false, true };
            data.SetObject("boolArray", boolArray);
            
            var retrievedBoolArray = data.GetValue<bool[]>("boolArray");
            Assert.IsNotNull(retrievedBoolArray);
            Assert.AreEqual(3, retrievedBoolArray.Length);
            Assert.AreEqual(true, retrievedBoolArray[0]);
            Assert.AreEqual(false, retrievedBoolArray[1]);
            
            // === Test null/not found ===
            var missingArray = data.GetValue<int[]>("notExist");
            Assert.IsNull(missingArray); // Safe mode retourne null par défaut
            
            Console.WriteLine("✅ GetValue<T[]> fonctionne parfaitement pour tous les types de tableaux!");

            data = new Nglib.DATA.COLLECTIONS.DictionaryData();

            // Test désérialisation JSON array int[]
            data.SetObject("jsonIntArray", "[1,2,3,4,5]");
            intArray = data.GetValue<int[]>("jsonIntArray");
            Assert.IsNotNull(intArray);
            Assert.AreEqual(5, intArray.Length);
            Assert.AreEqual(1, intArray[0]);
            Assert.AreEqual(5, intArray[4]);

            // Test désérialisation JSON array string[]
            data.SetObject("jsonStringArray", "[\"hello\",\"world\",\"test\"]");
            stringArray = data.GetValue<string[]>("jsonStringArray");
            Assert.IsNotNull(stringArray);
            Assert.AreEqual(3, stringArray.Length);
            Assert.AreEqual("hello", stringArray[0]);
            Assert.AreEqual("test", stringArray[2]);

            // Test désérialisation JSON array double[]
            data.SetObject("jsonDoubleArray", "[1.5, 2.7, 3.9]");
            doubleArray = data.GetValue<double[]>("jsonDoubleArray");
            Assert.IsNotNull(doubleArray);
            Assert.AreEqual(3, doubleArray.Length);
            Assert.AreEqual(1.5, doubleArray[0]);
            Assert.AreEqual(3.9, doubleArray[2]);

            // Test désérialisation JSON array bool[]
            data.SetObject("jsonBoolArray", "[true, false, true]");
            boolArray = data.GetValue<bool[]>("jsonBoolArray");
            Assert.IsNotNull(boolArray);
            Assert.AreEqual(3, boolArray.Length);
            Assert.IsTrue(boolArray[0]);
            Assert.IsFalse(boolArray[1]);

            // Test désérialisation JSON objet
            data.SetObject("jsonObject", "{\"Name\":\"John\",\"Age\":30}");
            var obj = data.GetValue<TestJsonObject>("jsonObject");
            Assert.IsNotNull(obj);
            Assert.AreEqual("John", obj.Name);
            Assert.AreEqual(30, obj.Age);

            // Test JSON array vide
            data.SetObject("emptyArray", "[]");
            var emptyArray = data.GetValue<int[]>("emptyArray");
            Assert.IsNotNull(emptyArray);
            Assert.AreEqual(0, emptyArray.Length);

            // Test fallback si le JSON est invalide
            data.SetObject("invalidJson", "[1,2,invalid]");
            var fallbackResult = data.GetValue<string>("invalidJson");
            Assert.AreEqual("[1,2,invalid]", fallbackResult); // Retourne la string originale

            Console.WriteLine("✅ GetValue<T> avec désérialisation JSON fonctionne parfaitement!");
        }

        [TestMethod()]
        public void DataAccessorFlags_Required_GetValue_Test()
        {
            // Test du flag Required: valeurs invalides doivent lever DataAccessorException
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            var opt = DataAccessorOptionEnum.Required;

            // Null, DBNull, empty string, zéros numériques = invalides
            var invalidCases = new (string key, object value, string expectedMsg)[]
            {
                ("null", null, "Required field"),
                ("dbnull", DBNull.Value, "Required field"),
                ("empty", "", "empty string"),
                ("int0", 0, "cannot be 0"),
                ("long0", 0L, "cannot be 0"),
                ("double0", 0.0, "cannot be 0")
            };

            foreach (var (key, value, expectedMsg) in invalidCases)
            {
                data.SetObject(key, value);
                Assert.ThrowsException<DataAccessorException>(() => 
                    data.GetValue<string>(key, default, opt), 
                    $"Required should reject {key}");
            }

            // Valeurs valides = OK
            data.SetObject("str", "Hello");
            data.SetObject("num", 42);
            Assert.AreEqual("Hello", data.GetValue<string>("str", default, opt));
            Assert.AreEqual(42, data.GetValue<int>("num", default, opt));
            Console.WriteLine("✅ Required validation (6 invalid + 2 valid cases)");
        }

        [TestMethod()]
        public void DataAccessorFlags_Required_SetValue_Test()
        {
            // Test du flag Required avec SetValue: valeurs invalides = exception
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            var opt = DataAccessorOptionEnum.Required;

            // Valeurs invalides
            Assert.ThrowsException<DataAccessorException>(() => data.SetValue("f", null, opt));
            Assert.ThrowsException<DataAccessorException>(() => data.SetValue("f", "", opt));
            Assert.ThrowsException<DataAccessorException>(() => data.SetValue("f", 0, opt));

            // Valeurs valides
            Assert.IsTrue(data.SetValue("str", "Hello", opt));
            Assert.IsTrue(data.SetValue("num", 42, opt));
            Assert.AreEqual("Hello", data.GetString("str"));
            Assert.AreEqual(42, data.GetInt("num"));
            Console.WriteLine("✅ SetValue Required (3 invalid + 2 valid)");
        }

        [TestMethod()]
        public void DataAccessorFlags_NotReplace_Test()
        {
            // Test NotReplace: empêche remplacement, autorise création
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            var opt = DataAccessorOptionEnum.NotReplace;

            // Existant: pas remplacé
            data.SetObject("existing", "original");
            Assert.IsTrue(data.SetValue("existing", "new", opt));
            Assert.AreEqual("original", data.GetString("existing"));

            // Inexistant: créé
            Assert.IsTrue(data.SetValue("newField", "created", opt));
            Assert.AreEqual("created", data.GetString("newField"));

            // Case-insensitive
            data.SetObject("CaseSensitive", "orig");
            data.SetValue("casesensitive", "mod", opt);
            Assert.AreEqual("orig", data.GetString("CaseSensitive"));
            Console.WriteLine("✅ NotReplace (no replace + create + case-insensitive)");
        }

        [TestMethod()]
        public void DataAccessorFlags_NotCreateColumn_Test()
        {
            // Test NotCreateColumn: empêche création, autorise modification
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();
            var opt = DataAccessorOptionEnum.NotCreateColumn;

            // Inexistant: exception
            Assert.ThrowsException<DataAccessorException>(() => 
                data.SetValue("nonExistent", "value", opt));

            // Existant: modifié
            data.SetObject("existing", "original");
            Assert.IsTrue(data.SetValue("existing", "modified", opt));
            Assert.AreEqual("modified", data.GetString("existing"));

            // Case-insensitive
            data.SetObject("CaseField", "orig");
            Assert.IsTrue(data.SetValue("casefield", "mod", opt));
            Assert.AreEqual("mod", data.GetString("CaseField"));
            Console.WriteLine("✅ NotCreateColumn (no create + update + case-insensitive)");
        }

        [TestMethod()]
        public void DataAccessorFlags_Combined_Test()
        {
            // Test combinaisons de flags: ordre d'évaluation et interactions
            var data = new Nglib.DATA.COLLECTIONS.DictionaryData();

            // NotReplace + Required: NotReplace empêche le remplacement AVANT validation Required
            data.SetObject("field1", "original");
            // NotReplace retourne true car la valeur existe déjà (pas de remplacement)
            Assert.IsTrue(data.SetValue("field1", null, DataAccessorOptionEnum.NotReplace | DataAccessorOptionEnum.Required));
            Assert.AreEqual("original", data.GetString("field1")); // Valeur préservée

            // NotCreateColumn + Required: NotCreateColumn prioritaire (exception car colonne n'existe pas)
            Assert.ThrowsException<DataAccessorException>(() => 
                data.SetValue("newField", "val", DataAccessorOptionEnum.NotCreateColumn | DataAccessorOptionEnum.Required));

            // NotReplace + NotCreateColumn: pas de modification sur champ existant
            data.SetObject("field2", "original");
            Assert.IsTrue(data.SetValue("field2", "new", DataAccessorOptionEnum.NotReplace | DataAccessorOptionEnum.NotCreateColumn));
            Assert.AreEqual("original", data.GetString("field2"));
            Console.WriteLine("✅ Combined flags (3 scenarios)");

            // Champ inexistant: NotReplace ne bloque pas, mais NotCreateColumn bloque
            try
            {
                data.SetValue("newField2", "value", DataAccessorOptionEnum.NotReplace | DataAccessorOptionEnum.NotCreateColumn);
                Assert.Fail("Should throw for non-existent field");
            }
            catch (DataAccessorException)
            {
                Console.WriteLine("✅ NotReplace+NotCreateColumn: exception pour champ inexistant");
            }
        }

         

        // Classe test pour JSON object
        public class TestJsonObject
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}