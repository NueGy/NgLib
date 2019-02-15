using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.DATAPO
{

    /// <summary>
    /// Objet de base utilisant le datarow
    /// DataPO avec les accesseurs de bases
    /// </summary>
    public class DataPOWithAccessor : DataPO, IDataAccessor
    {

        public string GetString(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetString(this, nameValue, DataAccessorOptionEnum.Safe);
        }

        public int GetInt( string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetInt(this, nameValue, 0);
        }
        public DateTime GetDateTime( string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetDateTime(this, nameValue, new DateTime());
        }
        public long GetLong( string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetLong(this, nameValue, 0);
        }
        public double GetDouble(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetLong(this, nameValue, 0);
        }
        public bool GetBoolean(string nameValue)
        {
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetBoolean(this, nameValue, false);
        }

        public TEnum GetEnum<TEnum>(string fieldname) where TEnum : struct
        {
            TEnum defaultValue = DataAccessorTools.GetEnumDefaultValue<TEnum>();
            return Nglib.DATA.ACCESSORS.DataAccessorExtentions.GetEnum(this, fieldname, defaultValue);
        }

    }


}
