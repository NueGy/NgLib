using Nglib.MANIPULATE.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.MANIPULATE.TEXT
{



    public class TextStructurePart  : DATAVALUES.DataValuesNode,  IComparable
    {
        /// <summary>
        /// Nom du text
        /// </summary>
        public string Name { get { return base.Name; } set { /* !!!m!!! */} }

        /// <summary>
        /// Contien la position
        /// </summary>
        public int Position { get { return this.GetInt("position"); } set { this.SetObject("position",value); } }
        //private int? PositionCache = 0;

        /// <summary>
        /// Contien la position
        /// </summary>
        public int Length { get { return this.GetInt("length"); } set { this.SetObject("length", value); } }

        /// <summary>
        /// DefaultValue
        /// </summary>
        public string DefaultValue { get { return this.GetString(null); } set { base.Value = value; } }



        /// <summary>
        /// Transformation en nombre, et padleft de 0 si nécessaire
        /// </summary>
        public bool CompleteNumber { get { return this.GetBoolean("CompleteNumber"); } set { this.SetObject("CompleteNumber", value); } }



        /// <summary>
        /// Nom du champs dans la base de données
        /// </summary>
        public string DataBaseColumn { get; set; }













        


        /// <summary>
        /// Valider et transformer une données selon une structure
        /// </summary>
        /// <param name="value">valeur</param>
        /// <param name="Transform">Transformer ou non</param>
        /// <param name="prefix">chaine_</param>
        /// <returns></returns>
        public string ValidateValue(string value, bool Transform=true, string prefix="chaine_")
        {
            try
            {
                Dictionary<string, object> fcnData = new Dictionary<string, object>();
                //foreach (DATAVALUES.DataValues_attribut itematr in this.data.getattributs(prefix))
                //    fcnData.Add(itematr.name.ToLower().Replace(prefix, ""), itematr.value);

                //if (Transform) value=TEXT.TextTreatment.Transform(fcnData, value);
                //TEXT.TextTreatment.Validate(fcnData, value);
                ///!!!m!!!
                return value;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(Name)) throw new Exception("["+this.Name+"]"+ex.Message,ex);
                throw new Exception("[" + this.Position.ToString() + "]" + ex.Message, ex);
            }

        }








        
    }





}
