using Nglib.DATA.COLLECTIONS;
using Nglib.DATA.CONNECTOR.QUERYBUILDER;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{

    /// <summary>
    /// Un dataConnector générique (multiengine) respectant le standard SQL (basé sur postgres)
    /// Accès aux drivers/dll par reflexion, cela permet de ne pas avoir à référencer toutes les DLL
    /// </summary>
    public class ConnectorGeneric : DataConnectorBase
    {
        /// <summary>
        /// Cache thread-safe des types IDbConnection pour éviter la réflexion répétée
        /// </summary>
        private static readonly ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type> _connectionTypeCache 
            = new ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type>();

        /// <summary>
        /// Cache thread-safe des types IDataAdapter pour éviter la réflexion répétée
        /// </summary>
        private static readonly ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type> _adapterTypeCache 
            = new ConcurrentDictionary<ConnectorConstants.ConnectorEngineEnum, Type>();


        /// <summary>
        /// Connecteur SGBD Générique
        /// </summary>
        public ConnectorGeneric()
        {
            this.MultiThreadingSafe = true;
        }

        /// <summary>
        /// Constructeur avec une connexion existante
        /// </summary>
        public ConnectorGeneric(System.Data.IDbConnection OriginDbConnection)
        {
            this.MultiThreadingSafe = true;
            this.connection = OriginDbConnection;
        }

        #region Factories Dynamiques

        /// <summary>
        /// Crée une connexion IDbConnection en fonction du moteur SGBD (via réflexion)
        /// </summary>
        protected override System.Data.IDbConnection CreateConnection()
        {
            ConnectorConstants.ConnectorEngineEnum engine = ConnectorTools.ParseEngineName(this.EngineName);
            if (engine == ConnectorConstants.ConnectorEngineEnum.NA) 
                throw new ArgumentException("EngineName empty or not recognized", nameof(EngineName));

            // Récupération du type avec cache thread-safe
            Type connectionType = _connectionTypeCache.GetOrAdd(engine, key => 
            {
                Type foundType = null;

                if (key == ConnectorConstants.ConnectorEngineEnum.MSSQL)
                {
                    // Essayer d'abord Microsoft.Data.SqlClient (recommandé pour .NET moderne)
                    foundType = Nglib.APP.CODE.ReflectionTools.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient");
                    // Fallback sur System.Data.SqlClient si Microsoft.Data.SqlClient n'est pas disponible
                    if (foundType == null) foundType = Nglib.APP.CODE.ReflectionTools.GetType("System.Data.SqlClient.SqlConnection, System.Data");
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
                        $"Engine/DLL IDbConnection for {EngineName} not found. Please include DLL for this engine in your project");

                return foundType;
            });

            return Nglib.APP.CODE.ReflectionTools.CreateInstance<System.Data.IDbConnection>(connectionType);
        }

        /// <summary>
        /// Crée un paramètre IDataParameter avec nom et valeur
        /// </summary>
        /// <param name="name">Nom du paramètre (sans @)</param>
        /// <param name="value">Valeur du paramètre</param>
        /// <returns>IDbDataParameter créé</returns>
        protected override IDbDataParameter CreateParameter(string name, object value)
        {
            if (connection == null) throw new InvalidOperationException("Connection not initialized");
            
            using (var cmd = connection.CreateCommand())
            {
                IDataParameter param = cmd.CreateParameter();
                param.ParameterName = "@" + name;
                param.Value = value ?? DBNull.Value;
                
                // Détection du type XML pour PostgreSQL
                if (ConnectorTools.ParseEngineName(this.EngineName) == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
                {
                    if (value != null && value is string && ((string)value).StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                        param.DbType = DbType.Xml;
                }
                
                return (IDbDataParameter)param;
            }
        }



        /// <summary>
        /// Crée un QueryBuilder spécifique au moteur de base de données
        /// </summary>
        /// <returns>Instance de QueryBuilder configurée pour le moteur actuel (PostgreSQL par défaut)</returns>
        public override IQueryBuilder CreateQueryBuilder()
        {
            return QUERYBUILDER.QueryBuilderTools.CreateQueryBuilder(this.EngineName ?? "postgresql");
        }

        /// <summary>
        /// Clone le connecteur
        /// </summary>
        public override object Clone()
        {
            IDataConnector dataConnectorClone = new ConnectorGeneric();
            dataConnectorClone.SetConnectionString(this.ConnectionString, this.EngineName);
            return dataConnectorClone;
        }

        #endregion

        #region Factory IDataAdapter (spécifique à ConnectorGeneric)

        /// <summary>
        /// Factory IDataAdapter avec cache pour optimiser les performances
        /// </summary>
        /// <param name="cmd">Commande SQL à associer à l'adapter</param>
        /// <returns>Instance de IDataAdapter</returns>
        /// <exception cref="InvalidOperationException">Si la DLL du moteur n'est pas trouvée</exception>
        protected virtual System.Data.IDataAdapter CreateDataAdapter(System.Data.IDbCommand cmd)
        {

            ConnectorConstants.ConnectorEngineEnum engine = ConnectorTools.ParseEngineName(this.EngineName);

            // Récupération du type avec cache thread-safe
            Type adapterType = _adapterTypeCache.GetOrAdd(engine, key =>
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
                        $"Engine/DLL IDataAdapter for {EngineName} not found. Please include DLL for this engine in your project");

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
            _connectionTypeCache.Clear();
            _adapterTypeCache.Clear();
        }

        #endregion




        #region Command Initialization (Spécifique à ConnectorGeneric)

        /// <summary>
        /// Initialisation de la commande, sql et params
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual System.Data.IDbCommand InitCommand(QueryContext query)
        {
            
            System.Data.IDbCommand cmd = null;
            try
            {
                cmd = this.connection.CreateCommand();
                if (this.transaction != null) cmd.Transaction = this.transaction; //La requette sera dans une transaction déja en cours
                cmd.CommandTimeout = this.DefaultTimeOut; // ajoute un timeout, mais ne fonctionne pas toujours !!!

                // Ajoute la commande SQL
                if (CONNECTOR.SqlTools.IsSQLQuery(query.SqlQuery)) cmd.CommandType = System.Data.CommandType.Text;
                else cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = query.SqlQuery;


                if (query.Parameters != null)
                {
                    // identifie les param nécessaires dans la requettes (pour ne pas envoyer sur le réseaux des parametres innutiles)
                    List<string> keyFilterIsNecessary = new List<string>();
                    if (cmd.CommandType == CommandType.StoredProcedure || cmd.CommandType == CommandType.TableDirect)  // Si procstock on prend tous les arguments
                        keyFilterIsNecessary = query.Parameters.Keys.ToList();
                    else if (cmd.CommandType == CommandType.Text) // sinon on prend que les clef utiles
                        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(query.SqlQuery.ToLower() + " ", "(\\@\\w+)"))
                            keyFilterIsNecessary.Add(match.Groups[1].Value.Replace("@",""));


                    // Ajoute les paramètres
                    foreach (string fieldKey in query.Parameters.Keys.Where(k => keyFilterIsNecessary.Contains(k, true)))
                        this.InitCommandSetParameter(query, cmd, fieldKey, query.Parameters[fieldKey]);
                }
                 
                    
                //this.connection.Prepare(); //https://www.npgsql.org/doc/prepare.html
                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception("Initialisation SQL", ex);
            }
        }


        protected virtual void InitCommandSetParameter(QueryContext query, System.Data.IDbCommand cmd, string parametername, object parameterValue)
        {
            IDataParameter sqlparam = null;
            //sqlparam = ConnectorTools.AddDataParameterWithValue(cmd, "@" + fieldKey, obj); //NpgsqlCommand cmd = conn.CreateCommand();
            sqlparam = cmd.CreateParameter();
            sqlparam.ParameterName = "@" + parametername;
            sqlparam.Value = parameterValue;

            // Détection du type
            if (ConnectorTools.ParseEngineName(this.EngineName) == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
            {
                // !!! A revoir
                //https://github.com/npgsql/Npgsql/issues/177
                if (parameterValue != null && parameterValue != DBNull.Value && parameterValue is string && ((string)parameterValue).StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)) sqlparam.DbType = DbType.Xml;
            }


            cmd.Parameters.Add(sqlparam);
        }

        #endregion

        #region Query Methods Override (Implémentation abstraite)

        // ****** SCALLAR *****

        /// <summary>
        /// Exécution d'une requête scalaire (retourne une seule valeur)
        /// </summary>
        /// <param name="queryContext">Contexte de la requête SQL</param>
        /// <returns>Valeur scalaire retournée par la requête</returns>
        public override async Task<object> QueryScalarAsync(QueryContext queryContext)
        {
            try
            {
                queryContext.Validate();
                queryContext.watchAll.Start();
                OnQueryBegin(queryContext);
                
                this.Open(false); // ouvre la connection si nécessaire (sera refermé juste après)

                try
                {
                    AcquireQueryLock(); // Synchronisation thread-safe
                    queryContext.ExecuteDate = DateTime.Now;
                    queryContext.watchExecute.Restart();
                    
                    object ret = QueryScalarExec(queryContext);
                    
                    queryContext.watchExecute.Stop();
                    IncrementRequestCount(); // Compteur thread-safe
                    
                    return ret;
                }
                catch (Exception e)
                {
                    queryContext.watchExecute.Stop();
                    queryContext.Error = e.Message;
                    throw;
                }
                finally
                {
                    ReleaseQueryLock(); // Libère le mutex
                    OnQueryCompleted(queryContext); // Event après exécution
                }
            }
            catch(ConnectorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConnectorException(queryContext, string.Format("connector.QueryScalarAsync {0}", ex.Message), ex);
            }
            finally
            {
                if (this.transaction == null && !this.keepOpenMode) // on ferme que si on n'est pas dans une transaction
                    this.Close();
                queryContext.watchAll.Stop();
            }
        }

        /// <summary>
        /// Exécution interne de la requête scalaire
        /// </summary>
        /// <param name="query">Contexte de requête</param>
        /// <returns>Valeur scalaire</returns>
        protected virtual object QueryScalarExec(QueryContext query)
        {
            using (System.Data.IDbCommand cmd = this.InitCommand(query))
            {
                return cmd.ExecuteScalar();
            }
        }




        /// <summary>
        /// Exécution d'une requête SQL avec retour de DataSet
        /// </summary>
        /// <param name="queryContext">Contexte de la requête SQL</param>
        /// <returns>DataSet contenant les résultats</returns>
        public override async Task<System.Data.DataSet> QueryDataSetAsync(QueryContext queryContext)
        {
            try
            {
                queryContext.Validate();
                queryContext.watchAll.Start();
                OnQueryBegin(queryContext);
                
                this.Open(false); // ouvre la connection si nécessaire (sera refermé juste après)

                try
                {
                    AcquireQueryLock(); // Synchronisation thread-safe
                    queryContext.ExecuteDate = DateTime.Now;
                    queryContext.watchExecute.Restart();
                    
                    System.Data.DataSet ret = QueryDataSetExec(queryContext);
                    
                    queryContext.watchExecute.Stop();
                    IncrementRequestCount(); // Compteur thread-safe
                    
                    return ret;
                }
                catch (Exception e)
                {
                    queryContext.watchExecute.Stop();
                    queryContext.Error = e.Message;
                    throw;
                }
                finally
                {
                    ReleaseQueryLock(); // Libère le mutex
                    OnQueryCompleted(queryContext); // Event après exécution
                }
            }
            catch (ConnectorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConnectorException(queryContext, string.Format("connector.QueryDataSetAsync {0}", ex.Message), ex);
            }
            finally
            {
                if (this.transaction == null && !this.keepOpenMode) // on ferme que si on n'est pas dans une transaction
                    this.Close();
                queryContext.watchAll.Stop();
            }
        }

        /// <summary>
        /// Exécution interne de la requête DataSet
        /// </summary>
        /// <param name="query">Contexte de requête</param>
        /// <returns>DataSet avec les résultats</returns>
        protected virtual DataSet QueryDataSetExec(QueryContext query)
        {
            System.Data.DataSet ret = new DataSet();
            using (System.Data.IDbCommand cmd = this.InitCommand(query))
            {
                System.Data.IDataAdapter reader = CreateDataAdapter(cmd);
                reader.Fill(ret);
                // !!! tester performance entre un dataadaptater et un simple datareader
            }
            return ret;
        }


 
 

        /// <summary>
        /// Insertion table (Insert optimisé en insertion multirows)
        /// </summary>
        /// <param name="dataTable">Table de données à insérer</param>
        /// <param name="SpecialTimeOut">Timeout spécifique</param>
        /// <param name="AutoIncrementColumn">Colonne auto-incrémentée</param>
        /// <returns>Liste des IDs insérés</returns>
        public override async Task<List<long>> InsertTableAsync(System.Data.DataTable dataTable, int SpecialTimeOut = 600, string AutoIncrementColumn = null)
        {
            List<System.Data.DataTable> tablesSpliteds = DataSetTools.DataTableSplit(dataTable, 50); // Découpe 50 lignes par 50 lignes
            bool UseTransaction = true;
            List<long> retourIncremented = new List<long>();
            try
            {
                UseTransaction = this.BeginTransaction();
                int count = 0;
                foreach (System.Data.DataTable tabl in tablesSpliteds) // list
                {
                    List<long> retourIncrementedSub = this.InsertTableSub(tabl, SpecialTimeOut, AutoIncrementColumn);
                    retourIncremented.AddRange(retourIncrementedSub);
                    count += tabl.Rows.Count;
                    tabl.Dispose();
                }

                if (UseTransaction) this.CommitTransaction();

                return retourIncremented;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exeption " + ex.Message);
                if (UseTransaction) this.RollBackTransaction();
                throw;
            }
            finally
            {

            }
        }


        /// <summary>
        /// Insertion interne par batch
        /// </summary>
        /// <param name="subtabl">Table à insérer</param>
        /// <param name="SpecialTimeOut">Timeout spécifique</param>
        /// <param name="AutoIncrementColumn">Colonne auto-incrémentée</param>
        /// <returns>Liste des IDs insérés</returns>
        private List<long> InsertTableSub(System.Data.DataTable subtabl, int SpecialTimeOut = 600, string AutoIncrementColumn = null)
        {
            //Obtien le SQL
            var sqlAndDatas = SqlTools.GenerateSqlMultiInsert(subtabl);
            string sql = sqlAndDatas.Item1;
            ConnectorConstants.ConnectorEngineEnum connectorEngine = ConnectorTools.ParseEngineName(this.EngineName);

            // Complete la requette pour obtenir les id du champs auto incrémenté (en un seul appel SQL)
            if (!string.IsNullOrWhiteSpace(AutoIncrementColumn))
            {
                if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
                    sql += " RETURNING " + AutoIncrementColumn;
                else if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.SQLITE)
                    sql += ";  select last_insert_rowid();";
                else if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.MYSQL)
                    sql += ";  SELECT LAST_INSERT_ID();"; // Calling last_insert_id() gives you the id of the FIRST row inserted in the last batch. All others inserted, are guaranteed to be sequential.
                else if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.MSSQL)
                    sql += ";  SELECT SCOPE_IDENTITY();"; // OUTPUT Inserted.ID https://stackoverflow.com/questions/7917695/sql-server-return-value-after-insert
                else if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.ORACLE)
                    sql += ";"; //
                else if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.ACCESS)
                    sql += ";  SELECT @@IDENTITY;";
            }




            //INSERT
            System.Data.DataTable ret = this.Query(sql, sqlAndDatas.Item2);


            // ---- OBTENIR LES ID INSERES ----
            List<long> retourIncremented = new List<long>();
            if (!string.IsNullOrWhiteSpace(AutoIncrementColumn))
            {
                if (connectorEngine == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
                {
                    if (subtabl.Rows.Count != ret.Rows.Count) throw new Exception("AutoIncrementColumn Rows Error");
                    foreach (System.Data.DataRow row in ret.Rows)
                        retourIncremented.Add(Convert.ToInt64(row[0]));
                }
                else if(ret.Rows.Count==1)
                {
                    // on obtient le dernier ID et on décompte les autres
                    long lastid = Convert.ToInt64(ret.Rows[0][0]);
                    int totalrow = subtabl.Rows.Count;
                    for (int i = 0; i < totalrow; i++)
                        retourIncremented.Add(lastid-(totalrow-1)+i);

                }
            }
            return retourIncremented;
        }

        #endregion







    }
}
