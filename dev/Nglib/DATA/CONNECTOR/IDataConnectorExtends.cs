using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Extentions de méthodes utiles pour les connecteurs
    /// </summary>
    public static class IDataConnectorExtends
    {


        public static ConnectorConstants.ConnectorEngineEnum GetEngine(this CONNECTOR.IDataConnector connector)
        {
            return ConnectorTools.FindEngine(connector.EngineName);
        }



        /// <summary>
        /// Execution d'une requette SQL, Return Dataset
        /// </summary>
        public static async Task<System.Data.DataSet> QueryDataSetAsync(this CONNECTOR.IDataConnector connector, string sqlQuery, Dictionary<string, object> parameters)
        {
            QueryContext query = new QueryContext(sqlQuery, parameters);
            return await connector.QueryDataSetAsync(query);
        }

        public static async Task<System.Data.DataSet> QueryDataSetAsync(this CONNECTOR.IDataConnector connector, SqlBuilder sqlBuilder)
        {
            sqlBuilder.SqlEngine = ConnectorTools.FindEngine(connector.EngineName);
            QueryContext query = new QueryContext(sqlBuilder.ToString(), sqlBuilder.SqlInputParameters);
            return await connector.QueryDataSetAsync(query);
        }



        /// <summary>
        /// Execution d'une requette SQL, Return Dataset
        /// </summary>
        public static System.Data.DataSet QueryDataSet(this CONNECTOR.IDataConnector connector, string sqlQuery, Dictionary<string, object> parameters)
        {
            QueryContext query = new QueryContext(sqlQuery, parameters);
            return connector.QueryDataSetAsync(query).GetAwaiter().GetResult();
        }

        public static System.Data.DataSet QueryDataSet(this CONNECTOR.IDataConnector connector, SqlBuilder sqlBuilder)
        {
            sqlBuilder.SqlEngine = ConnectorTools.FindEngine(connector.EngineName);
            QueryContext query = new QueryContext(sqlBuilder.ToString(), sqlBuilder.SqlInputParameters);
            return connector.QueryDataSetAsync(query).GetAwaiter().GetResult();
        }



        /// <summary>
        /// Execution d'une requette SQL avec retour de données
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<System.Data.DataTable> QueryAsync(this CONNECTOR.IDataConnector connector, string sqlQuery, Dictionary<string, object> parameters=null)
        {
            QueryContext query = new QueryContext(sqlQuery, parameters);
            System.Data.DataSet retset = await connector.QueryDataSetAsync(query);
            if (retset.Tables.Count == 0) return null;
            else return retset.Tables[0];
        }

        public static async Task<System.Data.DataTable> QueryAsync(this CONNECTOR.IDataConnector connector, SqlBuilder sqlBuilder)
        {
            sqlBuilder.SqlEngine = ConnectorTools.FindEngine(connector.EngineName);
            QueryContext query = new QueryContext(sqlBuilder.ToString(), sqlBuilder.SqlInputParameters);
            System.Data.DataSet retset = await connector.QueryDataSetAsync(query);
            if (retset.Tables.Count == 0) return null;
            else return retset.Tables[0];
        }


        /// <summary>
        /// Execution d'une requette SQL avec retour de données
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="oparameters"></param>
        /// <returns></returns>
        public static async Task<System.Data.DataTable> QueryAsync(this CONNECTOR.IDataConnector connector, string sqlQuery, params object[] oparameters)
        {
            QueryContext query = new QueryContext(sqlQuery, ConvertArrayParameters(oparameters));
            System.Data.DataSet retset = await connector.QueryDataSetAsync(query);
            return retset.Tables[0];
        }

        /// <summary>
        /// Execution d'une requette SQL avec retour de données
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static System.Data.DataTable Query(this CONNECTOR.IDataConnector connector, string sqlQuery, Dictionary<string, object> parameters=null)
        {
            return QueryAsync(connector, sqlQuery, parameters).GetAwaiter().GetResult();
        }

        public static System.Data.DataTable Query(this CONNECTOR.IDataConnector connector, SqlBuilder sqlBuilderl)
        {
            return QueryAsync(connector, sqlBuilderl).GetAwaiter().GetResult();
        }

        /// <summary>
        ///  Execution d'une requette SQL
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<object> QueryScalarAsync(this CONNECTOR.IDataConnector connector, string sqlQuery, Dictionary<string, object> parameters=null)
        {
            QueryContext query = new QueryContext(sqlQuery, parameters);
            return await connector.QueryScalarAsync(query);
        }


        /// <summary>
        ///  Execution d'une requette SQL
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        public static async Task<object> QueryScalarAsync(this CONNECTOR.IDataConnector connector, SqlBuilder sqlBuilder)
        {
            sqlBuilder.SqlEngine = ConnectorTools.FindEngine(connector.EngineName);
            QueryContext query = new QueryContext(sqlBuilder.ToString(), sqlBuilder.SqlInputParameters);
            return await connector.QueryScalarAsync(query);
        }




        /// <summary>
        /// Execution d'une requette SQL
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="oparameters"></param>
        /// <returns></returns>
        public static async Task<object> QueryScalarAsync(this CONNECTOR.IDataConnector connector, string sqlQuery, params object[] oparameters)
        {
          
            QueryContext query = new QueryContext(sqlQuery, ConvertArrayParameters(oparameters));
            return await connector.QueryScalarAsync(query);
        }

        /// <summary>
        ///  Execution d'une requette SQL
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object QueryScalar(this CONNECTOR.IDataConnector connector, string sqlQuery, Dictionary<string, object> parameters=null)
        {
            return QueryScalarAsync(connector, sqlQuery, parameters).GetAwaiter().GetResult();
        }


        public static object QueryScalar(this CONNECTOR.IDataConnector connector, SqlBuilder sqlBuilder)
        {
            return QueryScalarAsync(connector, sqlBuilder).GetAwaiter().GetResult();
        }

        private static Dictionary<string, object> ConvertArrayParameters(params object[] oparameters)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int ii = 1;
            if (oparameters != null)
                foreach (var item in oparameters)
                {
                    parameters.Add("p" + ii, item);
                    ii++;
                }
            return parameters;
        }



        //  Task<System.Data.DataTable> QueryOAsync(string sqlQuery, params object[] oparameters);
        //List<Tpoco> Query<Tpoco>(string sqlQuery, Dictionary<string, object> parameters = null);
        //Task<System.Data.DataTable> QueryAsync(string sqlQuery, Dictionary<string, object> parameters = null);
        //Task<List<Tpoco>> QueryAsync<Tpoco>(string sqlQuery, Dictionary<string, object> parameters = null);
        //System.Data.DataTable QueryO(string sqlQuery, params object[] oparameters);
        //List<Tpoco> QueryO<Tpoco>(string sqlQuery, params object[] oparameters);

        //Task<List<Tpoco>> QueryAsyncO<Tpoco>(string sqlQuery, params object[] oparameters);

        //object QueryScalarO(string sqlQuery, params object[] oparameters);
        //System.Data.DataSet QueryDataSet(string sqlQuery, Dictionary<string, object> parameters = null);
        //object QueryScalar(string sqlQuery, Dictionary<string, object> parameters = null);
        //List<long> InsertTable(System.Data.DataTable dataTable, int SpecialTimeOut = 600, string AutoIncrementColumn = null);
        //Task<List<long>> InsertTableAsync(System.Data.DataTable dataTable, int SpecialTimeOut = 600, string AutoIncrementColumn = null);

        //bool Update(string table, Dictionary<string, object> parametersKey, Dictionary<string, object> parametersValues);
        //Task<bool> UpdateAsync(string table, Dictionary<string, object> parametersKey, Dictionary<string, object> parametersValues);

        //bool Delete(string table, Dictionary<string, object> parametersKey);
        //Task<bool> DeleteAsync(string table, Dictionary<string, object> parametersKey);

        //long Insert(string table, Dictionary<string, object> parametersValues, string AutoIncrementColumn = null);
        //Task<long> InsertAsync(string table, Dictionary<string, object> parametersValues, string AutoIncrementColumn = null);

        //void CreateTable(string tableName, List<System.Data.DataColumn> cols, bool temporary=false);




        #region ------ UPDATE / INSERT / DELETE -------

        // modification sql

        /// <summary>
        /// Query with Delete sql auto build
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="table"></param>
        /// <param name="parametersKey"></param>
        /// <returns></returns>
        public static async Task DeleteAsync(this CONNECTOR.IDataConnector connector, string table, Dictionary<string, object> parametersKey)
        {
            SqlBuilder sql = new SqlBuilder(table).Delete().AddWheres(parametersKey);
            await connector.QueryScalarAsync(sql);
        }


        /// <summary>
        /// Query with Update sql auto build
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="table"></param>
        /// <param name="parametersKey"></param>
        /// <param name="parametersValues"></param>
        /// <returns></returns>
        public static async Task UpdateAsync(this CONNECTOR.IDataConnector connector, string table, Dictionary<string, object> parametersWhereKey, Dictionary<string, object> parametersUpdateValues)
        {
            SqlBuilder sql = new SqlBuilder(table).Update(parametersUpdateValues).AddWheres(parametersWhereKey);
            await connector.QueryScalarAsync(sql);
        }

        /// <summary>
        /// Query with insert sql auto build
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="table"></param>
        /// <param name="parametersValues"></param>
        /// <param name="AutoIncrementColumn"></param>
        /// <returns></returns>
        public static async Task InsertAsync(this CONNECTOR.IDataConnector connector, string table, Dictionary<string, object> parametersValues, string AutoIncrementColumn = null)
        {
            SqlBuilder sql = new SqlBuilder(table).Insert(parametersValues);
            await connector.QueryScalarAsync(sql);
        }


            #endregion




            /*

            ///// <summary>
            ///// Execute depuis une config
            ///// </summary>
            ///// <param name="dataParamSql">config</param>
            ///// <param name="defaultParametersKey">params prioritaires sur ceux de la config</param>
            ///// <returns></returns>
            //public virtual System.Data.DataTable QueryDataValue(DATAVALUES.DataValues_data dataParamSql, Dictionary<string, object> defaultParametersKey = null)
            //{
            //    Dictionary<string, object> sqlparameters = null;
            //    string attributsname = dataParamSql["attributs"];
            //    if (string.IsNullOrWhiteSpace(attributsname)) attributsname = dataParamSql["attribut"]; // comptatibilité
            //    if (!string.IsNullOrWhiteSpace(attributsname))
            //    {
            //        sqlparameters = dataParamSql.datavalues_parent.GetDatasAttribut(attributsname, true).ToDictionaryValues();
            //    }
            //    //if (dataParamSql.IsTrue("query"))
            //    string sql = dataParamSql.ToString(true, true);

            //    if (defaultParametersKey != null)
            //    {
            //        if (sqlparameters == null) sqlparameters = new Dictionary<string, object>();
            //        foreach (var itemk in defaultParametersKey.Keys)
            //            if (!sqlparameters.ContainsKey(itemk.ToLower()))
            //                sqlparameters.Add(itemk.ToLower(), defaultParametersKey[itemk]);
            //            else sqlparameters[itemk.ToLower()] = defaultParametersKey[itemk];
            //    }



            //    return this.Query(sql, sqlparameters);
            //    //throw new NotImplementedException();
            //}
            //public async Task<DataTable> QueryDataValueAsync(DATA.DATAVALUES.DataValues_data dataParamSql, Dictionary<string, object> defaultParametersKey = null)
            //{
            //    return await Task.Run(() => QueryDataValue(dataParamSql, defaultParametersKey));
            //}



            ///// <summary>
            ///// Execute depuis une config
            ///// </summary>
            ///// <param name="dataParamSql">config</param>
            ///// <param name="defaultParametersKey">params prioritaires sur ceux de la config</param>
            ///// <returns></returns>
            //public virtual System.Data.DataSet QueryDtSetDataValue(DATAVALUES.DataValues_data dataParamSql, Dictionary<string, object> defaultParametersKey = null)
            //{
            //    Dictionary<string, object> dataattribut = null;
            //    string attributsname = dataParamSql["attributs"];
            //    if (string.IsNullOrWhiteSpace(attributsname)) attributsname = dataParamSql["attribut"]; // comptatibilité
            //    if (!string.IsNullOrWhiteSpace(attributsname)) dataattribut = dataParamSql.datavalues_parent.GetDatasAttribut(attributsname, true).ToDictionaryValues();
            //    //if (dataParamSql.IsTrue("query"))
            //    string sql = dataParamSql.ToString(true, true);

            //    if (defaultParametersKey != null)
            //    {
            //        if (dataattribut == null) dataattribut = new Dictionary<string, object>();
            //        foreach (var itemk in defaultParametersKey.Keys)
            //            if (!dataattribut.ContainsKey(itemk.ToLower()))
            //                dataattribut.Add(itemk.ToLower(), defaultParametersKey[itemk]);
            //            else dataattribut[itemk.ToLower()] = defaultParametersKey[itemk];
            //    }

            //    return this.QueryDataSet(sql, dataattribut);
            //    //throw new NotImplementedException();
            //}
            //public async Task<DataSet> QueryDtSetDataValueAsync(DATA.DATAVALUES.DataValues_data dataParamSql, Dictionary<string, object> defaultParametersKey = null)
            //{
            //    return await Task.Run(() => QueryDtSetDataValue(dataParamSql, defaultParametersKey));
            //}





        */
        }
}
