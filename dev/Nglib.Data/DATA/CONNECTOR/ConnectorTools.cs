using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        /// Cache thread-safe des types IDbConnection pour éviter la réflexion répétée
        /// </summary>
        private static readonly ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type> connectionTypeCache 
            = new ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type>();

        /// <summary>
        /// Cache thread-safe des types IDataAdapter pour éviter la réflexion répétée
        /// </summary>
        private static readonly ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type> adapterTypeCache 
            = new ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type>();


        /// <summary>
        /// Identification du moteur sql
        /// </summary>
        public static ConnectorConstants.ConnectorEngineEnum ParseEngineName(string engineName)
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
        /// Utilise un cache pour optimiser les performances (évite la réflexion répétée)
        /// </summary>
        /// <param name="engineName">Nom du moteur SQL (postgresql, mssql, sqlite, etc.)</param>
        /// <returns>Instance de IDbConnection</returns>
        /// <exception cref="ArgumentException">Si le nom du moteur est invalide</exception>
        /// <exception cref="InvalidOperationException">Si la DLL du moteur n'est pas trouvée</exception>
        public static System.Data.IDbConnection ConnectionFactory(string engineName)
        {
            ConnectorConstants.ConnectorEngineEnum engine = ParseEngineName(engineName);
            if (engine == ConnectorConstants.ConnectorEngineEnum.NA) 
                throw new ArgumentException("EngineName empty or not recognized", nameof(engineName));

            // Récupération du type avec cache thread-safe
            Type connectionType = connectionTypeCache.GetOrAdd(engine, key => 
            {
                Type foundType = null;

                if (key == ConnectorConstants.ConnectorEngineEnum.MSSQL)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlConnection, System.Data");
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlConnection, System.Data.SqlClient");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("Npgsql.NpgsqlConnection, Npgsql");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.SQLITE)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SQLite.SQLiteConnection, System.Data.SQLite");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.ORACLE)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OracleClient.OracleConnection, System.Data.OracleClient");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.ACCESS)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbConnection, System.Data");
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbConnection, System.Data.OleDb");
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbConnection, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                }
                else
                {
                    throw new InvalidOperationException($"EngineName not found (IDbConnection): {key}");
                }

                if (foundType == null)
                    throw new InvalidOperationException(
                        $"Engine/DLL IDbConnection for {engineName} not found. Please include DLL for this engine in your project");

                return foundType;
            });

            return Nglib.APP.CODE.ReflectionTools.CreateInstance<System.Data.IDbConnection>(connectionType);
        }


        /// <summary>
        /// Factory IDataAdapter avec cache pour optimiser les performances
        /// </summary>
        /// <param name="engineName">Nom du moteur SQL</param>
        /// <param name="cmd">Commande SQL à associer à l'adapter</param>
        /// <returns>Instance de IDataAdapter</returns>
        /// <exception cref="InvalidOperationException">Si la DLL du moteur n'est pas trouvée</exception>
        public static System.Data.IDataAdapter DataAdapterFactory(string engineName, System.Data.IDbCommand cmd)
        {
            ConnectorConstants.ConnectorEngineEnum engine = ParseEngineName(engineName);

            // Récupération du type avec cache thread-safe
            Type adapterType = adapterTypeCache.GetOrAdd(engine, key =>
            {
                Type foundType = null;

                if (key == ConnectorConstants.ConnectorEngineEnum.MSSQL)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlDataAdapter, System.Data");
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlDataAdapter, System.Data.SqlClient");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("Npgsql.NpgsqlDataAdapter, Npgsql");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.SQLITE)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SQLite.SQLiteDataAdapter, System.Data.SQLite");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.ORACLE)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OracleClient.OracleDataAdapter, System.Data.OracleClient");
                }
                else if (key == ConnectorConstants.ConnectorEngineEnum.ACCESS)
                {
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbDataAdapter, System.Data");
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbDataAdapter, System.Data.OleDb");
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.OleDb.OleDbDataAdapter, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                }

                if (foundType == null)
                    throw new InvalidOperationException(
                        $"Engine/DLL IDataAdapter for {engineName} not found. Please include DLL for this engine in your project");

                return foundType;
            });

            System.Data.IDbDataAdapter adapter = Nglib.APP.CODE.ReflectionTools.CreateInstance<System.Data.IDbDataAdapter>(adapterType);
            adapter.SelectCommand = cmd;
            return adapter;
        }

        /// <summary>
        /// Efface le cache des types (utile pour tests ou rechargement dynamique de DLL)
        /// </summary>
        public static void ClearTypeCache()
        {
            connectionTypeCache.Clear();
            adapterTypeCache.Clear();
        }

         

    }
}
