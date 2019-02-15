using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Type de flux disponibles
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
        /// Aucunes opération spécial (standard)
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Permet de rendre une valeur null
        /// </summary>
        Nullable = 0x01,
        /// <summary>
        /// Ne provoquera pas d'erreur
        /// </summary>
        Safe = 0x02,
        /// <summary>
        /// Interdit le remplacement d'une valeur si elle exist déja
        /// </summary>
        NotReplace = 0x04,
        /// <summary>
        /// Créer la colonne si elle existe pas
        /// </summary>
        CreateIfNotExist = 0x08,
        /// <summary>
        /// Transforme la données (datavalue)
        /// </summary>
        Dynamise = 0x16,
        /// <summary>
        /// Transforme la données (datavalue)
        /// </summary>
        Tranform = 0x32,
        /// <summary>
        /// Valide la données (datavalue)
        /// </summary>
        Validate = 0x64,
        /// <summary>
        /// permet de convertir la données avec des convertisseur amélioré (get only)
        /// </summary>
        AdvancedConverter = 0x128,
        /// <summary>
        /// Permet l'utilisation ou la mise en cache
        /// </summary>
        UseCache = 0x256,
        /// <summary>
        /// Défini la donnée sans indiquer qu'elle à été changée (datapo/datarow)
        /// </summary>
        IgnoreChange = 0x512,

        /// <summary>
        /// Dynamise+Tranform+Validate (datavalue)
        /// </summary>
        DynamiseTransformValidate = Dynamise + Tranform + Validate
    }

}
