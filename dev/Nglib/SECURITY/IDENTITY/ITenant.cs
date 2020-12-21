using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.SECURITY.IDENTITY
{
    /// <summary>
    /// partitioning object
    /// </summary>
    public interface ITenant
    {
        int TenantId { get; set; }
    }
}
