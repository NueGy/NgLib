using Nglib.DATA.BASICS;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.NET.HTTPCLIENT
{
    public class DefaultApiWrapper<TApiModel> //where TApiModel : DATA.BASICS.IModel
    {

        public string BasePartUrl { get; set; }
        public IApiAuthModel apiAuthContext;
        public DATA.BASICS.ModelConfigAttribute ModelConfig { get; set; }

        public DefaultApiWrapper(IApiAuthModel apiAuthContext,string baseparturl=null) { 
            this.BasePartUrl = baseparturl;
            this.apiAuthContext = apiAuthContext;

            this.ModelConfig = Nglib.APP.CODE.AttributesTools.FindObjectAttribute<ModelConfigAttribute>(typeof(TApiModel));

            if(string.IsNullOrWhiteSpace(this.BasePartUrl) && this.ModelConfig != null)
            {
                this.BasePartUrl = this.ModelConfig.ApiPartUrl;
            }


        }


        public async virtual Task<DATA.COLLECTIONS.ListResult<TApiModel>> SearchAsync<TSearchModel>(TSearchModel formModel)
        {
            try
            {
                string parturl = this.BasePartUrl + $"/search";
                var reqm = this.apiAuthContext.PrepareRequest(System.Net.Http.HttpMethod.Post, parturl);
                reqm.Content = Nglib.NET.HTTPCLIENT.HttpClientTools.PrepareJsonContent(formModel);
                var resm = await this.apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                string json = await resm.Content.ReadAsStringAsync();
                ListResult<TApiModel> retour = null;
                //var test= Newtonsoft.Json.JsonConvert.DeserializeObject<ListResult<TRANSACTIONS.TransactionApiModel>>(json); //CustomerApiModel
                retour = System.Text.Json.JsonSerializer.Deserialize<ListResult<TApiModel>>(json, this.apiAuthContext.DefaultJsonSerializerOptions());

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetType().Name + ".SearchAsync " + ex.Message, ex);
            }
        }

        public async virtual Task<ListResult<TApiModel>> ListAsync()
        {
            try
            {
                var reqm = this.apiAuthContext.PrepareRequest(System.Net.Http.HttpMethod.Get, this.BasePartUrl);
                var resm = await this.apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                string json = await resm.Content.ReadAsStringAsync();
                ListResult<TApiModel> retour = null;
                //var test= Newtonsoft.Json.JsonConvert.DeserializeObject<ListResult<TRANSACTIONS.TransactionApiModel>>(json); //CustomerApiModel
                retour = System.Text.Json.JsonSerializer.Deserialize<ListResult<TApiModel>>(json, this.apiAuthContext.DefaultJsonSerializerOptions());

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetType().Name + ".ListAsync " + ex.Message, ex);
            }
        }


        public async virtual Task<TApiModel> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return default;
            try
            {
                var reqm = this.apiAuthContext.PrepareRequest(System.Net.Http.HttpMethod.Get, this.BasePartUrl + $"/{id}");
                var resm = await this.apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                string json = await resm.Content.ReadAsStringAsync();
                TApiModel retour =
                    System.Text.Json.JsonSerializer.Deserialize<TApiModel>(json, this.apiAuthContext.DefaultJsonSerializerOptions());

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetType().Name + ".GetAsync " + ex.Message, ex);
            }
        }





        public async virtual Task<TApiModel> CreateAsync(TApiModel model)
        {
            try
            {
                var reqm = this.apiAuthContext.PrepareRequest(System.Net.Http.HttpMethod.Post, this.BasePartUrl);
                reqm.Content = Nglib.NET.HTTPCLIENT.HttpClientTools.PrepareJsonContent(model);
                var resm = await this.apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                string json = await resm.Content.ReadAsStringAsync();
                TApiModel retour =
                    System.Text.Json.JsonSerializer.Deserialize<TApiModel>(json, this.apiAuthContext.DefaultJsonSerializerOptions());

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetType().Name + ".CreateAsync " + ex.Message, ex);
            }
        }

        public async virtual Task EditAsync(string id,TApiModel model)
        {
            try
            {
                var reqm = this.apiAuthContext.PrepareRequest(System.Net.Http.HttpMethod.Put, this.BasePartUrl + $"/{id}");
                reqm.Content = Nglib.NET.HTTPCLIENT.HttpClientTools.PrepareJsonContent(model);
                var resm = await this.apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetType().Name + ".EditAsync " + ex.Message, ex);
            }
        }


        public async virtual Task<bool> DeleteAsync(string id)
        {
            try
            {
                var reqm = this.apiAuthContext.PrepareRequest(System.Net.Http.HttpMethod.Delete, this.BasePartUrl + $"/{id}");
                var resm = await this.apiAuthContext.ExecuteAsync(reqm);
                if (resm.IsSuccessStatusCode) return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw new Exception(this.GetType().Name + ".DeleteAsync " + ex.Message, ex);
            }
        }

    }
}
