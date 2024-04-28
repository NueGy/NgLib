using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.SOLUTIONS.IDENTITY.TENANTS
{
    public interface ITenantProvider
    {

        Task<ITenant2> GetITenantAsync(int tenantId, bool fullLoad = false);

    }
}
