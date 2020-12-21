// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using Nglib.SECURITY.CRYPTO;


// Amélioreration avec http://tlevesque.developpez.com/tutoriels/dotnet/acces-aux-donnees-avec-dapper/
//https://www.exceptionnotfound.net/dapper-vs-entity-framework-vs-ado-net-performance-benchmarking/

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Objet de base utilisant le datarow (ADO.NET)
    /// </summary>
    public class DataPO : IDataPO, IDataAccessor
    {
        /// <summary>
        /// Ce sont les donnees de base du datapo !
        /// </summary>
        protected internal System.Data.DataRow localRow = null; // Données Sql 

        /// <summary>
        /// Flux de données noSQl dans une col/table
        /// </summary>
        protected internal List<IDataPOFlow> flows = null; // Données NoSql standard

        /// <summary>
        /// Le datapo à été chargé proprement avec les clefs et pret à faire des opération insert/update/delete/select
        /// </summary>
        protected internal bool _isDefined = false;

        /// <summary>
        /// L'objet à été chargé par un autre datarow
        /// </summary>
        protected internal bool _isLoaded = false;

        /// <summary>
        /// Parametre pour encrypter les données
        /// </summary>
        protected internal DATA.ACCESSORS.IDataAccessorCryptoContext CryptoContext { get; set; }




        /// <summary>
        /// constructeur vide
        /// </summary>
        public DataPO() 
        {
           
        }


        /// <summary>
        /// Accès aux données
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="isLegalCreateColIfNotExist"></param>
        /// <returns></returns>
        public object this[string nameValue, bool isLegalCreateColIfNotExist = true]
        {
            get { return this.GetObject(nameValue, DataAccessorOptionEnum.Safe); }
            set { this.SetObject(nameValue, value, isLegalCreateColIfNotExist ? DataAccessorOptionEnum.None: DataAccessorOptionEnum.NotCreateColumn); }
        }



        /// <summary>
        /// Obtient le datarow de l'objet
        /// Initialisera l'objet si il n'a pas été initialisé ou chargé déja
        /// </summary>
        /// <param name="RefreshFlow">Refrachit les champs qui contienne les flux nosql</param>
        /// <returns></returns>
        public System.Data.DataRow GetRow(bool RefreshFlow = true)
        {
            if (this.localRow == null)
                this.DefineSchemaPO();

            if (RefreshFlow && this.flows != null) // il faut sérialiser les données des collections Nosql dans le datarow
                foreach (var flow in this.flows)
                {
                    if (!flow.IsChanges()) continue; // aucun changement donc aucune modifications nécessaire (sauf pour l'insert)
                    string serialfield = flow.SerializeField();
                    string fieldname = flow.GetFieldName();
                    this.SetObject(fieldname, serialfield, flow.IsFieldEncrypted() ? DataAccessorOptionEnum.Encrypted : DataAccessorOptionEnum.None);
                }
            return this.localRow;
        }


        /// <summary>
        /// Définit le datarow dans l'objet
        /// </summary>
        /// <param name="row">L'objet de données</param>
        public void SetRow(System.Data.DataRow row)
        {
            try
            {
                this.localRow = row;
                this._isLoaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Set Row " + ex.Message, ex);
            }
        }




        


        /// <summary>
        /// Permet d'initialiser le schema du datapo (col,keys,tablename, ...)
        /// Il faut l'overider ensuite il sera executé automatiquement
        /// </summary>
        /// <returns>Retournera le Schema de l'objet en question</returns>
        public virtual System.Data.DataTable InitSchema()
        {
            return null;
        }

        /// <summary>
        /// Obtient un flow de données nosql
        /// </summary>
        /// <typeparam name="Tflow"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private IDataPOFlow GetFlow(string fieldName)
        {
            if (this.flows == null || string.IsNullOrWhiteSpace(fieldName)) return null;
            return this.flows.FirstOrDefault(f => f.GetFieldName().Equals(fieldName));
        }

        /// <summary>
        /// Obtient un flow de données nosql existant ou le crée
        /// </summary>
        /// <typeparam name="Tflow"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="FullEncrypted">Le champs xml/json est totalement crypté en text en base</param>
        /// <returns></returns>
        protected Tflow GetOrDefineFlow<Tflow>(string fieldName, DATA.ACCESSORS.FlowTypeEnum fieldType, bool FullEncrypted=false) where Tflow : class,IDataPOFlow,new()
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return null;
            if (this.flows == null) this.flows = new List<IDataPOFlow>();
            Tflow flow = GetFlow(fieldName) as Tflow;
            if (flow == null)
            {
                flow = new Tflow();
                flow.DefineField(fieldName, fieldType, FullEncrypted);
                string flowContent = this.GetString(fieldName, flow.IsFieldEncrypted() ? DataAccessorOptionEnum.Encrypted: DataAccessorOptionEnum.None);   // Chargement des données dans le flux
                flow.DeSerializeField(flowContent);
                this.flows.Add(flow);


            }


            return flow;
        }





        /// <summary>
        /// Obtient un Objet (du datarow, fluxxml ou des objets liés)
        /// méthode principale
        /// </summary>
        /// <returns></returns>
        public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            if (this.localRow == null) return null; // vraiement innutile, y'a aucune données
            try
            {
                if (string.IsNullOrWhiteSpace(nameValue)) return null;
                nameValue = nameValue.Trim();
                // -----------------------------
                if (nameValue.Contains(":")) // Il s'agit d'un champ provenant d'un autre datapo incorporé
                {
                    string[] fieldsPO = nameValue.Split(':');
                    if (fieldsPO.Length < 2 || string.IsNullOrWhiteSpace(fieldsPO[0]) || string.IsNullOrWhiteSpace(fieldsPO[1])) return null;
                    System.Reflection.PropertyInfo pi = this.GetType().GetProperty(fieldsPO[0]);
                    if (pi == null || !pi.CanRead) return null;
                    object ObjfieldIn = pi.GetValue(this, null);
                    if (ObjfieldIn == null) return null;
                    if (ObjfieldIn is DATAPO.DataPO)
                    {
                        DATAPO.DataPO dpo = (DATAPO.DataPO)ObjfieldIn;
                        return dpo.GetObject(fieldsPO[1], AccesOptions);
                    }
                    else if (pi.PropertyType.IsClass)
                    {
                        System.Reflection.PropertyInfo subpi = pi.PropertyType.GetProperty(fieldsPO[1]);
                        if (subpi == null) return null;
                        else return subpi.GetValue(ObjfieldIn, null);
                        // !!! ajouter la recherche dans une liste ou dictionary : 'documents[monchamp="valeur",monchamp2="valeur"]:monchamp3'
                    }
                }
                // -----------------------------
                else if (nameValue.StartsWith("/")) // Il s'agit du flux de données nosql
                {
                    //string fieldName = nameValue.Split('/')[1]; // le nom du champ est équivalent au nom du root
                    //IDataPOFlow flow = this.GetFlow(fieldName);
                    //// IDataAccessor flowdata = this.GetFlow(fieldName) as IDataAccessor; // On converti le 
                    //if (flow == null ||) return null;
                    //return flow.GetObject(nameValue, AccesOptions);
                    //!!!!!!!!!
                }
                // -----------------------------
                else // sinon c'est un champdu datarow
                {
                    System.Data.DataColumn realColumn = DataSetTools.GetColumn(this.localRow.Table, nameValue);
                    if (realColumn != null) return this.localRow[realColumn];
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }





        /// <summary>
        /// Défini un objet
        /// </summary>
        /// <returns></returns>
        public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameValue)) return false;
                nameValue = nameValue.Trim();


                if (nameValue.Contains(":"))
                { // Obtenir la valeur dans un datapo join à celui-ci
                    throw new Exception("DataPo In SubObject non géré");
                }
                else if (nameValue.StartsWith("/"))
                { // Obtenir la valeur dans le flux nosql lié
                    string fieldName = nameValue.Split('/')[1]; // le nom du champ est équivalent au nom du root
                    IDataPOFlow flow = this.GetFlow(fieldName);
                    if (flow == null) return false;
                    //bool iset = flow.SetObject(nameValue, obj, AccesOptions);
                    //if (iset) this.localRow.SetModified();
                    //return iset;
                    //!!!!!!!!!!!!
                    return false;
                }
                else
                { // Obtien la valeur normalement dans le datarow local
                    if (this.localRow == null) this.DefineSchemaPO(); // si pas de datarow, on l'initialiser (identique à GetRow())
                    System.Data.DataColumn realColumn = this.localRow.Table.Columns[nameValue];
                    if (realColumn == null)
                    {
                        if (AccesOptions.HasFlag(DataAccessorOptionEnum.NotCreateColumn)) // création de la colonne interdite
                        {
                            if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return false;
                            else throw new Exception("Colonne Introuvable dans le DataRow (NotCreateColumn = true)");
                        }
                        else if (obj == null || obj == DBNull.Value) // si la valeur est null et que la valeur existe pas c'est pas la peine de créer une colone
                        {
                            return false;
                            //if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return false;
                            //else throw new Exception("Colonne Introuvable dans le DataRow (Impossible de déterminer le type à partir d'un null pour créer la colonne)");
                        }
                        // Création de la colone
                        Type typetocreate = obj.GetType();
                        realColumn = this.localRow.Table.CreateColumn(nameValue.ToLower(), typetocreate);  // Création d'une nouvelle colonne dans la table pour stoker la valeur (toujours en minuscule)
                    }


                    object orgnobj = this.localRow[realColumn];

                    if (obj == null && orgnobj == null) return false;// inutile si pas modifié
                    if (obj != null && obj.Equals(orgnobj)) return false; // inutile si pas modifié

                    if (AccesOptions.HasFlag(DataAccessorOptionEnum.IgnoreChange)) // il faut tromper le datarow pour lui faire croire que la donnée n'a pas été modifié
                    {
                        Dictionary<string, object> prechanged = DataSetTools.GetChangedValues(this.localRow); // on obtien les changements précédents
                        if (prechanged.Count > 0) this.localRow.RejectChanges(); // on les rejetents pour les remettre après
                        this.localRow[realColumn] = obj; // on met à jours la données
                        this.localRow.AcceptChanges(); // on approuve la mise à jours
                        if (prechanged.Count > 0) prechanged.Keys.ToList().ForEach(k => this.localRow[k] = prechanged[k]); // on remet les anciennes modifications
                    }
                    else
                    {
                        this.localRow[realColumn] = obj; // on met à jours la données dans le datarow , l'objet passera au statut modified
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) return false;
                else throw new Exception("SetObject DataRow " + nameValue + " : " + ex.Message, ex);
            }
            finally
            {

            }
        }


        /// <summary>
        /// Permet de déterminer si l'objet est présent en base de données ou si il faut faire un insert
        /// </summary>
        /// <returns></returns>
        public bool IsInDataBase()
        {
            if (this.localRow == null) return false;
            //if (this._isLoaded) return true; // il provient d'un datatable donc oui il doit prevenir de la base
            if (this.localRow.RowState.HasFlag(System.Data.DataRowState.Detached)) return false; // détaché donc hors de la base
            if (this.localRow.RowState.HasFlag(System.Data.DataRowState.Deleted)) return false; // supprimé donc hors base
            return true;
        }









        /// <summary>
        /// Marque pour toutes les données que les changements ont été pris en comptes
        /// </summary>
        /// <returns>retourne si un changement était présent</returns>
        public bool AcceptChanges()
        {
            if (this.IsChanges())
            {
                try
                {
                    // Mise à jour du datarow
            
                        if (!this.localRow.RowState.HasFlag(System.Data.DataRowState.Detached)) // ne pas fair si détached !!! 
                            this.localRow.AcceptChanges();


                    // Mise à jours des flow
                    if (this.flows!=null)
                        this.flows.ForEach(f => f.AcceptChanges());
   
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else return false;
        }




        public string[] ListFieldsKeys()
        {
            if (this.localRow == null) return new string[]{ };
            return this.localRow.Table.GetColumns().Select(c => c.ColumnName).ToArray();
        }


        /// <summary>
        /// obtenir le context de cryptage de l'objet
        /// </summary>
        public virtual IDataAccessorCryptoContext GetCryptoContext()
        {
            return this.CryptoContext;
        }



        /// <summary>
        /// définir le context de cryptage de l'objet
        /// </summary>
        public void SetCryptoOptions(IDataAccessorCryptoContext dataPOCryptoContext)
        {
            this.CryptoContext = dataPOCryptoContext;
        }


        /// <summary>
        /// obtient un vecteur d'initialisation unique pour l'objet
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCryptoIV()
        {
            //string md5 = FORMAT.CryptHash.Hash(compose.ToString(), HashModeEnum.MD5);
            return "";
        }




        ///// <summary>
        ///// Obtient l'identifiant Unique 
        ///// </summary>
        ///// <returns></returns>
        //public override string ToString()
        //{
        //    return base.ToString();
        //}


    }
}
