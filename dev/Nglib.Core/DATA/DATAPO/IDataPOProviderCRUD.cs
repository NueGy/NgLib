using Nglib.DATA.COLLECTIONS;
using Nglib.SECURITY.IDENTITY;
using Nglib.SOLUTIONS.IDENTITY.TENANTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    public interface IDataPOProviderCRUD<TPo,TApiModel>
        where TApiModel : DATA.BASICS.IModel
        where TPo : DataPO
    {

        Task<ListResult<TApiModel>> SearchCrudAsync<TSearchModel>(ITenant2 tenant, TSearchModel form)
            where TSearchModel : DATA.BASICS.ISearchForm;

        Task<ListResult<TApiModel>> ListCrudAsync(ITenant2 tenant, int count = 100, string listMode = null);

        Task<TApiModel> GetCrudAsync(ITenant2 tenant, string fullIdB36);

        Task<TPo> SaveCrudAsync(ITenant2 tenant, TApiModel item);

        Task<TPo> InsertCrudAsync(ITenant2 tenant, TApiModel item);

        Task DeleteCrudAsync(ITenant2 tenant, TApiModel item);


    }
}
