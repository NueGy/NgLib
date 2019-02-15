// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/Nuegy/NGLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
namespace Nglib.DATA.DATAPO
{

    // !!!  A terminer
    // Il faut sérialiser le datapo en utilisant le DataTable.WriteXML
    // rechercher toutes les autes datapo inclus dans l'objet (simple ou dans une List<>) et les sérialiser aussi
    // puis désérialiser  (Réassembler les blocs de string et instancier les objets)
    // et tester au maximum
    // C'est une sérialisation custom, pas de standard officiel



    /// <summary>
    /// Permettre la sérialisation des Datapo ET des autres Datapo inclus par reflexion
    /// </summary>
    public class DataPoSerializer
    {



        #region Sérialisation 

        /// <summary>
        /// Sérialiser le Datapo
        /// </summary>
        /// <param name="po">Objet racine à sérialiser</param>
        /// <param name="includeInternalPO">Inclure les sous objets</param>
        /// <param name="fromProperty">Pour les sérialisations en boucles</param>
        /// <returns>string sérialiser custom</returns>
        public static string SerializeDatas(DataPO po, bool includeInternalPO = true, PropertyInfo fromProperty = null)
        {
            try
            {
                StringBuilder retour = new StringBuilder();
                retour.Append("[[$");
                System.Data.DataRow rowd = po.GetRow();
                string strd = MANIPULATE.DATASET.DataTableTools.SerializeDataRow(rowd);
                if (!string.IsNullOrEmpty(strd))
                {
                    retour.Append("[[!dataxml!]]");
                    retour.Append(strd);
                    retour.AppendLine();
                }
                if (!includeInternalPO) return retour.ToString();


                Type typepo = po.GetType();
                PropertyInfo[] properties = typepo.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo propertie in properties)
                {
                    string strproper = ToDatasXMLProperty(propertie, po);
                    if (string.IsNullOrWhiteSpace(strproper)) continue;
                    retour.AppendFormat("[[!{0}!]]", propertie.Name);
                    retour.Append(strproper);
                    retour.AppendLine();
                }
                retour.Append("$]]");
                return retour.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetDatasXML" + ex.Message, ex);
            }
        }

