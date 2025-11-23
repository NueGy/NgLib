using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Security filter interface for DataPO providers to manage encryption, authorization, and SQL filtering.
    /// Should be instantiated in each provider with specific security context.
    /// </summary>
    /// <remarks>
    /// This interface provides three layers of security:
    /// 1. SQL-level filtering (ApplyQueryFilter) - Most efficient, filters at database level
    /// 2. Object-level read/write authorization (IsAllowedRead/Write) - Fine-grained control
    /// 3. Encryption context (GetCryptoContext) - Transparent data encryption/decryption
    /// </remarks>
    public interface IDataPOSecurityFilter
    {

        /// <summary>
        /// Gets the cryptographic context for automatic data encryption/decryption.
        /// </summary>
        /// <param name="item">The DataPO object that needs encryption context</param>
        /// <returns>Encryption context for the object, or null if no encryption needed</returns>
        /// <remarks>
        /// Called after objects are loaded from database to enable transparent decryption.
        /// The returned context will be injected into the DataPO via SetCryptoOptions().
        /// </remarks>
        IDataAccessorCryptoContext GetCryptoContext(DataPO item);

        /// <summary>
        /// Determines if the current security context allows reading the specified objects.
        /// </summary>
        /// <param name="item">DataPO objects to check for read access</param>
        /// <returns>True if all objects can be read, false otherwise</returns>
        /// <remarks>
        /// Called after SQL execution but before returning results to caller.
        /// For better performance, prefer ApplyQueryFilter to avoid loading unauthorized data.
        /// Use this for fine-grained authorization that cannot be expressed in SQL.
        /// </remarks>
        bool IsAllowedRead(DataPO items);

        /// <summary>
        /// Determines if the current security context allows writing (insert/update/delete) the specified objects.
        /// </summary>
        /// <param name="items">Array of DataPO objects to check for write access</param>
        /// <returns>True if all objects can be written, false otherwise</returns>
        /// <remarks>
        /// Called before executing INSERT, UPDATE, or DELETE operations.
        /// Should verify user permissions, tenant isolation, and any business rules.
        /// </remarks>
        bool IsAllowedWrite(DataPO[] items);

        /// <summary>
        /// Applies security filters to the SQL query before execution.
        /// </summary>
        /// <param name="query">The SQL query context to be modified with security constraints</param>
        /// <returns>True if query is allowed to execute, false if query should be blocked entirely</returns>
        /// <remarks>
        /// Most efficient security layer - filters data at database level.
        /// Should modify the QueryContext by adding WHERE clauses, parameters, etc.
        /// Example: Add "AND tenantid = @currentTenantId" for multi-tenant isolation.
        /// Return false only for queries that should be completely forbidden. (Silent fail)
        /// </remarks>
        bool ApplyQueryFilter(CONNECTOR.QueryContext query);
         

    }
}
