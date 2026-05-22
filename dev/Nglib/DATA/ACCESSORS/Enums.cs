using System;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Available flow types for data serialization in DataPO
    /// </summary>
    public enum FlowTypeEnum
    {
        /// <summary>Automatic detection based on content</summary>
        AUTO,
        /// <summary>XML format</summary>
        XML,
        /// <summary>JSON format</summary>
        JSON,
        /// <summary>CSV format</summary>
        CSV,
        /// <summary>Plain text format</summary>
        TXT
    }


    /// <summary>
    /// Flags enum controlling data accessor behavior and options.
    /// <para>Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_accessors"/></para>
    /// </summary>
    [Flags]
    public enum DataAccessorOptionEnum
    {
        /// <summary>
        /// No special operations (default).
        /// </summary>
        None = 0,

        /// <summary>
        /// The value is REQUIRED (cannot be null, DBNull, empty string or 0).
        /// If the value is null/empty, an exception will be thrown (even in Safe mode).
        /// Use this flag for critical fields that must always have a value.
        /// </summary>
        Required = 1,

        /// <summary>
        /// Will not cause errors (safe mode).
        /// </summary>
        Safe = 2,

        /// <summary>
        /// Prevents replacing a value if it already exists.
        /// </summary>
        NotReplace = 4,

        /// <summary>
        /// Does not create the column if it doesn't exist.
        /// </summary>
        NotCreateColumn = 8,

        /// <summary>
        /// Defines the data without marking it as changed (datapo/datarow).
        /// </summary>
        IgnoreChange = 16,

        /// <summary>
        /// The data is encrypted (Use with IDataAccessorEncrypted).
        /// </summary>
        [Obsolete("SOON")] Encrypted = 32,

        /// <summary>
        /// Allows the use of cache for reading.
        /// Useful for operations requiring JSON deserialization.
        /// </summary>
        [Obsolete("SOON")] UseCache = 64,

        /// <summary>
        /// Allows converting data with advanced converters (get only). See FORMAT.ConvertTools.
        /// Obsolete to delete in next version
        /// </summary>
        AdvancedConverter = 128,

        /// <summary>
        /// Uses current culture (CurrentCulture) instead of InvariantCulture.
        /// Useful for UI/user conversions (ex: "123,45" in French).
        /// By default, InvariantCulture is used for data portability.
        /// </summary>
        CurrentCulture = 256,

        /// <summary>
        /// Default Parameter = NONE.
        /// </summary>
        Default = None,



    }
}