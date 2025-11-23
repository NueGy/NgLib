using System.Data;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Interface for DataPO (Data Persistent Object).
    /// Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_datapo"/>
    /// </summary>
    public interface IDataPO
    {
        /// <summary>
        /// Gets the DataRow from the object
        /// </summary>
        /// <param name="RefreshFlow">Synchronize NoSQL flows</param>
        /// <returns>DataRow instance</returns>
        DataRow GetRow(bool RefreshFlow = true);
        
        /// <summary>
        /// Sets the DataRow into the object
        /// </summary>
        /// <param name="row">The DataRow to set</param>
        void SetRow(DataRow row);
        
        /// <summary>
        /// Initializes the schema (columns, keys, table name)
        /// </summary>
        /// <returns>The schema DataTable</returns>
        DataTable CreateSchema();
    }
}