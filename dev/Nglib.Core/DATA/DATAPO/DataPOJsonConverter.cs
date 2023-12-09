using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Nglib.DATA.DATAPO
{

    public class DataPOJsonConverter : DataPOJsonConverter<Nglib.DATA.DATAPO.DataPO> { }

    /// <summary>
    /// Classe de conversion spécifique pour retourner l'objet intégral 
    /// A COPIER DANS NGLIB !!!
    /// </summary>
    public class DataPOJsonConverter<TPO> : System.Text.Json.Serialization.JsonConverter<TPO> where TPO : Nglib.DATA.DATAPO.DataPO
    {



        public override TPO Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("DataPOJsonConverter.Read " + ex.Message, ex);
            }
        }
        public override void Write(Utf8JsonWriter writer, TPO value, JsonSerializerOptions options)
        {
            if (value == null) return;
            try
            {
                Dictionary<string, object> fullValues = GetFullValues(value);
                WriteDictionary(writer, fullValues);
            }
            catch (Exception ex)
            {
                throw new Exception("DataPOJsonConverter.Write " + ex.Message, ex);
            }
        }
        public override bool CanConvert(Type typeToConvert)
        {
            return base.CanConvert(typeToConvert);
        }
        //public override bool HandleNull => base.HandleNull;


        public static Dictionary<string, object> GetFullValues(Nglib.DATA.DATAPO.DataPO item, bool preserveFluxNames = false)
        {
            if (item == null) return null;
            Dictionary<string, object> retour = new Dictionary<string, object>();
            Nglib.DATA.COLLECTIONS.CollectionsTools.AddRange(retour, Nglib.DATA.DATAPO.DataPOTools.GetValues(item, true, true), false);
            // Ajouter fullid,ajouter flux !!! 


            return retour;
        }
        public static void SetFullValues(Dictionary<string, object> datas, Nglib.DATA.DATAPO.DataPO item, bool preserveFluxNames = false)
        {
            // supprimer fullid,ajouter flux !!! 
            Nglib.DATA.DATAPO.DataPOTools.SetValues(item, datas);
        }

        /// <summary>
        /// Ecrire un dictionary amélioré
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="datas"></param>
        public static void WriteDictionary(Utf8JsonWriter writer, Dictionary<string, object> datas)
        {
            JsonSerializer.Serialize(writer, datas);
        }


    }
}
