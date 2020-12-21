using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.NET.HTTPCLIENT
{
    public interface IApiAuthModel
    {
        string ApiRootUrl { get;  }  

        string AppKey { get;  }

        string AppSecret { get;  }

        string LastToken { get; }

        int GetTenantId();

    }
     
}
