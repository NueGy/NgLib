using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.CONNECTOR
{
    public static class ConnectorGenericFactory
    {

        /// <summary>
        /// Identification du moteur sql
        /// </summary>
        /// <param name="enginename"></param>
        /// <returns></returns>
        public static ConnectorConstants.ConnectorEngineEnum FindEngine(string engineName)
        {
            if (string.IsNullOrWhiteSpace(engineName)) return ConnectorConstants.ConnectorEngineEnum.NA;
            engineName = engineName.ToLower();
            if (new List<string>() { "mssql", "Sqlclient", "system.data.sqlclient" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.MSSQL;
            else if (new List<string>() { "npgsql", "postgresql" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.POSTGRESQL;
            else if (new List<string>() { "sqlite", "sqllite" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.SQLITE;
            else return ConnectorConstants.ConnectorEngineEnum.NA;
        }


        /// <summary>
        /// Retourne le IDbConnection par factory de la bonne librairie SGBD
        /// </summary>
        public static System.Data.IDbConnection ConnectionFactory(string engineName)
        {
            if (string.IsNullOrWhiteSpace(engineName)) throw new Exception("EngineName empty");
            engineName = engineName.ToLower().Trim();
            System.Data.IDbConnection retour = null;

            if (new List<string>() { "mssql", "Sqlclient", "system.data.sqlclient" }.Contains(engineName))
            {
                retour = NewInstanceByReflexion<System.Data.IDbConnection>("System.Data.SqlClient.SqlConnection, System.Data");
                if (retour == null) retour = NewInstanceByReflexion<System.Data.IDbConnection>("System.Data.SqlClient.SqlConnection, System.Data.SqlClient");
            }
            else if (new List<string>() { "npgsql", "postgresql" }.Contains(engineName)) retour = NewInstanceByReflexion<System.Data.IDbConnection>("Npgsql.NpgsqlConnection, Npgsql");
            else if (new List<string>() { "sqlite", "sqllite" }.Contains(engineName)) retour = NewInstanceByReflexion<System.Data.IDbConnection>("System.Data.SQLite.SQLiteConnection, System.Data.SQLite");
            else throw new Exception("EngineName not found (IDbConnection)");

            if (retour == null) throw new Exception(string.Format("Engine/DLL IDbConnection for {0} not found. Please include DLL for this engine in your project", engineName));

            return retour;
        }


        public static System.Data.IDataAdapter DataAdapterFactory(string engineName, System.Data.IDbCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(engineName)) return null;
            engineName = engineName.ToLower().Trim();
            System.Data.IDataAdapter retour = null;

            if (new List<string>() { "mssql" }.Contains(engineName))
            {
                retour = NewInstanceByReflexion<System.Data.IDataAdapter>("System.Data.SqlClient.SqlDataAdapter, System.Data");
                if (retour == null) retour = NewInstanceByReflexion<System.Data.IDataAdapter>("System.Data.SqlClient.SqlDataAdapter, System.Data.SqlClient");
            }
            else if (new List<string>() { "npgsql", "postgresql" }.Contains(engineName)) retour = NewInstanceByReflexion<System.Data.IDataAdapter>("Npgsql.NpgsqlDataAdapter, Npgsql");
            else if (new List<string>() { "sqllite" }.Contains(engineName)) retour = NewInstanceByReflexion<System.Data.IDataAdapter>("System.Data.SQLite.SQLiteDataAdapter, System.Data.SQLite");
            else throw new Exception("EngineName not found");

            if (retour == null) return null;
            if (retour is System.Data.IDbDataAdapter)
                ((System.Data.IDbDataAdapter)retour).SelectCommand = cmd;


            return retour;
        }

        ///// <summary>
        ///// Créer un parametre, avec la gestion du type
        ///// </summary>
        ///// <param name="cmd"></param>
        ///// <param name="ParameterName"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static System.Data.IDataParameter AddDataParameterWithValue(System.Data.IDbCommand cmd, string ParameterName, object value)
        //{
        //    // !!! améliorer les perf en mettant en cache le type
        //    System.Data.IDataParameter retour = null;

        //    //!!!! : l'ajout d'un type xml pose probleme, le sgbd le détecte comme un string

        //    var parameters = cmd.Parameters;

        //    Type parameterstype = parameters.GetType();
        //    var members = parameterstype.GetMethods();

        //    var addwithvalueMethod = members.FirstOrDefault(m => m.ToString().Equals("Npgsql.NpgsqlParameter AddWithValue(System.String, System.Object)"));
        //    if (addwithvalueMethod != null) // invocation par réflection
        //        retour = addwithvalueMethod.Invoke(parameters, new object[] { ParameterName, value }) as System.Data.IDataParameter;
        //    //!!! et SQL server?

        //    if (retour == null) // création simple
        //    {
        //        retour = cmd.CreateParameter();
        //        retour.ParameterName = ParameterName;
        //        retour.Value = value;
        //        parameters.Add(retour);
        //    }



        //    return retour;
        //}


        //public static System.Data.IDbTransaction TransactionFactory(string engineName)
        //{
        //    if (string.IsNullOrWhiteSpace(engineName)) return null;
        //    engineName = engineName.ToLower().Trim();
        //    System.Data.IDbTransaction retour = null;

        //    return retour;
        //}
        //public static System.Data.IDbCommand CommandFactory(string engineName)
        //{
        //    if (string.IsNullOrWhiteSpace(engineName)) return null;
        //    engineName = engineName.ToLower().Trim();
        //    System.Data.IDbCommand retour = null;

        //    return retour;
        //}












        private static T NewInstanceByReflexion<T>(string MyFullyQualifiedTypeName)
        {
            try
            {
                Type itelType = Type.GetType(MyFullyQualifiedTypeName, false, true);
                if (itelType == null) throw new Exception("Type not found = DLL not load ?");
                T retour = (T)Activator.CreateInstance(itelType);
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("NewInstanceByReflexion: {0}  ({1})", ex.Message, MyFullyQualifiedTypeName));
            }

        }



    }
}
