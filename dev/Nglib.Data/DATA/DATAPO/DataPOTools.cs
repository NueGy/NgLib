// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Outil pour les objets DATAPO
    /// </summary>
    public static class DataPOTools
    {



        /// <summary>
        /// Création d'un nouvel objet
        /// </summary>
        public static T Create<T>(System.Data.DataRow row) where T : Nglib.DATA.DATAPO.DataPO, new()
        {
            if (row == null) return null;
            T retour = new T();
            retour.SetRow(row);
            return retour;
        }
        public static DataPO Create(System.Data.DataRow row) { return Create<DataPO>(row); }


        /// <summary>
        /// Création d'un nouvel objet (le premier de la table)
        /// </summary>
        public static T CreateFirst<T>(System.Data.DataTable table) where T : Nglib.DATA.DATAPO.DataPO, new()
        {
            if (table == null || table.Rows.Count < 1) return null;
            return Create<T>(table.Rows[0]);
        }
        public static DataPO CreateFirst(System.Data.DataTable table) { return CreateFirst<DataPO>(table); }









        /// <summary>
        /// Présence de changements
        /// </summary>
        /// <returns></returns>
        public static bool IsChanges(this DataPO item)
        {
            // On regarde si il y as eu un changement dans les flow
            if (item.flows != null && item.flows.Count(f => f.IsChanges()) > 0) return true;

            // on regarde si il y as eu un changement dans le datarow
            if (item.localRow == null) return false;
            else if (item.localRow.RowState.HasFlag(System.Data.DataRowState.Modified)) return true;
            else if (item.localRow.RowState.HasFlag(System.Data.DataRowState.Detached)) return true; // on considere que les datarow Detached nécesite un Insert donc ils nécessiteront un traitement
            else return false;
        }

        /// <summary>
        /// Obtientir les données du DataPO
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(this DataPO po, bool returnKeys, bool returnValues)
        {
            System.Data.DataRow row = po.GetRow(returnValues);// Si les valeurs sont demandé on refresh les flow
            if(returnKeys && !IsDefinedSchema(po)) DefineSchemaPO(po); // Si l'objet est pas définis il faut le daire
            return DataSetTools.GetValues(row, returnKeys, returnValues);
        }

        /// <summary>
        /// Obtientir les données du DataPO
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(this DataPO po, params string[] ColNames)
        {
            if (ColNames == null || ColNames.Length == 0) return new Dictionary<string, object>();
            bool refreshflow = false;
            if(po.flows!=null && po.flows.Count>0)
                refreshflow = po.flows.Select(f => f.GetFieldName()).Where(fn => !string.IsNullOrWhiteSpace(fn)).Any(fn => ColNames.Any(eq => fn.Equals(eq, StringComparison.OrdinalIgnoreCase)));
            
            System.Data.DataRow row = po.GetRow(refreshflow);
            return DataSetTools.GetValues(row, ColNames);
        }

        public static Dictionary<string, object> GetChangedValues(this DataPO po)
        {
            bool refreshflow = (po.flows != null && po.flows.Count(f => f.IsChanges()) > 0);
            System.Data.DataRow row = po.GetRow(refreshflow);
            return DataSetTools.GetChangedValues(row);
        }


        /// <summary>
        /// Définit les valeur dans le datarow dans l'objet
        /// </summary>
        /// <param name="row">L'objet de données</param>
        public static void SetValues(this DataPO po, Dictionary<string, object> DicDataRow)
        {
            try
            {
                if (po.localRow == null) po.DefineSchemaPO(); // si pas de datarow, on l'initialise (identique à GetRow())
                foreach (var item in DicDataRow)
                    po.SetObject(item.Key, item.Value);


                //po._isLoaded = false; // N'a pas été chargé depuis une base de données !!! voir comment on fait
            }
            catch (Exception ex)
            {
                throw new Exception("SetValues " + ex.Message, ex);
            }
        }





        /// <summary>
        /// Obtient toute les valeurs des propriétées déclarées dans la classe (PAR REFLEXION)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetDeclaredValues(this DataPO po)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (PropertyInfo prp in APP.CODE.PropertiesTools.GetProperties(po.GetType()))
            {
                object value = prp.GetValue(po, new object[] { });
                dict.Add(prp.Name, value);
            }
            return dict;
        }




        





        /// <summary>
        /// Permet de clonner les données des datarow en une seule datatable
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static System.Data.DataTable CloneDataTable(params DATAPO.DataPO[] datas)
        {
            
            try
            {
                List<System.Data.DataTable> alltables = datas.Select(dt => dt.GetRow().Table).Distinct().ToList();
                System.Data.DataTable tabinsert = DataSetTools.DataTableMergeSchemas(alltables.ToArray());
                DataSetTools.DataTableRemoveConstraints(tabinsert);
                foreach (var itemdata in datas)
                {
                    System.Data.DataRow newrow = tabinsert.NewRow();
                    System.Data.DataRow oldrow = itemdata.GetRow(true);
                    oldrow.CopyRow(newrow);
                    tabinsert.Rows.Add(newrow);
                }
                return tabinsert;
            }
            catch (Exception ex)
            {
                throw new Exception("CloneDataTable " + ex.Message);
            }
        }








        /// <summary>
        /// Permet de copier les propriété d'un PO dans un objet (need dev !!!)
        /// </summary>
        public static bool CopyProperties(object sourceObject, object targetObject, bool onlytypesimple = true, bool replace = true, bool safe = true)
        {
            PropertyInfo[] targetObjectProporties = targetObject.GetType().GetProperties();
            List<PropertyInfo> PropertiesTargetOK = CopyProperties_Validation(targetObjectProporties.ToList());
            return CopyProperties(sourceObject, targetObject, PropertiesTargetOK, replace);
        }


        public static List<PropertyInfo> CopyProperties_Validation(List<PropertyInfo> Properties, bool onlytypeGeneriques = true)
        {
            List<string> LegalTypes = new List<string>() { "string", "int", "int16", "int32", "int64", "long", "byte", "char", "decimal", "double", "float", "uint", "ulong", "boolean", "short" };
            List<PropertyInfo> PropertiesTargetOK = new List<PropertyInfo>();
            foreach (PropertyInfo targetProp in Properties)
            {
                // uniquement les type simple
                Type proptype = targetProp.GetType();
                if (proptype.IsCOMObject) continue;

                if (onlytypeGeneriques)
                {
                    if (!LegalTypes.Contains(targetProp.PropertyType.Name.ToLower())) continue;
                    //if(!proptype.IsGenericType) continue;
                    //if(proptype.IsClass) continue;
                    //if(!proptype.IsVisible) continue;
                }
                PropertiesTargetOK.Add(targetProp);
            }
            return PropertiesTargetOK;
        }



        public static bool CopyProperties(object sourceObject, object targetObject, List<PropertyInfo> PropertiesTargetOK, bool replaceifnotnull = true, bool safe = true)
        {
            bool retour = false;
            PropertyInfo[] sourceObjectProporties = sourceObject.GetType().GetProperties();

            foreach (PropertyInfo targetProp in PropertiesTargetOK)
            {
                try
                {
                    object objProptarget = targetProp.GetValue(targetObject, null);
                    if (objProptarget != null && !replaceifnotnull) continue; // on ne remplace pa si existe déja

                    PropertyInfo sourceProp = null;
                    foreach (PropertyInfo itemSp in sourceObjectProporties) if (itemSp.Name == targetProp.Name) { sourceProp = itemSp; break; }
                    if (sourceProp == null) continue;


                    object objPropSource = sourceProp.GetValue(sourceObject, null);
                    if (targetProp.GetSetMethod() == null) continue;
                    targetProp.SetValue(targetObject, objPropSource, null);
                    retour = true;
                }
                catch (Exception)
                {
                    if (!safe) throw; //uniquement si not safe
                }

            }
            return retour;
        }












        /// <summary>
        /// Permet de charger une table dans une liste de PO
        /// </summary>
        /// <typeparam name="Tpo"></typeparam>
        /// <param name="listPO"></param>
        /// <param name="table"></param>
        public static void LoadFromDataTable<Tpo>(this IList<Tpo> listPO, System.Data.DataTable table) where Tpo : DataPO, new()
        {
          
            try
            {
                int iiadd = 0;
                foreach (System.Data.DataRow row in table.Rows)
                {
                    Tpo ee = new Tpo();// Specifique, peut pas utiliser le constructeur classique
                    ee.SetRow(row);
                    iiadd++;
                    listPO.Add(ee);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static IList<Tpo> LoadFromDataTable<Tpo>(System.Data.DataTable table) where Tpo : DataPO, new()
        {
            List<Tpo> retour = new List<Tpo>();
            LoadFromDataTable<Tpo>(retour, table);
            return retour;
        }




        #region -----------  Initialisation SCHEMA --------------------

        /// <summary>
        /// Liste des structures de table en cache pour ne pas pas avoir à les recréer au moment de créer des PO
        /// Par contre, il est interdit d'y attacher les datarow, c'est seulement pour avoir la structure
        /// Donc après l'insert l'objet sera attaché sur un autre objet table clonner a partir de celui la
        /// </summary>
        private static Dictionary<Type, System.Data.DataTable> TablePoSchemasCache = new Dictionary<Type, DataTable>();


 


        /// <summary>
        /// Permet de preparer l'objet (notament définir le datarow et son schema)
        /// C'est nécessaire pour les opérations SQL, ou l'instanciation vide d'un DATAPO
        /// </summary>
        /// <param name="po"></param>
        /// <returns></returns>
        public static void DefineSchemaPO(this DataPO po, bool allowCache = true)
        {
            try
            {
                if (po == null) return;
                Type potype = po.GetType();
                System.Data.DataTable tableStd = DataPOTools.GetSchemaOnPO(potype, allowCache);

                // on prend un clone de la table
                DefineSchemaPO(po, tableStd, true); // voir si il est possible d'améliorer pour ne pas prendre de clone !!! (faire le clone que si il y as modification des colonnes)
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("InitalizeDataPo {0}", ex.Message));
            }
        }



        /// <summary>
        /// Permet de définir directement  la structure de la table pour ce datarow, création d'une nouvelle si elle existe pas
        /// Ne fonctionnera
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="ColsKeys"></param>
        public static void DefineSchemaPO(DataPO po, string nameTable, System.Data.DataColumn[] Cols, System.Data.DataColumn[] ColsKeys)
        {
            System.Data.DataTable table = null;
            table = DataSetTools.DefineDataTable(nameTable, Cols, ColsKeys);
            DefineSchemaPO(po, table);
            //   itemcc.ReadOnly = false; // pas de readonly dans les bulles, sinon on ne peut plus modifier
        }


        /// <summary>
        /// Permet de définir directement la structure du PO (Un nouveau Row a partir de la nouvelle table)
        /// </summary>
        /// <param name="po"></param>
        /// <param name="table"></param>
        /// <param name="AllowKeepOriginalRow">Si il y as déja un datarow avec des données, on préserve les données dans la nouvelle Table</param>
        public static void DefineSchemaPO(DataPO po, DataTable tableSchema, bool AllowKeepOriginalRow = true, bool CloneNewTable = true)
        {
            try
            {
                bool rowexisting = false;
                if (po.localRow != null && po.localRow.ItemArray != null && po.localRow.ItemArray.Length > 0)
                    rowexisting = true; // il y as des données dans le datarow existant

                if (rowexisting && AllowKeepOriginalRow) //Clone un nouveau datarow dans la nouvelle table avec des données d'origine
                    po.localRow.Table.MergeAddSchema(tableSchema); // il suffit juste de vérifier que le shémas est bien identique
                //po.localRow = DataSetTools.ReplaceRowInOtherTable(po.localRow, table); // premet de recopier les données orgininal dans la nouvelle table
                else
                {
                    DataTable tabledata = CloneNewTable ? tableSchema.Clone() : tableSchema;
                    // il faut clonner la table pour eviter les erreurs sur autoincrement, !!! voir si possible d'améliorer car multipli le temps de traitement x10
                    po.localRow = tabledata.NewRow(); // init simple
                }
               
                po._isDefined = true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("DefineSchemaPO {0}", ex.Message));
            }
        }


        /// <summary>
        /// Savoir si le datapo à été correctement défini pour des actions SQL
        /// </summary>
        /// <param name="po"></param>
        /// <returns></returns>
        public static bool IsDefinedSchema(this DataPO po)
        {
            if (po == null) return false;
            if (po.GetRow() == null) return false;
            if (!po._isDefined) return false;
            return true;
        }





        ///// <summary>
        ///// permet de preparer un Objet pour sont update ou insert en base
        ///// </summary>
        ///// <param name="bubble"></param>
        ///// <param name="PrepareForInsert"></param>
        //public static void PrepareDataPOForDB(DATAPO.DataPO bubble, bool ForcePrepareFlow)
        //{
        //    try
        //    {
        //        if (bubble == null) throw new Exception("DataPO null");
        //        System.Data.DataRow rowb = bubble.GetRow(true); // la structure du datapo sera défini à ce moment si nécessaire
        //        if (!bubble.IsDefinedSchema()) bubble.DefineSchemaPO(true);// initialisation
        //        if (!bubble.IsDefinedSchema()) throw new Exception("DataPo not defined"); // on peus pas traiter un datapo sans structure, car on aura pas les primarykey pour le sql
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(string.Format("PrepareDataPOForDB {0}", ex.Message), ex);
        //    }
        //}




        /// <summary>
        /// Enregistrer le schema/structure de l'objet PO dans le cache
        /// </summary>
        /// <param name="po"></param>
        /// <param name="table"></param>
        /// <param name="replace"></param>
        private static void SetSchemaInCache(Type potype, System.Data.DataTable table, bool replace = true)
        {
            if (!TablePoSchemasCache.ContainsKey(potype))
                TablePoSchemasCache.Add(potype, table);
            else if (replace) TablePoSchemasCache[potype] = table;

        }




        public static System.Data.DataTable GetSchemaOnPO(Type potype, bool allowCache = true)
        {
            //Si c'est un dataPO pur, on ne peus pas utiliser le cache
            if (potype == typeof(DataPO)) allowCache = false;
            System.Data.DataTable tableStd = null;
            // on commence par vérifier si on peut utiliser le cache
            if (allowCache && DataPOTools.TablePoSchemasCache.ContainsKey(potype))
            {
                tableStd= DataPOTools.TablePoSchemasCache[potype];

            }
            else
            {
                if (tableStd == null) tableStd = DataPOTools.CreateSchemaOnPO(potype);
                if (tableStd == null) tableStd = new DataTable(); // on créer une table vierge, si on as rien de défini
                if (allowCache) DataPOTools.SetSchemaInCache(potype, tableStd, true);
            }
            return tableStd;
        }

        //public static System.Data.DataTable CreateSchemaOnPO(DataPO po)
        //{
        //    System.Data.DataTable tableStd = null;
        //    if (tableStd == null) tableStd = po.InitSchema(); // sinon on tente avec la prédéinition hérité 
        //    if (tableStd == null) tableStd = CreateSchemaWithAttributes(po);// sinon on tente de mettre l'objet en cache avec les attributs
        //    return tableStd;
        //}


        public static System.Data.DataTable CreateSchemaOnPO(Type potype)
        {
            System.Data.DataTable tableStd = null;
            if (tableStd == null) // La structure est defini dans une methode hérité.
            {
                // sinon on tente avec la prédéinition hérité, mais pour cela il faut instancier un objet (!!! gérer si constructeur non vide)
                DataPO pofictif = APP.CODE.ReflectionTools.CreateInstance(potype) as DataPO;
                tableStd = pofictif.InitSchema(); 
            }
            // sinon on tente de voir si l'objet dispose d'attributs spécifiant sa structure
            if (tableStd == null) 
                tableStd = CreateSchemaWithAttributes(potype);
            return tableStd;
        }




        private static System.Data.DataTable CreateSchemaWithAttributes(DataPO po)
        {
            Type potype = po.GetType();
            return CreateSchemaWithAttributes(potype);
        }




        private static System.Data.DataTable CreateSchemaWithAttributes(Type potype)
        {
            System.Data.DataTable table = null;
            try
            {

                System.Attribute tableAttribute = APP.CODE.AttributesTools.GetAttribute(potype, DataAnnotationsFactory.TableAttributeType);
                if (tableAttribute == null) return null;
                string tableName = APP.CODE.PropertiesTools.GetString(tableAttribute, "Name");

                List<System.Data.DataColumn> Cols = new List<System.Data.DataColumn>();
                List<System.Data.DataColumn> ColKeys = new List<System.Data.DataColumn>();
                table = new DataTable();
                table.TableName = tableName;

                // Parcours des propriétés (cols)

                List<PropertyInfo> props = APP.CODE.PropertiesTools.GetProperties(potype).ToList();
                foreach (PropertyInfo prop in props)
                {
                    System.Data.DataColumn col = null;
                    //clef

                    if (DataAnnotationsFactory.AnnotationExist(prop, DataAnnotationsFactory.KeyAttributeType))
                    {
                        if (col == null)
                        {
                            col = new DataColumn();
                            col.ColumnName = prop.Name.ToLower();
                            if (prop.PropertyType.ToString().Contains("ParamValuesPOFlux")) // les flux seront consiédéré comme des string
                                col.DataType = typeof(string);
                            else col.DataType = prop.PropertyType;
                        }
                        ColKeys.Add(col);
                    }

                    // Création des colones
                    if (DataAnnotationsFactory.AnnotationExist(prop, DataAnnotationsFactory.ColumnAttributeType))
                    {
                        if (col == null)
                        {
                            col = new DataColumn();
                            col.ColumnName = prop.Name.ToLower();
                            if (prop.PropertyType.ToString().Contains("ParamValuesPOFlux")) // les flux seront consiédéré comme des string
                                col.DataType = typeof(string);
                            else col.DataType = prop.PropertyType;
                        }
                        string RealColumnName = DataAnnotationsFactory.AnnotationGetString(prop, DataAnnotationsFactory.ColumnAttributeType, "Name");
                        if (!string.IsNullOrWhiteSpace(RealColumnName))
                            col.ColumnName = RealColumnName;
                        //if (!string.IsNullOrWhiteSpace(columnAttribute.TypeName))
                        //    ;
                    }


                    string DatabaseGeneratedOption = DataAnnotationsFactory.AnnotationGetString(prop, DataAnnotationsFactory.DatabaseGeneratedAttributeType, "DatabaseGeneratedOption");
                    if (!string.IsNullOrEmpty(DatabaseGeneratedOption) && !DatabaseGeneratedOption.Equals("None", StringComparison.OrdinalIgnoreCase ))
                    {
                        if (col != null) col.AutoIncrement = true;
                    }


                    //System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute foreignKeyAttribute = propAttributes.FirstOrDefault(at => (at is ForeignKeyAttribute)) as ForeignKeyAttribute;
                    //System.ComponentModel.DataAnnotations.DataTypeAttribute dataTypeAttribute = propAttributes.FirstOrDefault(at => (at is DataTypeAttribute)) as DataTypeAttribute;
                    //System.ComponentModel.DataAnnotations.DisplayColumnAttribute displayColumnAttribute = propAttributes.FirstOrDefault(at => (at is DisplayColumnAttribute)) as DisplayColumnAttribute;
                    //System.ComponentModel.DataAnnotations.EditableAttribute editableAttribute = propAttributes.FirstOrDefault(at => (at is EditableAttribute)) as EditableAttribute;
                    //System.ComponentModel.DataAnnotations.RequiredAttribute requiredAttribute = propAttributes.FirstOrDefault(at => (at is RequiredAttribute)) as RequiredAttribute;


                    // Objets lié
                    //System.ComponentModel.DataAnnotations.AssociationAttribute associationAttribute = propAttributes.FirstOrDefault(at => (at is AssociationAttribute)) as AssociationAttribute;


                    if (col != null) Cols.Add(col);


                }


                // défini les colonnes sur la table
                DataSetTools.SetColumns(table, Cols.ToArray());
                DataSetTools.SetPrimaryKeys(table, ColKeys.ToArray());
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception("DefineRowWithAttributes" + ex.Message);
            }
        }






        //private static System.Data.DataTable CreateSchemaWithAttributes(Type potype)
        //{
        //    System.Data.DataTable table = null;
        //    System.ComponentModel.DataAnnotations.Schema.TableAttribute tableAttribute = null; // obgligatoire pour un datapo

        //    try
        //    {


        //        // obtient l'attribut principal
        //        object[] attrsclass = potype.GetCustomAttributes(true);
        //        if (attrsclass == null) return null; // pas d'attribut
        //        tableAttribute = (TableAttribute)attrsclass.FirstOrDefault(at => (at is TableAttribute));
        //        if (tableAttribute == null) return null; // attribut non présent

        //        table = new DataTable();
        //        table.TableName = tableAttribute.Name;
        //        // if (string.IsNullOrWhiteSpace(table.TableName)) return null; // le 


        //        // Parcours des propriétés (cols)
        //        List<System.Data.DataColumn> Cols = new List<System.Data.DataColumn>();
        //        List<System.Data.DataColumn> ColKeys = new List<System.Data.DataColumn>();
        //        List<PropertyInfo> props = GetDeclaredPropertys(potype);
        //        foreach (PropertyInfo prop in props)
        //        {
        //            List<System.Attribute> propAttributes = prop.GetCustomAttributes(true).Cast<System.Attribute>().ToList();
        //            if (propAttributes == null || propAttributes.Count == 0) continue;
        //            System.Data.DataColumn col = null; // colonne de l'attribut

        //            //clef
        //            KeyAttribute keyAttribute = propAttributes.FirstOrDefault(at => (at is KeyAttribute)) as KeyAttribute;
        //            if (keyAttribute != null)
        //            {
        //                if (col == null)
        //                {
        //                    col = new DataColumn();
        //                    col.ColumnName = prop.Name.ToLower();
        //                    col.DataType = prop.PropertyType;
        //                }
        //                ColKeys.Add(col);
        //            }

        //            // Création des colones
        //            ColumnAttribute columnAttribute = propAttributes.FirstOrDefault(at => (at is ColumnAttribute)) as ColumnAttribute;
        //            if (columnAttribute != null)
        //            {
        //                if (col == null)
        //                {
        //                    col = new DataColumn();
        //                    col.ColumnName = prop.Name.ToLower();
        //                    col.DataType = prop.PropertyType;
        //                }
        //                if (!string.IsNullOrWhiteSpace(columnAttribute.Name))
        //                    col.ColumnName = columnAttribute.Name;
        //                if (!string.IsNullOrWhiteSpace(columnAttribute.TypeName))
        //                    ;
        //            }



        //            DatabaseGeneratedAttribute databaseGeneratedAttribute = propAttributes.FirstOrDefault(at => (at is DatabaseGeneratedAttribute)) as DatabaseGeneratedAttribute;
        //            if (databaseGeneratedAttribute != null && databaseGeneratedAttribute.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
        //            {
        //                if (col != null) col.AutoIncrement = true;
        //            }


        //            System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute foreignKeyAttribute = propAttributes.FirstOrDefault(at => (at is ForeignKeyAttribute)) as ForeignKeyAttribute;
        //            System.ComponentModel.DataAnnotations.DataTypeAttribute dataTypeAttribute = propAttributes.FirstOrDefault(at => (at is DataTypeAttribute)) as DataTypeAttribute;
        //            System.ComponentModel.DataAnnotations.DisplayColumnAttribute displayColumnAttribute = propAttributes.FirstOrDefault(at => (at is DisplayColumnAttribute)) as DisplayColumnAttribute;
        //            System.ComponentModel.DataAnnotations.EditableAttribute editableAttribute = propAttributes.FirstOrDefault(at => (at is EditableAttribute)) as EditableAttribute;
        //            System.ComponentModel.DataAnnotations.RequiredAttribute requiredAttribute = propAttributes.FirstOrDefault(at => (at is RequiredAttribute)) as RequiredAttribute;


        //            // Objets lié
        //            System.ComponentModel.DataAnnotations.AssociationAttribute associationAttribute = propAttributes.FirstOrDefault(at => (at is AssociationAttribute)) as AssociationAttribute;


        //            if (col != null) Cols.Add(col);


        //        }


        //        // défini les colonnes sur la table
        //        DataSetTools.SetColumns(table, Cols.ToArray());
        //        DataSetTools.SetPrimaryKeys(table, ColKeys.ToArray());
        //        return table;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("DefineRowWithAttributes" + ex.Message);
        //    }
        //}




        //private static System.Data.DataTable CreateSchemaWithAttributes(Type potype)
        //{
        //    System.Data.DataTable table = null;
        //    DATAPO.ObjectPOAttribute poattribute = null;
        //    try
        //    {


        //        // obtient l'attribut principal
        //        object[] attrsclass = potype.GetCustomAttributes(true);
        //        if (attrsclass == null) return null; // pas d'attribut
        //        foreach (System.Attribute propattr in attrsclass)
        //        {
        //            if (!(propattr is IPoAttribute)) continue;
        //            if (propattr is DATAPO.ObjectPOAttribute)
        //            {
        //                poattribute = (ObjectPOAttribute)propattr;
        //                break;
        //            }
        //        }
        //        if (poattribute == null) return null; // attribut non présent
        //        table = new DataTable();

        //        table.TableName = poattribute.TableName;
        //        // if (string.IsNullOrWhiteSpace(table.TableName)) return null; // le 


        //        // Parcours des propriétés (cols)
        //        List<System.Data.DataColumn> Cols = new List<System.Data.DataColumn>();
        //        List<System.Data.DataColumn> ColKeys = new List<System.Data.DataColumn>();
        //        List<PropertyInfo> props = GetDeclaredPropertys(potype);
        //        foreach (PropertyInfo prop in props)
        //        {
        //            object[] attrs = prop.GetCustomAttributes(true);
        //            if (attrs == null) continue;
        //            foreach (System.Attribute propattr in attrs)
        //            {
        //                if (!(propattr is IPoAttribute)) continue;
        //                if (propattr is DATAPO.ColumnPOAttribute)
        //                {
        //                    ColumnPOAttribute propa = (ColumnPOAttribute)propattr;
        //                    Type colType = propa.ColumnType;
        //                    if (colType == null) colType = prop.PropertyType;
        //                    string colname = propa.ColumnName;
        //                    if (string.IsNullOrWhiteSpace(colname)) colname = prop.Name;
        //                    System.Data.DataColumn col = new DataColumn(colname, colType);
        //                    col.AutoIncrement = propa.AutoIncrement;
        //                    Cols.Add(col);
        //                    if (propa.PrimaryKey) ColKeys.Add(col);
        //                }
        //                else if (propattr is DATAPO.JoinPOAttribute)
        //                {
        //                }
        //                else if (propattr is DATAPO.DataPocoAttribute)
        //                {
        //                }
        //            }

        //        }


        //        // défini les colonnes sur la table
        //        DataSetTools.SetColumns(table, Cols.ToArray());
        //        DataSetTools.SetPrimaryKeys(table, ColKeys.ToArray());
        //        return table;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("DefineRowWithAttributes" + ex.Message);
        //    }
        //}







        #endregion

    }
}
