namespace Nglib.NET.HTTPCLIENT
{
    public enum HttpParameterTypeEnum
    {
        /// <summary>
        /// A path parameter which is inserted into the path portion of the request URI.
        /// </summary>
        Path = 0,

        /// <summary>
        /// A query parameter which is inserted into the query portion of the request URI.
        /// </summary>
        Query = 1,

        /// <summary>
        /// Add to Header
        /// </summary>
        Header = 2,

        /// <summary>
        /// Post FormData
        /// </summary>
        FormData = 3,


        /// <summary>
        /// JSON body
        /// </summary>
        Body = 4,

        /// <summary>
        /// Raw text body
        /// </summary>
        BodyRaw = 5


    }


    /// <summary>
    /// Authentication method for API token
    /// </summary>
    public enum TokenAuthTypeEnum
    {
        /// <summary>
        /// No authentication, will use lastToken only if defined
        /// </summary>
        none,

        /// <summary>
        /// Basic authentification, use username/password
        /// </summary>
        Basic,

        /// <summary>
        /// Standard Client_credentials OAuth2 flow
        /// </summary>
        OAuth2Client,

        /// <summary>
        /// https://www.oauth.com/oauth2-servers/access-tokens/password-grant/
        /// </summary>
        OAuth2Password,

        /// <summary>
        /// Signs a JWT HS256 with a secret key
        /// </summary>
        JwtHmac,

        /// <summary>
        /// Pre-defined token in configuration
        /// </summary>
        FixedBearerToken,


    }



}