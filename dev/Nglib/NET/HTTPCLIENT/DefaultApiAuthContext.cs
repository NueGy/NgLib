using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    public class DefaultApiAuthContext : IApiAuthModel
    {
   
        public HttpClient Client { get; set; }


        public string ApiRootUrl { get; set; }

        public string AppKey { get; set; }

        public string AppSecret { get; set; }

        public string LastToken { get; set; }

        public int GetTenantId()
        {
            return 0;
        }



        public HttpRequestMessage PrepareRequest(HttpMethod method, string reqUri)
        {
            if (!reqUri.StartsWith("http") && !string.IsNullOrWhiteSpace(ApiRootUrl))
                reqUri = ApiRootUrl.TrimEnd('/') + "/" + reqUri.TrimStart('/'); ;
            HttpRequestMessage requestMessage = new HttpRequestMessage(method, reqUri);
            return requestMessage;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage requestMessage)
        {
            if (Client == null) Client = new HttpClient();
            HttpResponseMessage retour = await Client.SendAsync(requestMessage);
            return retour;
        }


        public System.Text.Json.JsonSerializerOptions DefaultJsonSerializerOptions()
        {
            System.Text.Json.JsonSerializerOptions retour = new System.Text.Json.JsonSerializerOptions();

            return retour;
        }



    }
}
