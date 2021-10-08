// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Nglib.DATA.COLLECTIONS;

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
        public DataPOProviderSQL() : base() { }
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
        [Obsolete("use constructor with param")]
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
            if (this.Connectors == null) throw new Exception("connectors Not loaded");
        }

        /// <summary>
        /// manipulation des dataPo en base
        /// </summary>
        public DataPOProviderSQL(APP.ENV.IGlobalEnv env)
        {
            this.Connectors = env.Connectors;
            if (this.Connectors == null) throw new Exception("Env connectors Not loaded");
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
            //System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
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
            SqlBuilder sqlBuilder = new SqlBuilder(TableDefault, this.Connector.GetEngine()).AddWheres(paramKeySearch).Limit(nbMax);
            return this.GetCollectionPO<TCollectionPO>(sqlBuilder.ToString(), paramKeySearch);
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
        public async Task<bool> SavePOAsync(DATAPO.DataPO[] bubbles, bool ForceEvenIfNotModified = false)
        {
            if (bubbles == null || bubbles.Count() == 0) return false;
            if (this.Connector == null) throw new Exception("MasterConnector Not found");
            List<DATAPO.DataPO> bubblesNeedToSave;
            try
            {
                // Vérifier qu'il s'agit toujours du même type d'objets (on ne peus pas travailler sur plusieurs tables)
                if (bubbles.Select(b => b.GetType()).Distinct().Count() != 1) throw new Exception("different types");
                if(bubbles.FirstOrDefault().GetValues(true,false).Count==0) throw new Exception("Primarykey not found on " + bubbles.FirstOrDefault().GetType().Name);


                // Detecter et préparer les objets qui nécessite une modifications
                if (ForceEvenIfNotModified) bubblesNeedToSave = bubbles.ToList();
                else bubblesNeedToSave=bubbles.Where(bubble => bubble.IsChanges()).ToList();
                if (bubblesNeedToSave.Count == 0) return false;

                await this.SavePo_LineByLinesAsync(bubblesNeedToSave, ForceEvenIfNotModified);

                return true;

            }
            catch (Exception e)
            {
                throw new Exception(string.Format("SavePO {0} ", e.Message), e);
            }
        }
        public async Task<bool> SavePOAsync(DATAPO.DataPO bubble, bool ForceEvenIfNotModified = false) { return  await this.SavePOAsync(new DATAPO.DataPO[]{ bubble }, ForceEvenIfNotModified); }
        public bool SavePO(DATAPO.DataPO bubble, bool ForceEvenIfNotModified = false) { return this.SavePOAsync(bubble, ForceEvenIfNotModified).GetAwaiter().GetResult(); }


        /// <summary>
        /// Execute plusieurs Save en une requette
        /// </summary>
        /// <param name="bubblesNeedToSave"></param>
        /// <param name="ForceEvenIfNotModified"></param>
        private async Task SavePo_LineByLinesAsync(List<DATAPO.DataPO> bubblesNeedToSave, bool ForceEvenIfNotModified)
        {
            bool openedtransact = false;
            try
            {
                if (bubblesNeedToSave.Count > 1) // si nécessaire d'ouvrir une transaction pour optimiser les performances
                    openedtransact = this.Connector.BeginTransaction(null); //this.Connector.Open(true); 
                foreach (var bubble in bubblesNeedToSave)
                {
                    // mut.WaitOne();
                    Dictionary<string, object> vals = ForceEvenIfNotModified ? bubble.GetValues(false, true) : bubble.GetChangedValues();
                    if (vals.Count == 0 && ForceEvenIfNotModified) throw new Exception("GetChangedValues Empty");
                    else if (vals.Count == 0) continue;
                    Dictionary<string, object> keys = bubble.GetValues(true, false);
                    if (keys.Count == 0) throw new Exception("Primarykey Empty");
                    System.Data.DataRow rowb = bubble.GetRow();
                    string tablename = rowb.Table.TableName;
                    await this.Connector.UpdateAsync(tablename, keys, vals);

                    if (!DisableAcceptChange)
                        bubble.AcceptChanges();
                }
                if (openedtransact) this.Connector.CommitTransaction();
            }
            catch (Exception ex)
            {
                if (openedtransact) this.Connector.RollBackTransaction();
                throw;
            }
            finally
            {
                if(openedtransact)this.Connector.Close();
            }

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
                    if (!bubble.IsDefinedSchema()) bubble.DefineSchemaPO();
                    if (!bubble.IsDefinedSchema()) throw new Exception("DataPo not defined"); // on peus pas traiter un datapo sans structure, car on aura pas les primarykey pour le sql
                    if (string.IsNullOrWhiteSpace(rowb.Table.TableName)) throw new Exception("tablename empty");
                    // mut.WaitOne();

                    Dictionary<string, object> keys = DataSetTools.GetValues(rowb, true, false); //clef
                    if (keys.Count == 0) throw new Exception("Primarykey not found on " + bubble.GetType().Name);
                    await this.Connector.UpdateAsync(rowb.Table.TableName, keys, valeursParameters);

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

                // !!!voir si c'est pas plus performant de le faire un par un si il n'y as qu'une seule ligne
                //List<System.Data.DataTable> alltables = bubbles.Select(dt => dt.GetRow().Table).Distinct().ToList();
                System.Data.DataTable tabinsert = DataPOTools.CloneDataTable(bubbles);
                if(tabinsert.TableName.StartsWith("Table"))
                {
                    System.Data.DataTable tableStd = DataPOTools.GetSchemaOnPO(bubbles[0].GetType());
                    if(tableStd!=null) tabinsert.TableName = tableStd.TableName;
                }



                // Obtient si existe la colonne autoincrémenté
                List<System.Data.DataColumn> autoincrementedColumns = bubbles.FirstOrDefault().GetRow().Table.GetColumns().Where(c => c.AutoIncrement).ToList();
                autoincrementedColumns.ForEach(col=> {if(tabinsert.Columns.Contains(col.ColumnName)) tabinsert.Columns.Remove(col.ColumnName); });

                string colautoincrement = null;
                if (autoincrementedColumns.Count() > 0) colautoincrement = autoincrementedColumns.FirstOrDefault().ColumnName;

                // --- INSERT ---
                List<long> valsincrement = await this.Connector.InsertTableAsync(tabinsert, InsertPODefaultTimeOut, colautoincrement);

                // On réaffecte les id autoincrémenté aux objets
                if (!string.IsNullOrWhiteSpace(colautoincrement))
                {
                    if (valsincrement.Count != bubbles.Count()) throw new Exception("Insert Increment Error"); // il doit forcément y avoir autant de clef incrémenté que de lignes
                    for (int i = 0; i < valsincrement.Count; i++)
                        bubbles[i].SetObject(colautoincrement, valsincrement[i]); 
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

                foreach (var bubble in bubbles)
                {
                    if (bubble == null) throw new Exception("DataPO null");
                    Dictionary<string, object> keys = bubble.GetValues(true, false); //clef// mut.WaitOne();
                    if (keys.Count == 0) throw new Exception("Primarykey not found on " + bubble.GetType().Name);
                    System.Data.DataRow rowb = bubble.GetRow(); // la structure du datapo sera défini à ce moment si nécessaire
                    await this.Connector.DeleteAsync(rowb.Table.TableName, keys);
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
