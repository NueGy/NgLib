using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.NETWORK
{
    public static class DomainTools
    {



        // fonction statique pour obtenir l'ip depuis une adresse dns
        public static string GetIpFromDns(string dns)
        {
            try
            {
                System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(dns);
                System.Net.IPAddress[] addr = ipEntry.AddressList;
                return addr[0].ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Impossible de résoudre le nom de domaine " + dns, ex);
            }
        }


        // fonction statique pour obtenir le whois depuis une adresse dns
        public static string GetWhoisFromDns(string dns)
        {
            try
            {
                string whoisMasterServer = "whois.iana.org"; //whois.internic.net
                string whois = "";
                System.Net.Sockets.TcpClient tcpWhois = new System.Net.Sockets.TcpClient();
                tcpWhois.Connect(whoisMasterServer, 43);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(tcpWhois.GetStream());
                sw.WriteLine(dns);
                sw.Flush();
                System.IO.StreamReader sr = new System.IO.StreamReader(tcpWhois.GetStream(), Encoding.ASCII);
                while (sr.Peek() > -1)
                {
                    whois += sr.ReadLine() + "\n";
                }
                tcpWhois.Close();
                return whois;
            }
            catch (Exception ex)
            {
                throw new Exception("Impossible de résoudre le whois de " + dns, ex);
            }
        }

        public static async Task<string> WhoisViaApiAsync(string domainname)
        {
            try
            {
                var httpclient = new HttpClient();
                var whois = await httpclient.GetStringAsync("https://www.whois.com/whois/" + domainname);
                if (string.IsNullOrWhiteSpace(whois)) throw new Exception("whois null");
                return whois;
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la récupération du whois via API " + ex.Message, ex);
            }
        }




        // Parse string WHOIS into dictionary
        public static Dictionary<string, string> ParseWhois(string whois)
        {
            var result = new Dictionary<string, string>();
            var lines = whois.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length < 2) continue;
                var key = parts[0].Replace(" ","").ToUpper();
                var value = parts[1].Trim();
                if (result.ContainsKey(key))
                {
                    result[key] += "\n" + value;
                }
                else
                {
                    result.Add(key, value);
                }
            }
            return result;
        }

    }
}
