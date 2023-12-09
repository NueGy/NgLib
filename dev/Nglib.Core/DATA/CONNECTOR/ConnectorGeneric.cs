// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{

    /// <summary>
    /// Un dataConnector générique (multiengine) respectant le standard SQL (basé sur postgres)
    /// Accès aux drivers/dll par reflexion, cela permet de ne pas avoir à référencer toutes les DLL
    /// </summary>
    public class ConnectorGeneric  : IDataConnector
    {
        private long _CountRequests = 0;
        protected System.Threading.Mutex OpenedMutex = new System.Threading.Mutex(); // permet que le connecteur soit utilisé par plusieurs threads
        protected int _OpenedMutexCount = 0;
        protected System.Threading.Mutex QueryMutex = new System.Threading.Mutex(); // permet que le connecteur soit utilisé par plusieurs threads meme si MultiThreadingSafe=false
        protected string ConnectionString = null;
        protected System.Data.IDbConnection connection = null;
        protected System.Data.IDbTransaction transaction = null;
        bool keepOpenMode { get; set; }

        /// <summary>
        /// Un seul thread pourra ouvrir la connection simultanément (default true)
        /// </summary>
        public bool MultiThreadingSafe { get; set; }


        /// <summary>
        /// Time out strandard
        /// </summary>
        public int DefaultTimeOut = 60;

        /// <summary>
        /// Nom du moteur SGBD
        /// </summary>
        public string EngineName { get; private set; }

        /// <summary>
        /// Nom du connecteur
        /// </summary>
        public string ConnectorName { get; set; }

        /// <summary>
        /// Le connecteur est en mode readOnly
        /// </summary>
        public bool ReadOnly { get; set; }



        /// <summary>
        /// Event après l'éxecution
        /// </summary>
        public event QueryCompletedHandler QueryCompleted;

        /// <summary>
        /// Event avant l'éxecution
        /// </summary>
        public event QueryCompletedHandler QueryBegin;

        /// <summary>
        /// Connecteur SGBD Générique
        /// </summary>
        public ConnectorGeneric()
        {
            this.MultiThreadingSafe = true;
        }


        public ConnectorGeneric(System.Data.IDbConnection OriginDbConnection)
        {
            this.MultiThreadingSafe = true;
            this.connection = OriginDbConnection;
        }


        public IDbConnection GetDbConnection()
        {
           return this.connection;
        }


        #region transaction et ouverture






        /// <summary>
        /// Définir la chaine de connexion
        /// instanciation du  IDbConnection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="defaultEngine"></param>
        public void SetConnectionString(string connectionString, string defaultEngine)
        {
            //NpgsqlConnectionStringBuilder nchainbuild = new NpgsqlConnectionStringBuilder(this.ConnectionString);
            //this.ConnectionString = nchainbuild.ToString();
            
            if (this.connection != null) throw new Exception("Connector already in use");
            this.ConnectionString = connectionString;
            this.EngineName = defaultEngine;

            if (string.IsNullOrWhiteSpace(this.ConnectionString)) throw new Exception("Sql connexion string empty");

            //https://www.npgsql.org/doc/connection-string-parameters.html
            System.Data.Common.DbConnectionStringBuilder dbConnectionStringBuilder = null;
            // Factorisation du connecteur SGBD

            this.connection = this.ConnectionFactory();
            this.connection.ConnectionString = this.ConnectionString;
            //if (connection == null) throw new Exception();


        }





        /// <summary>
        /// Ouverture de la connection, (que si nécessaire)
        /// Mutex pour faire attendre les threads
        /// </summary>
        /// <param name="keepOpen">Garder la connexion ouverte après la première requette et après les commit/rollback transaction (penser à la fermer)</param>
        /// <returns></returns>
        public bool Open(bool keepOpen = false)
        {
            try
            {
                if (connection == null) throw new Exception("IDbConnection not init (please use SetConnectionString(string,string))");

                if (MultiThreadingSafe)
                {
                    // Il ne peus y avoir qu'un seul thread sur une connection, sera fermer lors du close
                    this.OpenedMutex.WaitOne(); //  (normalement quand le meme thread qui la ouvert repassera dessu, il n'aura pas besoin d'attendre)
                    _OpenedMutexCount++; // Il faut compter le nombre de fois ou le thread passe dessu pour faire autant de release
                }

                // ouverture de la connection nécessare
                if (this.connection.State==ConnectionState.Closed || this.connection.State==ConnectionState.Broken)
                {
                    this.connection.Open();
                    this.keepOpenMode = keepOpen; // ordonnera de garder la connection ouverte
                    return true;
                }
                else return false;
                    
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Connector.Open {0}",ex.Message),ex);
            }
        }


        /// <summary>
        /// Fermeture de la connection (SAFE)
        /// </summary>
        /// <returns></returns>
        public virtual bool Close(bool safe=true)
        {
            try
            {
                if (this.connection == null) return false; // ok n'a jamais ete initialisé
                if (this.transaction != null) this.RollBackTransaction(true);
                this.keepOpenMode = false; // on annule ce paramètre, le choix reviendra à la prochaine ouverture open(true)
                this.connection.Close();


                if (this.MultiThreadingSafe) { for (int i = 0; i < _OpenedMutexCount; i++) this.OpenedMutex.ReleaseMutex(); _OpenedMutexCount = 0; }   // A ete ouverte lors du Open()  this.OpenedMutex.ReleaseMutex(); 

                return true;
            }
            catch (Exception ex)
            {
                if (safe) return false;
                else throw new Exception(string.Format("Connector.Close {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Ouverture d'une nouvelle transaction SQL (1 par connector)
        /// </summary>
        /// <param name="transactionName"></param>
        /// <returns></returns>
        public virtual bool BeginTransaction(string transactionName = null)
        {
            if (this.transaction != null) return false; // une seule transaction par connecteur
            this.Open(); // ouverture si nécessaire
            try
            {
                this.transaction = this.connection.BeginTransaction();
                return true;
            }
            catch (Exception ex)
            {
                 throw new Exception(string.Format("Connector.BeginTransaction {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// annulation sql, fermera la connexion si keepOpenMode=false
        /// </summary>
        /// <param name="transactionName"></param>
        /// <returns></returns>
        public virtual bool RollBackTransaction(bool safe=false)
        {
            if (this.transaction == null) return false; // Il n'y as rien à fermer
            this.Open(); // Ne sert pas à l'ouverture mais bloquera le thread si il n'est pas concerné
            try
            {
                if (this.transaction != null) // on revérifie que la transaction est pas null, car à la libération du mutex dans Open la transaction est peut être passée null
                {
                    this.transaction.Rollback();
                    this.transaction.Dispose();
                    this.transaction = null;
                }
  
                if (!this.keepOpenMode) this.Close(safe);
                return true;
            }
            catch (Exception ex)
            {
                if (safe) return false;
                throw new Exception(string.Format("Connector.RollBackTransaction {0}", ex.Message), ex);
            }
        }


        /// <summary>
        /// Valider la transaction
        /// </summary>
        /// <param name="safe"></param>
        /// <returns></returns>
        public virtual bool CommitTransaction()
        {
            if (this.transaction == null) return false; // Il n'y as rien à fermer
            this.Open(); // Ne sert pas à l'ouverture mais bloquera le thread si il n'est pas concerné
            try
            {
                if (this.transaction != null) // on revérifie que la transaction est pas null, car à la libération du mutex dans Open la transaction est peut être passée null
                {
                    this.transaction.Commit();
                    this.transaction.Dispose();
                    this.transaction = null;
                }
                if (!this.keepOpenMode) this.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Connector.CommitTransaction {0}", ex.Message), ex);
            }
        }




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
                if (CONNECTOR.SqlTools.IsSQLQuery(query.sqlQuery)) cmd.CommandType = System.Data.CommandType.Text;
                else cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = query.sqlQuery;


                if (query.parameters != null)
                {
                    // identifie les param nécessaires dans la requettes (pour ne pas envoyer sur le réseaux des parametres innutiles)
                    List<string> keyFilterIsNecessary = new List<string>();
                    if (cmd.CommandType == CommandType.StoredProcedure || cmd.CommandType == CommandType.TableDirect)  // Si procstock on prend tous les arguments
                        keyFilterIsNecessary = query.parameters.Keys.ToList();
                    else if (cmd.CommandType == CommandType.Text) // sinon on prend que les clef utiles
                        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(query.sqlQuery.ToLower() + " ", "(\\@\\w+)"))
                            keyFilterIsNecessary.Add(match.Groups[1].Value.Replace("@",""));


                    // Ajoute les paramètres
                    foreach (string fieldKey in query.parameters.Keys.Where(k => keyFilterIsNecessary.Contains(k, true)))
                        this.InitCommandSetParameter(query, cmd, fieldKey, query.parameters[fieldKey]);
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
            if (ConnectorTools.FindEngine(this.EngineName) == ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
            {
                // !!! A revoir
                //https://github.com/npgsql/Npgsql/issues/177
                if (parameterValue != null && parameterValue != DBNull.Value && parameterValue is string && ((string)parameterValue).StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)) sqlparam.DbType = DbType.Xml;
            }


            cmd.Parameters.Add(sqlparam);
        }





        /// <summary>
        /// Clonner
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            IDataConnector dataConnectorClone = new ConnectorGeneric();
            dataConnectorClone.SetConnectionString(this.ConnectionString, this.EngineName);
            return dataConnectorClone;
        }


        /// <summary>
        /// Fermerture
        /// </summary>
        public void Dispose()
        {
            this.Close(true);
            if (this.connection != null)
                this.connection.Dispose();
        }


        /// <summary>
        /// retry si nécessaire
        /// </summary>
        /// <param name="e"></param>
        /// <param name="queryContext"></param>
        protected virtual bool Canretry(Exception e, QueryContext queryContext)
        {
            /*
            if (fiable && transac == null && !retentebug && retrypossible(e.Message)) //si erreur de base on retente
            {
                retentebug = true;
                System.Threading.Thread.Sleep(9000); //après une petite pause biensur
                return query(sql, paramvalues);
                return false;
            }
            else */
            return false;
        }



        #endregion



        #region Querrys





        // ****** SCALLAR *****

        public async Task<object> QueryScalarAsync(QueryContext queryContext)
        {
            try
            {
                queryContext.Validate();
                queryContext.watchAll.Start();
                if (QueryBegin != null) this.QueryBegin(queryContext);
                this.Open(false); // ouvre la connection si nécessaire (sera refermé juste après)
                while (true) // on se limite à 3 tentatives (gérer dans la methode ExceptionSQL)
                {
                    queryContext.QueryTry++;
                    try
                    {
                        this.QueryMutex.WaitOne();
                        queryContext.ExecuteDate = DateTime.Now;
                        queryContext.watchExecute.Restart();
                        object ret = QueryScalarExec(queryContext);
                        queryContext.watchExecute.Stop();
                        return ret;
                    }
                    catch (Exception e)
                    {
                        queryContext.watchExecute.Stop();
                        queryContext.error = e.Message;
                        if(!Canretry(e, queryContext)) throw; // vérifier si il est possible de retenter
                    }
                    finally
                    {
                        this.QueryMutex.ReleaseMutex();
                    }
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
                if (this.transaction == null && !this.keepOpenMode) // on ferme que si on n'est pas dans une transaction et que le développeur n'a pas explicitement ouvert lui même la connection
                    this.Close();
                queryContext.watchAll.Stop();
                if (this.QueryCompleted != null) this.QueryCompleted(queryContext);
            }
        }

        protected virtual object QueryScalarExec(QueryContext query)
        {
            using (System.Data.IDbCommand cmd = this.InitCommand(query))
            {
                return cmd.ExecuteScalar();
            }
        }




        public async Task<System.Data.DataSet> QueryDataSetAsync(QueryContext queryContext)
        {
            try
            {
                queryContext.Validate();
                queryContext.watchAll.Start();
                if(QueryBegin!=null) this.QueryBegin(queryContext);
                this.Open(false); // ouvre la connection si nécessaire (sera refermé juste après)
                while (true) // on se limite à 3 tentatives (gérer dans la methode ExceptionSQL)
                {
                    queryContext.QueryTry++;
                    try
                    {
                        this.QueryMutex.WaitOne();
                        queryContext.ExecuteDate = DateTime.Now;
                        queryContext.watchExecute.Restart();
                        System.Data.DataSet ret = QueryDataSetExec(queryContext);
                        queryContext.watchExecute.Stop();
                        return ret;
                    }
                    catch (Exception e)
                    {
                        queryContext.watchExecute.Stop();
                        queryContext.error = e.Message;
                        if (!Canretry(e, queryContext)) throw; // vérifier si il est possible de retenter
                    }
                    finally
                    {
                        this.QueryMutex.ReleaseMutex();
                    }
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
                if (this.transaction == null && !this.keepOpenMode) // on ferme que si on n'est pas dans une transaction et que le développeur n'a pas explicitement ouvert lui même la connection
                    this.Close();
                queryContext.watchAll.Stop();
                if (this.QueryCompleted != null) this.QueryCompleted(queryContext);
            }
        }


        protected virtual DataSet QueryDataSetExec(QueryContext query)
        {
            System.Data.DataSet ret = new DataSet();
            using (System.Data.IDbCommand cmd = this.InitCommand(query))
            {
                System.Data.IDataAdapter reader = DataAdapterFactory(cmd);
                reader.Fill(ret);
                // !!! tester performance entre un dataadaptater et un simple datareader
            }
            return ret;
        }


        protected virtual System.Data.IDataAdapter DataAdapterFactory(IDbCommand cmd)
        {
            return ConnectorTools.DataAdapterFactory(this.EngineName, cmd);
        }

        protected virtual System.Data.IDbConnection ConnectionFactory()
        {
            return ConnectorTools.ConnectionFactory(this.EngineName);
        }


        /// <summary>
        /// Insertion  table (Insert Optimsé en insertion multirows)
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="dataTable"></param>
        /// <param name="SpecialTimeOut"></param>
        /// <param name="AutoIncrementColumn"></param>
        /// <returns></returns>
        public async Task<List<long>> InsertTableAsync( System.Data.DataTable dataTable, int SpecialTimeOut = 600, string AutoIncrementColumn = null)
        {
            List<System.Data.DataTable> tablesSpliteds = DataSetTools.DataTableSplit(dataTable, 50); // Découpe 50 lignes par 50 lignes
            bool UseTransaction = true;
            List<long> retourIncremented = new List<long>();
            try
            {
                UseTransaction = this.BeginTransaction("t1");
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
                throw ex;
            }
            finally
            {

            }
        }


        /// <summary>
        /// Insertion
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="subtabl"></param>
        /// <param name="SpecialTimeOut"></param>
        /// <param name="AutoIncrementColumn"></param>
        /// <returns></returns>
        private List<long> InsertTableSub(System.Data.DataTable subtabl, int SpecialTimeOut = 600, string AutoIncrementColumn = null)
        {
            //Obtien le SQL
            var sqlAndDatas = SqlTools.GenerateSqlMultiInsert(subtabl);
            string sql = sqlAndDatas.Item1;
            ConnectorConstants.ConnectorEngineEnum connectorEngine = ConnectorTools.FindEngine(this.EngineName);

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
