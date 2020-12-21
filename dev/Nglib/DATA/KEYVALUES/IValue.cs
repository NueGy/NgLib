using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.KEYVALUES
{
    public interface IValue
    {


        object GetData();

        void AcceptChanges();

        bool IsChanges();



    }
}
