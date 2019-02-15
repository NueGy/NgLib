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
using System.Xml.Linq;

namespace Nglib.DATA.DATAVALUES
{

    public class DataValuesNode : DATA.ACCESSORS.IDataAccessor,  IComparable
    {

        /// <summary>
        /// Nom
        /// </summary>
        private string _fullname { get; set; }

        /// <summary>
        /// Données brut
        /// </summary>
        private object _value { get; set; }

        /// <summary>
        /// Si la valeur à été modifiée
        /// </summary>
        public System.Data.DataRowState ChangedState = System.Data.DataRowState.Unchanged;

        /// <summary>
        /// Sous valeurs
        /// </summary>
        public Dictionary<string,object> Attributs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Obtient l'objet DATAVALUES parent si il existe
        /// </summary>
        public DataValues datavalues_parent = null;


        public DataValuesNode()
        {
        }


        public DataValuesNode(string fullname)
        {
            this._fullname = fullname;
        }


        /// <summary>
        /// Type de la données demandé (Différent du vrai type de la donnée)
        /// datatype='string'  int,datetime,bool,...
        /// </summary>
        public string DataType
        {
            get { return this["datatype"]; }
            set { this["datatype"] = value.ToLower(); }
        }





        /// <summary>
        /// Obtient ou defini le nom de la données noeud // FULLNAME
        /// </summary>
        public string Name
        {
            get { return _fullname; }
            set { _fullname = ValidateAndCleanDataValueNodeName(value); }
        }





        /// <summary>
        /// Permet d'obtenir le nom simple /param/nom -> nom
        /// </summary>
        public string NodeName
        {
            get { return GetSimpleNameDataValueNode(this._fullname); }
        }



        /// <summary>
        /// obtient la données
        /// </summary>
        public object Value
        {
            get { return this.GetObject(null, DataAccessorOptionEnum.None); }
            set { this.SetObject(null,value); }
        }






        /// <summary>
        /// Obtient ou définit un attribut string. une valeur null détruira l'attribut 
        /// </summary>
        /// <returns></returns>
        public string this[string nameattribut, bool dynamic = false]
        {
            get { return this.GetString(nameattribut, dynamic? DataAccessorOptionEnum.Dynamise: DataAccessorOptionEnum.None); }
            set { this.SetObject(nameattribut, value); }
        }







        /// <summary>
        /// Attention ne copie pas le nom
        /// </summary>
        /// <param name="dataext"></param>
        /// <param name="ecraser"></param>
        public void Fusion(DataValuesNode dataext, bool ecraser = false)
        {
            if (ecraser || this._value == null)
            {
                if (ChangedState == System.Data.DataRowState.Unchanged && this._value != dataext._value) ChangedState = System.Data.DataRowState.Modified; 
                this._value = dataext._value;
            }
            foreach (var itemd in dataext.Attributs)
            {
                if (ecraser || this[itemd.Key] == "") this.SetObject(itemd.Key, itemd.Value);
            }
        }


        /// <summary>
        /// Clone dans de nouveaux objets
        /// </summary>
        public DataValuesNode Clone()
        {
            DataValuesNode clona = new DataValuesNode();
            clona.Value = this.Value;
            clona.Name = this.Name;
            clona.datavalues_parent = this.datavalues_parent;
            
            foreach (var bifdata in this.Attributs) // cloner dictionary !!!
            {
                clona.Attributs.Add(bifdata.Key,bifdata.Value);
            }
            return clona;
        }













            



   


        public void AcceptChange()
        {
            this.ChangedState = System.Data.DataRowState.Unchanged;
        }



        /// <summary>
        /// Accessor sur les attributs
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        public object GetObject(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            if (string.IsNullOrWhiteSpace(nameValue)) return this._value;
            if (this.Attributs.ContainsKey(nameValue)) return this.Attributs[nameValue];
            return null;
        }

