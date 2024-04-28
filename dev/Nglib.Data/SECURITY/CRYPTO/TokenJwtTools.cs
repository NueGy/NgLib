using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Nglib.FORMAT;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    ///     Outils pour gérer les token
    /// </summary>
    public static class TokenJwtTools
    {
        //https://pastebin.com/DvQz8vdb


        /// <summary>
        ///     Création d'un Token JWT avec les claims (iss,aud,sub,iat,exp,jti)
        /// </summary>
        /// <param name="keyHs256">Clef de signature</param>
        /// <param name="issuer">Emetteur du token</param>
        /// <param name="audience">Destinataire du token</param>
        /// <param name="subject">Sujet(user,...) concerné par ce token</param>
        /// <param name="addJti">Rendre le token unique</param>
        /// <param name="ExpireSecond">Expiration du token</param>
        /// <returns></returns>
        public static string EncodeBasicJWT(string keyHs256, string issuer, string audience = null,
            string subject = null, bool addJti = true, int ExpireSecond = 3600,
            Dictionary<string, object> payload = null)
        {
            if (payload == null) payload = new Dictionary<string, object>();
            CompletePayloadForEncode(payload, issuer, audience, subject, addJti, ExpireSecond);
            return Encode(payload, keyHs256);
        }


        public static Dictionary<string, object> CompletePayloadForEncode(Dictionary<string, object> payload,
            string issuer, string audience = null, string subject = null, bool addJti = true, int ExpireSecond = 3600)
        {
            if (payload == null) payload = new Dictionary<string, object>();
            if (!payload.ContainsKey("iss") && issuer != null)
                payload.Add("iss", issuer); // issuer représente le clientid
            if (!payload.ContainsKey("aud") && !string.IsNullOrWhiteSpace(audience))
                payload.Add("aud", audience); // audience
            if (!payload.ContainsKey("sub") && !string.IsNullOrWhiteSpace(subject))
                payload.Add("sub", subject); // subject 
            if (!payload.ContainsKey("iat"))
                payload.Add("iat", (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds); // now
            if (!payload.ContainsKey("exp"))
                payload.Add("exp",
                    (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + ExpireSecond); // not after
            if (!payload.ContainsKey("jti") && addJti)
                payload.Add("jti", "K" + StringTools.GenerateGuid32()); // token unique
            return payload;
        }


        /// <summary>
        ///     Permet de créer une signature Hmac256 comme les token JWT
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetHmacSha256(string stringToSign, string key)
        {
            if (string.IsNullOrWhiteSpace(stringToSign)) return null;
            try
            {
                stringToSign = stringToSign.Trim();
                var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var sha = new HMACSHA256(keyBytes);
                var signature = sha.ComputeHash(bytesToSign);
                var signval = Base64UrlEncode(signature);
                return signval;
            }
            catch (Exception ex)
            {
                throw new Exception("CreateSignature " + ex.Message);
            }
        }


        /// <summary>
        ///     Création d'un token HS256 HMAC AES
        /// </summary>
        /// <param name="payload">Données à signer</param>
        /// <param name="keyHs256">Clef UTF8</param>
        /// <returns></returns>
        public static string Encode(Dictionary<string, object> payload, string keyHs256)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyHs256)) throw new Exception("Invalid Key for sign");
                var segments = new List<string>();
                const string headerJson = "{\"alg\":\"HS256\",\"typ\": \"JWT\"}";
                var payloadJson = JsonSerializer.Serialize(payload);
                //string payloadJson = FILES.SERIAL.JsonTools.SerializeDictionaryValues(payload);
                segments.Add(Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson)));
                segments.Add(Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson)));
                var stringToSign = string.Join(".", segments.ToArray());
                var signpart = GetHmacSha256(stringToSign, keyHs256);
                segments.Add(signpart);
                return string.Join(".", segments.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("EncodeJWT " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Valider et obtenir les données d'un token JWT  HS256 HMAC AES
        /// </summary>
        /// <param name="token">Token signé</param>
        /// <param name="keyHs256">Clef</param>
        /// <returns></returns>
        public static Dictionary<string, object> Decode(string token, string keyHs256)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyHs256)) throw new Exception("Invalid Key for Decode");

                var segments = token.Split('.');
                if (segments.Length != 3) throw new Exception("invalid token");
                if (string.IsNullOrWhiteSpace(segments[2])) throw new Exception("invalid token signature");
                var stringToSign = string.Join(".", segments.Take(2).ToArray());
                var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
                var signature = GetHmacSha256(stringToSign, keyHs256);
                if (!signature.Equals(segments[2])) throw new Exception("invalid token signature");

                var payload = Base64UrlDecode(segments[1]);
                //Dictionary<string, object> retour = FILES.SERIAL.JsonTools.DeSerializeDictionaryValues(payload);
                var retour = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);

                // !!! ajouter sécurité sur exp

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("DecodeJWT " + ex.Message, ex);
            }
        }


        // from JWT spec
        public static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        // from JWT spec
        public static string Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2:
                    output += "==";
                    break; // Two pad chars
                case 3:
                    output += "=";
                    break; // One pad char
                default: throw new Exception("Illegal base64url string");
            }

            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return Encoding.UTF8.GetString(converted);
        }
    }
}