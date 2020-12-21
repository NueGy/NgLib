using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.ACCESSORS;

namespace Nglib.MANIPULATE.FORMAT
{
    [TestClass]
    public class StringTransformTests
    {
        [TestMethod]
        public void TestMethod1()
        {

            string val = "petit test {no}  avec {!data|maval} toto!";

            Nglib.DATA.DATAPO.DataPO valu = new DATA.DATAPO.DataPO();
            valu.SetObject("maval", "yesyesyes", DataAccessorOptionEnum.None);
            valu.SetObject("maval2", "ouiouioui", DataAccessorOptionEnum.None); ;

           // string finval = StringTransform.DynamiseWithAccessor(val, valu);


        }
    }
}
