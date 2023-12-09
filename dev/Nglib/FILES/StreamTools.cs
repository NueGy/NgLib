using System;
using System.IO;
using System.Threading.Tasks;

namespace Nglib.FILES
{
    public static class StreamTools
    {
        public static async Task<byte[]> ReadFullyAsync(Stream input, bool AutoDispose = true)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    await input.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ReadFully " + ex.Message, ex);
            }
            finally
            {
                input.Dispose();
            }
        }
    }
}