using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nglib.MANIPULATE.ACCESSORS;

namespace Nglib.MANIPULATE.TEXT
{

    [Nglib.MANIPULATE.TEXT.TextLine(TextMode = TextModeEnum.CSV)]
    public class TextExempleA : TextDatas
    {

        [Nglib.MANIPULATE.TEXT.TextPart()]
        public int ValNumber
        {
            get { return this.GetInt("ValNumber"); }
            set { this.SetObject("ValNumber",value); }
        }





        public static string getvalue(int nblines =200)
        {
            StringBuilder retour = new StringBuilder();
            retour.AppendLine("0001;fkoe85pp;15/08/2015;78.25;000620;azer   tyu");
            retour.AppendLine("2;azerg;01/05/2017;80.80;260000;uytreza");

            for (int i = 3; i < nblines; i++)
            {
                retour.AppendLine(i.ToString()+";azerg;01/05/2017;80.80;260000;uytreza");
            }


            return retour.ToString();
        }


        public static string getXmlSchema()
        {
            StringBuilder retour = new StringBuilder();
            retour.Append("<param>");
            retour.Append("<textschemas>");
            retour.Append("<TextExempleA>");
            retour.Append("<fields>");
            retour.AppendFormat("<valnumber position=0 />");
            retour.Append("</fields>");
            retour.Append("</TextExempleA>");
            retour.Append("</textschemas>");
            retour.Append("</param>");
            return retour.ToString();
        }


    }
}
