// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.DATAVALUES
{



    /// <summary>
    /// Dictionary avancé généralement utilisé pour le paramétrage
    /// </summary>
    public partial class DataValues : IDataAccessor
    {
        private IDictionary<string, DataValuesNode> dataStore = new Dictionary<string, DataValuesNode>();
        protected string _DataValueName;

        /// <summary>
        /// Creer une liste de données
        /// Peut etre stocker dans la bdd, xml, ...
        /// </summary>
        /// <param name="name">nom du datadoc (facultatif mais obligatoire pour sérialisation XML)</param>
        public DataValues(string dataValueName)
        {
            this._DataValueName = dataValueName;
        }
        public DataValues()
        {
            this._DataValueName = "param";
        }

        /// <summary>
        /// Obtenir le nom du datavalue
        /// </summary>
        /// <returns></returns>
        public string GetDataValuesName() { return _DataValueName; }
        /// <summary>
        /// Définir le nom du datavalue
        /// </summary>
        /// <param name="dvname"></param>
        public void SetDataValuesName(string dvname) { _DataValueName = dvname; }








        // -------------------------------------------------------------------------------------------
        #region Acces au donnees


        /// <summary>
        /// Obtenir un parametre (null = delete)
        /// </summary>
        public object this[string nameValue]
        {
            get
            {
                return this.GetObject(nameValue, DataAccessorOptionEnum.None);
            }
            set
            {
                this.SetObject(nameValue, value, DataAccessorOptionEnum.CreateIfNotExist);
            }
        }


        /// <summary>
        /// Obtenir l'Attribut d'un paramètre
        /// </summary>
        /// <param name="ise"></param>
        /// <param name="nameAttribut"></param>
        /// <returns></returns>
        public string this[string ise, string nameAttribut]
        {
            get
            {
                ise=this.PrepareNameNode(ise);
                DataValuesNode retour = this.Get(ise);
                if (retour == null) return null;
                else return retour[nameAttribut, false];
            }
            set
            {
                ise = this.PrepareNameNode(ise);
                DataValuesNode retour = this.Get(ise);
                if (retour == null)
                {
                    retour = new DataValuesNode(ise);
                    Add(retour);
                }
                retour[nameAttribut] = value;
            }
        }



        public DataValuesNode Get(string ise, bool CreateIfNotExist=true)
        {
            ise = this.PrepareNameNode(ise);
            DataValuesNode retour = null;
            if (!this.dataStore.ContainsKey(ise)) return null;
            retour=this.dataStore[ise];
            if (retour == null && CreateIfNotExist)
            {
                retour = new DataValuesNode(ise);
                Add(retour);
            }
            return retour;
        }
            


        /// <summary>
        /// Efface une donnée
        /// </summary>
        /// <param name="name">nom de l'élément</param>
        public void Remove(string namedata)
        {
            namedata = this.PrepareNameNode(namedata);
            //if (namedata.Substring(namedata.Length - 1) != "/") namedata = namedata + "/"; 
            try
            {
                WaitMutex();
                this.dataStore.Remove(namedata);
                return;
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                ReleaseMutex();
            }
        }

        /// <summary>
        /// ajoute une donnée pure
        /// </summary>
        /// <param name="name">nom de l'élément</param>
        public void Add(DataValuesNode data)
        {
            try
            {
                WaitMutex();
                data.datavalues_parent = this;
                DataValuesNode bufliste = this.Get(data.Name);
                if (bufliste != null) this.Remove(data.Name);
                this.dataStore.Add(data.Name, data);
            }
            catch (Exception)
            {

            }
            finally
            {
                ReleaseMutex();
            }
        }







        public object GetObject(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                DataValuesNode dat = this.Get(nameValue);
                if (dat == null) return null;
                else return dat.Value;

            }
            catch (Exception)
            {
                return null;
            }
        }


        

        /// <summary>
        /// Ajouter un objet
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="obj"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public bool SetObject(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                //if (!AccesOptions.HasFlag(BASICS.DataAccessorOptionEnum.NotReplace)) this.DelData(nameValue);
                // mutex déja pris
                object orgnobj = this.GetObject(nameValue, DataAccessorOptionEnum.None);
                if ((orgnobj == null || (orgnobj is string && string.IsNullOrEmpty(orgnobj.ToString()))) && (obj == null || (obj is string && string.IsNullOrEmpty(obj.ToString()))))
                    return false;
                if (obj != null && orgnobj!=null && obj.Equals(orgnobj)) return false; // inutile si pas modifié

                nameValue = this.PrepareNameNode(nameValue);
                DataValuesNode retour = this.Get(nameValue);
                if (retour == null)
                {
                    retour = new DataValuesNode(nameValue);
                    Add(retour);
                }
                retour.SetObject(null,obj);


                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }





        





        #endregion




        // -------------------------------------------------------------------------------------------
        #region Liste de données

        /// <summary>
        /// Récupère une liste de tuotes les Dta_Values contenues dans le dataStore
        /// </summary>
        /// <returns></returns>
        public List<DataValuesNode> GetList()
        {
            WaitMutex();
            List<DataValuesNode> listea = this.dataStore.Values.ToList();
            ReleaseMutex();
            return listea;
        }


        public string[] ListFieldsKeys()
        {
            return ListFieldsKeys(null);
        }

        public string[] ListFieldsKeys(string childrensOf)
        {
            try
            {
                WaitMutex();

                if (string.IsNullOrWhiteSpace(childrensOf))
                    return this.dataStore.Keys.ToArray();

                char[] separator = new List<char>() { '/' }.ToArray();
                List<string> retour = new List<string>();
                List<DATAVALUES.DataValuesNode> all = this.GetDatas(childrensOf);
                foreach (var itemdv in all)
                {
                    string nameM = itemdv.Name.ToLower();
                    if (nameM.Length < childrensOf.Length || nameM.Substring(0, childrensOf.Length) != childrensOf) continue;
                    string[] dvnameT = itemdv.Name.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    string[] trinameT = childrensOf.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    string findname = null;
                    if (dvnameT.Length <= trinameT.Length) continue; // doit etre superieur
                    findname = dvnameT[(trinameT.Length)];

                    if (!string.IsNullOrWhiteSpace(findname) && !retour.Contains(findname)) retour.Add(findname);
                }
                return retour.ToArray();

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                ReleaseMutex();
            }
        }









     



        /// <summary>
        /// Obtient tous les data qui contien l'attribut 'attributname'
        /// </summary>
        /// <param name="attributname"></param>
        /// <param name="replacenameparattribut"></param>
        /// <returns></returns>
        public DataValues GetDatasWithAttribut(string attributname, bool replacenameparattribut = false)
        {
            try
            {

                DATAVALUES.DataValues retour = new DataValues();
                WaitMutex();
                foreach (DATAVALUES.DataValuesNode datb in this.dataStore.Values)
                {
                    if (datb[attributname] != "")
                    {
                        DATAVALUES.DataValuesNode ee = datb.Clone();
                        if (replacenameparattribut) ee.Name = datb[attributname];
                        retour.Add(ee);
                    }
                }
                return retour;
            }
            catch (Exception e)
            {
                throw new Exception("Impossible d'obtenir des datas avec l'attribut " + attributname + " " + e.Message);
            }
            finally
            {
                ReleaseMutex();
            }
        }

 


        public List<DATAVALUES.DataValuesNode> GetDatas(string childrensOf=null)
        {
            List<DATAVALUES.DataValuesNode> retour = new List<DataValuesNode>();
            if (string.IsNullOrWhiteSpace(childrensOf)) return retour;
            childrensOf = childrensOf.ToLowerInvariant();
            try
            {
                WaitMutex();
                DATAVALUES.DataValuesNode[] datalocal = this.dataStore.Values.ToArray();
                for (int i = 0; i < this.dataStore.Count; i++)
                {
                    try
                    {
                        string nameM = datalocal[i].Name.ToLower();
                        if (nameM.Length < childrensOf.Length || nameM.Substring(0, childrensOf.Length) != childrensOf) continue;
                        retour.Add(datalocal[i]);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            finally
            {
                ReleaseMutex();
            }
            return retour;
        }










        #endregion





        // -------------------------------------------------------------------------------------------
        #region Traitement des données



        /// <summary>
        /// Creer une liste de données
        /// </summary>
        /// <param name="dodoc">autre datadoc, Ne link pas les données, il recopie intégralement l'objet</param>
        public DataValues Clone()
        {
            DataValues retour = new DataValues(this._DataValueName);
            try
            {
                WaitMutex();
                foreach (DataValuesNode itemd in this.dataStore.Values)
                {
                    retour.Add(itemd.Clone());
                }
            }
            finally
            {
                ReleaseMutex();
            }
            return retour;
        }

        /// <summary>
        /// Delete liste
        /// </summary>
        public void Clear()
        {
            this.dataStore.Clear();
        }
        /// <summary>
        /// taille liste, nb elements
        /// </summary>
        public int Count()
        {
            return this.dataStore.Count;
        }













        protected void ReleaseMutex()
        {
            // mut.ReleaseMutex();
            }

        protected void WaitMutex()
            {
            // mut.WaitOne();
        }


        

        private string PrepareNameNode(string original)
        {
            original = original.ToLowerInvariant();
            if (!original.Contains("/")) original = "/param/" + original;
            return original;
        }



        /// <summary>
        /// Trier la liste (Remplace le dictonary par sorteddictonary)
        /// </summary>
        /// <returns></returns>
        public void Sort()
        {
            IDictionary<string, DataValuesNode> oldDataStore = this.dataStore;
            SortedDictionary<string, DataValuesNode> newDataStore = new SortedDictionary<string, DataValuesNode>(oldDataStore);
            this.dataStore = newDataStore;
            oldDataStore.Clear();
        }



        public void Fusion(DATAVALUES.DataValues dataext, bool ecraser = false)
        {
            foreach (DataValuesNode itemd in dataext.GetList())
            {
                DataValuesNode itemsearch = this.Get(itemd.Name);
                if (itemsearch != null && itemsearch.Name != "")
                {
                    if (ecraser) this.Add(itemd.Clone());
                }
                else this.Add(itemd.Clone());
            }
        }


        /// <summary>
        /// Présence de changements
        /// </summary>
        /// <returns></returns>
        public bool IsChanges()
        {
            if (this.GetList().Count(d=>d.ChangedState!= System.Data.DataRowState.Unchanged) > 0) return true;
            else return false;
        }




        /// <summary>
        /// Marque pour toutes les données que les changements ont été pris en comptes
        /// </summary>
        /// <returns>retourne si un changement était présent</returns>
        public bool AcceptChanges()
        {
            bool retour = false;
            foreach (var item in this.GetList())
            {
                if(item.ChangedState != System.Data.DataRowState.Unchanged)
                {
                    item.ChangedState = System.Data.DataRowState.Unchanged;
                    retour = true;
                }
            }
            return retour;
        }





        #endregion







  



    }
}
