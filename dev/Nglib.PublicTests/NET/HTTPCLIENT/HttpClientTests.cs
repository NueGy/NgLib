using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.FORMAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;

namespace Nglib.NET.HTTPCLIENT
{
    [TestClass]
    public class HttpClientTests
    {
        // URL API publique stable pour les tests
        private const string TEST_API_URL = "https://httpbin.org/get";

        /// <summary>
        /// Test exhaustif de tous les composants HttpClient de Nglib
        /// </summary>
        [TestMethod]
        public async Task HttpClientToolsFullyTest()
        {
            try
            {
                // Test 1: HttpTools - Combinaison URLs
                TestHttpTools();

                // Test 2: HttpClientConfigModel
                TestHttpClientConfigModel();

                // Test 3: HttpClientTools - Création de clients
                TestHttpClientCreation();

                // Test 4: HttpRequestMessage et Headers
                TestHttpRequestPreparation();
 
                // Test 6: Sérialisation et contenu
                await TestContentSerialization();

                // Test 7: Validation des réponses
                TestResponseValidation();

                // Test 8: Lecture des réponses
                await TestResponseReading();

                // Test 9: TokenHandler basique
                await TestTokenHandler();

                Console.WriteLine("✅ Tous les tests HttpClient ont réussi");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Erreur dans HttpClientToolsFullyTest: {ex.Message}");
            }
        }

        private void TestHttpTools()
        {
            // Test CombineRootUrl
            var result1 = HttpTools.CombineRootUrl("https://api.example.com", "/users");
            Assert.AreEqual("https://api.example.com/users", result1);

            var result2 = HttpTools.CombineRootUrl("https://api.example.com/", "users");
            Assert.AreEqual("https://api.example.com/users", result2);

            var result3 = HttpTools.CombineRootUrl("https://api.example.com/", "/users");
            Assert.AreEqual("https://api.example.com/users", result3);

            var result4 = HttpTools.CombineRootUrl("", "/users");
            Assert.AreEqual("/users", result4);

            var result5 = HttpTools.CombineRootUrl("https://api.example.com", "https://other.com/users");
            Assert.AreEqual("https://other.com/users", result5);

            // Test GetQueryString
            var parameters = new Dictionary<string, string>
            {
                {"name", "test"},
                {"value", "123"},
                {"special", "a&b=c"}
            };
            var queryString = HttpTools.GetQueryString(parameters);
            Assert.IsTrue(queryString.Contains("name=test"));
            Assert.IsTrue(queryString.Contains("value=123"));
            Assert.IsTrue(queryString.Contains("special=a%26b%3Dc"));

            // Test AppendQueryToUrl
            var urlWithQuery = HttpTools.AppendQueryToUrl("https://api.com/users", parameters);
            Assert.IsTrue(urlWithQuery.StartsWith("https://api.com/users?"));

            var urlWithExistingQuery = HttpTools.AppendQueryToUrl("https://api.com/users?existing=true", parameters);
            Assert.IsTrue(urlWithExistingQuery.Contains("existing=true&"));

            Assert.AreEqual(HttpMethod.Post, HttpTools.ConvertToHttpMethod("Post"));
            Assert.AreEqual(HttpMethod.Post, HttpTools.ConvertToHttpMethod("POST"));
        }

        private void TestHttpClientConfigModel()
        {
            // Test constructeur par défaut
            var config1 = new HttpClientConfigModel();
            Assert.AreEqual(TokenAuthTypeEnum.none, config1.AuthType);

            // Test constructeur avec baseUrl
            var config2 = new HttpClientConfigModel("https://api.example.com");
            Assert.AreEqual("https://api.example.com", config2.BaseUrl);
            Assert.AreEqual(TokenAuthTypeEnum.none, config2.AuthType);

            // Test constructeur avec authType
            var config3 = new HttpClientConfigModel(TokenAuthTypeEnum.Basic);
            Assert.AreEqual(TokenAuthTypeEnum.Basic, config3.AuthType);

            // Test PrepareWithFixedToken
            var config4 = HttpClientConfigModel.PrepareWithFixedToken("test-token", "https://api.example.com");
            Assert.AreEqual(TokenAuthTypeEnum.FixedBearerToken, config4.AuthType);
            Assert.AreEqual("test-token", config4.FixedToken);
            Assert.AreEqual("https://api.example.com", config4.BaseUrl);
        }

