using Nglib.DATA.BASICS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nglib.DATA.COLLECTIONS
{


    /// <summary>
    /// Represents a result list with additional metadata information.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_collections"/></para>
    /// </summary>
    public class ListResult<T>
    {
 
        public ListResult()
        {}

        public ListResult(IEnumerable<T> orgnData) 
        {
            if (orgnData != null)
                this.data = orgnData.ToList();
        }

        /// <summary>
        /// Données de la liste
        /// </summary>
        public List<T> data { get; private init; } = new List<T>();

        /// <summary>
        /// Informations sur le résultat
        /// </summary>
        public ResultMetadataModel info { get; private init; } = new ResultMetadataModel();



        public void Add(T item)
        {
            if (item != null)
                this.data.Add(item);
        }





        public class ResultMetadataModel
        {


            /// <summary>
            /// Nombre de résultat total disponible sur le serveur (Si disponible)
            /// </summary>
            public int TotalCount { get; set; }


            /// <summary>
            /// Message d'erreur
            /// </summary>
            public string Error { get; set; }

            /// <summary>
            /// Temps d'exécution en ms
            /// </summary>
            public long ExecutionTimeMs { get; set; }

            /// <summary>
            /// Identifiant de la requête (Si disponible)
            /// </summary>
            public string RequestId { get; set; }

            /// <summary>
            /// Horodatage de la requête
            /// </summary>
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }



        //public void ParseInfoFrom(System.Net.Http.HttpResponseMessage httpResponseMessage)
        //{
        //    if (httpResponseMessage == null) return;

        //    foreach (var item in httpResponseMessage.Headers)
        //    {
        //        if (item.Key == "X-Total-Count" && item.Value.Any())
        //            this.TotalCount = int.Parse(item.Value.FirstOrDefault());
        //        //else if (item.Key == "X-Error-Message")
        //        //    this.ErrorMessage = item.Value;
        //    }
        //}


 
 

        public static ListResult<T> PrepareForError(string errorMsg)
        {
            var retour = new ListResult<T>();
            retour.info.Error = errorMsg;
            return retour;
        }

    }
 
}