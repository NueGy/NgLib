using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using Nglib.SECURITY.CRYPTO;




namespace Nglib.DATA.DATAPO
{
    
    /// <summary>
    /// DataPO (Data Persistent Object) - Hybrid ADO.NET/NoSQL ORM. Architecture inspired by Active Record pattern with NoSQL flows support.
    /// Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_datapo"/>
    /// </summary>
    public class DataPO : IDataPO, IDataAccessor
    {
        /// <summary>
        /// Core data storage for the DataPO
        /// </summary>
        protected internal System.Data.DataRow localRow = null; // SQL data 

        /// <summary>
        /// NoSQL data flows stored in a column/table
        /// </summary>
        protected internal List<IDataPOFlow> flows = null; // Standard NoSQL data

        /// <summary>
        /// The DataPO has been properly initialized with keys and is ready for insert/update/delete/select operations
        /// </summary>
        protected internal bool _isDefined = false;

        /// <summary>
        /// The object has been loaded from another DataRow
        /// </summary>
        protected internal bool _isLoaded = false;

        /// <summary>
        /// Encryption context for data security
        /// </summary>
        protected internal DATA.ACCESSORS.IDataAccessorCryptoContext CryptoContext { get; set; }




        /// <summary>
        /// Empty constructor
        /// </summary>
        public DataPO()
        {

        }

        /// <summary>
        /// Constructor with DataRow initialization
        /// </summary>
        public DataPO(System.Data.DataRow row)
        {
            this.SetRow(row);
        }


        /// <summary>
        /// Data indexer for direct field access
        /// </summary>
        /// <param name="nameValue">Field name</param>
        /// <returns>Field value</returns>
        public object this[string nameValue]
        {
            get { return this.GetValue<object>(nameValue, DataAccessorOptionEnum.Safe); }
            set { this.SetValue(nameValue, value); }
        }



        /// <summary>
        /// Gets the DataRow from the object. Initializes the object if not already initialized or loaded.
        /// </summary>
        /// <param name="syncFlows">Synchronizes fields containing NoSQL flows</param>
        /// <returns>The DataRow instance</returns>
        public DataRow GetRow(bool syncFlows = true)
        {
            // Initialize schema if necessary (fallback pour DataPO standalone)
            if (this.localRow == null)
                this.DefineSchemaPO(); // Si non initialisé et qu'on veut setter une valeur on init le schéma
            if (this.localRow == null)
                this.localRow = (new DataTable()).NewRow(); // fallback (should not happen if DefineSchemaOnPO works correctly


            if (syncFlows && this.flows?.Count > 0)
                SyncFlowsToRow();
            return this.localRow;
        }


        /// <summary>
        /// Sets the DataRow into the object
        /// </summary>
        /// <param name="row">The data object</param>
        public void SetRow(System.Data.DataRow row)
        {
            try
            {
                this.localRow = row;
                this._isDefined = false; // alr 02/2023 bug fix if savePo after reSetRow
                this._isLoaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Set Row " + ex.Message, ex);
            }
        }




        /// <summary>
        /// Initializes the DataPO schema (columns, keys, table name, ...). Must be overridden, then it will be executed automatically.
        /// </summary>
        /// <returns>Returns the schema of the object</returns>
        public virtual System.Data.DataTable CreateSchema()
        {
            return null; // Par défaut, on ne fait rien
            //return DataPOSchemaTools.CreateSchemaWithAttributes(this.GetType());   //  on utilise le système d'attributs?
        }

        /// <summary>
        /// Gets a NoSQL data flow by field name
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The flow instance or null</returns>
        public IDataPOFlow GetDataPOFlow(string fieldName)
        {
            if (this.flows == null || string.IsNullOrWhiteSpace(fieldName)) return null;
            return this.flows.FirstOrDefault(f => f.GetFieldName().Equals(fieldName));
        }

