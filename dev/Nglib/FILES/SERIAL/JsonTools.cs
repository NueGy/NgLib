using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.FILES.SERIAL
{

    /// <summary>
    /// Utilitaires JSON
    /// </summary>
    public static class JsonTools
    {


      
            /// <summary>
            /// Serializes an object to JSON
            /// </summary>
            public static string Serialize<TType>(TType instance) where TType : class
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(TType));
                using (var stream = new System.IO.MemoryStream())
                {
                    serializer.WriteObject(stream, instance);
                    return Encoding.Default.GetString(stream.ToArray());
                }
            }

            /// <summary>
            /// DeSerializes an object from JSON
            /// </summary>
            public static TType DeSerialize<TType>(string json) where TType : class
            {
                using (var stream = new System.IO.MemoryStream(Encoding.Default.GetBytes(json)))
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(TType));
                    return serializer.ReadObject(stream) as TType;
                }
            }
        
    }
}
