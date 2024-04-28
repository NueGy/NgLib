using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.FORMAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    [TestClass]
    public class HttpClientTests
    {
        [TestMethod]
        public async Task CallApiTest()
        {
            HttpClient client = null;
            //TokenConfigModel tokenConfig = new TokenConfigModel( TokenAuthTypeEnum.Basic);
            //tokenConfig.Username = "alex";
            //tokenConfig.Password = "1234";
            HttpClientTokenHandler tokenHandler = new HttpClientTokenHandler();
            tokenHandler.SetToken("azertyn");
            client = new HttpClient(tokenHandler);


            HttpResponseMessage response = await client.GetAsync("https://api.github.com/users/alexandrebl");
            Assert.IsTrue(response.IsSuccessStatusCode);

        }
    }
}