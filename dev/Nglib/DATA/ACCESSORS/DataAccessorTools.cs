using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Outils et extentions pour les accesseurs
    /// </summary>
    public static class DataAccessorTools
    {
        /// <summary>
        /// Obtient la valeur Enum par default
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TEnum GetEnumDefaultValue<TEnum>() where TEnum : struct
        {
            Type t = typeof(TEnum);
            DefaultValueAttribute[] attributes = (DefaultValueAttribute[])t.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attributes != null &&
                attributes.Length > 0)
            {
                return (TEnum)attributes[0].Value;
            }
            else
            {
                return default(TEnum);
            }
        }












    }
}
