﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT.ENDPOINTS
{
    public interface IResponseModel
    {
        HttpResponseMessage HttpResponse { get; set; }
    }
}
