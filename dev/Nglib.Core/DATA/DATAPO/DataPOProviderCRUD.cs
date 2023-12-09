//// ----------------------------------------------------------------
//// Open Source Code on the MIT License (MIT)
//// Copyright (c) 2015 NUEGY SARL
//// https://github.com/NueGy/NgLib
//// ----------------------------------------------------------------

//using Nglib.DATA.ACCESSORS;
//using Nglib.DATA.CONNECTOR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Linq.Expressions;
//using Nglib.DATA.COLLECTIONS;
//using Nglib.SECURITY.IDENTITY;

//namespace Nglib.DATA.DATAPO
//{

//    /// <summary>
//    /// manipulation des dataPo en base
//    /// </summary>
//    public class DataPOProviderCRUD<TApiModel> : DataPOProviderCRUD<DATAPO.DataPO, TApiModel>
//        where TApiModel : DATA.BASICS.IModel, new()
//    {
//        public DataPOProviderCRUD() : base() { }
//        public DataPOProviderCRUD(APP.ENV.IGlobalEnv env) : base(env) { }
//        public DataPOProviderCRUD(CONNECTOR.IDataConnector connector) : base(connector) { }
//    }




//    /// <summary>
//    /// manipulation des models en base
//    /// </summary>
//    public class DataPOProviderCRUD<TPo, TApiModel> : DataPOProviderSQL<TPo>/*, IDataPOProviderCRUD<TPo, TApiModel>*/
//        where TPo : DATA.DATAPO.DataPO,new()
//        where TApiModel : DATA.BASICS.IModel, new()
//    {

//        public Nglib.DATA.BASICS.ModelConfigAttribute ModelApiConfig { get; set; }

 
//        public DataPOProviderCRUD() : base() 
//        {
//            if (this.ModelApiConfig == null)
//                this.ModelApiConfig = Nglib.APP.CODE.AttributesTools.FindObjectAttribute<Nglib.DATA.BASICS.ModelConfigAttribute>(typeof(TApiModel));
//        }
//        public DataPOProviderCRUD(APP.ENV.IGlobalEnv env) : base(env) 
//        {
//            if (this.ModelApiConfig == null)
//                this.ModelApiConfig = Nglib.APP.CODE.AttributesTools.FindObjectAttribute<Nglib.DATA.BASICS.ModelConfigAttribute>(typeof(TApiModel));
//        }
//        public DataPOProviderCRUD(CONNECTOR.IDataConnector connector) : base(connector) 
//        {
//            if (this.ModelApiConfig == null)
//                this.ModelApiConfig = Nglib.APP.CODE.AttributesTools.FindObjectAttribute<Nglib.DATA.BASICS.ModelConfigAttribute>(typeof(TApiModel));
//        }




//        public virtual async Task<ListResult<TApiModel>> ListCrudAsync(ITenant tenant, int count = 100, string listMode = null)
//        {
//            Dictionary<string, object> ins = new Dictionary<string, object>();
//            if (tenant != null) ins.Add("tenantid", tenant.TenantId);
//            var items =  base.GetCollectionPO<CollectionPO<TPo>>(count, ins);
//            ListResult<TApiModel> retour = new ListResult<TApiModel>();
//            retour.AddRange(items.Select(item => MapFromPO(tenant, item)).ToList());
//            return retour;
//        }



//        public virtual async Task<ListResult<TApiModel>> SearchCrudAsync<TSearchModel>(ITenant tenant, TSearchModel form) where TSearchModel : DATA.BASICS.ISearchForm
//        {
//            // !!!!

//            return retour;
//        }



         

//        public virtual async Task<TApiModel> GetCrudAsync(ITenant tenant, string fullIdB36)
//        {

//            // var fullkey = Nglib.FORMAT.KeyTools.ParseKeyB36(fullIdB36);
//            // if (!fullkey.IsValid) return default(TApiModel);
//            // if (tenant != null && tenant.TenantId != fullkey.TenantId) return default(TApiModel);
//            // Dictionary<string, object> paramKeys = fullkey.ToDictionary("elementid");


//            // Type potype = typeof(TPo);
//            // System.Data.DataTable schema = DATAPO.DataPOTools.GetSchemaOnPO(potype);
//            // if (schema == null || string.IsNullOrWhiteSpace(schema.TableName)) throw new Exception(string.Format("GetFirst: schema on {0} is not defined", potype.ToString()));

//            // string fieldkeyName = schema.GetColumns().Where(col => col.AutoIncrement).Select(col => col.ColumnName).FirstOrDefault();
//            // if (string.IsNullOrWhiteSpace(fieldkeyName)) throw new Exception(string.Format("GetFirst: column AutoIncrement not found in {0}", potype.ToString()));


//            // string sql = $"SELECT * FROM {schema.TableName} WHERE ";
//            // if (fullkey.TenantId > 0) sql += "tenantid = @tenantid AND ";
//            // if (fullkey.DateIndex.Year > 1900) sql += "dateindex =@dateindex AND ";
//            // sql += $"{fieldkeyName}=@elementid LIMIT 1;";
//            //base.GetPOWithB36key
//            // TPo retour = this.GetListPO<TPo>(sql, paramKeys).FirstOrDefault();
//            TPo retour = await this.GetPOWithB36keyAsync(fullIdB36);
//            if (retour == null) return default(TApiModel);
//            if (tenant.TenantId != retour.GetInt("tenantid")) throw new Exception("InvalidTenant");
//            var model = this.MapFromPO(tenant,retour);
//            return model;
//        }

//        public virtual async Task<TPo> SaveCrudAsync(ITenant tenant, TApiModel model)
//        {
//            if (model == null) return null;
//            if (string.IsNullOrWhiteSpace(model.Id)) throw new Exception("model.Id Invalid");
//            TPo item = this.GetPOWithB36key(model.Id);
//            if (item == null) return null;//throw new Exception($"SaveCrud {model.Id} Not found");
//            if (tenant != null && tenant.TenantId != item.GetInt("tenantid")) throw new Exception("Tenant Invalid");
//            item=this.MapToPO(tenant, model, item);
//            await this.SavePOAsync(item);
//            return item;
//        }

//        public virtual async Task<TPo> InsertCrudAsync(ITenant tenant, TApiModel model)
//        {
//            if (model == null) return null;
//            TPo item = null;
//            //item["tenantid"] = tenant.TenantId;
//            item = this.MapToPO(tenant, model, item);
//            await this.InsertPOAsync(item);
//            return item;
//        }

//        public virtual async Task<bool> DeleteCrudAsync(ITenant tenant, string idb36)
//        {
//            if (string.IsNullOrWhiteSpace(idb36)) throw new Exception("model.Id Invalid");
//            TPo item = this.GetPOWithB36key(idb36);
//            if (item == null) return false;// throw new Exception($"SaveCrud {model.Id} Not found");
//            if (tenant != null && tenant.TenantId != item.GetInt("tenantid")) throw new Exception("Tenant Invalid");
//            await this.DeletePOAsync(item);
//            return true;
//        }








//        public virtual TApiModel MapFromPO(ITenant tenant, TPo item)
//        {
//            if (item == null) return default;
//            TApiModel retour = new TApiModel();
//            //item.ToObject(retour);
//            retour.Id = item.ToString();
//            return retour;
//        }

//        public virtual TPo MapToPO(ITenant tenant, TApiModel model, TPo item)
//        {
//            throw new NotImplementedException($"NotImplemented DEV Ovveride {this.GetType().Name}.MapToPO()");
//        }



 
//    }
//}
