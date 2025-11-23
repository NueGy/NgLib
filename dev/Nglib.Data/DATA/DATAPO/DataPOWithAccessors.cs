using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Objet de base utilisant le datarow
    /// DataPO avec les accesseurs de bases
    /// </summary>
    [Obsolete("Use DataPO with IDataAccessor Extensions")]
    public class DataPOWithAccessors : DataPO, IDataAccessor
    {

        public T GetValue<T>(string nameValue, DataAccessorOptionEnum option= DataAccessorOptionEnum.None)
            => Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetValue<T>(this, nameValue, option);


        public void SetObject(string nameValue, object obj)
        {
            Nglib.DATA.ACCESSORS.DataAccessorExtensions.SetObject(this, nameValue, obj);
        }

        public string GetString(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetString(this, nameValue);
        }

        public int GetInt(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetInt(this, nameValue);
        }
        public DateTime GetDateTime(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetDateTime(this, nameValue);
        }
        public long GetLong(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetLong(this, nameValue);
        }
        public double GetDouble(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetLong(this, nameValue);
        }
        public bool GetBoolean(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetBoolean(this, nameValue);
        }

        public TEnum GetEnum<TEnum>(string fieldname) where TEnum : struct
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtensions.GetValue<TEnum>(this, fieldname);
        }

    }
}
