using System;
using System.ComponentModel;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    ///     Outils et extentions pour les accesseurs
    /// </summary>
    public static class DataAccessorTools
    {
        /// <summary>
        ///     Obtient la valeur Enum par default
        /// </summary>
        public static TEnum GetEnumDefaultValue<TEnum>() where TEnum : struct
        {
            var t = typeof(TEnum);
            //var attributes = (DefaultValueAttribute[])t.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            //if (attributes != null &&
            //    attributes.Length > 0)
            //    return (TEnum)attributes[0].Value;
            return default;
        }

        public static string[] ConvertoArrayString(object value)
        {
            if (value == null) return null;
            if (value is string[]) return value as string[]; // déja fait
            if (!value.GetType().IsArray) return null; // c'est pas un tableau
            var arrayvalue = value as Array;
            var retour = new string[arrayvalue.Length];
            for (var i = 0; i < arrayvalue.Length; i++)
            {
                var val = arrayvalue.GetValue(i);
                if (val != null) retour[i] = Convert.ToString(val);
            }

            return retour;
        }

        /// <summary>
        /// Copier deux DataAccessors
        /// </summary>
        public static void CopyTo(this IDataAccessor source, IDataAccessor destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            foreach (var key in source.ListFieldsKeys())
            {
                var obj = source.GetData(key, DataAccessorOptionEnum.None);
                if (obj == null) continue;
                destination.SetData(key, obj, DataAccessorOptionEnum.None);
            }
        }



    }
}