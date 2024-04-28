using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Outils statiques pour les connecteurs
    /// </summary>
    public static class ConnectorTools
    {


        /// <summary>
        /// Identification du moteur sql
        /// </summary>
        /// <param name="enginename"></param>
        /// <returns></returns>
        public static ConnectorConstants.ConnectorEngineEnum FindEngine(string engineName)
        {
            if (string.IsNullOrWhiteSpace(engineName)) return ConnectorConstants.ConnectorEngineEnum.NA;
            engineName = engineName.ToLower().Trim();
            if (new List<string>() { "mssql", "Sqlclient", "system.data.sqlclient" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.MSSQL;
            else if (new List<string>() { "npgsql", "postgresql" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.POSTGRESQL;
            else if (new List<string>() { "sqlite", "sqllite" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.SQLITE;
            else if (new List<string>() { "access", "msaccess", "oledb" }.Contains(engineName)) return ConnectorConstants.ConnectorEngineEnum.ACCESS;
            else return ConnectorConstants.ConnectorEngineEnum.NA;
        }




        /// <summary>
        /// Retourne le IDbConnection par factory de la bonne librairie SGBD
        /// </summary>
        public static System.Data.IDbConnection ConnectionFactory(string engineName)
        {
            ConnectorConstants.ConnectorEngineEnum engine = FindEngine(engineName);
            if (engine == ConnectorConstants.ConnectorEngineEnum.NA) throw new Exception("EngineName empty");

            Type retour = null;

            //System.Data.OleDb.OleDbConnection

            if (engine == ConnectorConstants.ConnectorEngineEnum.MSSQL)
            {
                retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlConnection, System.Data");
                if (retour == null) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlConnection, System.Data.SqlClient");
            }
            else if (engine == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL) retour = Nglib.APP.CODE.ReflectionTools.GetType("Npgsql.NpgsqlConnection, Npgsql");
            else if (engine == ConnectorConstants.ConnectorEngineEnum.SQLITE) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SQLite.SQLiteConnection, System.Data.SQLite");
            else if (engine == ConnectorConstants.ConnectorEngineEnum.ORACLE) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OracleClient.OracleConnection, System.Data.OracleClient");
            else if (engine == ConnectorConstants.ConnectorEngineEnum.ACCESS)
            {
                retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbConnection, System.Data");
                if (retour == null) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbConnection, System.Data.OleDb");
                if (retour == null) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbConnection, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"); // !!!
            }
            else throw new Exception("EngineName not found (IDbConnection)");



            if (retour == null) throw new Exception(string.Format("Engine/DLL IDbConnection for {0} not found. Please include DLL for this engine in your project", engineName));
            return Nglib.APP.CODE.ReflectionTools.CreateInstance<System.Data.IDbConnection>(retour);
        }


        public static System.Data.IDataAdapter DataAdapterFactory(string engineName, System.Data.IDbCommand cmd)
        {
            ConnectorConstants.ConnectorEngineEnum engine = FindEngine(engineName);
            Type retour = null;

            if (engine == ConnectorConstants.ConnectorEngineEnum.MSSQL)
            {
                retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlDataAdapter, System.Data");
                if (retour == null) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlDataAdapter, System.Data.SqlClient");
            }
            else if (engine == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL) retour = Nglib.APP.CODE.ReflectionTools.GetType("Npgsql.NpgsqlDataAdapter, Npgsql");
            else if (engine == ConnectorConstants.ConnectorEngineEnum.SQLITE) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SQLite.SQLiteDataAdapter, System.Data.SQLite");
            else if (engine == ConnectorConstants.ConnectorEngineEnum.ORACLE) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OracleClient.OracleDataAdapter, System.Data.OracleClient");
            else if (engine == ConnectorConstants.ConnectorEngineEnum.ACCESS)
            {
                retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbDataAdapter, System.Data");
                if (retour == null) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbDataAdapter, System.Data.OleDb");
                if (retour == null) retour = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbDataAdapter, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"); // !!!
            }

            if (retour == null) throw new Exception(string.Format("Engine/DLL IDataAdapter for {0} not found. Please include DLL for this engine in your project", engineName));

            System.Data.IDbDataAdapter val = Nglib.APP.CODE.ReflectionTools.CreateInstance<System.Data.IDbDataAdapter>(retour);
            val.SelectCommand = cmd;
            return val;
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









        //private static T NewInstanceByReflexion<T>(string MyFullyQualifiedTypeName)
        //{
        //    try
        //    {
        //        Type itelType = Type.GetType(MyFullyQualifiedTypeName, false, true);
        //        //if (itelType == null) throw new Exception("Type not found = DLL not load ?");
        //        T retour = (T)Activator.CreateInstance(itelType);
        //        return retour;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(string.Format("NewInstanceByReflexion: {0}  ({1})", ex.Message, MyFullyQualifiedTypeName));
        //    }

        //}

    }
}
