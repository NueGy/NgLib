using Nglib.MANIPULATE.ACCESSORS;
using Nglib.MANIPULATE.TEXT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{

   

    /// <summary>
    /// Permet la manipulations de données, Réprésente les données d'une ligne
    /// Objet Héritable qui représente les données dans chaque ligne
    /// </summary>
    public class TextDatas : Dictionary<string,string>, ACCESSORS.IDataAccessor
    {
        /// <summary>
        /// Structure de la ligne si chargé
        /// </summary>
        protected TextStructure DataSchema = null;
        //protected SortedDictionary<int, TextPart> PartValues = new SortedDictionary<int, TextPart>();


        ///// <summary>
        ///// Chaine stocké en cache pour éviter de recalculer à chaque fois
        ///// </summary>
        //private string CacheStringCalculate = null;


        public TextDatas() { }


        public TextDatas(TextStructure structure)
        {
            this.SetSchema(structure);
        }


        public TextDatas(TextStructure structure, string OriginalValue) : base(TextTools.SplitText(OriginalValue, structure))
        {
            this.SetSchema(structure);
        }




        /// <summary>
        /// Permet de définir la structure de la chaine de données
        /// </summary>
        /// <param name="structure"></param>
        public void SetSchema(TextStructure structure)
        {
            this.DataSchema = structure;
        }

        /// <summary>
        /// Obtient la structure paramétré dans la chaine de données
        /// </summary>
        /// <returns></returns>
        public TextStructure GetSchema()
        {
            return this.DataSchema;
        }


        

        public string[] ListFieldsName()
        {
           // if(StructureLine!=null)return StructureLine.lis
            throw new NotImplementedException(); // !!!m!!!
        }


        /// <summary>
        /// Obtenir/Définir une données dans le dictionary de données de la ligne
        /// </summary>
        /// <param name="numeropartie"></param>
        /// <returns></returns>
        public new string this[int PositionPart]
        {
            get { return this.GetString(PositionPart); } 
            set { this.SetObject(PositionPart.ToString(), value, DataAccessorOptionEnum.CreateIfNotExist); }
        }

        /// <summary>
        /// Obtenir une partie le la chaine en utilisant le nom de la colonne
        /// Il faut que la structure soit préchargé
        /// </summary>
        /// <param name="numeropartie"></param>
        /// <returns></returns>
        public string this[string namePart]
        {
            get { return this.GetString(namePart); }
            set { this.SetObject(namePart, value, DataAccessorOptionEnum.CreateIfNotExist); }
        }


        /// <summary>
        /// Permet d'obtenir la position a partir d'un nom en tenant compte du schema
        /// </summary>
        /// <param name="namepart"></param>
        /// <returns></returns>
        public int GetPosition(string namepart)
        {
            if (string.IsNullOrWhiteSpace(namepart)) throw new Exception("Position invalid");
            namepart = namepart.Trim();
            int positionPartWant = -1;
            // c'est un numéro, donc c'est déja la position, il faut juste le convertir
            if (FORMAT.StringUtilities.IsNumeric(namepart))
            {
                int.TryParse(namepart, out positionPartWant);
            }
            else if (this.DataSchema != null) // on recherche dans la structure la corespondance
            {
                positionPartWant = this.DataSchema.GetPosition(namepart);
            }


            if (positionPartWant < 0) throw new Exception("Position not found");

            return positionPartWant;
        }



        public string GetString(int position)
        {
            try
            {
                // la position est caster en string puis au moment du parcour est recaster en int (pour allez plus vite sur le dev mais cela aura un impact sur les perfs)
                return this.GetString(position.ToString(), DataAccessorOptionEnum.None);// !!! améliorer sans caster en string a chaque fois
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }











        /// <summary>
        /// Obtenir une valeur, methode standard DataAccessor
        /// </summary>
        public object GetObject(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameValue)) return null;
                //int positionPartWant = GetPosition(nameValue);
                

                if (!this.ContainsKey(nameValue)) return null;
                object valstring = base[nameValue];

                // if(AccesOptions.HasFlag( BASICS.DataAccessorOptionEnum.Dynamise)) 

                return valstring;
            }
            catch (Exception)
            {
                if(AccesOptions.HasFlag( DataAccessorOptionEnum.Safe)) return null;
                throw;
            }
        }










        /// <summary>
        /// Définir une valeur, methode standard DataAccessor
        /// </summary>
        public bool SetObject(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameValue)) return false;
                //int positionPartWant = GetPosition(nameValue);
            

                if (!this.ContainsKey(nameValue)) this.Add(nameValue, Convert.ToString(obj));
                else base[nameValue] = Convert.ToString(obj);

                return true;
            }
            catch (Exception ex)
            {

                throw new Exception(string.Format("SetObject({0}){1}", nameValue, ex.Message), ex);
            }
        }



        ///// <summary>
        ///// Permet de définir une valeur ou si la position est définie créer une structure
        ///// </summary>
        ///// <param name="numPart"></param>
        ///// <param name="value"></param>
        ///// <param name="lenght"></param>
        ///// <param name="CreateLegal"></param>
        ///// <returns></returns>
        //public bool SetObjectWithPosition(string namepart, string value, int? position = null, int lenght = 0)
        //{
        //    try
        //    {
        //        int positionPartWant = GetPosition(namepart);

        //        if (positionPartWant < 0) // modecréation
        //        {
        //            if (!position.HasValue) throw new Exception("position required for create part");
        //            string realNamepart = null;
        //            if (!FORMAT.StringUtilities.IsNumeric(namepart)) realNamepart = namepart.ToLower().Trim();
        //            if (this.DataSchema == null) this.DataSchema = new TextStructure();
        //            TextStructurePart StructurePart = this.DataSchema.SetPart(position.Value, lenght, realNamepart); // Défini la structure du champ
        //        }

        //        return this.SetObject(namepart, value, DataAccessorOptionEnum.None);
        //    }
        //    catch (Exception e)
        //    {
        //       // return false;
        //        throw new Exception(string.Format("SetOrCreateString ({0}) {1}", namepart,e.Message),e);
        //        //throw new Exception("Impossible d'affecter une valeur dans une chaine manipulable(" + numPart + "): " + e.Message);
        //    }
        //}






 





        /// <summary>
        /// Retourner la chaine de caractère assemblée
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                return TextTools.JoinText(this, this.DataSchema);
            }
            catch (Exception)
            {
                return base.ToString();
            }
        }


 


    }
}
