using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Classe de base abstraite pour tous les connecteurs de base de données.
    /// Fournit les fonctionnalités communes : gestion des transactions, synchronisation thread-safe,
    /// compteurs de performance, gestion du cycle de vie (IDisposable).
    /// </summary>
    public abstract class DataConnectorBase : IDataConnector, IDisposable
    {
        #region Fields & Properties

        // Compteurs et statistiques
        private long _countRequests = 0;
        
        // Synchronisation thread-safe
        private readonly Mutex _openMutex = new Mutex();
        private readonly Mutex _queryMutex = new Mutex();
        
        // Gestion du cycle de vie
        private bool _disposed = false;

        /// <summary>
        /// État de la connexion à la base de données
        /// </summary>
        protected IDbConnection connection = null;
        
        /// <summary>
        /// Transaction en cours
        /// </summary>
        protected IDbTransaction transaction = null;
        
        /// <summary>
        /// Indique si la connexion doit rester ouverte après les requêtes
        /// </summary>
        protected bool keepOpenMode { get; set; }


        /// <summary>
        /// Indique si le connecteur est en lecture seule
        /// </summary>
        public virtual bool ReadOnly { get; set; } = false;

        /// <summary>
        /// Chaîne de connexion à la base de données
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Nom du moteur de base de données (POSTGRES, MYSQL, SQLITE, etc.)
        /// </summary>
        public string EngineName { get; protected set; }

        /// <summary>
        /// Nombre total de requêtes exécutées par ce connecteur
        /// </summary>
        public long RequestCount => Interlocked.Read(ref _countRequests);

        /// <summary>
        /// Active la synchronisation thread-safe pour Open/Close et les requêtes (défaut: true)
        /// </summary>
        public bool MultiThreadingSafe { get; set; } = true;

        /// <summary>
        /// Timeout par défaut pour les commandes SQL (en secondes)
        /// </summary>
        public int DefaultTimeOut { get; set; } = 60;

        /// <summary>
        /// Timeout pour l'acquisition des mutex (en millisecondes)
        /// </summary>
        public int MutexTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Événement déclenché avant l'exécution d'une requête
        /// </summary>
        public event QueryCompletedHandler QueryBegin;

        /// <summary>
        /// Événement déclenché après l'exécution d'une requête
        /// </summary>
        public event QueryCompletedHandler QueryCompleted;

        #endregion

        #region Factories

        /// <summary>
        /// Crée une instance de IDbConnection spécifique au SGBD
        /// </summary>
        protected abstract IDbConnection CreateConnection();

        /// <summary>
        /// Crée un paramètre de commande spécifique au SGBD
        /// </summary>
        protected abstract IDbDataParameter CreateParameter(string name, object value);

 

        #endregion

        #region Connection Management

        /// <summary>
        /// Initialise la connexion avec la chaîne de connexion
        /// </summary>
        public virtual void SetConnectionString(string connectionString, string engineName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(engineName))
                throw new ArgumentException("Engine name cannot be null or empty", nameof(engineName));

            if (connection != null)
                throw new InvalidOperationException("Connector already initialized. Dispose and create a new instance.");

            ConnectionString = connectionString;
            EngineName = engineName;

            connection = CreateConnection();
            connection.ConnectionString = connectionString;
        }

        /// <summary>
        /// Ouvre la connexion à la base de données (thread-safe avec Mutex)
        /// </summary>
        public virtual bool Open(bool keepOpen = false)
        {
            if (_disposed) 
                throw new ObjectDisposedException(GetType().Name, "Cannot open a disposed connector");

            if (connection == null) 
                throw new InvalidOperationException("Connection not initialized. Call SetConnectionString first.");

            try
            {
                if (MultiThreadingSafe)
                {
                    if (!_openMutex.WaitOne(MutexTimeoutMs))
                        throw new TimeoutException($"Timeout waiting for connection mutex after {MutexTimeoutMs}ms");
                }

                // Ouverture uniquement si nécessaire
                if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                {
                    connection.Open();
                    keepOpenMode = keepOpen;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new ConnectorException(null, $"Failed to open connection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ferme la connexion à la base de données
        /// </summary>
        public virtual bool Close(bool safe = true)
        {
            try
            {
                if (connection == null) return false;
                
                if (transaction != null) 
                    RollBackTransaction(true);
                
                keepOpenMode = false;
                connection.Close();

                if (MultiThreadingSafe)
                {
                    try { _openMutex.ReleaseMutex(); } catch { }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (!safe) throw new ConnectorException(null, $"Failed to close connection: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Retourne l'objet IDbConnection sous-jacent
        /// </summary>
        public virtual IDbConnection GetDbConnection() => connection;

        #endregion

        #region Transaction Management

        /// <summary>
        /// Démarre une transaction
        /// </summary>
        public virtual bool BeginTransaction()
        {
            if (transaction != null)
                return false; // dejà une transaction ouverte

            Open(true);
            transaction = connection.BeginTransaction();
            return true;
        }

        /// <summary>
        /// Valide la transaction en cours
        /// </summary>
        public virtual bool CommitTransaction()
        {
            if (transaction == null) 
                throw new InvalidOperationException("No active transaction to commit");

            try
            {
                transaction.Commit();
                return true;
            }
            finally
            {
                transaction.Dispose();
                transaction = null;
                if (!keepOpenMode) Close();
            }
        }

        /// <summary>
        /// Annule la transaction en cours
        /// </summary>
        public virtual bool RollBackTransaction(bool safe = false)
        {
            if (transaction == null)
            {
                if (!safe) throw new InvalidOperationException("No active transaction to rollback");
                return false;
            }

            try
            {
                transaction.Rollback();
                return true;
            }
            catch (Exception ex)
            {
                if (!safe) throw new ConnectorException(null, $"Failed to rollback transaction: {ex.Message}", ex);
                return false;
            }
            finally
            {
                transaction?.Dispose();
                transaction = null;
                if (!keepOpenMode) Close();
            }
        }

        #endregion

        #region Query Execution (Protected helpers)

        /// <summary>
        /// Incrémente le compteur de requêtes de manière thread-safe
        /// </summary>
        protected void IncrementRequestCount()
        {
            Interlocked.Increment(ref _countRequests);
        }

        /// <summary>
        /// Acquiert le verrou de requête de manière thread-safe
        /// </summary>
        protected bool AcquireQueryLock()
        {
            if (!MultiThreadingSafe) return true;
            
            if (!_queryMutex.WaitOne(MutexTimeoutMs))
                throw new TimeoutException($"Timeout waiting for query mutex after {MutexTimeoutMs}ms");
            
            return true;
        }

        /// <summary>
        /// Libère le verrou de requête
        /// </summary>
        protected void ReleaseQueryLock()
        {
            if (MultiThreadingSafe)
            {
                try { _queryMutex.ReleaseMutex(); } catch { }
            }
        }

        /// <summary>
        /// Invoque un événement de manière sécurisée (ne propage pas les exceptions)
        /// </summary>
        protected void SafeInvokeEvent(QueryCompletedHandler eventHandler, QueryContext context)
        {
            if (eventHandler == null) return;

            try
            {
                eventHandler.Invoke(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in event handler: {ex.Message}");
            }
        }

        /// <summary>
        /// Déclenche l'événement QueryBegin
        /// </summary>
        protected void OnQueryBegin(QueryContext context)
        {
            SafeInvokeEvent(QueryBegin, context);
        }

        /// <summary>
        /// Déclenche l'événement QueryCompleted
        /// </summary>
        protected void OnQueryCompleted(QueryContext context)
        {
            SafeInvokeEvent(QueryCompleted, context);
        }

        #endregion

        #region Abstract Query Methods (à implémenter)

        /// <summary>
        /// Crée un QueryBuilder spécifique au moteur
        /// </summary>
        public abstract QUERYBUILDER.IQueryBuilder CreateQueryBuilder();

        /// <summary>
        /// Exécute une requête scalaire et retourne une valeur unique
        /// </summary>
        public abstract Task<object> QueryScalarAsync(QueryContext queryContext);

        /// <summary>
        /// Exécute une requête et retourne un DataSet
        /// </summary>
        public abstract Task<DataSet> QueryDataSetAsync(QueryContext queryContext);

        /// <summary>
        /// Insère les données d'une DataTable dans la base
        /// </summary>
        public abstract Task<System.Collections.Generic.List<long>> InsertTableAsync(
            DataTable dataTable, 
            int SpecialTimeOut = 600, 
            string AutoIncrementColumn = null);

        /// <summary>
        /// Clone le connecteur (pour ICloneable)
        /// </summary>
        public abstract object Clone();

        #endregion

        #region IDisposable Pattern

        /// <summary>
        /// Libère les ressources utilisées par le connecteur
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libère les ressources managées et non-managées
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Libération des ressources managées
                Close(true);
                
                transaction?.Dispose();
                connection?.Dispose();
                
                // Libération des mutex
                _openMutex?.Dispose();
                _queryMutex?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~DataConnectorBase()
        {
            Dispose(false);
        }

        #endregion
    }
}
