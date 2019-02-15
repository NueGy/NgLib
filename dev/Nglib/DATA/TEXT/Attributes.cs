using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{



    /// <summary>
    /// Attribut de ligne de fichier
    /// </summary>
    public class TextLineAttribute : System.Attribute
    {
        /// <summary>
        /// Type de structure
        /// </summary>
        public TextModeEnum TextMode = TextModeEnum.CSV;

        /// <summary>
        /// Nom de la structure
        /// </summary>
        public string StructureLineName { get; set; }

        /// <summary>
        /// La ligne doit obligatoirement commencé par
        /// </summary>
        public string Startwith { get; set; }




        public TextLineAttribute()
        {
        }



        public void ToSchema(TextStructure item)
        {
            item.TextMode = TextMode;
            item.StructureLineName = this.StructureLineName;
            ///Startwith//!!!
        }




        public static TextLineAttribute GetObjectLineAttributes(System.Type t)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t, typeof(TextLineAttribute));  // Reflection.
            foreach (System.Attribute attr in attrs)
            {
                if (attr is TextLineAttribute) return (TextLineAttribute)attr;
            }
            return null;
        }


    }


    /// <summary>
    /// Attribut de champ
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    //[System.ComponentModel.EditorBrowsable(EditorBrowsableState.Never)]
    public class TextPartAttribute : System.Attribute
    {
        /// <summary>
        /// Nom du champs
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// la position
        /// </summary>
        public int Position { get; set; }


        /// <summary>
        /// la taille du champs
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Controle Regex
        /// </summary>
        public string ValidateRegex { get; set; }
        //controle regex


        /// <summary>
        /// Nom dans la base de données
        /// </summary>
        public string DataBaseColumn { get; set; }






        /// <summary>
        /// Completer par des 0
        /// </summary>
        public bool CompleteNumber { get; set; }

        /// <summary>
        /// La valeur par default
        /// </summary>
        public string DefaultValue { get; set; }


        /// <summary>
        /// Options suplémentaires (transformation  Voir https://github ...)
        /// </summary>
        public DATAVALUES.DataValuesNode OptionsPart { get; set; }




        public void ToSchema(TextStructurePart item)
        {
            if (this.OptionsPart != null) item.Fusion(this.OptionsPart);
            item.Name = this.Name;
            item.Position = this.Position;
            item.DataBaseColumn = this.DataBaseColumn;
            item.DefaultValue = this.DefaultValue;


            // Traitements spécifiques régulièrement utilisé réécrit pour plus de facilité d'accès
            if (!string.IsNullOrEmpty(this.ValidateRegex))
                item[FORMAT.StringTransform.DefaultPrefix + "regex"] = this.ValidateRegex;
            if (this.CompleteNumber)
                item[FORMAT.StringTransform.DefaultPrefix + "CompleteNumber"] = "true";

        }





        internal string ObjectPropertyName { get; set; }
        internal System.Reflection.PropertyInfo ObjectPropertyInfo { get; set; }





        public static TextPartAttribute[] GetObjectPartAttributes(System.Type t)
        {
            // Using reflection.
            //System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);  // Reflection.
            List<TextPartAttribute> retour = new List<TextPartAttribute>();
            foreach (System.Reflection.PropertyInfo proper in t.GetProperties())
            {

                foreach (System.Attribute item in proper.GetCustomAttributes(false))
                {
                    if (item is TextPartAttribute)
                    {
                        TextPartAttribute itemfield = (TextPartAttribute)item;
                        itemfield.ObjectPropertyName = proper.Name;
                        itemfield.ObjectPropertyInfo = proper;
                        retour.Add(itemfield);
                    }
                }
            }
            return retour.ToArray();
        }
       
    }








}
