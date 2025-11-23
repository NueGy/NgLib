using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nglib.DATA.COLLECTIONS
{
    [TestClass]
    public class CollectionsToolsTests
    {
        [TestMethod]
        public void CollectionsToolsFullyTest()
        {
            // Test ContainsKey
            var dicTest = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "KEY2", "Value2" },
                { "key3", "Value3" }
            };

            Assert.IsTrue(dicTest.ContainsKey("key1", true));
            Assert.IsTrue(dicTest.ContainsKey("KEY1", true));
            Assert.IsFalse(dicTest.ContainsKey("KEY1", false));
            Assert.IsFalse(dicTest.ContainsKey("nonexistent", true));
            Assert.IsFalse(dicTest.ContainsKey(null, true));

            // Test Contains avec ICollection<string>
            var keys = new List<string> { "test1", "TEST2", "Test3" };
            var searchKeys = new List<string> { "TEST1", "test4" };
            
            Assert.IsTrue(keys.Contains(searchKeys, true));
            Assert.IsFalse(keys.Contains(searchKeys, false));
            Assert.IsFalse(keys.Contains("test4", true));
            Assert.IsTrue(keys.Contains("TEST1", true));
            Assert.IsFalse(keys.Contains("TEST1", false));

            // Test edge cases Contains
            Assert.IsFalse(((List<string>)null).Contains("test", true));
            Assert.IsFalse(new List<string>().Contains("test", true));
            Assert.IsFalse(keys.Contains((string)null, true));

            // Test ContainInList
            Assert.IsTrue("test1".ContainInList(keys, true));
            Assert.IsTrue("TEST1".ContainInList(keys, true));
            Assert.IsFalse("TEST1".ContainInList(keys, false));
            Assert.IsTrue("test1".ContainInList("test1", "TEST2", "other"));
            Assert.IsFalse("notfound".ContainInList(keys, true));

            // Test AddRange
            var dic1 = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
            var dic2 = new Dictionary<string, string> { { "b", "3" }, { "c", "4" } };
            
            dic1.AddRange(dic2, true);
            Assert.AreEqual("3", dic1["b"]); // Override
            Assert.AreEqual("4", dic1["c"]); // New key
            
            var dic3 = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };
            var dic4 = new Dictionary<string, string> { { "b", "3" }, { "d", "5" } };
            
            dic3.AddRange(dic4, false);
            Assert.AreEqual("2", dic3["b"]); // No override
            Assert.AreEqual("5", dic3["d"]); // New key

            // Test AddOrReplace
            var dic5 = new Dictionary<string, string>();
            dic5.AddOrReplace("key1", "value1");
            Assert.AreEqual("value1", dic5["key1"]);
            
            dic5.AddOrReplace("key1", "value2");
            Assert.AreEqual("value2", dic5["key1"]);

            // Test AddOrReplace avec insensitive
            var dic6 = new Dictionary<string, string> { { "Key1", "original" } };
            dic6.AddOrReplace("KEY1", "updated", true);
            Assert.AreEqual(1, dic6.Count);
            Assert.AreEqual("updated", dic6["Key1"]);

            // Test GetSafeValue*
            var dicString = new Dictionary<string, string> { { "Key1", "Value1" }, { "NULL_KEY", null } };
            Assert.AreEqual("Value1", dicString.GetSafeString("key1"));
            Assert.AreEqual("Value1", dicString.GetSafeString("KEY1"));
            Assert.IsNull(dicString.GetSafeString("nonexistent"));
            Assert.IsNull(dicString.GetSafeString("NULL_KEY"));

            var dicObject = new Dictionary<string, object> 
            { 
                { "String", "test" }, 
                { "Int", 42 }, 
                { "Null", null },
                { "DBNull", DBNull.Value }
            };
            
            Assert.AreEqual("test", dicObject.GetSafeString("string"));
            Assert.AreEqual("42", dicObject.GetSafeString("INT"));
            Assert.AreEqual("", dicObject.GetSafeString("Null"));
            Assert.AreEqual("", dicObject.GetSafeString("DBNull"));
            Assert.AreEqual("", dicObject.GetSafeString("nonexistent"));

            Assert.AreEqual("test", dicObject.GetSafeObject("string"));
            Assert.AreEqual(42, dicObject.GetSafeObject("INT"));
            Assert.IsNull(dicObject.GetSafeObject("Null"));
            Assert.AreEqual(DBNull.Value, dicObject.GetSafeObject("DBNull"));

            var dicGeneric = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };
            Assert.AreEqual("one", dicGeneric.GetSafeValue(1));
            Assert.IsNull(dicGeneric.GetSafeValue(99));



            // Test Divide
            var numbers = Enumerable.Range(1, 10).ToList();
            var divided = numbers.Divide(3);
            
            Assert.AreEqual(4, divided.Count); // 10/3 = 3 chunks + 1 remainder
            Assert.AreEqual(3, divided[0].Length);
            Assert.AreEqual(3, divided[1].Length);
            Assert.AreEqual(3, divided[2].Length);
            Assert.AreEqual(1, divided[3].Length);

            // Test DivideFixed
            var numbersFixed = Enumerable.Range(1, 10).ToList();
            var dividedFixed = numbersFixed.DivideFixed(3);
            
            Assert.AreEqual(3, dividedFixed.Count);
            Assert.AreEqual(4, dividedFixed[0].Length); // 10/3 = 3 + remainder distributed
            Assert.AreEqual(3, dividedFixed[1].Length);
            Assert.AreEqual(3, dividedFixed[2].Length);
            
            // Verify all elements are included
            var allElements = dividedFixed.SelectMany(x => x).OrderBy(x => x).ToArray();
            CollectionAssert.AreEqual(numbersFixed.ToArray(), allElements);

            // Test edge cases DivideFixed
            var emptyDivided = new List<int>().DivideFixed(3);
            Assert.AreEqual(3, emptyDivided.Count);
            Assert.AreEqual(0, emptyDivided[0].Length);

            var singleDivided = new List<int> { 1 }.DivideFixed(1);
            Assert.AreEqual(1, singleDivided.Count);
            Assert.AreEqual(1, singleDivided[0].Length);

            var nullDivided = ((List<int>)null).DivideFixed(3);
            Assert.AreEqual(0, nullDivided.Count);

            var invalidCountDivided = numbersFixed.DivideFixed(0);
            Assert.AreEqual(0, invalidCountDivided.Count);
        }

        [TestMethod]
        public void CollectionsExtensionsFullyTest()
        {
            // Test NotNull
            var listWithNulls = new List<string> { "a", null, "b", null, "c" };
            var notNullResult = listWithNulls.NotNull().ToList();
            Assert.AreEqual(3, notNullResult.Count);
            CollectionAssert.AreEqual(new List<string> { "a", "b", "c" }, notNullResult);

            // Test ForEach
            var sum = 0;
            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            numbers.ForEach(x => sum += x);
            Assert.AreEqual(15, sum);

            // Test ForEachSafe avec erreurs
            var errorList = new List<int> { 1, 2, 0, 4 };
            var results = new List<int>();
            bool hasError = errorList.ForEachSafe(x => results.Add(10 / x)); // Division par zéro sur le 3ème élément
            
            Assert.IsTrue(hasError);
            Assert.AreEqual(3, results.Count); // 3 éléments traités sans erreur
            CollectionAssert.AreEqual(new List<int> { 10, 5, 2 }, results); // 10/1, 10/2, 10/4

            // Test Clone
            var originalList = new List<string> { "a", "b", "c" };
            var clonedList = originalList.Clone();
            
            Assert.AreNotSame(originalList, clonedList);
            CollectionAssert.AreEqual(originalList, clonedList);
            
            // Modification de l'original ne doit pas affecter le clone
            originalList.Add("d");
            Assert.AreEqual(3, clonedList.Count);
            Assert.AreEqual(4, originalList.Count);

            // Test Clone avec liste null
            List<string> nullList = null;
            var clonedNull = nullList.Clone();
            Assert.IsNotNull(clonedNull);
            Assert.AreEqual(0, clonedNull.Count);

            // Test MoveFirstToLast
            var moveList = new List<string> { "first", "second", "third" };
            moveList.MoveFirstToLast();
            CollectionAssert.AreEqual(new List<string> { "second", "third", "first" }, moveList);
            
            // Test MoveFirstToLast avec liste vide
            var emptyList = new List<string>();
            emptyList.MoveFirstToLast(); // Ne doit pas planter
            Assert.AreEqual(0, emptyList.Count);
            
            // Test MoveFirstToLast avec un seul élément
            var singleList = new List<string> { "only" };
            singleList.MoveFirstToLast();
            Assert.AreEqual(1, singleList.Count);
            Assert.AreEqual("only", singleList[0]);
        }

        //[TestMethod]
        //public void ListResultFullyTest()
        //{
        //    // Test constructeur vide
        //    var emptyResult = new ListResult<string>();
        //    Assert.AreEqual(0, emptyResult.Count);
        //    Assert.IsNotNull(emptyResult.info);

        //    // Test constructeur avec données
        //    var sourceData = new List<string> { "a", "b", "c" };
        //    var result = new ListResult<string>(sourceData);
        //    Assert.AreEqual(3, result.Count);
        //    CollectionAssert.AreEqual(sourceData, result.ToList());

        //    // Test FromList
        //    var fromListResult = ListResult<string>.FromList(sourceData);
        //    Assert.IsNotNull(fromListResult);
        //    Assert.AreEqual(3, fromListResult.Count);
            
        //    var fromNullResult = ListResult<string>.FromList(null);
        //    Assert.IsNull(fromNullResult);

        //    // Test PrepareForError
        //    var errorResult = ListResult<string>.PrepareForError("Test error message");
        //    Assert.IsNotNull(errorResult);
        //    Assert.AreEqual(0, errorResult.Count);
        //    Assert.AreEqual("Test error message", errorResult.info.Error);

        //    // Test ToJsonWithEnveloppe
        //    var jsonResult = new ListResult<string> { "item1", "item2" };
        //    jsonResult.info.Error = "test error";
            
        //    var json = jsonResult.ToJsonWithEnveloppe();
        //    Assert.IsTrue(json.Contains("\"data\""));
        //    Assert.IsTrue(json.Contains("\"info\""));
        //    Assert.IsTrue(json.Contains("item1"));
        //    Assert.IsTrue(json.Contains("test error"));

        //    // Test ToJsonWithEnveloppe avec options nulles
        //    var jsonWithNull = jsonResult.ToJsonWithEnveloppe(null);
        //    Assert.IsNotNull(jsonWithNull);
        //    Assert.IsTrue(jsonWithNull.Contains("\"data\""));
        //}
    }
}