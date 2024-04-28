using Nglib.DATA.KEYVALUES;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nglib.DATA.COLLECTIONS
{


    /// <summary>
    /// Représente un résultat avec des informations supplémentaires
    /// </summary>
    [Obsolete("BETA")]
    public class ListResult<T> : List<T> 
    {
 
        public ListResult()
        {}

        public ListResult(IEnumerable<T> orgnData) : base(orgnData)
        {}


        public BASICS.ResultInfoModel info { get; set; } = new BASICS.ResultInfoModel();
         

        public string ToJsonWithEnveloppe(JsonSerializerOptions serialopt=null)
        {
            var objtoSerial = new { data = this, info = this.info };
            if(serialopt==null) serialopt = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            string json = JsonSerializer.Serialize(objtoSerial, objtoSerial.GetType(), serialopt);
            return json;
        }
 

        public static ListResult<T> FromList(IEnumerable<T> orgnData)
        {
            if (orgnData == null) return null;
            return new ListResult<T>(orgnData);
        }



        public static ListResult<T> PrepareForError(string errorMsg)
        {
            var retour = new ListResult<T>();
            retour.info.ErrorMessage = errorMsg;
            return retour;
        }

    }
 
}