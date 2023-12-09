using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Nglib.APP.CODE;
using Nglib.DATA.BASICS;
using Nglib.DATA.COLLECTIONS;
using Nglib.DATA.KEYVALUES;
using Nglib.NET.HTTPCLIENT;

namespace Nglib.DATA.DATAMODEL
{
    /// <summary>
    ///     Wrapper pour la manipulation des datamodels par API
    /// </summary>
    /// <typeparam name="TApiModel"></typeparam>
    public class DataModelApiWrapper<TApiModel> : BaseApiWrapper where TApiModel : IModel, new()
    {
        public DataModelApiWrapper(IHttpClientContext apiAuthContext, string baseurl = null) : base(apiAuthContext,
            baseurl)
        {
            ModelConfig = AttributesTools.FindObjectAttribute<DataModelConfigAttribute>(typeof(TApiModel));

            if (string.IsNullOrWhiteSpace(BasePartUrl) && ModelConfig != null)
                BasePartUrl = ModelConfig.ApiPartUrl; // base url défini en attribute
        }


        /// <summary>
        ///     Configuration de l'objet fournis par attribute
        /// </summary>
        public DataModelConfigAttribute ModelConfig { get; set; }


        /// <summary>
        ///     Recherche
        /// </summary>
        /// <typeparam name="TSearchModel"></typeparam>
        /// <param name="formModel"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ListResult<TApiModel>> ListAsync<TSearchModel>(TSearchModel formModel,
            bool safe = false)
        {
            try
            {
                var parturl = BasePartUrl + "/";
                var querydatas = formModel.PrepareUrlQueryContent();
                if (!string.IsNullOrEmpty(querydatas)) parturl += "?" + querydatas;


                var reqm = PrepareRequest(HttpMethod.Get, parturl);
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                ListResult<TApiModel> retour = null;
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    retour = kserializer.DeSerializeList<TApiModel>(json);
                }
                else
                {
                    retour = JsonSerializer.Deserialize<ListResult<TApiModel>>(json, DefaultJsonSerializerOptions());
                }

                return retour;
            }
            catch (Exception ex)
            {
                if (safe) return ListResult<TApiModel>.PrepareForError(ex.Message);
                throw new Exception(GetType().Name + ".ListAsync " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Liste
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ListResult<TApiModel>> ListAsync(int LimitResults = 20)
        {
            try
            {
                var reqm = PrepareRequest(HttpMethod.Get, BasePartUrl + $"?LimitResults={LimitResults}");
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                ListResult<TApiModel> retour = null;
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    retour = kserializer.DeSerializeList<TApiModel>(json);
                }
                else
                {
                    retour = JsonSerializer.Deserialize<ListResult<TApiModel>>(json, DefaultJsonSerializerOptions());
                }


                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".ListAsync " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Recherche avancé avec POST (non standard)
        /// </summary>
        /// <typeparam name="TSearchModel"></typeparam>
        /// <param name="formModel"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ListResult<TApiModel>> SearchPostAsync<TSearchModel>(TSearchModel formModel,
            bool safe = false)
        {
            try
            {
                var parturl = BasePartUrl + "/search";
                var reqm = PrepareRequest(HttpMethod.Post, parturl);
                reqm.Content = HttpClientTools.PrepareJsonContent(formModel);
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                ListResult<TApiModel> retour = null;
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    retour = kserializer.DeSerializeList<TApiModel>(json);
                }
                else
                {
                    retour = JsonSerializer.Deserialize<ListResult<TApiModel>>(json, DefaultJsonSerializerOptions());
                }


                return retour;
            }
            catch (Exception ex)
            {
                if (safe) return ListResult<TApiModel>.PrepareForError(ex.Message);
                throw new Exception(GetType().Name + ".SearchAsync " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Get objet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<TApiModel> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return default;
            try
            {
                var reqm = PrepareRequest(HttpMethod.Get, BasePartUrl + $"/{id}");
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                TApiModel retour;
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    retour = kserializer.DeSerialize<TApiModel>(json);
                }
                else
                {
                    retour = JsonSerializer.Deserialize<TApiModel>(json, DefaultJsonSerializerOptions());
                }

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".GetAsync " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<TApiModel> CreateAsync(TApiModel model)
        {
            try
            {
                var reqm = PrepareRequest(HttpMethod.Post, BasePartUrl);
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    var bodyjsoncontent = kserializer.Serialize(model as KeyValues);
                    reqm.Content = HttpClientTools.PrepareJsonContent(bodyjsoncontent);
                }
                else
                {
                    reqm.Content = HttpClientTools.PrepareJsonContent(model);
                }

                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                TApiModel retour;
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    retour = kserializer.DeSerialize<TApiModel>(json);
                }
                else
                {
                    retour = JsonSerializer.Deserialize<TApiModel>(json, DefaultJsonSerializerOptions());
                }

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".CreateAsync " + ex.Message, ex);
            }
        }

        /// <summary>
        ///     PUT
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task EditAsync(string id, TApiModel model)
        {
            try
            {
                var reqm = PrepareRequest(HttpMethod.Put, BasePartUrl + $"/{id}");
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    var kserializer = new KeyValuesSerializerJson();
                    var bodyjsoncontent = kserializer.Serialize(model as KeyValues);
                    reqm.Content = HttpClientTools.PrepareJsonContent(bodyjsoncontent);
                }
                else
                {
                    reqm.Content = HttpClientTools.PrepareJsonContent(model);
                }

                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".EditAsync " + ex.Message, ex);
            }
        }

        /// <summary>
        ///     DELETE
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var reqm = PrepareRequest(HttpMethod.Delete, BasePartUrl + $"/{id}");
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                if (resm.IsSuccessStatusCode) return true;
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".DeleteAsync " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Obtenir un datamodel pour edition du model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<DataModel<TApiModel>> GetDataModelAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return default;
            try
            {
                var reqm = PrepareRequest(HttpMethod.Get, BasePartUrl + $"/{id}/Edit");
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                var retour = new DataModel<TApiModel>();


                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    // Corriger pour prendre directement du json !!! triche pour ne pas avoir a gérer le keyvalues
                    retour = JsonSerializer.Deserialize<DataModel<TApiModel>>(json, DefaultJsonSerializerOptions());
                    if (retour != null) retour.Model = await GetAsync(id);
                }
                else
                {
                    retour = JsonSerializer.Deserialize<DataModel<TApiModel>>(json, DefaultJsonSerializerOptions());
                }

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".GetMetForEditAsync " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     mettre à jours un model
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modelcomplex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task SetDataModelAsync(string id, DataModel<TApiModel> modelcomplex)
        {
            try
            {
                if (modelcomplex == null || modelcomplex.Model == null) return;
                // ajouter la gestion des keyvalue
                var reqm = PrepareRequest(HttpMethod.Put, BasePartUrl + $"/{id}/Edit");
                if (typeof(KeyValues).IsAssignableFrom(typeof(TApiModel)))
                {
                    //Nglib.DATA.KEYVALUES.KeyValuesSerializerJson kserializer = new DATA.KEYVALUES.KeyValuesSerializerJson();
                    //string bodyjsoncontent = kserializer.Serialize(modelcomplex as Nglib.DATA.KEYVALUES.KeyValues);
                    var nmodel = new DataModel<TApiModel>
                        { FormValues = modelcomplex.FormValues }; // !!! triche pour ne pas avoir a gérer le keyvalues
                    reqm.Content = HttpClientTools.PrepareJsonContent(nmodel);
                }
                else
                {
                    reqm.Content = HttpClientTools.PrepareJsonContent(modelcomplex);
                }

                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
            }
            catch (Exception ex)
            {
                throw new Exception(GetType().Name + ".EditMetAsync " + ex.Message, ex);
            }
        }


        public async Task<List<ModelValue>> GetInformations(string id)
        {
            try
            {
                var reqm = PrepareRequest(HttpMethod.Get, BasePartUrl + $"/{id}/infos");
                var resm = await apiAuthContext.ExecuteAsync(reqm);
                resm.Validate();
                var json = await resm.Content.ReadAsStringAsync();
                var retour = JsonSerializer.Deserialize<List<ModelValue>>(json, DefaultJsonSerializerOptions());
                //var retour = System.Text.Json.JsonSerializer.Deserialize<ListResult<RessourceApiModel>>(json, this.apiAuthContext.DefaultJsonSerializerOptions());

                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception($"{GetType().Name}.GetInformations {ex.Message}", ex);
            }
        }
    }
}