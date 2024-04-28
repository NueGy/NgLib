using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.BASICS
{
    /// <summary>
    /// Objet informations résultat de recherche
    /// </summary>
    [Obsolete("soon")]
    public class ResultInfoModel : ISearchResults
    {
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Nombre de résultat total disponible
        /// </summary>
        public int TotalCount { get; set; }









        public void ParseInfoFrom(System.Net.Http.HttpResponseMessage httpResponseMessage)
        {
            if(httpResponseMessage==null) return;   
 
            foreach (var item in httpResponseMessage.Headers)
            {
                if (item.Key == "X-Total-Count" && item.Value.Any())
                    this.TotalCount = int.Parse(item.Value.FirstOrDefault());
                //else if (item.Key == "X-Error-Message")
                //    this.ErrorMessage = item.Value;
            }
        }


    }
}
