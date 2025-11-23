using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.NET.HTTPCLIENT;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    /// <summary>
    /// Unit tests for Endpoint attribute system
    /// Uses JSONPlaceholder public API for testing (https://jsonplaceholder.typicode.com)
    /// </summary>
    [TestClass]
    public class EndpointTests
    {
        private static HttpClient _httpClient;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _httpClient = new HttpClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _httpClient?.Dispose();
        }

        /// <summary>
        /// Request model for GET /posts/{id}
        /// Tests: Path parameter, Query parameter, Header, Required validation
        /// </summary>
        [Endpoint("GET", "/posts/{postId}")]
        public class GetPostRequest
        {
            [Required]
            [EndpointParameter(HttpParameterTypeEnum.Path)]
            public int? PostId { get; set; }

            [EndpointParameter("userId", HttpParameterTypeEnum.Query)]
            public int? UserId { get; set; }

            [EndpointParameter("X-Custom-Header", HttpParameterTypeEnum.Header)]
            public string CustomHeader { get; set; }

            [EndpointParameter("Content-Type", HttpParameterTypeEnum.Header)]
            public string ResponseContentType { get; set; }
        }

        /// <summary>
        /// Request model for POST /posts
        /// Tests: Body parameter
        /// </summary>
        [Endpoint("POST", "/posts")]
        public class CreatePostRequest
        {
            [EndpointParameter("X-API-Key", HttpParameterTypeEnum.Header)]
            public string ApiKey { get; set; }

            [EndpointParameter(HttpParameterTypeEnum.Body)]
            public PostData Post { get; set; }
        }

        public class PostData
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public int UserId { get; set; }
        }

        public class PostResponse
        {
            public int UserId { get; set; }
            public int Id { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
        }

        [Endpoint("GET", "/posts/{wrongName}")]
        private class InvalidPathRequest
        {
            [EndpointParameter("postId", HttpParameterTypeEnum.Path)]
            public int? PostId { get; set; }
        }

        [Endpoint("GET", "/users/{userId}")]
        private class AutoNameRequest
        {
            [EndpointParameter(HttpParameterTypeEnum.Path)]
            public int? UserId { get; set; }

            [EndpointParameter(HttpParameterTypeEnum.Query)]
            public int? Page { get; set; }
        }

        /// <summary>
        /// Complete test covering all Endpoint features:
        /// - GET with Path, Query, Header parameters
        /// - POST with Body (JSON) parameter
        /// - [Required] validation
        /// - Auto-name detection (P8)
        /// - Path parameter validation (P1)
        /// - Response header parsing
        /// </summary>
        [TestMethod]
        public async Task Test_EndpointTools_Complete()
        {
            Console.WriteLine("=== Starting Endpoint Complete Test ===\n");

            // TEST 1: GET Request with all parameter types
            Console.WriteLine("TEST 1: GET Request (Path + Query + Header)");
            var getRequest = new GetPostRequest
            {
                PostId = 1,
                UserId = 1,
                CustomHeader = "test-value"
            };

            var httpGetRequest = EndpointTools.CreateRequestFromModel(getRequest, "https://jsonplaceholder.typicode.com");
            
            Assert.IsNotNull(httpGetRequest, "HttpRequestMessage should be created");
            Assert.AreEqual(HttpMethod.Get, httpGetRequest.Method, "HTTP method should be GET");
            Assert.IsTrue(httpGetRequest.RequestUri.ToString().Contains("/posts/1"), "Path parameter should be replaced");
            Assert.IsTrue(httpGetRequest.RequestUri.ToString().Contains("userId=1"), "Query parameter should be added");
            Assert.IsTrue(httpGetRequest.Headers.Contains("X-Custom-Header"), "Custom header should be present");

            var response = await _httpClient.SendAsync(httpGetRequest);
            Assert.IsTrue(response.IsSuccessStatusCode, $"GET request should succeed. Status: {response.StatusCode}");

            EndpointTools.ParseResponseParameters(getRequest, response);
            Assert.IsNotNull(getRequest.ResponseContentType, "Response Content-Type should be captured");
            Assert.IsTrue(getRequest.ResponseContentType.Contains("application/json"), "Content-Type should be JSON");

            var json = await response.Content.ReadAsStringAsync();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var post = System.Text.Json.JsonSerializer.Deserialize<PostResponse>(json, options);
            
            Assert.IsNotNull(post, "Post should be deserialized");
            Assert.AreEqual(1, post.Id, "Post ID should be 1");
            Console.WriteLine($"✅ GET Test - Post Title: {post.Title}\n");

            // TEST 2: POST Request with Body (JSON)
            Console.WriteLine("TEST 2: POST Request (Body JSON)");
            var postRequest = new CreatePostRequest
            {
                ApiKey = "test-key-12345",
                Post = new PostData
                {
                    Title = "Nglib Test Post",
                    Body = "Unit test from EndpointTools",
                    UserId = 1
                }
            };

            var httpPostRequest = EndpointTools.CreateRequestFromModel(postRequest, "https://jsonplaceholder.typicode.com");
            
            Assert.IsNotNull(httpPostRequest.Content, "POST request should have content");
            Assert.AreEqual("application/json", httpPostRequest.Content.Headers.ContentType.MediaType, "Content-Type should be JSON");
            
            var postResponse = await _httpClient.SendAsync(httpPostRequest);
            Assert.IsTrue(postResponse.IsSuccessStatusCode, $"POST request should succeed. Status: {postResponse.StatusCode}");

            var responseJson = await postResponse.Content.ReadAsStringAsync();
            var createdPost = System.Text.Json.JsonSerializer.Deserialize<PostResponse>(responseJson, options);
            
            Assert.IsNotNull(createdPost, "Created post should be returned");
            Assert.AreEqual(101, createdPost.Id, "JSONPlaceholder returns ID 101 for new posts");
            Console.WriteLine($"✅ POST Test - Created Post ID: {createdPost.Id}\n");

            // TEST 3: [Required] Validation
            Console.WriteLine("TEST 3: [Required] Validation");
            var invalidRequest = new GetPostRequest { UserId = 1 }; // PostId is null

            try
            {
                EndpointTools.CreateRequestFromModel(invalidRequest, "https://jsonplaceholder.typicode.com");
                Assert.Fail("Should have thrown exception for missing required parameter");
            }
            catch (APP.DIAG.CascadeException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException), "Inner exception should be InvalidOperationException");
                Assert.IsTrue(ex.InnerException.Message.Contains("Required parameter"), "Error should mention required parameter");
                Console.WriteLine($"✅ Required Validation - Exception caught: {ex.InnerException.Message}\n");
            }

            // TEST 4: Path Parameter Not Found (P1 Validation)
            Console.WriteLine("TEST 4: Path Parameter Validation");
            var pathTestRequest = new InvalidPathRequest { PostId = 1 };
            try
            {
                EndpointTools.CreateRequestFromModel(pathTestRequest, "https://jsonplaceholder.typicode.com");
                Assert.Fail("Should have thrown exception for missing path placeholder");
            }
            catch (APP.DIAG.CascadeException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException), "Inner exception should be InvalidOperationException");
                Assert.IsTrue(ex.InnerException.Message.Contains("Path parameter 'postId' not found"), "Error should mention missing path parameter");
                Console.WriteLine($"✅ Path Validation - Exception caught: {ex.InnerException.Message}\n");
            }

            // TEST 5: Auto-Name Detection (P8)
            Console.WriteLine("TEST 5: Auto-Name Detection");
            var autoNameRequest = new AutoNameRequest { UserId = 42, Page = 2 };
            var autoNameHttpRequest = EndpointTools.CreateRequestFromModel(autoNameRequest, "https://jsonplaceholder.typicode.com");
            
            var uri = autoNameHttpRequest.RequestUri.ToString();
            Assert.IsTrue(uri.Contains("/users/42"), "UserId property name should be used for path");
            Assert.IsTrue(uri.Contains("Page=2"), "Page property name should be used for query");
            Console.WriteLine($"✅ Auto-Name Detection - URI: {uri}\n");

            Console.WriteLine("=== All Endpoint Tests Passed ✅ ===");
        }
    }
}
