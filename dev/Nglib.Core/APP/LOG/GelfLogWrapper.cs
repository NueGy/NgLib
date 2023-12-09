using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.LOG
{

    public class GelfLogWrapper
    {
        public string OvhToken { get; set; }
        public string NgyToken { get; set; }
        private System.Net.Sockets.UdpClient udpClient { get; set; }

        public GelfLogWrapper(string host, int port)
        {
            this.udpClient = new System.Net.Sockets.UdpClient(host, port);

        }



        public async Task<bool> SendAsync(string source, string shortmessage, int level = 0, string fullmessage = null, Dictionary<string, object> additionalFields = null)
        {
            var messagefields = new Dictionary<string, object>();
            //messagefields.Add("version", "1.1");
            messagefields.Add("host", source);
            messagefields.Add("short_message", shortmessage);
            //fields.Add("timestamp", 1622297998);
            messagefields.Add("level", level);
            if (!string.IsNullOrEmpty(fullmessage)) messagefields.Add("full_message", fullmessage);

            if (additionalFields != null && additionalFields.Count > 0)
                additionalFields.Keys.ToList().ForEach(key => {
                    string fkey = key;
                    if (!fkey.StartsWith("_")) fkey = "_" + fkey;
                    messagefields.Add(fkey, additionalFields[key]);
                });

            return await SendAsync(messagefields);
        }


        public bool Send(string source, string shortmessage, int level = 0, string fullmessage = null, Dictionary<string, object> additionalFields = null)
        { return SendAsync(source, shortmessage, level, fullmessage, additionalFields).GetAwaiter().GetResult(); }




        public async Task<bool> SendAsync(Dictionary<string, object> message)
        {
            try
            {


                //if (!message.ContainsKey("timestamp"))
                //    message.Add("timestamp", Nglib.FORMAT.DateTools.Time().ToString());
                //1622297998
                //

                if (!string.IsNullOrEmpty(this.OvhToken) && !message.ContainsKey("_X-OVH-TOKEN"))
                    message.Add("_X-OVH-TOKEN", this.OvhToken);
                if (!string.IsNullOrEmpty(this.NgyToken) && !message.ContainsKey("_X-NGY-TOKEN"))
                    message.Add("_X-NGY-TOKEN", this.NgyToken);


                //var serializedGelfMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message, Newtonsoft.Json.Formatting.None);
                var serializedGelfMessage = System.Text.Json.JsonSerializer.Serialize(message);
                //serializedGelfMessage += "\0";
                byte[] bytes = null;
                //bytes = ConvertGzip(serializedGelfMessage, System.Text.Encoding.UTF8);
                bytes = System.Text.Encoding.UTF8.GetBytes(serializedGelfMessage);


                // limit size : 8192 bytes


                int ret = await this.udpClient.SendAsync(bytes, bytes.Length);
                if (ret > 0) return true;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }




        private void CallBack(System.IAsyncResult ar)
        {

        }


        /// <summary>
        /// Gzips a string
        /// </summary>
        public static byte[] ConvertGzip(string message, System.Text.Encoding encoding)
        {
            byte[] data = encoding.GetBytes(message);

            using (var compressedStream = new System.IO.MemoryStream())
            using (var zipStream = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }


    }

}
