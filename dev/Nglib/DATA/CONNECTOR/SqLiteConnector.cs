using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace Nglib.COMPONENTS.CONNECTOR
{
    public class SqLiteConnector : BaseDataConnector, IDataConnector
    {
        private System.Data.SQLite.SQLiteConnection conn = null;
        private System.Data.SQLite.SQLiteTransaction transac = null;

        public bool DoNotCloseAutomaticaly = false;

        public override bool Open()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString)) throw new Exception("Chaine de connexion SQL vide ...");
            if (this.ConnectionString.Contains("{BaseDirectory}")) this.ConnectionString = this.ConnectionString.Replace("{BaseDirectory}", AppDomain.CurrentDomain.BaseDirectory);
            SQLiteConnectionStringBuilder nchainbuild = new SQLiteConnectionStringBuilder(this.ConnectionString);
            conn = new SQLiteConnection(nchainbuild.ToString());
            conn.Open();
            //Data Source=c:\mydb.db;Version=3;
            //Data Source=:memory:;Version=3;New=True;
            //Data Source=c:\mydb.db;Version=3;Password=myPassword;

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
            if (conn == null) this.Open();
            if (this.transac != null) return false; // postgres authorise qu'une transaction
            transac = conn.BeginTransaction();
            return true;
        }

        public override bool CommitTransaction(string transactionName = null)
        {
            if (conn == null) return false;
            if (transac == null) return false;
            transac.Commit();
            transac.Dispose();
            transac = null;
            return true;
        }



        public override bool RollBackTransaction(string transactionName = null)
        {
            if (conn == null) return false;
            if (transac == null) return false;
            transac.Rollback();
            transac.Dispose();
            transac = null;
            return true;
        }








        public SQLiteCommand InitCommand(string sql, Dictionary<string, object> parameters)
        {

            if (conn == null) this.Open();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(this.conn);
                cmd.CommandTimeout = this.DefaultTimeOut;

                if (DataConnectorTools.IsSQLQuery(sql)) cmd.CommandType = System.Data.CommandType.Text;
                else cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = sql;
                List<string> paramneed = new List<string>();
                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(sql.ToLower() + " ", "(\\@\\w+)"))
                    paramneed.Add(match.Groups[1].Value);

                if (parameters != null)
                    foreach (string fieldKey in parameters.Keys)
                    {
                        if (cmd.CommandType == System.Data.CommandType.Text) // envoyer que les params demandés
                            if (!paramneed.Contains("@" + fieldKey.ToLower())) continue; // innutile

                        object obj = parameters[fieldKey];
                        //if(obj is bool) // convertion de type
                        //{ 
                        //    int iobj = 0;
                        //    if((bool)obj)iobj=1;
                        //    obj= iobj;
                        //}
                        // if (itemd.IsDynamic && !itemd.IsTrue("notransform") && itemd.NameMinimal.ToLower() != "fluxxml") itemd.value = itemd.Transformation();
                        cmd.Parameters.AddWithValue("@" + fieldKey, obj);
                    }


                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception("Initialisation SQL", ex);
            }
        }


        

        protected override System.Data.DataTable QueryExec(QueryContext query)
        {
            SQLiteDataReader read = null;
            SQLiteCommand cmd = null;
            SQLiteDataAdapter adapter = null;
            SQLiteTransaction t = null;
            try
            {
                this.sqlMutex.WaitOne();
                cmd = this.InitCommand(query.sqlQuery, query.parameters);
                if (cmd.CommandType == CommandType.StoredProcedure && this.transac == null)
                    t = conn.BeginTransaction();

                System.Data.DataTable datas = new System.Data.DataTable();
                read = cmd.ExecuteReader();
                datas.Load(read);
                read.Close();
                read = null;
                cmd.Dispose();
                if(t!=null)t.Commit();
                return datas;
            }
            catch
            {
                if (t != null) t.Rollback();
                throw;
            }
            finally
            {
                if (transac == null && !DoNotCloseAutomaticaly) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }





        protected override System.Data.DataSet QueryDataSetExec(QueryContext query)
        {
            SQLiteDataReader read = null;
            SQLiteCommand cmd = null;
            SQLiteDataAdapter adapter = null;
            SQLiteTransaction t = null;
            try
            {
                this.sqlMutex.WaitOne();
                cmd = this.InitCommand(query.sqlQuery, query.parameters);
                if (cmd.CommandType == CommandType.StoredProcedure && this.transac == null)
                    t = conn.BeginTransaction();

                System.Data.DataSet datas = new System.Data.DataSet();
                adapter = new SQLiteDataAdapter(cmd);
                adapter.Fill(datas);
                read = null;
                cmd.Dispose();
                if (t != null) t.Commit();
                return datas;
            }
            catch
            {
                if (t != null) t.Rollback();
                throw;
            }
            finally
            {
                if (transac == null && !DoNotCloseAutomaticaly) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }






        protected override object QueryScalarExec(QueryContext query)
        {
            SQLiteDataReader read = null;
            SQLiteCommand cmd = null;
            try
            {
                this.sqlMutex.WaitOne();
                cmd = this.InitCommand(query.sqlQuery, query.parameters);
                object retourobj = cmd.ExecuteScalar();
                cmd.Dispose();
                return retourobj;
            }
            finally
            {
                if (transac == null && !DoNotCloseAutomaticaly) this.Close();
                this.sqlMutex.ReleaseMutex();
            }
        }




        public override long Insert(string table, Dictionary<string, object> parametersValues, string AutoIncrementColumn = null)
        {
            bool istransac = false;
            try
            {
                istransac=this.BeginTransaction("insrt1");
                this.QueryScalar(CONNECTOR.SqlTools.GenerateInsertSQL(table, parametersValues.Keys.ToArray()), parametersValues);
                int lastinsertid = 0;
                if (!string.IsNullOrEmpty(AutoIncrementColumn))
                {
                    object ret = this.QueryScalar("SELECT last_insert_rowid()", null);
                    lastinsertid = Convert.ToInt32(ret);
                }
                if (istransac) this.CommitTransaction("insrt1");
                return lastinsertid;

            }
            catch (Exception)
            {
                if (istransac) this.RollBackTransaction("insrt1");
                throw;
            }
        }


        //protected virtual void bulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        //{ Console.Write("."); }



        public bool ExistTable(string tableDataName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableDataName)) throw new Exception("tableDataName invalide");
                string sql = string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}';",tableDataName);
                //eyesLib3.Encaissement.DATA.DATAVALUES.DataValues ins = new SeyesLib3.Encaissement.DATA.DATAVALUES.DataValues();
                //ins.setstring("tableDataName", tableDataName);
                System.Data.DataTable ret = this.Query(sql, null);
                if (ret.Rows.Count==0 || ret.Rows[0][0] == null || ret.Rows[0][0] == DBNull.Value) return false;
                else return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public override void CreateColumn(string tableName, System.Data.DataColumn col)
        {
            string format = "varchar(64)";
            if (col.DataType == typeof(string)) format = "varchar(64)";
            else if (col.DataType == typeof(long)) format = "BIGINT";
            else if (col.DataType == typeof(int)) format = "INT";
            else if (col.DataType == typeof(DateTime)) format = "DATE";
            else if (col.DataType == typeof(bool)) format = "BIT";
            StringBuilder query = new StringBuilder();
            query.Append("ALTER TABLE " + tableName + " ");
            query.Append("ADD " + col.ColumnName + " " + format);
            this.QueryScalar(query.ToString());
        }



        public void CreateTable(string tableName, List<System.Data.DataColumn> cols, bool temporary = false)
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


        public void CreateTable(string tableName, List<DATA.DATAVALUES.DataValues_data> cols, bool temporary = false)
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



    }
}
