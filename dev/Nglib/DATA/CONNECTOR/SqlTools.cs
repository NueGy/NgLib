// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Permet d'aider dans la conception d'une requette SQL (remplacer par un sqlbuilder)
    /// </summary>
    public class SqlTools
    {


        /// <summary>
        /// Savoir Si il s'agit d'une requette
        /// </summary>
        public static bool IsSQLQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;
            if (sql.Trim().Contains(" ")) return true;
            else return false;
        }



        /// <summary>
        /// création de ('value1','value2','valueN')
        /// </summary>
        /// <param name="chaine">séparateur ;</param>
        /// <returns></returns>
        public static string ConvertToinsql(string chainecsv)
        {
            List<string> elements = chainecsv.Split(';').ToList();
            return ConvertToinsql(elements);
        }

        /// <summary>
        /// création de ('value1','value2','valueN')
        /// </summary>
        /// <param name="chaine"></param>
        /// <returns></returns>
        public static string ConvertToinsql(List<string> elements)
        {
            string retour = " (";
            string virgule = "";
            foreach (string item in elements)
            {
                if (string.IsNullOrEmpty(item)) continue;
                retour += virgule + "'" + item + "'";
                virgule = ",";
            }
            retour += ") ";
            return retour;
        }

        /// <summary>
        /// création de (value1,value2,valueN)
        /// </summary>
        /// <param name="chaine"></param>
        /// <returns></returns>
        public static string ConvertToinsql(List<int> elements)
        {
            string retour = " (";
            string virgule = "";
            foreach (int item in elements)
            {
                if (item == 0) continue;
                retour += virgule + item;
                virgule = ",";
            }
            retour += ") ";
            return retour;
        }

        /// <summary>
        /// création de (value1,value2,valueN)
        /// </summary>
        /// <param name="chaine"></param>
        /// <returns></returns>
        public static string ConvertToinsql(List<long> elements)
        {
            string retour = " (";
            string virgule = "";
            foreach (int item in elements)
            {
                if (item == 0) continue;
                retour += virgule + item;
                virgule = ",";
            }
            retour += ") ";
            return retour;
        }


        public static string GenerateCreateWhereSQL(string[] keys, string useTable = "", bool AppendDynamicWhere = false)
        {
            string sqlwhere = "";
            string virgule = "";
            if (!string.IsNullOrWhiteSpace(useTable) && !useTable.Contains('.')) useTable += ".";
            foreach (string itemkey in keys)
            {
                sqlwhere += virgule + useTable;

                if (AppendDynamicWhere && itemkey.EndsWith("_wheremin")) sqlwhere += itemkey.Replace("_wheremin", "") + " >= @" + itemkey;
                else if (AppendDynamicWhere && itemkey.EndsWith("_wheremax")) sqlwhere += itemkey.Replace("_wheremax", "") + " <= @" + itemkey;
                else if (AppendDynamicWhere && itemkey.EndsWith("_wherein")) continue;
                else sqlwhere += itemkey.ToLower() + "=@" + itemkey;
                virgule = " AND ";
            }
            return sqlwhere;
        }



        public static string GenerateCreateWhereSQL(Dictionary<string, object> WhereObjects, string useTable = "", bool FieldDynamicWhere = false)
        {
            string sqlwhere = "";
            string virgule = "";
            if (!string.IsNullOrWhiteSpace(useTable) && !useTable.Contains('.')) useTable += ".";
            foreach (string itemkey in WhereObjects.Keys)
            {
                sqlwhere += virgule + useTable;

                if (FieldDynamicWhere && itemkey.EndsWith("_wheremin")) sqlwhere += itemkey.Replace("_wheremin", "") + " >= @" + itemkey;
                else if (FieldDynamicWhere && itemkey.EndsWith("_wheremax")) sqlwhere += itemkey.Replace("_wheremax", "") + " <= @" + itemkey;
                else if (FieldDynamicWhere && itemkey.EndsWith("_wherein") && WhereObjects[itemkey] != null) sqlwhere += itemkey.Replace("_wherein", "") + " in " + ConvertToinsql(WhereObjects[itemkey].ToString());
                else sqlwhere += itemkey.ToLower() + "=@" + itemkey;
                virgule = " AND ";
            }
            return sqlwhere;
        }



        private static List<string> DynamicWhereCleanupCols(List<string> input)
        {
            List<string> retour = new List<string>();

            foreach (string item in input)
            {
                string ifield =item;
                ifield=ifield.Replace("_wheremax","");
                ifield = ifield.Replace("_wheremin", "");
                ifield = ifield.Replace("_wherein", "");
                if (!retour.Contains(ifield)) retour.Add(ifield);
            }

            return retour;
        }







        //public static string GenerateQuerySQL(DATA.CONNECTOR.IDataConnector Connector, Dictionary<string, object> searchWhere, string TableName, List<string> SelectCols = null, int nbMax = 0, bool FieldDynamicWhere = false, List<string> aggregationCols = null, List<string> Orderby = null)
        //{
        //    string sqlQuery = "";
        //    string sel = "*";
        //    string selgrp = "";


        //    List<string> LSelectCols = null;
        //    if (FieldDynamicWhere) LSelectCols = DynamicWhereCleanupCols(SelectCols);
        //    else LSelectCols = aggregationCols;


        //    //string virgule = "";

        //    if (aggregationCols == null && LSelectCols != null && LSelectCols.Count > 0)
        //    {
        //        // on force l'ajout de clef
        //        sel = "";
        //        string virgule = "";
        //        foreach (string item in LSelectCols)
        //        {
        //            sel += virgule + item.ToLower(); virgule = ",";
        //        }

        //        // cols valeurs 
        //        //foreach (string itemkey in searchWhere.Keys) if (!LSelectCols.Contains(itemkey)) LSelectCols.Add(itemkey);
        //    }
        //    else if (aggregationCols!=null)
        //    {
        //        List<string> aggregationColsclean = null;
        //        if (FieldDynamicWhere) aggregationColsclean = DynamicWhereCleanupCols(aggregationCols);
        //        else aggregationColsclean = aggregationCols;


        //        sel = "";
        //        string virgule = "";

        //        virgule = "";
        //        foreach (string item in aggregationColsclean)
        //        {
        //            selgrp += virgule + "" + item.ToLower() + "";
        //            virgule = ",";
        //        }

        //        virgule = "";
        //        foreach (string item in LSelectCols)
        //        {
        //            if (aggregationColsclean.Contains(item))
        //                sel += virgule + "" + item.ToLower() + "";
        //            else
        //                sel += virgule + "SUM(" + item.ToLower() + ") as agreg" + item.ToLower() + "";
        //            virgule = ",";
        //        }
        //    }


        //    if (nbMax>0)sqlQuery = "SELECT TOP "+nbMax+" ";
        //    else sqlQuery = "SELECT ";

        //    sqlQuery += sel + " FROM " + TableName + " WHERE " + DATA.CONNECTOR.SqlTools.GenerateCreateWhereSQL(searchWhere, "", FieldDynamicWhere);



        //    if (aggregationCols != null) sqlQuery += " GROUP BY  " + selgrp;
        //    if (Orderby != null) { } //!!!
        //    if (Orderby==null && aggregationCols != null) sqlQuery += " ORDER BY  " + selgrp;

        //     return sqlQuery;
        //}












        /*
        public static string GenerateCreateWhereSQL(Dictionary<string,string> keysvalues, string useTable = "", bool ecriredirectement = false)
        {
            string sqlwhere = "";
            string virgule = "";
            if (!string.IsNullOrWhiteSpace(useTable) && !useTable.Contains('.')) useTable += ".";
            foreach (DATA.DATAVALUES.DataValues_data item in keysvalues)
            {
                if (!ecriredirectement) sqlwhere += virgule + useTable + item.NameMinimal + "=@" + item.NameMinimal;
                else
                {
                    if (item.value == null) sqlwhere += virgule + useTable + item.NameMinimal + "=null";
                    else sqlwhere += virgule + useTable + item.NameMinimal + "='" + item.value.ToString() + "'";
                }
                virgule = " AND ";
            }
            return sqlwhere;
        }*/





        /*

    public static string GenerateInsertSQL(string tablebd, string[] keys)
    {
        string retour = "INSERT INTO " + tablebd + " (";
        string virgule = "";
        foreach (string itemkey in keys)
        {
            retour += virgule + itemkey.ToLower();
            virgule = ", ";
        }
        retour += ") VALUES (";
        virgule = "";

        foreach (string itemkey in keys)
        {
            retour += virgule + "@" + itemkey + "";
            virgule = ", ";
        }
        retour += ")";
        return retour;
    }


    public static string GenerateDeleteSQL(string tablebd, string[] keys)
    {

        string sql = "DELETE FROM " + tablebd + " ";
        if (keys != null)
        {
            sql += " WHERE " + GenerateCreateWhereSQL(keys);
        }
        return sql;

    }
    

        public static string GenerateUpdateSQL(string tablebd, string[] Values, string[] keysWhere)
        {

            string sql = "UPDATE " + tablebd + " SET ";
            string virgule = "";
            foreach (string itemKeyValues in Values)
            {
                sql += virgule + itemKeyValues.ToLower() + "=@" + itemKeyValues;
                virgule = " , ";
            }
            virgule = "";
            sql += " WHERE " + GenerateCreateWhereSQL(keysWhere);
            return sql;

        }



*/











        //public static List<string> ObtainMSSQLListPrimaryKey(CONNECTOR.IDataConnector connector, string TableName)
        //{

        //    string sql = "SELECT Col.Column_Name FROM     INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,     INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col ";
        //    sql += " WHERE Col.Constraint_Name = Tab.Constraint_Name    AND Col.Table_Name = Tab.Table_Name    AND Constraint_Type = 'PRIMARY KEY'    AND Col.Table_Name = '" + TableName + "'";

        //    System.Data.DataTable retPrimKey = connector.Query(sql, null);
        //    List<string> retour = new List<string>();
        //    foreach (System.Data.DataRow item in retPrimKey.Rows)
        //    {
        //        string primaryJey = Convert.ToString(item[0]);
        //        if (!string.IsNullOrWhiteSpace(primaryJey)) retour.Add(primaryJey);
        //    }
        //    return retour;
        //}





        //public static void SqlWhereDateIndexBetween(string DateMin, string DateMax, string FieldNameDb,
        //                                    ref List<string> wheres, ref Dictionary<string, object> ins, bool NoTime = false, bool DateSafe = false)
        //{
        //    DateTime? iDateMin = null;
        //    DateTime? iDateMax = null;

        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(DateMin))
        //            iDateMin = FORMAT.ConvertPlus.ToDateTime(DateMin);
        //        if (!string.IsNullOrWhiteSpace(DateMax))
        //            iDateMax = FORMAT.ConvertPlus.ToDateTime(DateMax);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!DateSafe) throw new Exception("Date format invalide " + ex.Message);
        //    }

        //    SqlWhereDateIndexBetween(iDateMin, iDateMax, FieldNameDb, ref wheres, ref ins, NoTime);
        //}




        /// <summary>
        /// Permet de générer un code SQL d'INSERT multilignes
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static Tuple<string, Dictionary<string, object>> GenerateSqlMultiInsert(System.Data.DataTable retSrc, List<string> removeColumn = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(retSrc.TableName)) throw new Exception("TableName Source Empty");
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> ins = new Dictionary<string, object>();

                sql.AppendFormat("INSERT INTO {0} ", retSrc.TableName);

                // CREATION PARTIE LIST COLUMN
                string virgule = "";
                sql.Append("(");
                foreach (System.Data.DataColumn col in retSrc.Columns)
                {
                    sql.AppendFormat("{0}{1}", virgule, col.ColumnName);
                    virgule = ",";
                }
                sql.Append(")");


                // PARTIE DONNEES
                sql.AppendLine(" VALUES ");
                int ii = 1;
                virgule = "";
                foreach (System.Data.DataRow rowdata in retSrc.Rows)
                {

                    List<string> ipartjoi = new List<string>();
                    foreach (System.Data.DataColumn col in retSrc.Columns)
                    {
                        string iisnjey = "p" + ii; ii++;
                        ins.Add(iisnjey, rowdata[col]); // !!! optimiser pour réutiliser les même données déja dans le dictionary et économiser la bande passante
                        ipartjoi.Add("@" + iisnjey);
                    }


                    sql.AppendFormat("{0}(", virgule);
                    virgule = ",";
                    sql.Append(string.Join(",", ipartjoi.ToArray()));
                    sql.Append(")");
                }



                Tuple<string, Dictionary<string, object>> retour = new Tuple<string, Dictionary<string, object>>(sql.ToString(), ins);
                return retour;
            }
            catch (Exception)
            {

                throw;
            }



        }




    }
}
