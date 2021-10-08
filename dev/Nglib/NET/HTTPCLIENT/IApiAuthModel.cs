using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    public interface IApiAuthModel
    {
        string ApiRootUrl { get;  }  

        string AppKey { get;  }

        string AppSecret { get;  }

        string LastToken { get; }

        int GetTenantId();


        System.Net.Http.HttpRequestMessage PrepareRequest(System.Net.Http.HttpMethod method, string reqUri);

        Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage requestMessage);


        System.Text.Json.JsonSerializerOptions DefaultJsonSerializerOptions();

    }
     
}
