using Nglib.APP.CODE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.FILES.SERIAL
{
    /// <summary>
    /// Outils pour gérer un flux CSV
    /// </summary>
    public static class CsvTools
    {
        public const char DefaultCsvSeparator = ';';

        //SOON
        /*
        Création d’un CSV reader avec attribute ( remove header line, text type, startwith, …)
        StartwithRegex
        */



        /// <summary>
        /// Header Line ;;;
        /// </summary>
        public static string GetHeaderLine<T>(T obj) where T : class, new()
        {
            var columns = APP.CODE.PropertiesTools.GetProperties(typeof(T)).Select(a => a.Name).ToArray();
            return string.Join(DefaultCsvSeparator.ToString(), columns);
            //  return Nglib.APP.CODE.AttributesTools.GetPropertiesDescriptions(T.GetType();
        }




        public class ParseCsvParameter 
        {
            public bool FirstLineHeader = true;
            public char CsvSeparator = ';';
        }




        public static System.Data.DataTable ParseCsv(string csvContent, ParseCsvParameter parameter=null)
        {
            if(csvContent==null)return null;
            try
            {
                if(parameter == null) parameter = new ParseCsvParameter();
                string[] lines = csvContent.Split(Environment.NewLine);
                System.Data.DataTable dataTable = new System.Data.DataTable();


                if (parameter.FirstLineHeader)
                {
                    string[] firstlineParts = lines[0].Split(parameter.CsvSeparator);
                    int coli = 0;
                    foreach (string firstlinePart in firstlineParts)
                    {
                        coli++;
                        System.Data.DataColumn col = new System.Data.DataColumn();
                        col.ColumnName = Nglib.FORMAT.StringTools.CleanString(firstlinePart).ToUpper();
                        if (string.IsNullOrWhiteSpace(col.ColumnName)) col.ColumnName = $"C"+ coli.ToString();
                        if(dataTable.Columns.Contains(col.ColumnName)) col.ColumnName = col.ColumnName+$"C" + coli.ToString();
                        dataTable.Columns.Add(col);
                    }
                }
                else
                {   // mode simple
                    int nbcolcont = lines[0].Split(parameter.CsvSeparator).Count();
                    for (int i = 1; i <= nbcolcont; i++)
                        dataTable.Columns.Add("C"+ i.ToString());
                }

                int colsCount = dataTable.Columns.Count;
                int rowi = 0;
                foreach (string line in lines)
                {
                    rowi++;
                    if (rowi == 1 && parameter.FirstLineHeader) continue;
                    if (string.IsNullOrWhiteSpace(line)) continue; // empty
                    string[] partline = line.Split(parameter.CsvSeparator);
                    if (partline.Length != colsCount) throw new Exception($"LINE{rowi} invalideCsvParts");
                    System.Data.DataRow row= dataTable.NewRow();
                    row.ItemArray = partline.Cast<object>().ToArray();
                    dataTable.Rows.Add(row);
                }   

                return dataTable;
            }
            catch (Exception ex)
            {
                throw;
            }
        }




        public static string[] SerializeLines(params object[] objs)
        {
            if (objs == null) return null;
            if (objs.Length == 0) return new string[0];
            Type objtype = objs.FirstOrDefault().GetType();
            try
            {
                var columns = APP.CODE.PropertiesTools.GetProperties(objtype);
                List<string> retour = new List<string>();
                foreach (var obj in objs)
                {
                    List<string> linec = new List<string>();
                    foreach (var col in columns)
                    {
                        object val = col.GetValue(obj, null);
                        if (val == null) linec.Add("");
                        else linec.Add(Convert.ToString(val)); /// !!! amélioration
                    }

                    string linetext = string.Join(DefaultCsvSeparator.ToString(), linec);
                    retour.Add(linetext);
                }

                return retour.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"SerializeLinesCSV({objtype.Name}) {ex.Message}",ex);
            }
        }


        public static T[] DeSerializeLines<T>(params string[] lines) where T : class, new()
        {
            try
            {
                var columns = APP.CODE.PropertiesTools.GetProperties(typeof(T));
                List<T> retour = new List<T>();
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] lineT = SplitCsv(line);

                    T reti = new T();// default(T);

                    int ii = 0;
                    foreach (var col in columns)
                    {
                        if ((lineT.Length + 1) < (ii)) break;
                        string val = lineT[ii];
                        // !!! gérer les diffée
                        col.SetValue(reti, val);
                        ii++;
                    }
                    retour.Add(reti);
                }

                return retour.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"DeSerializeLinesCSV({typeof(T).Name}) {ex.Message}", ex);
            }
        }


        /// <summary>
        /// permet de découper une chaine CSV
        /// </summary>
        public static string[] SplitCsv(string line, bool TrimAndClean = true, bool toUpper = false)
        {
            if (line == null) return null;
            if (string.IsNullOrWhiteSpace(line)) return new string[0];
            string[] retour = line.Split(DefaultCsvSeparator, StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (TrimAndClean)
            {
                retour = retour.Select(x => x.Replace("\r", "").Replace("\n", "").Replace("\t", "")
                                            .Trim().Trim('"').Trim('\'').Trim())
                    .Where(x => !string.IsNullOrEmpty(x)).ToArray();

            }
            if (toUpper)
            {
                retour = retour.Select(x => x.ToUpperInvariant()).ToArray();
            }
            return retour;
        }


        /// <summary>
        /// Permet de découper et simplifié la chaine, Separateur csv ';';
        /// Utilisé pour la gestion des tags, vides interdit
        /// </summary>
        public static string[] SplitTags(string line) => SplitCsv(line, true, true);


        public static string JoinCleanTags(string[] items)
        {
            if (items == null) return null;
            items = items.Where(x => x != null).ToArray();
            items = items.Where(x => !string.IsNullOrEmpty(x)).Select(x=>x.Trim().ToUpperInvariant()).ToArray();
            string retour = string.Join(DefaultCsvSeparator, items);
            return retour;
        }


        public static string CleanTags(string items)
        {
            var ttags = SplitTags(items);  
            return JoinCleanTags(ttags);
        }


    }
}
