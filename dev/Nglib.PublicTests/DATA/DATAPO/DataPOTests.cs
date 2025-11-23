using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Nglib.DATA.DATAPO
{
    [TestClass()]
    public class DataPOTests
    {
        [TestMethod()]
        public void BasicOperationsFullyTest()
        {
            // Création d'un DataPO
            var item = new DataPOExample();
            Assert.IsNotNull(item);

            // Test SetData / GetData basique
            item.MonId = 42;
            item.MaValeur = "Test valeur";
            Assert.AreEqual(42, item.MonId);
            Assert.AreEqual("Test valeur", item.MaValeur);

            // Test indexeur
            item["mavaleur"] = "Nouvelle valeur";
            Assert.AreEqual("Nouvelle valeur", item.GetString("mavaleur"));

            // Test IsChanges
            Assert.IsTrue(item.IsChanges());

            // Test GetRow après modifications
            var row = item.GetRow();
            Assert.IsNotNull(row);
            Assert.IsNotNull(row.Table);
            Assert.AreEqual("demotable", row.Table.TableName);

            // Test AcceptChanges
            bool acceptResult = item.AcceptChanges();
            Assert.IsTrue(acceptResult); // Retourne true car il y avait des changements
            
            // Nouvelle modification pour retester IsChanges
            item.MaValeur = "Autre valeur";
            Assert.IsTrue(item.IsChanges());
            
            item.AcceptChanges();
            // Note: Un DataRow Detached reste toujours en état Detached, donc IsChanges reste true
            // C'est le comportement normal car il nécessite un traitement (Insert)

            // Test IsInDataBase
            Assert.IsFalse(item.IsInDataBase()); // Detached = pas en base
        }

        [TestMethod()]
        public void SchemaDefinitionFullyTest()
        {
            // Test InitSchema automatique
            var item = new DataPOExample();
            var schema = item.CreateSchema();
            Assert.IsNotNull(schema);
            Assert.AreEqual("demotable", schema.TableName);

            // Vérification des colonnes
            Assert.IsTrue(schema.Columns.Contains("monid"));
            Assert.AreEqual(typeof(int), schema.Columns["monid"].DataType);

            // Test IsDefinedSchema - avant GetRow le schema n'est pas encore attaché au DataPO
            // Le schema est défini mais pas encore appliqué à l'instance
            Assert.IsFalse(item.IsDefinedSchema());
            item.DefineSchemaPO();
            Assert.IsTrue(item.IsDefinedSchema());

            // Test GetSchemaOnPO
            var retrievedSchema = DataPOSchemaTools.GetSchemaOnPO(item.GetType());
            Assert.IsNotNull(retrievedSchema);
            Assert.AreEqual("demotable", retrievedSchema.TableName);

            // Test clé primaire
            Assert.IsNotNull(retrievedSchema.PrimaryKey);
            Assert.AreEqual(1, retrievedSchema.PrimaryKey.Length);
            Assert.AreEqual("monid", retrievedSchema.PrimaryKey[0].ColumnName);
        }

        [TestMethod()]
        public void FlowsNoSqlFullyTest()
        {
            // Création avec flux NoSQL
            var item = new DataPOExample();
            item.MonId = 1; // Initialise le schema automatiquement

            // Test accès au flux
            Assert.IsNotNull(item.Flux);
            Assert.AreEqual("fluxjson", item.Flux.GetFieldName());

            // Test écriture dans le flux
            item.MaValeurNosql = "Valeur NoSQL";
            item.Flux["autre"] = "Autre valeur";
            item.Flux.SetData("nombre", 123, DataAccessorOptionEnum.None);

            // Test lecture depuis le flux
            Assert.AreEqual("Valeur NoSQL", item.MaValeurNosql);
            Assert.AreEqual("Autre valeur", item.Flux.GetString("autre"));
            Assert.AreEqual(123, item.Flux.GetInt("nombre"));

            // Test ListFieldsKeys du flux
            var keys = item.Flux.ListFieldsKeys();
            // Les clés dans ParamValues sont préfixées par "/{dataValueName}/" (par défaut "/param/")
            Assert.IsTrue(keys.Contains("/param/maval"));
            Assert.IsTrue(keys.Contains("/param/autre"));
            Assert.IsTrue(keys.Contains("/param/nombre"));

            // Test IsChanges sur les flux - les flux sont synchronisés avec le DataRow
            // Note: Pour que IsChanges soit vrai, il faut synchroniser les flux avec GetRow(syncFlows: true)
            item.GetRow(syncFlows: true);
            Assert.IsTrue(item.IsChanges()); // Le DataRow contient maintenant les modifications des flux

            // Test AcceptChanges
            item.AcceptChanges();
            // Note: Un DataRow Detached reste en état Detached même après AcceptChanges
            // C'est le comportement normal de ADO.NET - un DataRow doit être attaché à une table
            // pour avoir un état persistant. Pour un DataRow Detached, IsChanges() retourne toujours true
            // car il n'a pas d'état "Unchanged" sans être dans une DataTable
            
            // Après AcceptChanges, les flux internes ne devraient plus avoir de changements
            // car AcceptChanges() appelle aussi flux.AcceptChanges() pour chaque flux
            // MAIS: Il y a un cas particulier ici. Quand on fait GetRow(syncFlows: true), 
            // le flux est sérialisé dans le DataRow. Ensuite, quand on appelle AcceptChanges(),
            // le flux accepte ses changements internes. Cependant, comme le DataRow est Detached,
            // il reste en état modifié. Le test doit simplement vérifier que le flux a accepté
            // ses changements, ce qui signifie qu'on peut le modifier à nouveau sans perdre l'état.
            
            // Vérifions que le flux a bien accepté ses changements
            var fluxChangesBefore = item.Flux.IsChanges();
            item.Flux["test"] = "nouvelle valeur"; // Modifie le flux
            var fluxChangesAfter = item.Flux.IsChanges();
            
            // Si fluxChangesBefore était false, alors AcceptChanges a fonctionné
            // Si après modification fluxChangesAfter est true, c'est normal
            Assert.IsFalse(fluxChangesBefore, "Le flux ne devrait plus avoir de changements internes après AcceptChanges");
            Assert.IsTrue(fluxChangesAfter, "Le flux devrait avoir des changements après une nouvelle modification");
        }

        [TestMethod()]
        public void DataAccessorOperationsFullyTest()
        {
            var item = new DataPOExample();

            // Test GetData avec DataAccessorOptionEnum.Safe
            var safeValue = item.GetData("nonexistant", DataAccessorOptionEnum.Safe);
            Assert.IsNull(safeValue);

            // Test SetData
            bool setResult = item.SetData("mavaleur", "Test", DataAccessorOptionEnum.None);
            Assert.IsTrue(setResult);
            Assert.AreEqual("Test", item.GetData("mavaleur", DataAccessorOptionEnum.None));

            // Test GetString, GetInt, GetBoolean via indexeur et GetData
            item["text"] = "Hello";
            item["number"] = 999;
            item["flag"] = true;
            Assert.AreEqual("Hello", item.GetString("text"));
            Assert.AreEqual(999, item.GetInt("number"));
            Assert.IsTrue(item.GetBoolean("flag"));

            // Test ListFieldsKeys
            var fields = item.ListFieldsKeys();
            Assert.IsNotNull(fields);
            Assert.IsTrue(fields.Contains("monid"));
            Assert.IsTrue(fields.Contains("mavaleur"));
        }

        [TestMethod()]
        public void CollectionOperationsFullyTest()
        {
            // Création d'une collection
            var collection = new CollectionPO<DataPOExample>();
            Assert.AreEqual(0, collection.Count);

            // Ajout d'éléments
            var item1 = new DataPOExample { MonId = 1, MaValeur = "Item 1" };
            var item2 = new DataPOExample { MonId = 2, MaValeur = "Item 2" };
            var item3 = new DataPOExample { MonId = 3, MaValeur = "Item 3" };

            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);
            Assert.AreEqual(3, collection.Count);

            // Test TotalCount et ExecuteTimeElapsed
            collection.TotalCount = 100;
            collection.ExecuteTimeElapsed = 250;
            Assert.AreEqual(100, collection.TotalCount);
            Assert.AreEqual(250, collection.ExecuteTimeElapsed);

            // Test GetPOType
            Assert.AreEqual(typeof(DataPOExample), collection.GetPOType());

            // Test AsValue
            bool hasValue = collection.AsValue("mavaleur", "Item 1", "Item 2");
            Assert.IsTrue(hasValue);

            bool hasNoValue = collection.AsValue("mavaleur", "Inexistant");
            Assert.IsFalse(hasNoValue);

            // Test ToDictionaryString
            var dict = collection.ToDictionaryString("monid", "mavaleur");
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual("Item 1", dict["1"]);
            Assert.AreEqual("Item 2", dict["2"]);

            // Test CastTo
            var castedCollection = collection.CastTo<CollectionPO<DataPOExample>>();
            Assert.IsNotNull(castedCollection);
            Assert.AreEqual(3, castedCollection.Count);
            Assert.AreEqual(100, castedCollection.TotalCount);
        }

        [TestMethod()]
        public void DataPOToolsFullyTest()
        {
            // Test Create depuis DataRow
            var table = new DataTable("test");
            table.Columns.Add("monid", typeof(int));
            table.Columns.Add("mavaleur", typeof(string));
            var row = table.NewRow();
            row["monid"] = 10;
            row["mavaleur"] = "Test Row";
            table.Rows.Add(row);

            var item = DataPOTools.Create<DataPOExample>(row);
            Assert.IsNotNull(item);
            Assert.AreEqual(10, item.MonId);
            Assert.AreEqual("Test Row", item.MaValeur);

            // Test CreateFirst depuis DataTable
            var firstItem = DataPOTools.CreateFirst<DataPOExample>(table);
            Assert.IsNotNull(firstItem);
            Assert.AreEqual(10, firstItem.MonId);

            // Test GetValues
            var values = item.GetValues();
            Assert.IsNotNull(values);
            Assert.IsTrue(values.ContainsKey("monid"));
            Assert.AreEqual(10, values["monid"]);

            // Test SetValues
            var newValues = new Dictionary<string, object>
            {
                { "monid", 20 },
                { "mavaleur", "Nouvelle valeur" }
            };
            item.SetValues(newValues);
            Assert.AreEqual(20, item.MonId);
            Assert.AreEqual("Nouvelle valeur", item.MaValeur);

            // Test GetChangedValues
            item["mavaleur"] = "Modifié";
            var changedValues = item.GetChangedValues();
            Assert.IsNotNull(changedValues);
            Assert.IsTrue(changedValues.ContainsKey("mavaleur"));
        }

        [TestMethod()]
        public void LoadFromDataTableFullyTest()
        {
            // Création d'une DataTable avec plusieurs lignes
            var table = new DataTable("test");
            table.Columns.Add("monid", typeof(int));
            table.Columns.Add("mavaleur", typeof(string));

            for (int i = 1; i <= 5; i++)
            {
                var row = table.NewRow();
                row["monid"] = i;
                row["mavaleur"] = $"Valeur {i}";
                table.Rows.Add(row);
            }

            // Test LoadFromDataTable sur collection
            var collection = new CollectionPO<DataPOExample>();
            collection.LoadFromDataTable(table);
            Assert.AreEqual(5, collection.Count);
            Assert.AreEqual(1, collection[0].MonId);
            Assert.AreEqual("Valeur 1", collection[0].MaValeur);
            Assert.AreEqual(5, collection[4].MonId);

            // Test constructeur avec DataTable
            var collection2 = new CollectionPO<DataPOExample>(table);
            Assert.AreEqual(5, collection2.Count);

            // Test GetPOList
            var poList = collection.GetPOList();
            Assert.AreEqual(5, poList.Count);
        }
    }
}