        public bool SetObject(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if (ChangedState == System.Data.DataRowState.Unchanged) ChangedState = System.Data.DataRowState.Modified; // !!! améliorer pour gérer les cas ou la valeur est identique et donc ne change pas vraiement
            if (string.IsNullOrWhiteSpace(nameValue)) { this._value = obj; } // Si vide, c'est qu'il s'agit de l'objet principal
            else
            { // sinon il s'agit d'un attributxml
                if (this.Attributs.ContainsKey(nameValue)) this.Attributs[nameValue] = obj; 
                else this.Attributs.Add(nameValue.ToLowerInvariant(), obj);
            }
            return true;
        }



        public string[] ListFieldsKeys()
        {
            List<string> retour = new List<string>() { "" }; // on ajoute une clef vide qui désigne l'objet de base
            retour.AddRange(this.Attributs.Keys);
            return retour.ToArray();
        }



        public int CompareTo(object obj)
        {
            DataValuesNode Compare = (DataValuesNode)obj;
            if (!string.IsNullOrEmpty(Compare["dataorder"]) && !string.IsNullOrEmpty(this["dataorder"]))
            {
                try
                {
                    int orderCompare = Convert.ToInt32(Compare["dataorder"]);
                    int orderthis = Convert.ToInt32(this["dataorder"]);
                    int resulti = orderthis.CompareTo(orderCompare);
                    if (resulti == 0) resulti = orderthis.CompareTo(orderCompare);
                    return resulti;
                }
                catch (Exception) { }

            }
            int result = this.Name.CompareTo(Compare.Name);
            if (result == 0) result = this.Name.CompareTo(Compare.Name);
            return result;
        }




        public override string ToString()
        {
            if (this.Value == null) return string.Empty;
            return this.Value.ToString();
        }




        // --- FOnctions



        public static string ValidateAndCleanDataValueNodeName(string original)
        {
            if (string.IsNullOrWhiteSpace(original)) throw new Exception("Impossible d'affecter une null au name d'un noeud Datavalues");
            original = original.ToLower();
            if (!original.Contains("/")) original = "/param/" + original;
            return original;
        }

        public static string GetSimpleNameDataValueNode(string original)
        {
            if (string.IsNullOrWhiteSpace(original)) return string.Empty;
            string zz = "";
            string[] rr = original.Split('/');
            if (rr.Length > 1) zz = rr[rr.Length - 1];
            else if (rr.Length == 1) zz = original;
            return zz;
        }










        internal static System.Xml.Linq.XElement ToXml(DataValuesNode model)
        {
            if (model == null) return null;
            XElement element = new XElement(model.Name);
            element.Value = model.GetString(null);
            //if (!string.IsNullOrEmpty(model.IssuerElementId)) element.Add(new XAttribute("id", model.IssuerElementId));
            //element.Add(new XAttribute("type", "ComplexType"));
            if (model.Attributs != null)
                foreach (var attributeKey in model.Attributs.Keys)
                {
                    string val = model.GetString(attributeKey, DataAccessorOptionEnum.None);
                    if (string.IsNullOrEmpty(val)) continue; // cela sert à rien d'écrire un attribut vide
                    XAttribute nodeAttribute = new XAttribute(attributeKey, val);
                    element.Add(nodeAttribute);
                }

            return element;
        }


        internal static DataValuesNode FromXml(XElement element)
        {
            if (element == null) return null;
            DataValuesNode model = new DataValuesNode();
            model.Name = element.BaseUri;
            
            foreach (XElement delm in element.Descendants())
            {
                if (delm.HasElements) continue;
                if (delm.Parent.HasElements && delm.Parent != element) continue;
                if (model.Attributs.ContainsKey(delm.Name.ToString())) continue; // déja présent (multible attribut interdit)
                model.Attributs.Add(delm.Name.ToString().ToLower(), delm.Value);
            }
            return model;
        }




    }



}
