using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.TESTS.MODELS
{
    public class ValuesDemo : Nglib.DATA.ACCESSORS.IDataAccessor
    {

        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();


        public IDataAccessorCryptoContext GetCryptoContext()
        {
            throw new NotImplementedException();
        }

        public object GetData(string nameValue, DataAccessorOptionEnum AccesOptions)
        {
            return this.Values[nameValue];
        }

        public string[] ListFieldsKeys()
        {
            return Values.Keys.ToArray();
        }

        public bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
        {
            if(!this.Values.ContainsKey(nameValue))
                this.Values.Add(nameValue, null);
            this.Values[nameValue] = obj;
            return true;
        }
    }
}
