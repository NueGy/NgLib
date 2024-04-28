using System;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    ///     Type de flux disponibles
    /// </summary>
    public enum FlowTypeEnum
    {
        AUTO,
        XML,
        JSON,
        CSV,
        TXT
    }


    [Flags]
    public enum DataAccessorOptionEnum
    {
        /// <summary>
        ///     Aucunes opération spécial (default)
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Permet de rendre une valeur null
        /// </summary>
        Nullable = 0x01,

        /// <summary>
        ///     Ne provoquera pas d'erreur
        /// </summary>
        Safe = 0x02,

        /// <summary>
        ///     Interdit le remplacement d'une valeur si elle existe déja
        /// </summary>
        NotReplace = 0x04,

        /// <summary>
        ///     Créer la colonne si elle existe pas
        /// </summary>
        NotCreateColumn = 0x08,

        /// <summary>
        ///     La donnée est cryptée (Use with IDataAccessorEncrypted)
        /// </summary>
        [Obsolete("DevSoon")] Encrypted = 0x16,

        /// <summary>
        ///     Défini la donnée sans indiquer qu'elle à été changée (datapo/datarow)
        /// </summary>
        IgnoreChange = 0x32,

        /// <summary>
        ///     Permet l'utilisation d'un cache en lecture
        /// </summary>
        UseCache = 0x64,

        /// <summary>
        ///     permet de convertir la données avec des convertisseur amélioré (get only)
        /// </summary>
        [Obsolete("DevSoon")] AdvancedConverter = 0x128,

        /// <summary>
        /// Default Parameter = NONE
        /// </summary>
        Default = None,



    }
}