        private string ToDatasXMLProperty(PropertyInfo property, DataPO po)
        {
            if (property == null || po == null) return null;
            try
            {
                // !!! filtrer les types innutiles
                object val = property.GetValue(po, null);
                if (val == null) return null;
                if (val is DataPO)
                {
                    string str = this.SerializeDatas((DataPO)val, true, property);
                    return str;
                }

                var attributes = property.GetCustomAttributes(true);
                foreach (var item in attributes)
                {
                    if (item is DATAPO.JoinPOAttribute)
                    {
                        if (val is List<DataPO>) // si c'est une liste
                        {
                            return this.ToDatasXMLMultiPos(property, (List<DataPO>)val);
                        }
                    }

                }
                return null;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }

        private string ToDatasXMLMultiPos(PropertyInfo property, List<DataPO> pos)
        {
            if (pos == null || pos.Count == 0) return null;
            List<System.Data.DataRow> rows = new List<DataRow>();
            foreach (var item in pos) rows.Add(item.GetRow());
            System.Data.DataTable ret = DATA.MANIPULATE.DATASET.DataTableTools.UnifyRowInDatatable(rows);

            return DATA.MANIPULATE.DATASET.DataTableTools.SerializeDataTable(ret);
        }


        #endregion




        #region deserialisation


        //(System.Data.DataTable ret, object paramconstructeur = null) where Tobj : DATA.DATAPO.DataPO, new()
        /// <summary>
        /// Désérialisation des données
        /// </summary>
        /// <param name="SerialInputData">Données sérialisé</param>
        /// <returns></returns>
        public DataPO DeserializationDatas(string SerialInputData, DataPO orgnPo = null)
        {
            if (string.IsNullOrWhiteSpace(SerialInputData)) return null;
            DataPO retour = orgnPo;
            try
            {

                System.Data.DataTable ret = new DataTable();
                //if(retour==null)retour = new    




                //string[] orgstringT = orgstring.Split(new string[] { "[[!!" }, StringSplitOptions.RemoveEmptyEntries);

                //foreach (string orgstringTitem in orgstringT)
                //{
                //    string nameValue = orgstringTitem.Substring(0, orgstringTitem.IndexOf("!!]]") + 1);
                //    string value = orgstringTitem.Substring((nameValue.Length + 4) - 1);
                //    retour.Add(nameValue, value);
                //}
                //return retour;






                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("DeserializationDatas " + ex.Message, ex);
            }

        }

        //private DATAPO.DataPO FromDatasXMLPO(string SerialInputData)

        #endregion





        #region Sérialisation Binaire light


        //    // sera fait dans un second temps
        //private byte[] Tobytes(DataPO po) // !!! a faire
        //{
        //    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    MemoryStream ms = new MemoryStream();
        //    bf.Serialize(ms, po);
        //    return ms.ToArray();
        //}

        //private DataPO  FromBytes(byte[] arrBytes)// !!! a faire
        //{
        //    MemoryStream memStream = new MemoryStream();
        //    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binForm = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    memStream.Write(arrBytes, 0, arrBytes.Length);
        //    memStream.Seek(0, SeekOrigin.Begin);
        //    Object obj = (Object)binForm.Deserialize(memStream);
        //    if (obj is DATA.DATAPO.DataPO)
        //    {
        //        DataPO po = (DATA.DATAPO.DataPO)obj;
        //        return po;
        //    }
        //    else
        //        return null;
        //}



        #endregion





        #region Tools Serialisation et reflexion

        private static Dictionary<string, object> DictionaryFromType(object atype)
        {
            if (atype == null) return new Dictionary<string, object>();
            Type t = atype.GetType();
            PropertyInfo[] props = t.GetProperties();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (PropertyInfo prp in props)
            {
                object value = prp.GetValue(atype, new object[] { });
                dict.Add(prp.Name, value);
            }
            return dict;
        }

        private List<DATAPO.DataPO> GetAllInternal(DataPO po)
        {
            Type typepo = po.GetType();
            PropertyInfo[] properties = typepo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<DATAPO.DataPO> retour = new List<DataPO>();
            foreach (PropertyInfo prp in properties)
            {
                // Only work with strings
                //if (prp.DeclaringType != typeof(DATAPO.DataPO)) { continue; }

                // If not writable then cannot null it; if not readable then cannot check it's value
                if (!prp.CanWrite || !prp.CanRead) { continue; }
                //if(prp.)
                object objval = null;
                try
                {
                    objval = prp.GetValue(po, new object[] { });
                }
                catch (Exception) // fnc a revoir
                {
                    continue;
                }


                if (!(objval is DATAPO.DataPO)) continue;

                DATAPO.DataPO value = objval as DATAPO.DataPO;
                if (value == null) continue;
                //dict.Add(prp.Name, value);
                retour.Add(value);

            }
            return retour;
        }






        public DataPO GetOrInstanciateInternalPO(DataPO poOrgn, string propName, bool allowInstanciate = true) //where Tobj : DATA.DATAPO.DataPO, new()
        {
            DataPO retour = null;
            Type poType = poOrgn.GetType();
            PropertyInfo poProperty = poType.GetProperty(propName);
            object poino = poProperty.GetValue(poOrgn, null);
            if (poino != null)
            {
                if (poino is DataPO) retour = (DataPO)poino;
                else return null; // erreur dans les types
            }
            if (retour == null) retour = (DataPO)Activator.CreateInstance(poProperty.PropertyType);
            return retour;
        }


        public IEnumerable<Tobj> GetOrInstanciateInternalPOs<Tobj>(DataPO poOrgn, string propName, bool allowInstanciate = true) where Tobj : DATAPO.DataPO, new()
        {
            return null;
        }



        #endregion





    }





}
