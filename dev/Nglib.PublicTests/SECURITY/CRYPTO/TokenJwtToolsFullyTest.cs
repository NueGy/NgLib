using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.SECURITY.CRYPTO;
using System;
using System.Collections.Generic;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Test exhaustif pour TokenJwtTools
    /// </summary>
    [TestClass]
    public class TokenJwtToolsFullyTest
    {
        [TestMethod]
        public void TokenJwtToolsFullyTestMethod()
        {
            try
            {
                string secretKey = "ClefSecrete123456789!@#$%^&*()";
                string issuer = "TestIssuer";
                string audience = "TestAudience";
                string subject = "TestUser";

                // Test EncodeBasicJWT
                string token = TokenJwtTools.EncodeBasicJWT(secretKey, issuer, audience, subject, true, 3600);
                Assert.IsNotNull(token);
                Assert.IsTrue(token.Contains("."));
                string[] parts = token.Split('.');
                Assert.AreEqual(3, parts.Length); // Header.Payload.Signature

                // Test Decode du token créé
                Dictionary<string, object> decodedPayload = TokenJwtTools.Decode(token, secretKey);
                Assert.IsNotNull(decodedPayload);
                Assert.AreEqual(issuer, decodedPayload["iss"].ToString());
                Assert.AreEqual(audience, decodedPayload["aud"].ToString());
                Assert.AreEqual(subject, decodedPayload["sub"].ToString());
                Assert.IsTrue(decodedPayload.ContainsKey("iat"));
                Assert.IsTrue(decodedPayload.ContainsKey("exp"));
                Assert.IsTrue(decodedPayload.ContainsKey("jti"));

                // Test avec payload custom
                Dictionary<string, object> customPayload = new Dictionary<string, object>
                {
                    {"custom_claim", "custom_value"},
                    {"role", "admin"},
                    {"userId", 12345}
                };
                
                string customToken = TokenJwtTools.Encode(customPayload, secretKey);
                Assert.IsNotNull(customToken);
                
                Dictionary<string, object> decodedCustom = TokenJwtTools.Decode(customToken, secretKey);
                Assert.AreEqual("custom_value", decodedCustom["custom_claim"].ToString());
                Assert.AreEqual("admin", decodedCustom["role"].ToString());

                // Test CompletePayloadForEncode
                Dictionary<string, object> payload = new Dictionary<string, object>();
                TokenJwtTools.CompletePayloadForEncode(payload, issuer, audience, subject, true, 1800);
                Assert.IsTrue(payload.ContainsKey("iss"));
                Assert.IsTrue(payload.ContainsKey("aud"));
                Assert.IsTrue(payload.ContainsKey("sub"));
                Assert.IsTrue(payload.ContainsKey("iat"));
                Assert.IsTrue(payload.ContainsKey("exp"));
                Assert.IsTrue(payload.ContainsKey("jti"));

                // Test GetHmacSha256
                string testString = "TestStringToSign";
                string signature = TokenJwtTools.GetHmacSha256(testString, secretKey);
                Assert.IsNotNull(signature);
                Assert.IsTrue(signature.Length > 0);
                
                // Même string doit donner même signature
                string signature2 = TokenJwtTools.GetHmacSha256(testString, secretKey);
                Assert.AreEqual(signature, signature2);

                // Test Base64UrlEncode/Decode
                byte[] testBytes = System.Text.Encoding.UTF8.GetBytes("Test Base64Url");
                string encoded = TokenJwtTools.Base64UrlEncode(testBytes);
                Assert.IsNotNull(encoded);
                Assert.IsFalse(encoded.Contains("=")); // Pas de padding
                Assert.IsFalse(encoded.Contains("+")); // Remplacé par -
                Assert.IsFalse(encoded.Contains("/")); // Remplacé par _

                string decoded = TokenJwtTools.Base64UrlDecode(encoded);
                Assert.AreEqual("Test Base64Url", decoded);

                // Test échecs de validation
                // Token avec mauvaise signature
                Assert.ThrowsException<Exception>(() => TokenJwtTools.Decode(token, "MauvaiseClef"));
                
                // Token malformé
                Assert.ThrowsException<Exception>(() => TokenJwtTools.Decode("token.malformé", secretKey));
                Assert.ThrowsException<Exception>(() => TokenJwtTools.Decode("token", secretKey));

                // Test avec clé vide
                Assert.ThrowsException<Exception>(() => TokenJwtTools.Encode(payload, ""));
                Assert.ThrowsException<Exception>(() => TokenJwtTools.Decode(token, ""));

                // Test GetHmacSha256 avec valeurs nulles/vides
                Assert.IsNull(TokenJwtTools.GetHmacSha256(null, secretKey));
                Assert.IsNull(TokenJwtTools.GetHmacSha256("", secretKey));

                // Tests spécifiques pour la validation temporelle exp/nbf
                TestExpiredToken(secretKey);
                TestNotBeforeToken(secretKey);
                TestValidTimeToken(secretKey);

                Console.WriteLine("✅ TokenJwtToolsFullyTest : Tous les tests réussis");
            }
            catch (Exception ex)
            {
                Assert.Fail($"TokenJwtToolsFullyTest échoué : {ex.Message}");
            }
        }

        /// <summary>
        /// Test avec un token expiré (exp dans le passé)
        /// </summary>
        private void TestExpiredToken(string secretKey)
        {
            // Créer un payload avec exp dans le passé
            var expiredPayload = new Dictionary<string, object>
            {
                ["iss"] = "TestIssuer",
                ["aud"] = "TestAudience", 
                ["sub"] = "TestUser",
                ["iat"] = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(),
                ["exp"] = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds(), // Expiré il y a 1h
                ["jti"] = Guid.NewGuid().ToString()
            };

            string expiredToken = TokenJwtTools.Encode(expiredPayload, secretKey);
            
            // Le décodage doit lever une SecurityException
            var ex = Assert.ThrowsException<System.Security.SecurityException>(() => 
                TokenJwtTools.Decode(expiredToken, secretKey));
            Assert.IsTrue(ex.Message.Contains("Token expired"));
        }

        /// <summary>
        /// Test avec un token pas encore valide (nbf dans le futur)
        /// </summary>
        private void TestNotBeforeToken(string secretKey)
        {
            // Créer un payload avec nbf dans le futur
            var notYetValidPayload = new Dictionary<string, object>
            {
                ["iss"] = "TestIssuer",
                ["aud"] = "TestAudience",
                ["sub"] = "TestUser", 
                ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nbf"] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(), // Valide dans 1h
                ["exp"] = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds(),
                ["jti"] = Guid.NewGuid().ToString()
            };

            string notYetValidToken = TokenJwtTools.Encode(notYetValidPayload, secretKey);
            
            // Le décodage doit lever une SecurityException
            var ex = Assert.ThrowsException<System.Security.SecurityException>(() => 
                TokenJwtTools.Decode(notYetValidToken, secretKey));
            Assert.IsTrue(ex.Message.Contains("Token not yet valid"));
        }

        /// <summary>
        /// Test avec un token valide temporellement
        /// </summary>
        private void TestValidTimeToken(string secretKey)
        {
            // Créer un payload valide avec nbf et exp
            var validPayload = new Dictionary<string, object>
            {
                ["iss"] = "TestIssuer",
                ["aud"] = "TestAudience",
                ["sub"] = "TestUser",
                ["iat"] = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds(),
                ["nbf"] = DateTimeOffset.UtcNow.AddMinutes(-2).ToUnixTimeSeconds(), // Valide depuis 2min
                ["exp"] = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(), // Expire dans 30min
                ["jti"] = Guid.NewGuid().ToString()
            };

            string validToken = TokenJwtTools.Encode(validPayload, secretKey);
            
            // Le décodage doit réussir
            var decoded = TokenJwtTools.Decode(validToken, secretKey);
            Assert.IsNotNull(decoded);
            Assert.AreEqual("TestIssuer", decoded["iss"].ToString());
            Assert.AreEqual("TestUser", decoded["sub"].ToString());
        }
    }
}