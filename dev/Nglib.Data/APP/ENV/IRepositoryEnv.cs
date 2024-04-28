using Nglib.SOLUTIONS.IDENTITY.TENANTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.APP.ENV
{
    public interface IRepositoryEnv //: IGlobalEnv
    {


        Nglib.FILES.STORAGE.REPOSITORY.IStoreRepository GetStoreRepository(ITenant2 tenant, string storeCode = null);
        




    }
}
