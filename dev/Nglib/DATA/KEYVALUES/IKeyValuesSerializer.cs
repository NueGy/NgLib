using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Nglib.DATA.KEYVALUES
{
    public interface IKeyValuesSerializer
    {


      
        string Serialize(KeyValues values);

        KeyValues DeSerialize(string fluxstring);




    }



   


   

}
