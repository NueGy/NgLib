using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Interface for NoSQL data flows (XML or JSON) stored in database fields.
    /// Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_datapo"/>
    /// </summary>
    public interface IDataPOFlow
    {
        /// <summary>
        /// Gets the field name to modify in the database
        /// </summary>
        /// <returns>Field name</returns>
        string GetFieldName();


        /// <summary>
        /// Gets the field type in the database
        /// </summary>
        /// <returns>Flow type enum</returns>
        FlowTypeEnum GetFieldType();


        /// <summary>
        /// Indicates if the field has been fully encrypted in the database
        /// </summary>
        /// <returns>True if encrypted</returns>
        bool IsFieldEncrypted();


        /// <summary>
        /// Defines the flow configuration
        /// </summary>
        /// <param name="fieldName">Database field name</param>
        /// <param name="fieldType">Flow type</param>
        /// <param name="isFullEncrypted">Whether the field is fully encrypted</param>
        void DefineField(string fieldName, FlowTypeEnum fieldType, bool isFullEncrypted = false);


        /// <summary>
        /// Serializes all local data for database update
        /// </summary>
        /// <returns>Serialized string</returns>
        string SerializeField();


        /// <summary>
        /// Deserializes data from database into the flow
        /// </summary>
        /// <param name="dataField">Serialized data from database</param>
        void DeSerializeField(string dataField);


        /// <summary>
        /// Checks if there are modifications among objects (requiring an update)
        /// </summary>
        /// <returns>True if changes detected</returns>
        bool IsChanges();

        /// <summary>
        /// Marks all modifications as accepted
        /// </summary>
        /// <returns>True if changes were accepted</returns>
        bool AcceptChanges();
    }
}