        /// <summary>
        /// Gets an existing NoSQL data flow or creates it
        /// </summary>
        /// <typeparam name="Tflow">Flow type</typeparam>
        /// <param name="fieldName">Field name</param>
        /// <param name="fieldType">Flow type enum</param>
        /// <param name="FullEncrypted">Whether the XML/JSON field is fully encrypted as text in database</param>
        /// <returns>The flow instance</returns>
        public Tflow GetOrDefineFlow<Tflow>(string fieldName, DATA.ACCESSORS.FlowTypeEnum fieldType, bool FullEncrypted = false) where Tflow : class, IDataPOFlow, new()
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return null;
            if (this.flows == null) this.flows = new List<IDataPOFlow>();
            Tflow flow = GetDataPOFlow(fieldName) as Tflow;
            if (flow == null)
            {
                flow = new Tflow();
                flow.DefineField(fieldName, fieldType, FullEncrypted);
                string flowContent = this.GetValue<string>(fieldName, flow.IsFieldEncrypted() ? DataAccessorOptionEnum.Encrypted : DataAccessorOptionEnum.None);   // Load data into flow
                flow.DeSerializeField(flowContent);
                this.flows.Add(flow);


            }


            return flow;
        }





        /// <summary>
        /// Gets a value (from DataRow, XML flow, or linked objects). Main optimized method with complex path handling.
        /// </summary>
        /// <param name="nameValue">Field name or path (e.g., "field", "datapo:field", "/flowname/path")</param>
        /// <param name="AccesOptions">Data access options</param>
        /// <returns>The requested value or null if not found</returns>
        public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            if (this.localRow == null) return null;
            if (string.IsNullOrWhiteSpace(nameValue)) return null;
            
            nameValue = nameValue.Trim();

            // Access data from linked DataPO (format "property:field")
            if (nameValue.Contains(":"))
                return GetDataFromLinkedObject(nameValue, AccesOptions);

            // Access NoSQL flows (format "/flowname/path")
            if (nameValue.StartsWith("/"))
                return GetDataFromFlow(nameValue, AccesOptions);

