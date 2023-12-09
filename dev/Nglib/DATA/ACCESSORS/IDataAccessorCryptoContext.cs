using Nglib.SECURITY.CRYPTO;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    ///     Permet de gérer le chiffrement/déchiffrement des objets dataPO
    /// </summary>
    public interface IDataAccessorCryptoContext
    {
        /// <summary>
        ///     Option de cryptage si disponible
        /// </summary>
        ICryptoOption OptionForEncrypt { get; set; }

        /// <summary>
        ///     Option de décryptage si disponible
        /// </summary>
        ICryptoOption OptionForDecrypt { get; set; }

        /// <summary>
        ///     Obtenir le vecteur d'initialisation
        /// </summary>
        /// <param name="datapo"></param>
        /// <returns></returns>
        string GetIV(IDataAccessor obj);


        object EncryptObjectValue(IDataAccessor dataAccessor, string nameValue, object value,
            DataAccessorOptionEnum options);

        object DecryptObjectValue(IDataAccessor dataAccessor, string nameValue, object valuecc,
            DataAccessorOptionEnum options);
    }
}