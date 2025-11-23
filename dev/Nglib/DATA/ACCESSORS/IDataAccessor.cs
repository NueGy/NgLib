using System.ComponentModel;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Provides standardized data accessors for value transformation and type conversion.
    /// <para>USE: using Nglib.DATA.ACCESSORS for use extensions methods in DataAccessorExtensions</para>
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_accessors"/></para>
    /// </summary>
    public interface IDataAccessor //<TRetour>
    {
        /// <summary>
        /// ⚠️ LOW-LEVEL API - Use GetValue&lt;T&gt; extension method instead for type-safe access.
        /// Gets raw data from the underlying source without type conversion.
        /// </summary>
        /// <param name="nameValue">Field name to retrieve</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>Raw object value or null if not found</returns>
        /// <remarks>
        /// This method is intended for internal/advanced use only.
        /// For type-safe access with automatic conversion, use <see cref="DataAccessorExtensions.GetValue{T}(IDataAccessor, string, T, DataAccessorOptionEnum)"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)] // Hides interface in IntelliSense
        object GetData(string nameValue, DataAccessorOptionEnum AccesOptions);

        /// <summary>
        /// ⚠️ LOW-LEVEL API - Use SetValue extension method instead for type-safe access.
        /// Sets raw data to the underlying source without validation.
        /// </summary>
        /// <param name="nameValue">Field name to set</param>
        /// <param name="obj">Object value to store</param>
        /// <param name="AccesOptions">Access options for behavior control</param>
        /// <returns>True if operation succeeded</returns>
        /// <remarks>
        /// This method is intended for internal/advanced use only.
        /// For type-safe access with validation, use <see cref="DataAccessorExtensions.SetValue(IDataAccessor, string, object, DataAccessorOptionEnum)"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)] //masque l'interface dans l'intellisense
        bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions);

        /// <summary>
        /// Gets all available field keys from the data source
        /// </summary>
        /// <returns>Array of field names</returns>
        string[] ListFieldsKeys();

        /// <summary>
        /// Gets the crypto context for encryption/decryption operations
        /// </summary>
        /// <returns>Crypto context or null if not available</returns>
        IDataAccessorCryptoContext GetCryptoContext();
    }
}