using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nglib.FILES.SERIAL
{
    /// <summary>
    /// Utilitaires XML
    /// </summary>
    public static class XmlTools
    {
        /// <summary>
        /// Affiche un Xml proprement indenté
        /// </summary>
        /// <param name="XML"></param>
        /// <returns></returns>
        public static string PrintFriendlyXML(string XML)
        {
            if(string.IsNullOrWhiteSpace(XML))return XML;
            String Result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, System.Text.Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(XML);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                String FormattedXML = sReader.ReadToEnd();

                Result = FormattedXML;
            }
            catch (XmlException)
            {
            }

            mStream.Close();
            writer.Close();

            return Result;
        }


        /// <summary>
        /// Chaine valide pour un nom de noeud XML
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidForXMLNodeName(string value)
        {
            int result = 0;
            bool isValid = true;

            //la chaine est vide
            if (string.IsNullOrWhiteSpace(value)) isValid = false;

            //la chaine fait moins de 3 caractéres
            if (value.Length < 3) isValid = false;

            //la chaine fait plus de 200 caractéres
            if (value.Length > 128) isValid = false;

            //le premier caractére est un nombre
            if (int.TryParse(value.Substring(1, 1), out result)) isValid = false;

            // la chaine contient au moins un espace
            if (value.Contains(" ")) isValid = false;

            //la chaine contient des caractéres spéciaux
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^ \\w]");
            if (regex.Match(value).Success) isValid = false;

            return isValid;
        }

    }
}
