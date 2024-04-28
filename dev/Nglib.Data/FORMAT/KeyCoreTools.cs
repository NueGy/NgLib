using Nglib.SECURITY.CRYPTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    public class KeyCoreTools
    {



        /// <summary>
        ///     Valider une signature HMACSHA256 avec expiration (SAFE)
        /// </summary>
        [Obsolete("NonTeste")]
        public static bool StringSignValidate(string fullidwithkey, string signkey)
        {
            if (string.IsNullOrWhiteSpace(fullidwithkey) || string.IsNullOrWhiteSpace(signkey))
                return false;
            try
            {
                if (!fullidwithkey.Contains(".S")) return false; // ne contient pas de partie signature
                var fullid = KeyTools.StringSignRemove(fullidwithkey);
                var signpref = fullidwithkey.Substring(fullidwithkey.LastIndexOf(".S"), 12);
                DateTime expiredate;
                // validation de la date d'expiration
                if (!DateTime.TryParseExact(signpref.Substring(2), "yyyyMMddHHmm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out expiredate))
                    return false; // formatage de la date invalide
                if (expiredate > DateTime.Now) return false; // date expirée

                var sign = fullidwithkey.Substring(fullidwithkey.LastIndexOf(".S") + 12);
                var resign = TokenJwtTools.GetHmacSha256(fullid + signpref, signkey);

                if (string.IsNullOrEmpty(sign) || string.IsNullOrEmpty(resign)) return false;
                if (!sign.Equals(resign)) return false;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        ///     Ajoute une signature ([...].Sxxxxxx) à une chaine de caractères.
        /// </summary>
        public static string StringSignGenerate(string fullid, string signkey, DateTime? expiredate = null)
        {
            if (string.IsNullOrWhiteSpace(fullid) || string.IsNullOrWhiteSpace(signkey)) return null;
            if (!expiredate.HasValue) expiredate = DateTime.Now.AddDays(1); // valable 1 jour par defaults
            var signpref = ".S" + expiredate.Value.ToString("yyyyMMddHHmm"); //size:12
            var sign = TokenJwtTools.GetHmacSha256(fullid + signpref, signkey);
            return $"{fullid}{signpref}{sign}";
        }

    }
}
