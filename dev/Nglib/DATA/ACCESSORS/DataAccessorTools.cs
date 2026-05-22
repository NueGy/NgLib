using Nglib.FORMAT;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Static utility tools for data accessor manipulation and conversion.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_accessors"/></para>
    /// </summary>
    public static class DataAccessorTools
    {

        // Cache pour IsComplexType (#13)
        private static readonly ConcurrentDictionary<Type, bool> _complexTypeCache = new ConcurrentDictionary<Type, bool>();


        /// <summary>
        /// Gets the default value for any enum type
        /// </summary>
        /// <typeparam name="TEnum">Enum type to get default value for</typeparam>
        /// <returns>Default enum value (typically first enum value)</returns>
        public static TEnum GetEnumDefaultValue<TEnum>() where TEnum : struct
        {
            var t = typeof(TEnum);
            //var attributes = (DefaultValueAttribute[])t.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            //if (attributes != null &&
            //    attributes.Length > 0)
            //    return (TEnum)attributes[0].Value;
            return default;
        }

        /// <summary>
        /// Converts any array type to string array with safe type conversion
        /// </summary>
        /// <param name="value">Array object to convert</param>
        /// <returns>String array or null if input is not an array</returns>
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
        /// Copies all data from source accessor to destination accessor
        /// </summary>
        /// <param name="source">Source data accessor to copy from</param>
        /// <param name="destination">Destination data accessor to copy to</param>
        /// <exception cref="ArgumentNullException">Thrown when source or destination is null</exception>
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





        /// <summary>
        /// Determines the culture to use based on options.
        /// </summary>
        internal static CultureInfo GetCulture(DataAccessorOptionEnum options)
        {
            return options.HasFlag(DataAccessorOptionEnum.CurrentCulture)
                ? CultureInfo.CurrentCulture
                : CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Applies decryption if necessary.
        /// </summary>
        internal static object ApplyDecryption(IDataAccessor accessor, string name, object obj, DataAccessorOptionEnum options)
        {
            var cryptoctx = accessor.GetCryptoContext();
            if (cryptoctx != null)
                return cryptoctx.DecryptObjectValue(accessor, name, obj, options);

            return obj;
        }

        /// <summary>
        /// Valide qu'une valeur n'est pas "vide" pour le flag Required
        /// Vérifie: null, DBNull, string vide, tous types numériques à zéro
        /// </summary>
        /// <param name="value">Valeur à valider</param>
        /// <exception cref="DataAccessorException">Si la valeur est considérée comme vide</exception>
        internal static void ValidateRequiredValue(object value)
        {
            if (value == null || value is DBNull)
                throw new DataAccessorException("Required field cannot be null or DBNull");

            if (value is string str && string.IsNullOrWhiteSpace(str))
                throw new DataAccessorException("Required field cannot be empty string");

            // Validation pour tous les types numériques = 0
            if (value is int intVal && intVal == 0)
                throw new DataAccessorException("Required field cannot be 0");
            if (value is long longVal && longVal == 0L)
                throw new DataAccessorException("Required field cannot be 0");
            if (value is double doubleVal && doubleVal == 0.0)
                throw new DataAccessorException("Required field cannot be 0");
            if (value is decimal decimalVal && decimalVal == 0m)
                throw new DataAccessorException("Required field cannot be 0");
            if (value is float floatVal && floatVal == 0f)
                throw new DataAccessorException("Required field cannot be 0");
            if (value is short shortVal && shortVal == 0)
                throw new DataAccessorException("Required field cannot be 0");
            if (value is byte byteVal && byteVal == 0)
                throw new DataAccessorException("Required field cannot be 0");
        }

        /// <summary>
        /// Gestion des types Nullable&lt;T&gt;
        /// </summary>
        internal static bool TryNullableConversion<T>(object obj, ref Type targetType, Type objType, out T result)
        {
            result = default(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType);

            if (underlyingType == null)
                return false;

            targetType = underlyingType; // Mise à jour pour la suite du traitement

            // Try direct cast for nullable
            if (objType == targetType)
            {
                result = (T)obj;
                return true;
            }

            // AMÉLIORATION #4: Fast path pour Nullable<Enum>
            if (underlyingType.IsEnum)
            {
                if (obj is string strEnum)
                    result = (T)Enum.Parse(underlyingType, strEnum, ignoreCase: true);
                else if (obj is int || obj is long || obj is byte || obj is short)
                    result = (T)Enum.ToObject(underlyingType, obj);
                else
                    return false;
                return true;
            }

            return false; // Continuer le traitement normal
        }

        /// <summary>
        /// Conversions complexes: TimeSpan, Array→List, JSON deserialization
        /// </summary>
        internal static bool TryComplexConversion<T>(object obj, Type targetType, Type objType, CultureInfo culture, DataAccessorOptionEnum options, out T result)
        {
            result = default(T);

            // AMÉLIORATION #12: Support TimeSpan natif
            if (targetType == typeof(TimeSpan))
            {
                if (obj is string strTime && TimeSpan.TryParse(strTime, culture, out var timeSpan))
                    result = (T)(object)timeSpan;
                else if (obj is long || obj is int || obj is double)
                    result = (T)(object)TimeSpan.FromMilliseconds(Convert.ToDouble(obj, culture));
                else
                    return false;
                return true;
            }

            // AMÉLIORATION #8: Conversion automatique Array → List<T>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                if (objType.IsArray)
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = Activator.CreateInstance(listType) as System.Collections.IList;
                    foreach (var item in (Array)obj)
                        list.Add(item);
                    result = (T)list;
                    return true;
                }
            }

            // JSON deserialization path
            if (IsComplexType(targetType) && IsValidJson(obj as string))
            {
                try
                {
                    var deserializedValue = System.Text.Json.JsonSerializer.Deserialize<T>(obj.ToString());
                    if (deserializedValue != null)
                    {
                        result = deserializedValue;
                        return true;
                    }
                }
                catch (Exception exr)
                {
                    if (!options.HasFlag(DataAccessorOptionEnum.Safe))
                        throw new DataAccessorException($"InvalidJsonArray: " + exr.Message, exr);
                }
            }

            return false;
        }

        /// <summary>
        /// Détermine si un type nécessite une désérialisation JSON/XML (types complexes non primitifs)
        /// AMÉLIORATION #13: Optimisation avec cache ConcurrentDictionary
        /// </summary>
        /// <param name="type">Type à analyser</param>
        /// <returns>True si le type nécessite une désérialisation</returns>
        internal static bool IsComplexType(Type type)
        {
            // Utiliser le cache pour éviter l'analyse répétée des mêmes types
            return _complexTypeCache.GetOrAdd(type, t =>
            {
                // Types primitifs et simples : pas de désérialisation
                if (t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal))
                    return false;

                // Nullable types : vérifier le type sous-jacent
                var underlyingType = Nullable.GetUnderlyingType(t);
                if (underlyingType != null)
                    return IsComplexType(underlyingType);

                // Types système communs : pas de désérialisation (DateTime, TimeSpan, Guid, etc.)
                if (t == typeof(DateTime) || t == typeof(DateTimeOffset) ||
                    t == typeof(TimeSpan) || t == typeof(Guid))
                    return false;

                // Arrays : nécessitent une désérialisation si stockés en JSON
                if (t.IsArray)
                    return true;

                // Collections génériques (List<T>, Dictionary<K,V>, etc.)
                if (t.IsGenericType)
                {
                    var genericDef = t.GetGenericTypeDefinition();
                    if (genericDef == typeof(List<>) || genericDef == typeof(Dictionary<,>) ||
                        genericDef == typeof(IEnumerable<>) || genericDef == typeof(IList<>) ||
                        genericDef == typeof(ICollection<>))
                        return true;
                }

                // Classes personnalisées (sauf string qui est déjà traité)
                if (t.IsClass && !t.IsAbstract)
                    return true;

                return false;
            });
        }

        /// <summary>
        /// AMÉLIORATION #3: Validation JSON optimisée avec conditions moins gourmandes d'abord
        /// </summary>
        internal static bool IsValidJson(string text)
        {
            // Conditions les plus rapides d'abord (moins gourmandes en ressources)
            if (text == null) return false;
            if (text.Length < 2) return false; // Minimum "{}" ou "[]"

            // Trim seulement si nécessaire
            char firstChar = text[0];
            char lastChar = text[text.Length - 1];

            // Vérification rapide des caractères de début/fin sans allocation
            if (firstChar == '{' && lastChar == '}') return true;
            if (firstChar == '[' && lastChar == ']') return true;

            // Si espaces, trim et re-vérifier
            if (char.IsWhiteSpace(firstChar) || char.IsWhiteSpace(lastChar))
            {
                text = text.Trim();
                if (text.Length < 2) return false;
                return (text[0] == '{' && text[text.Length - 1] == '}') ||
                       (text[0] == '[' && text[text.Length - 1] == ']');
            }

            return false;
        }

    }
}