// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NuegyAgency/NGLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace NGLib.DATA.CONNECTOR
{
    public class MsSQLConnector : BaseDataConnector,IDataConnector,IDisposable
    {
        private SqlConnection conn = null;
        SqlTransaction transac = null;
        long lastinsert = 0;
        public List<int> RetryErrorSQLNumber = new List<int>() {-1,-2,53,233,1222}; // codes erreurs que l'on peut retenter
        public const int RetryMax = 2;
        

        public override bool Open()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString)) throw new Exception("Chaine de connexion SQL vide ...");
            SqlConnectionStringBuilder nchainbuild = new SqlConnectionStringBuilder(this.ConnectionString);
            //nchainbuild.ApplicationName = COMPONENTS.APP.AppCore.AppName;
            //if (!nchain.Contains("Timeout")) nchain += ";Connection Timeout=" + this.DefaultTimeOut.ToString();
            conn = new SqlConnection(nchainbuild.ToString());
            conn.Open();
            return true;
        }

        public override bool Close()
        {
            try
            {
                if (conn != null)
                {
                    try
                    {
                        if (transac != null) this.RollBackTransaction();
                    }
                    catch (Exception)
                    {
                    }
                    conn.Close();
                    conn = null;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Commence une transaction sql isolée
        /// ATTENTION LES OUVERTURES ET FERMETURES DE LA BASE NE SERONT PLUS AUTOMATIQUES
        /// </summary>
        public override bool BeginTransaction(string transactionName = null)
        {
            this.sqlMutex.WaitOne();
            if (conn == null) this.Open();
            transac = conn.BeginTransaction();
            this.sqlMutex.ReleaseMutex();
            return true;
        }

        public override bool CommitTransaction(string transactionName = null)
        {
            if (conn == null) return false;
            if (transac == null) return false;
            this.sqlMutex.WaitOne();
            transac.Commit();
            transac.Dispose();
            transac = null;
            this.sqlMutex.ReleaseMutex();
            return true;
        }



        public override bool RollBackTransaction(string transactionName = null)
        {
            if (conn == null) return false;
            if (transac == null) return false;
            this.sqlMutex.WaitOne();
            transac.Rollback();
            transac.Dispose();
            transac = null;
            this.sqlMutex.ReleaseMutex();
            return true;
        }





        public SqlCommand InitCommand(QueryContext query)
        {

            if (conn == null) this.Open();
            try
            {

                SqlCommand cmd = conn.CreateCommand();
                if (transac != null) cmd.Transaction = transac;
                cmd.CommandTimeout = this.DefaultTimeOut;

                if (DataConnectorTools.IsSQLQuery(query.sqlQuery)) cmd.CommandType = CommandType.Text;
                else cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = query.sqlQuery;

                if (query.parameters != null)
                {
                    foreach (string fieldKey in query.parameters.Keys)
                    {
                        // if (itemd.IsDynamic && !itemd.IsTrue("notransform") && itemd.NameMinimal.ToLower() != "fluxxml") itemd.value = itemd.Transformation();
                        cmd.Parameters.AddWithValue("@" + fieldKey, query.parameters[fieldKey]);
                    }
                }
                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception("Initialisation SQL", ex);
            }
        }





        public void Change(DataTable tablechange)
        {

            if(string.IsNullOrWhiteSpace(tablechange.TableName)) throw new Exception("ChangeSQL tablename error");

            SqlDataReader read = null;
            //SqlCommand cmd = null;
            SqlDataAdapter adapter = null;
            try
            {
                this.sqlMutex.WaitOne();
                 if (conn == null) this.Open();


                adapter= new System.Data.SqlClient.SqlDataAdapter();
                adapter.SelectCommand = new System.Data.SqlClient.SqlCommand("SELECT * FROM " + tablechange.TableName, this.conn);

                System.Data.SqlClient.SqlCommandBuilder cmdBldr = new System.Data.SqlClient.SqlCommandBuilder(adapter);
                adapter.UpdateCommand=cmdBldr.GetUpdateCommand(); 
                //System.Data.SqlClient.SqlCommand cmd = cmdBldr.GetUpdateCommand();
                //Console.WriteLine(cmd.ToString());

                adapter.Update(tablechange);
            }
            finally
            {
                try { adapter.Dispose(); }  catch { }
                if (transac == null) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }
        


        public override DataTable Query(string sqlQuery, Dictionary<string, object> parameters = null)
        {
            int QueryTentative = 0;
            while (true)
            {
                try
                {
                    QueryTentative++;
                    QueryContext query = new QueryContext(sqlQuery, parameters);
                    DataTable ret = QueryExec(query);
                    return ret;
                }
                catch (SqlException sqlExc)
                {
                    if (QueryTentative<RetryMax && RetryErrorSQLNumber.Contains(sqlExc.Number)) { System.Threading.Thread.Sleep(300); continue; } // on recommence
                    else { ExceptionSQL(sqlExc, sqlQuery,QueryTentative);}
                }
                catch (Exception e)
                {
                    ExceptionSQL(e, sqlQuery, QueryTentative);
                }
            }
            return null; 
        }



        private DataTable QueryExec(QueryContext query)
        {
            SqlDataReader read = null;
            SqlCommand cmd = null;
            SqlDataAdapter adapter = null;
            try
            {
                this.sqlMutex.WaitOne();
                query.Open();
                cmd = this.InitCommand(query);
                
                DataTable datas = new DataTable();
                read = cmd.ExecuteReader();
                datas.Load(read);
                read.Close();
                read = null;
                cmd.Dispose();
                if (datas != null) query.CountResult = datas.Rows.Count;
                query.Close();
                return datas;
            }
            catch (Exception e)
            {
                query.Close(e);
                throw;
            }
            finally
            {
                if (transac == null) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }




        public override DataSet QueryDataSet(string sqlQuery, Dictionary<string, object> parameters = null)
        {
            int QueryTentative = 0;
            while (QueryTentative < 2)
            {
                try
                {
                    QueryTentative++;
                    QueryContext query = new QueryContext(sqlQuery, parameters);
                    DataSet ret = QueryDataSetExec(query);
                    return ret;
                }
                catch (SqlException sqlExc)
                {
                    if (QueryTentative < RetryMax && RetryErrorSQLNumber.Contains(sqlExc.Number)) { System.Threading.Thread.Sleep(300); continue; } // on recommence
                    else { ExceptionSQL(sqlExc, sqlQuery, QueryTentative); }
                }
                catch (Exception e)
                {
                    ExceptionSQL(e, sqlQuery, QueryTentative);
                }
            }
            return null; 
        }


        private DataSet QueryDataSetExec(QueryContext query)
        {
            SqlDataReader read = null;
            SqlCommand cmd = null;
            SqlDataAdapter adapter = null;

            try
            {
                this.sqlMutex.WaitOne();
                query.Open();
                cmd = this.InitCommand(query);
                adapter = new SqlDataAdapter(cmd);
                DataSet datas = new DataSet();
                adapter.Fill(datas);
                adapter.Dispose();
                cmd.Dispose();
                adapter = null;
                query.Close();
                return datas;
            }
            catch (Exception e)
            {
                query.Close(e);
                throw;
            }
            finally
            {
                if (transac == null) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }




        public override object QueryScalar(string sqlQuery, Dictionary<string, object> parameters = null)
        {
            int QueryTentative = 0;
            while (true)
            {
                
                try
                {
                    QueryTentative++;
                    QueryContext query = new QueryContext(sqlQuery, parameters);
                    object ret = QueryScalarExec(query);
                    return ret;
                }
                catch (SqlException sqlExc)
                {
                    if (QueryTentative < RetryMax && RetryErrorSQLNumber.Contains(sqlExc.Number)) { System.Threading.Thread.Sleep(300); continue; } // on recommence
                    else { ExceptionSQL(sqlExc, sqlQuery, QueryTentative); }
                }
                catch (Exception e)
                {
                    ExceptionSQL(e, sqlQuery, QueryTentative);
                }
            }
            return null; 
        }

        private object QueryScalarExec(QueryContext query)
        {
            SqlDataReader read = null;
            SqlCommand cmd = null;
            try
            {
                this.sqlMutex.WaitOne();
                query.Open();
                cmd = this.InitCommand(query);
                object retourobj = cmd.ExecuteScalar();
                cmd.Dispose();
                query.Close();
                return retourobj;
            }
            catch (Exception e)
            {
                query.Close(e);
                throw;
            }
            finally
            {
                if (transac == null) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }






        public bool BulkCopyTrace10000 = false;
        public override List<long> InsertTable(DataTable dataTable, int SpecialTimeOut = 600, string AutoIncrementColumn = null)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) return new List<long>();

            try
            {
                this.sqlMutex.WaitOne();
                if (conn == null) this.Open();
                SqlDataReader read = null;
                SqlBulkCopyOptions copyoption = SqlBulkCopyOptions.Default;
                if (this.transac == null) copyoption = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction;

                SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, copyoption, this.transac);
                bulkCopy.BulkCopyTimeout = this.DefaultTimeOut; ;
                bulkCopy.DestinationTableName = dataTable.TableName;
                if (BulkCopyTrace10000)
                {
                    bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(bulkCopy_SqlRowsCopied);
                    bulkCopy.NotifyAfter = 10000;
                }
                bulkCopy.WriteToServer(dataTable);

                if (BulkCopyTrace10000) Console.WriteLine();

                return new List<long>();
            }
            catch (Exception ex)
            {
                
                ExceptionSQL(ex, "BULKINSERT");
                return null;
            }
            finally
            {
                if (transac == null) this.Close();
                this.sqlMutex.ReleaseMutex();
                //if (base.LogManager != null) base.LogManager.AddLogs(elog);
            }

        }




        private void ExceptionSQL(Exception e, string sqlQuery, int QueryTentative=0)
        {
            /*
            if (fiable && transac == null && !retentebug && retrypossible(e.Message)) //si erreur de base on retente
            {
                retentebug = true;
                System.Threading.Thread.Sleep(9000); //après une petite pause biensur
                return query(sql, paramvalues);
            }
            else */
            throw new Exception("[" + QueryTentative.ToString() + "]SQL =[" + sqlQuery + "]" + e.Message, e);

        }


        /// <summary>
        /// Permet de savoir si l'erreur permet de rententer la transaction ou pas
        /// </summary>
        /// <param name="messageexeption"></param>
        /// <returns></returns>
        private bool retrypossible(string messageexeption)
        {
            if (messageexeption.Contains("a été bloquée sur les ressources lock par un autre processus")) return true;
            if (messageexeption.Contains("erreur s'est produite lors de l'établissement d'une connexion au serveur")) return true;
            if (messageexeption.Contains("The timeout period elapsed prior to completion of the operation")) return true;
            // The server was not found or was not accessible
            return false;
        }



        public override long Insert(string table, Dictionary<string, object> parametersValues, string AutoIncrementColumn = null)
        {
            this.QueryScalar(CONNECTOR.SqlTools.GenerateInsertSQL(table, parametersValues.Keys.ToArray()), parametersValues);
            int lastinsertid = 0;
            if (!string.IsNullOrEmpty(AutoIncrementColumn))
            {
                object ret = this.QueryScalar("SELECT IDENT_CURRENT('" + table + "') as nb", null);
                lastinsertid = Convert.ToInt32(ret);
            }
            return lastinsertid;
        }

        protected virtual void bulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        { Console.Write("."); }



        public bool ExistTable(string tableDataName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableDataName)) throw new Exception("tableDataName invalide");
                string sql = "select OBJECT_ID('" + tableDataName + "') as oid";
                //ins.setstring("tableDataName", tableDataName);
                System.Data.DataTable ret = this.Query(sql, null);
                if (ret.Rows.Count < 1 && ret.Rows[0][0] == null || ret.Rows[0][0] == DBNull.Value) return false;
                else return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public override void CreateColumn(string tableName, DataColumn col)
        {
            string format = "varchar(64)";
            if (col.DataType == typeof(string)) format = "varchar(64)";
            else if (col.DataType == typeof(long)) format = "BIGINT";
            else if (col.DataType == typeof(int)) format = "INT";
            else if (col.DataType == typeof(DateTime)) format = "DATETIME";
            else if (col.DataType == typeof(bool)) format = "BIT";
            StringBuilder query = new StringBuilder();
            query.Append("ALTER TABLE " + tableName + " ");
            query.Append("ADD " + col.ColumnName + " " + format);
            this.QueryScalar(query.ToString());
        }



        public void CreateTable(string tableName, List<System.Data.DataColumn> cols, bool temporary=false)
        {
            List<DATA.DATAVALUES.DataValues_data> retour = new List<DATA.DATAVALUES.DataValues_data>();
            foreach (System.Data.DataColumn item in cols)
            {
                DATA.DATAVALUES.DataValues_data dat = new DATA.DATAVALUES.DataValues_data();
                dat.value = item.ColumnName.Replace(" ", "");
                retour.Add(dat);
            }
            CreateTable(tableName, retour, temporary);
        }


        public void CreateTable(string tableName, List<DATA.DATAVALUES.DataValues_data> cols, bool temporary=false)
        {
            try
            {
                string defaultformat = "VARCHAR(256)";

                StringBuilder query = new StringBuilder();
                query.Append("CREATE ");
                if (temporary) query.Append("TEMPORARY ");
                query.Append("TABLE ");
                query.Append(tableName);
                query.Append(" ( ");

                string virgule = " ";
                foreach (DATA.DATAVALUES.DataValues_data col in cols)
                {
                    string name = col.ToString();
                    string format = col["format"];
                    if (string.IsNullOrWhiteSpace(format)) format = defaultformat;

                    query.Append(virgule);
                    query.Append("[" + name + "]");
                    query.Append(" ");
                    query.Append("" + format + "");

                    virgule = ",";

                }
                query.Append(")");

                this.QueryScalar(query.ToString());

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void Dispose()
        {
            this.Close();

            Console.WriteLine("Connector.Dispose()");
        }



        public override IDataConnector Clone()
        {
            MsSQLConnector retour = new MsSQLConnector();
            retour.SetConnectionString(this.ConnectionString);
            retour.DefaultTimeOut = this.DefaultTimeOut;
            return retour;
        }















        public static List<string> ObtainListPrimaryKey(DATA.CONNECTOR.IDataConnector connector, string TableName)
        {

            string sql = "SELECT Col.Column_Name FROM     INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,     INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col ";
            sql += " WHERE Col.Constraint_Name = Tab.Constraint_Name    AND Col.Table_Name = Tab.Table_Name    AND Constraint_Type = 'PRIMARY KEY'    AND Col.Table_Name = '" + TableName + "'";

            System.Data.DataTable retPrimKey = connector.Query(sql, null);
            List<string> retour = new List<string>();
            foreach (System.Data.DataRow item in retPrimKey.Rows)
            {
                string primaryJey = Convert.ToString(item[0]);
                if (!string.IsNullOrWhiteSpace(primaryJey)) retour.Add(primaryJey);
            }
            return retour;
        }











    }
}