        private void TestHttpClientCreation()
        {
            // Test création client basique
            var client1 = HttpClientTools.CreateNewClient();
            Assert.IsNotNull(client1);

            // Test création avec rootUrl
            var client2 = HttpClientTools.CreateNewClient(null, "https://api.example.com/");
            Assert.IsNotNull(client2);
            Assert.AreEqual("https://api.example.com/", client2.BaseAddress.ToString());

            // Test création avec config
            var config = HttpClientConfigModel.PrepareWithFixedToken("test-token");
            var client3 = HttpClientTools.CreateNewClient(config);
            Assert.IsNotNull(client3);
        }

        private void TestHttpRequestPreparation()
        {
            // Test PrepareRequest
            var request1 = HttpClientTools.PrepareRequest(HttpMethod.Get, "https://api.example.com/users");
            Assert.AreEqual(HttpMethod.Get, request1.Method);
            Assert.AreEqual("https://api.example.com/users", request1.RequestUri.ToString());

            // Test SetBearerToken
            var request2 = HttpClientTools.PrepareRequest(HttpMethod.Get, "https://api.example.com/test");
            request2.SetBearerToken("test-token");
            Assert.AreEqual("bearer", request2.Headers.Authorization.Scheme);
            Assert.AreEqual("test-token", request2.Headers.Authorization.Parameter);

            // Test SetBearerToken avec token vide (ne doit rien faire)
            var request3 = HttpClientTools.PrepareRequest(HttpMethod.Get, "https://api.example.com/test");
            request3.SetBearerToken("");
            Assert.IsNull(request3.Headers.Authorization);

            // Test SetBasicAuth
            var request4 = HttpClientTools.PrepareRequest(HttpMethod.Get, "https://api.example.com/test");
            request4.SetBasicAuth("user", "pass");
            Assert.AreEqual("Basic", request4.Headers.Authorization.Scheme);
            Assert.IsNotNull(request4.Headers.Authorization.Parameter);

            // Test SetParameterHeader
            var request5 = HttpClientTools.PrepareRequest(HttpMethod.Get, "https://api.example.com/test");
            var headerSet = HttpClientTools.SetParameterHeader(request5, "Custom-Header", "test-value");
            Assert.IsTrue(headerSet);
            Assert.IsTrue(request5.Headers.Contains("custom-header"));

            // Test header déjà existant
            var headerSet2 = HttpClientTools.SetParameterHeader(request5, "Custom-Header", "other-value");
            Assert.IsFalse(headerSet2);
        }
         
