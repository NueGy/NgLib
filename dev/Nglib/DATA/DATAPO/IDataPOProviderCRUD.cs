using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    public interface IDataPOProviderCRUD<TPo,TApiModel> 
        where TPo: DATA.DATAPO.DataPO,new()
        where TApiModel : DATA.BASICS.IModel
    {

        Task<CollectionPO<TPo>> SearchCrudAsync<TSearchModel>(SECURITY.IDENTITY.ITenant tenant, TSearchModel form)
            where TSearchModel : DATA.BASICS.ISearchForm;

        Task<CollectionPO<TPo>> ListCrudAsync(SECURITY.IDENTITY.ITenant tenant, int count=100, string listMode=null);

        Task<TPo> GetCrudAsync(SECURITY.IDENTITY.ITenant tenant, string fullIdB36);

        Task SaveCrudAsync(SECURITY.IDENTITY.ITenant tenant, TPo item);

        Task DeleteCrudAsync(SECURITY.IDENTITY.ITenant tenant, TPo item);

        Task InsertCrudAsync(SECURITY.IDENTITY.ITenant tenant, TPo item);

        TApiModel MapFromPO(SECURITY.IDENTITY.ITenant tenant, TPo item);

        TPo MapToPO(SECURITY.IDENTITY.ITenant tenant, TApiModel model, TPo item);


    }
}