            // Direct DataRow access (standard case)
            return GetDataFromRow(nameValue);
        }






        /// <summary>
        /// Sets a value in the DataPO (DataRow or NoSQL flows). Main optimized method - DOES NOT support linked objects with ':'.
        /// </summary>
        /// <param name="nameValue">Field name or path (e.g., "field", "/flowname/path")</param>
        /// <param name="obj">Value to set</param>
        /// <param name="AccesOptions">Modification options</param>
        /// <returns>True if the value was modified, False otherwise</returns>
        public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrWhiteSpace(nameValue)) return false;
            nameValue = nameValue.Trim();

            // Initialize schema if necessary (fallback pour DataPO standalone)
            if (this.localRow == null) 
                this.DefineSchemaPO(); // Si non initialisé et qu'on veut setter une valeur on init le schéma
            if (this.localRow == null) 
                this.localRow = (new DataTable()).NewRow(); // fallback (should not happen if DefineSchemaOnPO works correctly

            // Linked objects (format "property:field") are NOT supported for writing
            if (nameValue.Contains(":"))
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return false;
                throw new NotSupportedException($"SetData does not support linked objects (format 'property:field'): {nameValue}");
            }

            // Write to NoSQL flow (format "/flowname/path")
            if (nameValue.StartsWith("/"))
                return SetDataToFlow(nameValue, obj, AccesOptions);

            // Write to DataRow (standard case)
            return SetDataToRow(nameValue, obj, AccesOptions);
        }


        /// <summary>
        /// Determines if the object exists in the database or if an insert is required
        /// </summary>
        /// <returns>True if in database</returns>
        public bool IsInDataBase()
        {
            if (this.localRow == null) return false;
            //if (this._isLoaded) return true; // comes from a datatable so yes it must come from the database
            if (this.localRow.RowState.HasFlag(System.Data.DataRowState.Detached)) return false; // detached so outside the database
            if (this.localRow.RowState.HasFlag(System.Data.DataRowState.Deleted)) return false; // deleted so outside database
            return true;
        }









        /// <summary>
        /// Marks all data changes as accepted
        /// </summary>
        /// <returns>True if changes were present</returns>
        public bool AcceptChanges()
        {
            if (this.IsChanges())
            {
                try
                {
                    // Update the DataRow

                    if (this.localRow!=null && !this.localRow.RowState.HasFlag(System.Data.DataRowState.Detached)) // don't do if detached!!! 
                        this.localRow.AcceptChanges();


                    // Update flows
                    if (this.flows != null)
                        this.flows.ForEach(f => f.AcceptChanges());

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else return false;
        }


        /// <summary>
        /// Lists all field/column names available in the DataRow
        /// </summary>
        /// <returns>Array of column names</returns>
        public string[] ListFieldsKeys()
        {
            if (this.localRow == null) return new string[] { };
            return this.localRow.Table.GetColumns().Select(c => c.ColumnName).ToArray();
        }


        /// <summary>
        /// Gets the encryption context of the object
        /// </summary>
        public virtual IDataAccessorCryptoContext GetCryptoContext()
        {
            return this.CryptoContext;
        }



        /// <summary>
        /// Sets the encryption context of the object
        /// </summary>
        public void SetCryptoOptions(IDataAccessorCryptoContext dataPOCryptoContext)
        {
            this.CryptoContext = dataPOCryptoContext;
        }


        /// <summary>
        /// Gets a unique initialization vector for the object
        /// </summary>
        /// <returns>IV string</returns>
        public virtual string GetCryptoIV()
        {
            //string md5 = FORMAT.CryptHash.Hash(compose.ToString(), HashModeEnum.MD5);
            return "";
        }




        #region ---- TOOLS AND LOCAL MANAGEMENT ----


        /// <summary>
        /// Applies NoSQL flow modifications to the DataRow
        /// </summary>
        private void SyncFlowsToRow()
        {
            foreach (var flow in this.flows.Where(f => f.IsChanges()))
            {
                try
                {
                    var serialized = flow.SerializeField();
                    var fieldName = flow.GetFieldName();
                    var options = flow.IsFieldEncrypted()
                        ? DataAccessorOptionEnum.Encrypted
                        : DataAccessorOptionEnum.None;

                    this.SetData(fieldName, serialized, options);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Flow synchronization error '{flow.GetFieldName()}': {ex.Message}", ex);
                }
            }
        }



        /// <summary>
        /// Gets a value from a linked object via property (format "property:field")
        /// </summary>
        private object GetDataFromLinkedObject(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            string[] fieldsPO = nameValue.Split(new[] { ':' }, 2);
            if (fieldsPO.Length < 2 || string.IsNullOrWhiteSpace(fieldsPO[0]) || string.IsNullOrWhiteSpace(fieldsPO[1]))
                return null;

            PropertyInfo pi = this.GetType().GetProperty(fieldsPO[0]);
            if (pi == null || !pi.CanRead) return null;

            object linkedObj = pi.GetValue(this, null);
            if (linkedObj == null) return null;

            // If it's a DataPO, delegate access
            if (linkedObj is DataPO dataPO)
                return dataPO.GetValue<object>(fieldsPO[1], AccesOptions);

            // If it's a standard object, access via reflection
            if (pi.PropertyType.IsClass)
            {
                PropertyInfo subPi = pi.PropertyType.GetProperty(fieldsPO[1]);
                return subPi?.GetValue(linkedObj, null);
            }

            return null;
        }

        /// <summary>
        /// Gets a value from a NoSQL flow (format "/flowname/path")
        /// </summary>
        private object GetDataFromFlow(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            if (this.flows == null || this.flows.Count == 0) return null;

            string[] pathParts = nameValue.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length == 0) return null;

            string flowName = pathParts[0];
            IDataPOFlow flow = this.GetDataPOFlow(flowName);
            if (flow == null) return null;

            // If the flow implements IDataAccessor, delegate full access
            if (flow is IDataAccessor flowAccessor)
                return flowAccessor.GetValue<object>(nameValue, AccesOptions);

            // Otherwise, return the flow itself if it's the only path element
            return pathParts.Length == 1 ? flow : null;
        }

        /// <summary>
        /// Gets a value from the local DataRow (optimized direct access)
        /// </summary>
        private object GetDataFromRow(string nameValue)
        {
            if (this.localRow == null) return null;
            DataColumn column = DataSetTools.GetColumn(this.localRow.Table, nameValue);
            if (column == null) return null;

            object value = this.localRow[column];
            return value == DBNull.Value ? null : value;
        }


        /// <summary>
        /// Sets a value in a NoSQL flow (format "/flowname/path")
        /// </summary>
        private bool SetDataToFlow(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (this.flows == null || this.flows.Count == 0) return false;

            string[] pathParts = nameValue.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length == 0) return false;

            string flowName = pathParts[0];
            IDataPOFlow flow = this.GetDataPOFlow(flowName);
            if (flow == null) return false;

            // If the flow implements IDataAccessor, delegate modification
            if (flow is IDataAccessor flowAccessor)
            {
                bool isSet = flowAccessor.SetValue(nameValue, obj);
                if (isSet && this.localRow != null)
                    this.localRow.SetModified(); // Mark DataRow as modified
                return isSet;
            }

            return false;
        }

        /// <summary>
        /// Sets a value in the local DataRow with column creation management
        /// </summary>
        private bool SetDataToRow(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (this.localRow == null) // Ne devrais jamais se produire si DefineSchemaPO fonctionne correctement
             throw new InvalidOperationException($"SetDataToRow: localRow is null for field '{nameValue}'. Schema not initialized."); 

            // Get or create the column
            DataColumn column = GetOrCreateColumn(nameValue, obj, AccesOptions);
            if (column == null) return false;

            // Check if value changed (optimization to avoid unnecessary modifications)
            object currentValue = this.localRow[column];
            if (IsValueUnchanged(obj, currentValue))
                return false;

            // Apply value according to mode
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.IgnoreChange))
                SetValueWithoutTracking(column, obj);
            else
                this.localRow[column] = obj ?? DBNull.Value;

            return true;
        }

        /// <summary>
        /// Gets an existing column or creates a new one if necessary
        /// </summary>
        private DataColumn GetOrCreateColumn(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if(this.localRow==null) return null;
            DataColumn column = DataSetTools.GetColumn(this.localRow.Table, nameValue);
            if (column != null) return column;

            // Column doesn't exist
            if (AccesOptions.HasFlag(DataAccessorOptionEnum.NotCreateColumn))
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return null;
                throw new InvalidOperationException($"Column '{nameValue}' not found (NotCreateColumn = true)");
            }

            // Don't create column for null value
            if (obj == null || obj == DBNull.Value)
                return null;

            // Create new column
            Type columnType = obj.GetType();
            return this.localRow.Table.CreateColumn(nameValue.ToLower(), columnType);
        }

        /// <summary>
        /// Checks if a value hasn't changed (optimization)
        /// </summary>
        private bool IsValueUnchanged(object newValue, object currentValue)
        {
            // Normalize null values
            bool isNewNull = newValue == null || newValue == DBNull.Value;
            bool isCurrentNull = currentValue == null || currentValue == DBNull.Value;

            if (isNewNull && isCurrentNull) return true;
            if (isNewNull != isCurrentNull) return false;

            return newValue.Equals(currentValue);
        }

        /// <summary>
        /// Sets a value without change tracking (IgnoreChange mode)
        /// </summary>
        private void SetValueWithoutTracking(DataColumn column, object value)
        {
            if (this.localRow == null) return;

            // Save previous changes
            Dictionary<string, object> previousChanges = DataSetTools.GetChangedValues(this.localRow);
            
            // Reset state if necessary
            if (previousChanges.Count > 0)
                this.localRow.RejectChanges();

            // Apply new value
            this.localRow[column] = value ?? DBNull.Value;
            this.localRow.AcceptChanges();

            // Restore previous changes
            if (previousChanges.Count > 0)
            {
                foreach (var kvp in previousChanges)
                    this.localRow[kvp.Key] = kvp.Value;
            }
        }


        /// <summary>
        /// Permet de définir directement la structure du PO (un nouveau Row à partir de la nouvelle table)
        /// </summary>
        /// <param name="tableSchema">Table contenant le schéma à appliquer</param>
        /// <param name="AllowKeepOriginalRow">Si true et qu'il y a déjà un datarow avec des données, on préserve les données dans la nouvelle Table</param>
        /// <param name="CloneNewTable">Si true, clone la table entière. Si false, clone uniquement le schéma</param>
        public void DefineSchemaPO(DataTable tableSchema, bool AllowKeepOriginalRow = true, bool CloneNewTable = true)
        {
            try
            {
                if(tableSchema==null) throw new Exception("tableSchema is null");
                bool rowexisting = false;
                if (this.localRow != null && this.localRow.ItemArray != null && this.localRow.ItemArray.Length > 0)
                    rowexisting = true; // il y as des données dans le datarow existant

                if (rowexisting && AllowKeepOriginalRow) //Clone un nouveau datarow dans la nouvelle table avec des données d'origine
                    this.localRow.Table.MergeAddSchema(tableSchema); // il suffit juste de vérifier que le shémas est bien identique
                //po.localRow = DataSetTools.ReplaceRowInOtherTable(po.localRow, table); // premet de recopier les données orgininal dans la nouvelle table
                else
                {
                    DataTable tabledata = CloneNewTable ? tableSchema.Clone() : tableSchema;
                    // il faut clonner la table pour eviter les erreurs sur autoincrement, !!! voir si possible d'améliorer car multipli le temps de traitement x10
                    this.localRow = tabledata.NewRow(); // init simple
                }

                this._isDefined = true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("DefineSchemaPO {0}", ex.Message));
            }
        }


        /// <summary>
        /// Permet de préparer l'objet (notamment définir le datarow et son schéma)
        /// C'est nécessaire pour les opérations SQL, ou l'instanciation vide d'un DATAPO
        /// </summary>
        /// <param name="allowCache">Si true, utilise le cache pour optimiser les performances</param>
        public bool DefineSchemaPO(bool allowCache = true)
        {
            try
            {
                Type potype = this.GetType();
                System.Data.DataTable tableStd = DataPOSchemaTools.GetSchemaOnPO(potype, allowCache);
                if (tableStd == null) return false;

                // on prend un clone de la table
                this.DefineSchemaPO(tableStd, true); // voir si il est possible d'améliorer pour ne pas prendre de clone !!! (faire le clone que si il y as modification des colonnes)
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("DefineSchemaPO {0}", ex.Message));
            }
        }






        /// <summary>
        /// Vérifie si le DataPO a été correctement défini pour des actions SQL
        /// </summary>
        /// <param name="po">Instance du DataPO à vérifier</param>
        /// <returns>True si le DataPO est correctement initialisé (localRow défini et _isDefined = true), false sinon</returns>
        public bool IsDefinedSchema()
        {
            if (this.localRow == null) return false;
            if (this._isDefined) return true;  // si il a été défini explicitement
            if (this.localRow.Table.PrimaryKey!=null && this.localRow.Table.PrimaryKey.Length > 0) return true; // Si clef primaire défini, on considère que c'est ok
            return false;
        }






        #endregion

    }
}


