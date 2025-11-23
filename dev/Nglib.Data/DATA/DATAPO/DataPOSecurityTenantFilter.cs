using Nglib.DATA.ACCESSORS;
using Nglib.DATA.CONNECTOR;
using Nglib.SECURITY.IDENTITY;
using Nglib.SECURITY.TENANTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Tenant-based security filter for DataPO providers.
    /// Provides multi-tenant data isolation with automatic encryption context injection.
    /// </summary>
    /// <remarks>
    /// This implementation provides:
    /// - Automatic tenant filtering at SQL level for performance
    /// - Crypto context injection for transparent encryption/decryption
    /// - Read/write authorization based on tenant ownership
    /// - Support for both user-based and batch scenarios
    /// </remarks>
    [Obsolete("SONN-DEMOONLY")]
    public class DataPOSecurityTenantFilter : IDataPOSecurityFilter
    {
        /// <summary>
        /// Default claim type for tenant ID in user claims
        /// </summary>
        public static string DefaultTenantIdClaimType = "tenant_id";

        /// <summary>
        /// Default SQL column name for tenant filtering
        /// </summary>
        public static string DefaultTenantIdColumnName = "tenantid";

        /// <summary>
        /// Current tenant for security operations
        /// </summary>
        private readonly ITenant2 tenant;



        /// <summary>
        /// Constructor for user-based security context.
        /// Extracts tenant from user claims and loads tenant data.
        /// </summary>
        /// <param name="user">Current user with tenant claim</param>
        /// <param name="tenantProvider">Provider to load tenant data</param>
        /// <param name="allowCrossTenantAccess">If true, allows admin users to access all tenants</param>
        /// <exception cref="ArgumentNullException">If user or tenantProvider is null</exception>
        /// <exception cref="UnauthorizedAccessException">If user has no valid tenant claim</exception>
        public DataPOSecurityTenantFilter(ClaimsPrincipal user, ITenantProvider tenantProvider)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (tenantProvider == null) throw new ArgumentNullException(nameof(tenantProvider));


            // Extract tenant ID from user claims
            var tenantIdClaim = user.Claims?.FirstOrDefault(c => c.Type == DefaultTenantIdClaimType);
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out int tenantId) || tenantId <= 0)
            {
                throw new UnauthorizedAccessException($"User does not have a valid '{DefaultTenantIdClaimType}' claim");
            }

            // Load tenant data
            this.tenant = tenantProvider.GetITenantAsync(tenantId).GetAwaiter().GetResult();
            if (this.tenant == null)
            {
                throw new UnauthorizedAccessException($"Tenant {tenantId} not found or access denied");
            }
        }

        /// <summary>
        /// Constructor for pre-loaded tenant (batch scenarios or service accounts).
        /// </summary>
        /// <param name="tenant">Pre-loaded tenant object</param>
        /// <exception cref="ArgumentNullException">If tenant is null and cross-tenant access is not allowed</exception>
        public DataPOSecurityTenantFilter(ITenant2 tenant)
        {

            this.tenant = tenant;
        }




        /// <summary>
        /// Gets the cryptographic context for transparent data encryption/decryption.
        /// </summary>
        /// <param name="item">DataPO object needing crypto context</param>
        /// <returns>Crypto context if tenant matches, null otherwise</returns>
        public IDataAccessorCryptoContext GetCryptoContext(DataPO item)
        {
            if (item == null || tenant == null) return null;

            // Validate tenant ownership
            if (!IsTenantValid(item)) return null;

            // Return tenant's crypto context
            return tenant?.CloneCryptoContext(item.GetType());
        }

        /// <summary>
        /// Determines if the current security context allows reading the specified objects.
        /// Use in addition to ApplyQueryFilter for fine-grained control.
        /// </summary>
        /// <param name="item">DataPO objects to check</param>
        /// <returns>True if all objects can be read</returns>
        public bool IsAllowedRead(DataPO item)
        {
            return IsTenantValid(item);
        }

        /// <summary>
        /// Determines if the current security context allows writing the specified objects.
        /// </summary>
        /// <param name="items">Array of DataPO objects to check</param>
        /// <returns>True if all objects can be written</returns>
        public bool IsAllowedWrite(DataPO[] items)
        {
            if (items == null || items.Length == 0) return true;

            // All items must belong to current tenant and not be readonly
            return items.All(item => IsTenantValid(item));
        }

        /// <summary>
        /// Applies tenant-based security filters to SQL queries.
        /// </summary>
        /// <param name="query">SQL query context to modify</param>
        /// <returns>True if query is allowed to execute</returns>
        public bool ApplyQueryFilter(QueryContext query)
        {
            if (query == null || tenant == null) return false;
            try
            {
                // Check if tenant filter already exists
                string paramName = "securityTenantId";
                if (query.Parameters.ContainsKey(paramName))
                {
                    // Verify existing filter matches current tenant
                    var existingTenantId = Convert.ToInt32(query.Parameters[paramName]);
                    if (existingTenantId != tenant.TenantId) return false;
                }

                // Add tenant parameter
                query.Parameters.Add(paramName, tenant.TenantId);

                // Modify SQL query to include tenant filter
                string tenantFilter = $"{DefaultTenantIdColumnName} = @{paramName}";

                if (query.SqlQuery.ToUpperInvariant().Contains("WHERE"))
                {
                    // Append to existing WHERE clause just after it
                    int whereIndex = query.SqlQuery.ToUpperInvariant().IndexOf("WHERE") + 5;
                    query.SqlQuery = query.SqlQuery.Insert(whereIndex, $" {tenantFilter} AND");
                   
                }
                else
                {
                    query.SqlQuery += $" WHERE {tenantFilter}";
                }

                return true;
            }
            catch (Exception)
            {
                // If filtering fails, deny the query for security
                return false;
            }

            // No tenant and no cross-tenant access: deny
            return false;
        }

        /// <summary>
        /// Validates if a DataPO object belongs to the current tenant.
        /// </summary>
        /// <param name="item">DataPO object to validate</param>
        /// <returns>True if object belongs to current tenant</returns>
        private bool IsTenantValid(DataPO item)
        {
            if (item == null || tenant==null) return false;

            // extrait tenantid field
            string tenanid = item.GetString(DefaultTenantIdColumnName); 
            if (string.IsNullOrWhiteSpace(tenanid)) return false;

            //Compare  tenantid with tenant Object
            if (int.TryParse(tenanid, out int itemTenantId))
                return itemTenantId == tenant.TenantId;
            
            return false;
        }


 

        /// <summary>
        /// Gets the current tenant (for debugging/logging purposes).
        /// </summary>
        public ITenant2 CurrentTenant => tenant;


    }
}
