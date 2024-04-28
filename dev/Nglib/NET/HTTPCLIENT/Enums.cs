namespace Nglib.NET.HTTPCLIENT
{
    public enum HttpParameterTypeEnum
    {
        /// <summary>
        ///     A path parameter which is inserted into the path portion of the request URI.
        /// </summary>
        Path = 0,

        /// <summary>
        ///     A query parameter which is inserted into the query portion of the request URI.
        /// </summary>
        Query = 1,

        /// <summary>
        ///     Add to Header
        /// </summary>
        Header = 2,

        /// <summary>
        /// Post FormData
        /// </summary>
        FormData = 3,


    }


    /// <summary>
    /// Méthode authentification pour api token
    /// </summary>
    public enum TokenAuthTypeEnum
    {
        /// <summary>
        /// Aucune authentification, utilisera le lastToken seulement si il est défini
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
        /// Signe un JWT HS256 avec une clé secrète 
        /// </summary>
        JwtHmac,

        /// <summary>
        /// Token pré-défini dans la configuration
        /// </summary>
        FixedBearerToken,


    }



}