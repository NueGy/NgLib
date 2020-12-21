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
                    string[] lineT = SplitCsvLine(line);

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







        public static string[] SplitCsvLine(string line)
        {
            string[] retour = line.Split(DefaultCsvSeparator);
            return retour;
        }


    }
}
