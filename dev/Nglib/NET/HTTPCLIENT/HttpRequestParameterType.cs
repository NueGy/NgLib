using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.NET.HTTPCLIENT
{

    public enum HttpRequestParameterType
    {

        /// <summary>
        ///  A path parameter which is inserted into the path portion of the request URI.
        /// </summary>
        Path = 0,
        /// <summary>
        /// A query parameter which is inserted into the query portion of the request URI.
        /// </summary>
        Query = 1,
        //
        // Résumé :
        //     A group of user-defined parameters that will be added in to the query portion
        //     of the request URI. If this type is being used, the name of the RequestParameterAttirbute
        //     is meaningless.
        UserDefinedQueries = 2
    }
}
