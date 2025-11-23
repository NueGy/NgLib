using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.SECURITY.TENANTS
{
    public interface ITenantProvider
    {

        Task<ITenant2> GetITenantAsync(int tenantId, bool fullLoad = false);

    }
}