        private async Task TestContentSerialization()
        {
            // Test objet simple
            var testObject = new { Name = "Test", Value = 123, IsActive = true };
            
            // Test SetContent avec POST (JSON)
            var request1 = HttpClientTools.PrepareRequest(HttpMethod.Post, "https://api.example.com/test");
            var content1 = request1.SetContent(testObject);
            Assert.IsNotNull(content1);
            Assert.AreEqual("application/json", content1.Headers.ContentType.MediaType);

            var jsonContent = await content1.ReadAsStringAsync();
            Assert.IsTrue(jsonContent.Contains("Test"));
            Assert.IsTrue(jsonContent.Contains("123"));

            // Test SetContent avec GET (QueryString dans URL)
            var request2 = HttpClientTools.PrepareRequest(HttpMethod.Get, "https://api.example.com/test");
            request2.SetContent(testObject);
            Assert.IsTrue(request2.RequestUri.Query.Contains("Name=Test"));
            Assert.IsTrue(request2.RequestUri.Query.Contains("Value=123"));

            // Test PrepareJsonContent
            var jsonContent2 = HttpClientTools.PrepareJsonContent(testObject);
            Assert.IsNotNull(jsonContent2);
            Assert.AreEqual("application/json", jsonContent2.Headers.ContentType.MediaType);

            // Test PrepareFormUrlContent
            var formData = new Dictionary<string, object> { {"key1", "value1"}, {"key2", 42} };
            var formContent = HttpClientTools.PrepareFormUrlContent(formData);
            Assert.IsNotNull(formContent);
            Assert.AreEqual("application/x-www-form-urlencoded", formContent.Headers.ContentType.MediaType);

            // Test SetContent avec ForcePostFormUrlContent
            var request3 = HttpClientTools.PrepareRequest(HttpMethod.Post, "https://api.example.com/test");
            var content3 = request3.SetContent(testObject, true);
            Assert.AreEqual("application/x-www-form-urlencoded", content3.Headers.ContentType.MediaType);

            // Test avec string
            var request4 = HttpClientTools.PrepareRequest(HttpMethod.Post, "https://api.example.com/test");
            var content4 = request4.SetContent("{\"test\": \"value\"}");
            Assert.IsNotNull(content4);
            Assert.AreEqual("application/json", content4.Headers.ContentType.MediaType);

            // Test avec null
            var request5 = HttpClientTools.PrepareRequest(HttpMethod.Post, "https://api.example.com/test");
            var content5 = request5.SetContent(null);
            Assert.IsNull(content5);
        }

        private void TestResponseValidation()
        {
            // Test validation réponse null
            try
            {
                HttpClientTools.Validate(null);
                Assert.Fail("Exception attendue pour réponse null");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("HTTPResponseMessage null"));
            }

            // Pour tester les réponses d'erreur, on créerait des HttpResponseMessage fictifs
            // mais c'est complexe à mocker sans framework. On teste principalement la logique.
        }

        private Task TestResponseReading()
        {
            // Test ReadResponseTextSafe avec null
            var result = HttpClientTools.ReadResponseTextSafe(null);
            Assert.IsNull(result);

            // Test GetResponseHeader avec null
            var header = HttpClientTools.GetResponseHeader(null, "test");
            Assert.IsNull(header);
            
            return Task.CompletedTask;
        }

        private Task TestTokenHandler()
        {
            // Test création handler basique
            var handler1 = new HttpClientTokenHandler();
            Assert.IsNotNull(handler1);
            Assert.IsNull(handler1.Config);

            // Test avec config - utilise le constructeur non obsolète puis set Config
            var config = new HttpClientConfigModel(TokenAuthTypeEnum.FixedBearerToken)
            {
                FixedToken = "test-token"
            };
            var handler2 = new HttpClientTokenHandler();
            handler2.Config = config;
            Assert.IsNotNull(handler2.Config);
            Assert.AreEqual("test-token", handler2.Config.FixedToken);

            // Test propriétés token
            handler2.LastToken = "previous-token";
            handler2.LastTokenDate = DateTime.Now.AddMinutes(-5);
            handler2.LastTokenExpireSeconds = 300;

            Assert.AreEqual("previous-token", handler2.LastToken);
            Assert.IsTrue(handler2.LastTokenDate.HasValue);
            
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task CallApiTest()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                // Test avec une API publique stable (httpbin.org)
                HttpResponseMessage response = await client.GetAsync(TEST_API_URL);
                
                Assert.IsTrue(response.IsSuccessStatusCode, 
                    $"La requête a échoué avec le code {response.StatusCode}");
                
                // Vérifier que le contenu est bien reçu
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsFalse(string.IsNullOrEmpty(content), "Le contenu de la réponse est vide");
                
                Console.WriteLine($"✅ Test API réussi : {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                Assert.Inconclusive($"Test ignoré car l'API n'est pas accessible : {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                Assert.Inconclusive("Test ignoré car l'API a timeout");
            }
        }
    }
}