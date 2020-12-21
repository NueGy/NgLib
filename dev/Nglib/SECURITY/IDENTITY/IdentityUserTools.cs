using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.SECURITY.IDENTITY
{
    public static class IdentityUserTools
    {

        /// <summary>
        /// Permet de définir un claim et le remplacer si exsite déja
        /// </summary>
        public static void SetClaim(this System.Security.Claims.ClaimsIdentity claimsIdentity, string claimType, string claimValue, string issuer=null)
        {
            if (claimsIdentity == null || string.IsNullOrWhiteSpace(claimType)) return;
            System.Security.Claims.Claim findClaim = claimsIdentity.Claims.Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (findClaim != null) claimsIdentity.RemoveClaim(findClaim);
            if(claimValue == null) return; // mode delete

            System.Security.Claims.Claim newClaim = null;
            if (findClaim != null) newClaim = new System.Security.Claims.Claim(findClaim.Type, claimValue, findClaim.ValueType, issuer);
            else newClaim = new System.Security.Claims.Claim(claimType, claimValue,System.Security.Claims.ClaimValueTypes.String, issuer);
            claimsIdentity.AddClaim(newClaim);
        }


        public static string GetClaimString(this IEnumerable<System.Security.Claims.Claim> Claims, string claimType)
        {
            return Claims.Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).FirstOrDefault();
        }


        public static System.Security.Claims.ClaimsPrincipal CloneClaimsPrincipal(System.Security.Claims.ClaimsPrincipal origin, Dictionary<string,object> newClaims)
        {
            try
            {
                System.Security.Claims.ClaimsIdentity claimsIdentity = new System.Security.Claims.ClaimsIdentity(origin.Identity, origin.Claims);
                newClaims.Keys.ToList().ForEach(ck=> claimsIdentity.SetClaim(ck, Convert.ToString(newClaims[ck])));
                System.Security.Claims.ClaimsPrincipal claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                throw new Exception("CloneClaimsPrincipal " + ex.Message);
            }
        }




        public static bool IsAuthenticated(this System.Security.Principal.IPrincipal user)
        {
            if (user == null) return false;
            if (user.Identity == null) return false;
            if (!user.IsAuthenticated()) return false;
            if (string.IsNullOrWhiteSpace(user.Identity.Name)) return false;
            return true;
        }

        public static Dictionary<string,object> ToClaimDictionary(System.Security.Claims.ClaimsPrincipal user)
        {
            Dictionary<string, object> retour = new Dictionary<string, object>();
            foreach (var c in user.Claims)
            {
                if (!retour.Keys.Any(k => k.Equals(c.Type, StringComparison.OrdinalIgnoreCase))) retour.Add(c.Type, c.Value);
            }
            return retour;
        }



    }
}
