using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.PARAMVALUES
{
    public interface IParamValuesSerializer
    {


        string Serialize(ParamValues dataValues);

        ParamValues DeSerialize(string fluxstring, ParamValues retour=null);


    }
}
