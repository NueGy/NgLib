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

        public static string[] ConvertoArrayString(object value)
        {
            if (value == null) return null; 
            if (value is string[]) return value as string[]; // déja fait
            if (!value.GetType().IsArray) return null; // c'est pas un tableau
            Array arrayvalue = value as Array;
            string[] retour = new string[arrayvalue.Length];
            for (int i = 0; i < arrayvalue.Length; i++)
            {
                object val = arrayvalue.GetValue(i);
                if (val != null) retour[i] = Convert.ToString(val);
            }
            return retour;
        }




    }
}
