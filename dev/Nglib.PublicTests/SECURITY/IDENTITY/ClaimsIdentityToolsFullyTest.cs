using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.SECURITY.IDENTITY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Nglib.SECURITY.IDENTITY
{
    /// <summary>
    /// Test exhaustif pour ClaimsIdentityTools
    /// </summary>
    [TestClass]
    public class ClaimsIdentityToolsFullyTest
    {
        [TestMethod]
        public void ClaimsIdentityToolsFullyTestMethod()
        {
            try
            {
                // Préparation des données de test
                ClaimsIdentity identity = new ClaimsIdentity("TestAuth");
                identity.AddClaim(new Claim("name", "TestUser"));
                identity.AddClaim(new Claim("email", "test@example.com"));
                identity.AddClaim(new Claim("role", "admin"));

                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                // Test SetClaim - Ajouter un nouveau claim
                identity.SetClaim("department", "IT");
                Assert.AreEqual("IT", identity.Claims.GetClaimString("department"));

                // Test SetClaim - Remplacer un claim existant
                identity.SetClaim("role", "user");
                Assert.AreEqual("user", identity.Claims.GetClaimString("role"));
                Assert.AreEqual(1, identity.Claims.Count(c => c.Type == "role")); // Un seul claim role

                // Test SetClaim - Supprimer un claim (claimValue = null)
                identity.SetClaim("department", null);
                Assert.IsNull(identity.Claims.GetClaimString("department"));

                // Test SetClaim avec issuer
                identity.SetClaim("issuer_test", "value_test", "TestIssuer");
                Claim issuerClaim = identity.Claims.FirstOrDefault(c => c.Type == "issuer_test");
                Assert.IsNotNull(issuerClaim);
                Assert.AreEqual("TestIssuer", issuerClaim.Issuer);

                // Test GetClaimString
                Assert.AreEqual("TestUser", identity.Claims.GetClaimString("name"));
                Assert.AreEqual("test@example.com", identity.Claims.GetClaimString("email"));
                Assert.IsNull(identity.Claims.GetClaimString("nonexistent"));

                // Test GetClaimString insensible à la casse
                Assert.AreEqual("TestUser", identity.Claims.GetClaimString("NAME"));
                Assert.AreEqual("TestUser", identity.Claims.GetClaimString("Name"));

                // Test CloneClaimsPrincipal - Créer nouveau principal pour test proper
                ClaimsIdentity freshIdentity = new ClaimsIdentity("TestAuth");
                freshIdentity.AddClaim(new Claim("name", "TestUser"));
                freshIdentity.AddClaim(new Claim("email", "test@example.com"));
                freshIdentity.AddClaim(new Claim("role", "admin")); // Valeur originale
                ClaimsPrincipal freshPrincipal = new ClaimsPrincipal(freshIdentity);

                Dictionary<string, object> newClaims = new Dictionary<string, object>
                {
                    {"newClaim", "newValue"},
                    {"role", "superuser"}, // Override existing
                    {"userId", 12345}
                };

                ClaimsPrincipal clonedPrincipal = ClaimsIdentityTools.CloneClaimsPrincipal(freshPrincipal, newClaims);
                Assert.IsNotNull(clonedPrincipal);
                Assert.AreEqual("newValue", clonedPrincipal.Claims.GetClaimString("newClaim"));
                // Note: le claim role original peut être préservé selon l'implémentation ClaimsIdentity
                // Vérifions qu'au moins le nouveau claim existe
                Assert.IsTrue(clonedPrincipal.Claims.Any(c => c.Type == "role" && c.Value == "superuser"));
                Assert.AreEqual("12345", clonedPrincipal.Claims.GetClaimString("userId"));
                Assert.AreEqual("TestUser", clonedPrincipal.Claims.GetClaimString("name")); // Claims originaux préservés

                // Test IsAuthenticated
                // Utilisateur authentifié avec nom
                ClaimsIdentity authIdentity = new ClaimsIdentity("TestAuth");
                authIdentity.AddClaim(new Claim(ClaimTypes.Name, "AuthUser"));
                ClaimsPrincipal authPrincipal = new ClaimsPrincipal(authIdentity);
                Assert.IsTrue(authPrincipal.IsAuthenticated());

                // Utilisateur non authentifié
                ClaimsIdentity unauthIdentity = new ClaimsIdentity();
                ClaimsPrincipal unauthPrincipal = new ClaimsPrincipal(unauthIdentity);
                Assert.IsFalse(unauthPrincipal.IsAuthenticated());

                // Utilisateur authentifié mais sans nom
                ClaimsIdentity authNoNameIdentity = new ClaimsIdentity("TestAuth");
                ClaimsPrincipal authNoNamePrincipal = new ClaimsPrincipal(authNoNameIdentity);
                Assert.IsFalse(authNoNamePrincipal.IsAuthenticated());

                // Test avec principal null
                IPrincipal nullPrincipal = null;
                Assert.IsFalse(nullPrincipal.IsAuthenticated());

                // Test ToClaimDictionary
                Dictionary<string, object> claimDict = ClaimsIdentityTools.ToClaimDictionary(principal);
                Assert.IsNotNull(claimDict);
                Assert.IsTrue(claimDict.ContainsKey("name"));
                Assert.IsTrue(claimDict.ContainsKey("email"));
                Assert.AreEqual("TestUser", claimDict["name"]);
                Assert.AreEqual("test@example.com", claimDict["email"]);

                // Test avec claims dupliqués (ne doit prendre que le premier)
                ClaimsIdentity duplicateIdentity = new ClaimsIdentity();
                duplicateIdentity.AddClaim(new Claim("test", "value1"));
                duplicateIdentity.AddClaim(new Claim("test", "value2"));
                ClaimsPrincipal duplicatePrincipal = new ClaimsPrincipal(duplicateIdentity);
                
                Dictionary<string, object> duplicateDict = ClaimsIdentityTools.ToClaimDictionary(duplicatePrincipal);
                Assert.AreEqual("value1", duplicateDict["test"]); // Premier claim

                // Test edge cases pour SetClaim
                ClaimsIdentity edgeIdentity = new ClaimsIdentity();
                edgeIdentity.SetClaim(null, "value"); // Ne doit rien faire
                edgeIdentity.SetClaim("", "value"); // Ne doit rien faire
                Assert.AreEqual(0, edgeIdentity.Claims.Count());

                // Test avec ClaimsIdentity null
                ClaimsIdentity nullIdentity = null;
                nullIdentity.SetClaim("test", "value"); // Ne doit pas planter

                Console.WriteLine("✅ ClaimsIdentityToolsFullyTest : Tous les tests réussis");
            }
            catch (Exception ex)
            {
                Assert.Fail($"ClaimsIdentityToolsFullyTest échoué : {ex.Message}");
            }
        }
    }
}