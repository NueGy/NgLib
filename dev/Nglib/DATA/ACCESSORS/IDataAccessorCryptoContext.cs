using Nglib.SECURITY.CRYPTO;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Manages encryption/decryption of DataPO objects.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_accessors"/></para>
    /// </summary>
    public interface IDataAccessorCryptoContext
    {
        /// <summary>
        /// Encryption option if available.
        /// </summary>
        ICryptoOption OptionForEncrypt { get; set; }

        /// <summary>
        /// Decryption option if available.
        /// </summary>
        ICryptoOption OptionForDecrypt { get; set; }

        /// <summary>
        /// Gets the initialization vector.
        /// </summary>
        /// <param name="obj">Data accessor</param>
        /// <returns>Initialization vector string</returns>
        string GetIV(IDataAccessor obj);

        /// <summary>
        /// Encrypts an object value before storage.
        /// </summary>
        object EncryptObjectValue(IDataAccessor dataAccessor, string nameValue, object value,
            DataAccessorOptionEnum options);

        /// <summary>
        /// Decrypts an object value after retrieval.
        /// </summary>
        object DecryptObjectValue(IDataAccessor dataAccessor, string nameValue, object valuecc,
            DataAccessorOptionEnum options);
    }
}