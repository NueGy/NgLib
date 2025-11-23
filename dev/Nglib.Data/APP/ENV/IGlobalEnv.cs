using Microsoft.Extensions.Configuration;
using Nglib.SECURITY.TENANTS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.ENV
{
    /// <summary>
    /// Environnement d'execution
    /// </summary>
    public interface IGlobalEnv : IMasterEnv, IRepositoryEnv, IEnv
    {

       Task<ITenant2> GetITenantAsync(int tenantId, bool fullLoad = false);


 


    }
}
