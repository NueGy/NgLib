using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.APP.CONSOLE
{
    internal class ConsoleInformation
    {



        /// <summary>
        /// Obtenir la version de dotnet
        /// https://docs.microsoft.com/fr-fr/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        /// </summary>
        /// <param name="releaseKey"></param>
        /// <returns></returns>
        internal static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return "4.7.2 or later";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
    }
    // This example displays output like the following:
    //      



}
