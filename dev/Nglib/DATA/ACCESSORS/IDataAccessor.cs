namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    ///     Fournis des accesseurs pour la transformation de valeurs
    /// </summary>
    public interface IDataAccessor //<TRetour>
    {
        /// <summary>
        ///     Obtenir un objet depuis la source
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        object GetData(string nameValue, DataAccessorOptionEnum AccesOptions);

        /// <summary>
        ///     Définir un Objet depuis la source
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="obj"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        bool SetData(string nameValue, object obj, DataAccessorOptionEnum AccesOptions);


        /// <summary>
        ///     Obtient la liste de tous les champs de l'objet
        /// </summary>
        /// <returns></returns>
        string[] ListFieldsKeys();


        /// <summary>
        ///     obtenir le context de cryptage de l'objet
        /// </summary>
        IDataAccessorCryptoContext GetCryptoContext();
    }
}