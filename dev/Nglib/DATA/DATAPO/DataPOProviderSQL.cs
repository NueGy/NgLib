// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using Nglib.DATA.DATASET;
using Nglib.DATA.DATAVALUES;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Nglib.DATA.DATAPO
{


    /// <summary>
    /// manipulation des dataPo en base
    /// </summary>
    /// <typeparam name="Tobj">DataPO</typeparam>
    public class DataPOProviderSQL<Tobj, TCollectionPO> : DataPOProviderSQL<Tobj> where Tobj : DATAPO.DataPO, new() where TCollectionPO : ICollectionPO, new()
    {
        public DataPOProviderSQL(DATA.CONNECTOR.IDataConnector connector) : base(connector) { }
        public DataPOProviderSQL(APP.ENV.IGlobalEnv env) : base(env) { }

        public TCollectionPO GetCollectionPO(string SqlQuery, params object[] insparam)
        {
            return base.GetCollectionPO<TCollectionPO>(SqlQuery, insparam);
        }

        public TCollectionPO GetCollectionPO(string SqlQuery, Dictionary<string, object> paramKeySearch = null)
        {
            return base.GetCollectionPO<TCollectionPO>(SqlQuery, paramKeySearch);
        }



    }


    /// <summary>
    /// manipulation des dataPo en base
    /// </summary>
    /// <typeparam name="Tobj">DataPO</typeparam>
    public class DataPOProviderSQL<Tobj> : DataPOProviderSQL where Tobj : DATAPO.DataPO, new()
    {
        public DataPOProviderSQL(DATA.CONNECTOR.IDataConnector connector) : base(connector) { }
        public DataPOProviderSQL(APP.ENV.IGlobalEnv env) : base(env) { }

        public CollectionPO<Tobj> GetListPO(string SqlQuery, params object[] insparam)
        {
            return base.GetListPO<Tobj>(SqlQuery, insparam);
        }

        public CollectionPO<Tobj> GetListPO(string SqlQuery, Dictionary<string, object> paramKeySearch = null)
        {
            return base.GetListPO<Tobj>(SqlQuery, paramKeySearch);
        }

        public Tobj GetFirstPO(long idIncrement)
        {
            return base.GetFirstPO<Tobj>(idIncrement);
        }
    }





    /// <summary>
    /// manipulation des dataPo en base
    /// </summary>
    public class DataPOProviderSQL
    {
     

        /// <summary>
        /// Liste des connecteurs/serveurs disponibles
        /// </summary>
        public DATA.CONNECTOR.ConnectorCollection Connectors = null;



        /// <summary>
        /// Permet de désactiver les acceptchange lors des Update,insert : pratique pour le inmemory
        /// </summary>
        public bool DisableAcceptChange = false;



        /// <summary>
        /// Connecteur principal avec le SGBD
        /// </summary>
        public DATA.CONNECTOR.IDataConnector Connector { get { return this.Connectors == null ? null : this.Connectors.GetDefaultConnector(); } }


        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL()
        {
        }


        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL(DATA.CONNECTOR.IDataConnector connector)
        {
            this.Connectors = new DATA.CONNECTOR.ConnectorCollection();
            this.Connectors.Add(connector);
        }

        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL(DATA.CONNECTOR.ConnectorCollection connectors)
        {
            this.Connectors = connectors;
        }

        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL(APP.ENV.IGlobalEnv env)
        {
            this.Connectors = env.Connectors;
        }




        #region ------- Lectures SQL -------



        /// <summary>
        /// Obtient le premier enregistrement 
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="idIncrement"></param>
        /// <returns></returns>
        public Tobj GetFirstPO<Tobj>(long idIncrement) where Tobj : DATAPO.DataPO, new()
        {
            Type potype = typeof(Tobj);
            System.Data.DataTable schema = DATAPO.DataPOTools.GetSchemaOnPO(potype);
            if (schema == null || string.IsNullOrWhiteSpace(schema.TableName)) throw new Exception(string.Format("GetFirst: schema on {0} is not defined", potype.ToString()));
            Dictionary<string, object> paramKeys = new Dictionary<string, object>();
            paramKeys.Add("p1", idIncrement);
            string fieldkeyName = schema.GetColumns().Where(col => col.AutoIncrement).Select(col => col.ColumnName).FirstOrDefault();
            if(string.IsNullOrWhiteSpace(fieldkeyName)) throw new Exception(string.Format("GetFirst: column AutoIncrement not found in {0}", potype.ToString()));
            return this.GetListPO<Tobj>(string.Format("SELECT * FROM {0} WHERE {1}=@p1;", schema.TableName, fieldkeyName), paramKeys).FirstOrDefault();
        }


        /// <summary>
        /// Obtient le premier enregistrement avec ces clefs
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="paramKeys"></param>
        /// <returns></returns>
        public Tobj GetFirstPO<Tobj>(Dictionary<string, object> paramKeys) where Tobj : DATAPO.DataPO, new()
        {
            Type potype = typeof(Tobj);
            var schema = DATAPO.DataPOTools.GetSchemaOnPO(potype);
            if (schema == null || string.IsNullOrWhiteSpace(schema.TableName)) throw new Exception(string.Format("GetFirst :schema on {0} is not defined", potype.ToString()));
            string wheresql = string.Empty;
            if (paramKeys!=null && paramKeys.Count>0)
            {
                List<string> wheres = new List<string>();
                paramKeys.Keys.ToList().ForEach(k => wheres.Add(string.Format("{0}=@{0}", k)));
                wheresql = " WHERE "+string.Join(" AND ", wheres.ToArray());
            }
                
            return this.GetListPO<Tobj>(string.Format("SELECT * FROM {0} {1};", schema.TableName,wheresql), paramKeys).FirstOrDefault();
        }



  

        //public Tobj GetFirstPO<Tobj>(Expression<Func<Tobj, bool>> predicate)
        //{
        //    string nm = predicate.Name;
        //    throw new NotImplementedException();
        //    return default(Tobj);
        //}










        /// <summary>
        /// OBtient une liste d'objets
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="SqlQuery"></param>
        /// <param name="insparam"></param>
        /// <returns></returns>
        public TCollectionPO GetCollectionPO<TCollectionPO>(string SqlQuery, params object[] insparam) where TCollectionPO : ICollectionPO, new()
        {
            Dictionary<string, object> paramKeySearch = new Dictionary<string, object>();
            if(insparam!=null)
            {
                int ii = 1;
                foreach (var item in insparam)
                {
                    paramKeySearch.Add("p" + ii, item);
                    ii++;
                }
            }
            return this.GetCollectionPO<TCollectionPO>(SqlQuery, paramKeySearch);
        }


        public CollectionPO<Tobj> GetListPO<Tobj>(string SqlQuery, params object[] insparam) where Tobj : DATAPO.DataPO, new()
        {
            return this.GetCollectionPO<CollectionPO<Tobj>>(SqlQuery, insparam);
        }




        /// <summary>
        /// Obtient une liste d'objets
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <param name="SqlQuery"></param>
        /// <param name="paramKeySearch"></param>
        /// <returns></returns>
        public TCollectionPO GetCollectionPO<TCollectionPO>(string SqlQuery, Dictionary<string, object> paramKeySearch) where TCollectionPO : ICollectionPO, new()
        {
            // On obtient la table du PO
            string TableDefault = "";
            TCollectionPO retour = new TCollectionPO();
            try
            {
                Type typepo = retour.GetPOType();
                var dtable = DataPOTools.GetSchemaOnPO(typepo);
                if (dtable != null) TableDefault = dtable.TableName;
                if (string.IsNullOrWhiteSpace(TableDefault) || TableDefault.Length < 3) throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception("GetListPO (DEV) Impossible d'obtenir la table sql de l objet");
            }

            // Finir la Construction de la requette
            try
            {
                if (SqlQuery != null && !SqlQuery.ToUpper().Contains("SELECT ") && !SqlQuery.ToUpper().Contains("WHERE "))
                    SqlQuery = " WHERE "+SqlQuery;

                if(SqlQuery == null || !SqlQuery.ToUpper().Contains("SELECT "))
                    SqlQuery = "SELECT * FROM " + TableDefault + " " + SqlQuery;


                System.Data.DataTable  ret = this.Connector.Query(SqlQuery, paramKeySearch);
               
                retour.LoadFromDataTable(ret);
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("GetListPO " + ex.Message, ex);
            }
        }

        public CollectionPO<Tobj> GetListPO<Tobj>(string SqlQuery, Dictionary<string, object> paramKeySearch) where Tobj : DATAPO.DataPO, new()
        {
            return this.GetCollectionPO<CollectionPO<Tobj>>(SqlQuery, paramKeySearch);
        }




        public TCollectionPO GetCollectionPO<TCollectionPO>(int nbMax = 1000, Dictionary<string, object> paramKeySearch = null) where TCollectionPO : ICollectionPO, new()
        {
            // On obtient la table du PO
            string TableDefault = "";
            TCollectionPO retour = new TCollectionPO();
            try
            {
                Type typepo = retour.GetPOType();
                var dtable = DataPOTools.GetSchemaOnPO(typepo);
                if (dtable != null) TableDefault = dtable.TableName;
                if (string.IsNullOrWhiteSpace(TableDefault) || TableDefault.Length < 3) throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception("GetListPO (DEV) Impossible d'obtenir la table sql de l objet");
            }

            // Construction de la requette
            string sql = null;
            try
            {
                //if (nbMax > 0 && this.Connector is CONNECTOR.MsSQLConnector)sql = "SELECT TOP " + nbMax + " * FROM " + TableDefault + " ";
                sql = string.Format("SELECT  * FROM {0} ",TableDefault);
                if (paramKeySearch != null && paramKeySearch.Count > 0)
                    sql += " WHERE " + DATA.CONNECTOR.SqlTools.GenerateCreateWhereSQL(paramKeySearch.Keys.ToArray());
                if (nbMax > 0) sql += string.Format("LIMIT{0} ", nbMax); 
                // !!! gérer le sqlbuilder selon le sgbd
            }
            catch (Exception ex)
            {
                throw new Exception("GetListPO " + ex.Message, ex);
            }
            return this.GetCollectionPO<TCollectionPO>(sql, paramKeySearch);
        }


        public CollectionPO<Tobj> GetListPO<Tobj>(int nbMax = 1000, Dictionary<string, object> paramKeySearch = null) where Tobj : DATAPO.DataPO, new()
        {
            return this.GetCollectionPO<CollectionPO<Tobj>>(nbMax, paramKeySearch);
        }




        public TCollectionPO SearchPO<TCollectionPO, Tsearch>(Tsearch form) where Tsearch : ISearchFormPO, new() where TCollectionPO : ICollectionPO, new()
        {
            throw new NotImplementedException();
            TCollectionPO retour = new TCollectionPO();
            retour.LoadFromDataTable(null);
            return retour;
        }



        //public TCollectionPO SearchPO<TCollectionPO, Tsearch>(Expression<Func<Tsearch, bool>> predicate) where Tsearch : ISearchFormPO, new() where TCollectionPO : ICollectionPO, new()
        //{
        //    string nm = predicate.Name;
        //    throw new NotImplementedException();

        //    TCollectionPO retour = new TCollectionPO();
        //    retour.LoadFromDataTable(null);
        //    return retour;
        //}










        #endregion





        #region ------- Ecritures SQL -------




        /// <summary>
        /// Mettre à jour (update only) des objets en base
        /// </summary>
        /// <param name="bubbles">DataPos</param>
        /// <param name="ForceEvenIfNotModified">Force la mise à jours de tous les champs même si il n'ont pas été modifié</param>
        /// <returns>Modification effectué</returns>
        public async Task<bool> SavePOAsync(DATAPO.DataPO[] bubbles, bool ForceEvenIfNotModified = false, bool AllowInsert=true)
        {
            if (bubbles == null || bubbles.Count() == 0) return false;
            if (this.Connector == null) throw new Exception("MasterConnector Not found");
            List<DATAPO.DataPO> bubblesNeedToSave = new List<DataPO>();
            List<DATAPO.DataPO> bubblesNeedToInsert = new List<DataPO>();
            try
            {
                // Vérifier qu'il s'agit toujours du même type d'objets (on ne peus pas travailler sur plusieurs tables)
                if (bubbles.Select(b => b.GetType()).Distinct().Count() != 1) throw new Exception("different types");

                // Detecter et préparer les objets qui nécessite une modifications
                foreach (var bubble in bubbles)
                {
                    if(!bubble.IsInDataBase()) // Mode Insert
                    {
                        if (!AllowInsert) throw new Exception("Disallow Insert");
                        bubblesNeedToInsert.Add(bubble);
                    }
                    else // Mode update/save
                    {
                        DataPOTools.PrepareDataPOForDB(bubble, ForceEvenIfNotModified); // preparera les Flux dans le datapo que si il ont été modifiés
                        // Améliorer l'upate des flow avec des save partial de json !!!
                        if (bubble.IsChanges() || ForceEvenIfNotModified)  //sera modifié que si nécessaire
                            bubblesNeedToSave.Add(bubble);
                    }
                }

                // On commence par les INSERT
                if (bubblesNeedToInsert.Count > 0) await this.InsertPOAsync(bubblesNeedToInsert.ToArray());
                if (bubblesNeedToSave.Count == 0) { return (bubblesNeedToInsert.Count > 0) ? true : false; } // plus rien à modifier (on signal si on as fait des insert aussi)


                SavePo_OneByOne(bubblesNeedToSave, ForceEvenIfNotModified);






                return true;

            }
            catch (Exception e)
            {
                throw new Exception(string.Format("SavePO {0} ", e.Message), e);
            }
        }
        public async Task<bool> SavePOAsync(DATAPO.DataPO bubble, bool ForceEvenIfNotModified = false, bool AllowInsert = true) { return  await this.SavePOAsync(new DATAPO.DataPO[]{ bubble }, ForceEvenIfNotModified, AllowInsert); }
        public bool SavePO(DATAPO.DataPO bubble, bool ForceEvenIfNotModified = false, bool AllowInsert = true) { return this.SavePOAsync(bubble, ForceEvenIfNotModified, AllowInsert).GetAwaiter().GetResult(); }
     


        /// <summary>
        /// Une requette un par un
        /// </summary>
        /// <param name="bubblesNeedToSave"></param>
        private void SavePo_OneByOne(List<DATAPO.DataPO> bubblesNeedToSave, bool ForceEvenIfNotModified)
        {
            foreach (var bubble in bubblesNeedToSave)
            {
                // mut.WaitOne();
                System.Data.DataRow rowb = bubble.GetRow(); // la structure du datapo sera défini à ce moment si nécessaire
                Dictionary<string, object> vals = null;
                if (ForceEvenIfNotModified) vals = DATASET.DataSetTools.GetValues(rowb, false, true);
                else vals = DATASET.DataSetTools.GetChangedValues(rowb);
                Dictionary<string, object> keys = DATASET.DataSetTools.GetValues(rowb, true, false);
                this.Connector.UpdateAsync(rowb.Table.TableName, keys, vals).GetAwaiter().GetResult();

                if (!DisableAcceptChange)
                    bubble.AcceptChanges();
            }
        }

        /// <summary>
        /// Execute plusieurs Save en une requette
        /// </summary>
        /// <param name="bubblesNeedToSave"></param>
        /// <param name="ForceEvenIfNotModified"></param>
        private void SavePo_MultiLines(List<DATAPO.DataPO> bubblesNeedToSave, bool ForceEvenIfNotModified)
        {
        }



            /// <summary>
            /// Mettre à jours plusieurs objets en même temps avec les mêmes valeurs
            /// </summary>
            /// <param name="bubbles"></param>
            /// <param name="valeursParameters"></param>
            public async Task UpdatePOAsync(DATAPO.DataPO[] bubbles, Dictionary<string, object> valeursParameters)
        {
            try
            {
                if (this.Connector == null) throw new Exception("MasterConnector Not found");
                // Vérifier qu'il s'agit toujours du même type d'objets (on ne peus pas travailler sur plusieurs tables)
                if (bubbles.Select(b => b.GetType()).Distinct().Count() != 1) throw new Exception("different types");

                // !!! Vérifier que les objets on bien tous les memes types de clefs primarykey

                //maj simple
                foreach (DATAPO.DataPO bubble in bubbles)
                {
                    if (bubble == null) throw new Exception("DataPO null");
                    System.Data.DataRow rowb = bubble.GetRow(); // la structure du datapo sera défini à ce moment si nécessaire
                    if (!bubble.IsDefinedSchema()) bubble.InitalizeDataPO();
                    if (!bubble.IsDefinedSchema()) throw new Exception("DataPo not defined"); // on peus pas traiter un datapo sans structure, car on aura pas les primarykey pour le sql
                    if (string.IsNullOrWhiteSpace(rowb.Table.TableName)) throw new Exception("tablename empty");
                    // mut.WaitOne();

                    Dictionary<string, object> keysParameters = DATASET.DataSetTools.GetValues(rowb, true, false); //clef
                    await this.Connector.UpdateAsync(rowb.Table.TableName, keysParameters, valeursParameters);

                }


            }
            catch (Exception e)
            {
                throw new Exception("UpdatePO " + e.Message, e);
                //exceptionbulle("Impossible de mettre à jour", e);
            }
            finally
            {
                //  mut.ReleaseMutex();
            }
        }

        /// <summary>
        /// Mettre à jours plusieurs objets en même temps avec les mêmes valeurs
        /// </summary>
        public async Task UpdatePOAsync(DATAPO.DataPO[] bubbles, string columnKey, object ColumnValue)
        {
            await this.UpdatePOAsync(bubbles, new Dictionary<string, object>() { { columnKey, ColumnValue } });
        }






        /// <summary>
        /// Insertion de du dataPo en base
        /// </summary>
        public async Task InsertPOAsync(DATAPO.DataPO[] bubbles)
        {
            if (bubbles == null || bubbles.Length == 0) return;
            try
            {
                if (this.Connector == null) throw new Exception("MasterConnector Not found");

                // Vérifier qu'il s'agit toujours du même type d'objets (on ne peus pas travailler sur plusieurs tables)
                if (bubbles.Select(b => b.GetType()).Distinct().Count() != 1) throw new Exception("different types");

                // Préparation des objets
                bubbles.ToList().ForEach(bubble => DataPOTools.PrepareDataPOForDB(bubble, true));


                // !!!voir si c'est pas plus performant de le faire un par un si il n'y as qu'une seule ligne
                //List<System.Data.DataTable> alltables = bubbles.Select(dt => dt.GetRow().Table).Distinct().ToList();
                System.Data.DataTable tabinsert = DataPOTools.CloneInNewDataTable(bubbles);

                // Obtient si existe la colonne autoincrémenté
                List<System.Data.DataColumn> autoincrementedColumns = bubbles.FirstOrDefault().GetRow().Table.GetColumns().Where(c => c.AutoIncrement).ToList();
                autoincrementedColumns.ForEach(col=> {if(tabinsert.Columns.Contains(col.ColumnName)) tabinsert.Columns.Remove(col.ColumnName); });

                string colautoincrement = null;
                if (autoincrementedColumns.Count() > 0) colautoincrement = autoincrementedColumns.FirstOrDefault().ColumnName;
                List<long> valsincrement = await this.Connector.InsertTableAsync(tabinsert, InsertPODefaultTimeOut, colautoincrement);

                // On réaffecte les id autoincrémenté aux objets
                if (!string.IsNullOrWhiteSpace(colautoincrement))
                {
                    if (valsincrement.Count != bubbles.Count()) throw new Exception("Insert Increment Error"); // il doit forcément y avoir autant de clef incrémenté que de lignes
                    for (int i = 0; i < valsincrement.Count; i++)
                        bubbles[i].SetObject(colautoincrement, valsincrement[i], DataAccessorOptionEnum.CreateIfNotExist); 
                }

                //var colsDatasInsert = MANIPULATE.DATASET.DataSetTools.GetValues(rowb, UseKey, true);
                //var colsKeysInsert = MANIPULATE.DATASET.DataSetTools.GetValues(rowb, true, false);
                //long retnum = this.Connector.Insert(rowb.Table.TableName, colsDatasInsert, colautoincrement);

                // Déclare les objet inserer, AcceptChanges, pour indiquer qu'il ne sera pas nécessaire de les remodifier
                foreach (DataPO item in bubbles)
                {
                    try
                    {
                        System.Data.DataRow rowb = item.GetRow();
                        if (rowb.RowState == System.Data.DataRowState.Detached)
                            rowb.Table.Rows.Add(rowb);
                        if (!DisableAcceptChange)
                            rowb.AcceptChanges();
                    }
                    catch (Exception) { }
                }

      
            }
            catch (Exception e)
            {
                throw new Exception("InsertPO " + e.Message, e);
                //exceptionbulle("Impossible de mettre à jour", e);
            }
            finally
            {
                //  mut.ReleaseMutex();
            }
        }
        public async Task InsertPOAsync(DATAPO.DataPO bubble) { await this.InsertPOAsync(new DataPO[] { bubble }); }

        public void InsertPO(DATAPO.DataPO bubble) { this.InsertPOAsync(new DataPO[] { bubble }).GetAwaiter().GetResult(); }

        public int InsertPODefaultTimeOut = 600;






















        /// <summary>
        /// Supprimer un objet en base
        /// </summary>
        /// <param name="bubble"></param>
        public async Task DeletePOAsync(params DATAPO.DataPO[] bubbles)
        {
            try
            {
                if (this.Connector == null) throw new Exception("MasterConnector Not found");

                // Préparation des objets
                bubbles.ToList().ForEach(bubble => DataPOTools.PrepareDataPOForDB(bubble, false));

                foreach (var bubble in bubbles)
                {
                    if (bubble == null) throw new Exception("DataPO null");
                    System.Data.DataRow rowb = bubble.GetRow(); // la structure du datapo sera défini à ce moment si nécessaire
                    if (rowb == null || !bubble.IsDefinedSchema()) throw new Exception("DataPo not defined"); // on peus pas traiter un datapo sans structure, car on aura pas les primarykey pour le sql
                                                                                                               // mut.WaitOne();

                    await this.Connector.DeleteAsync(rowb.Table.TableName, DATASET.DataSetTools.GetValues(rowb, true, false));
                    try { rowb.Delete(); }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                throw new Exception("DeletePO " + e.Message, e);
                //exceptionbulle("Impossible de mettre à jour", e);
            }
            finally
            {
                //  mut.ReleaseMutex();
            }
        }
        public void DeletePO(params DATAPO.DataPO[] bubbles) { this.DeletePOAsync(bubbles).GetAwaiter().GetResult(); }



        #endregion






    }
}
