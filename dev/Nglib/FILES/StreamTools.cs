using System;
using System.IO;
using System.Threading.Tasks;

namespace Nglib.FILES
{
    /// <summary>
    /// Permet de manipuler les streams
    /// </summary>
    public static class StreamTools
    {
        /// <summary>
        /// Lecture complète d'un stream dans un array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="AutoDispose">Fermera le stream après la lecture</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<byte[]> ReadFullyAsync(Stream input, bool AutoDispose = true)
        {
            if (input == null) return null;
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