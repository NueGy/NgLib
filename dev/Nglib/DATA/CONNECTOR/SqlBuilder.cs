using Nglib.DATA.COLLECTIONS;
using Nglib.DATA.CONNECTOR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Permet de composer une requete SQL
    /// </summary>
    public class SqlBuilder
    {


        public SqlBuilder(string tablename, ConnectorConstants.ConnectorEngineEnum  SqlEngine = ConnectorConstants.ConnectorEngineEnum.POSTGRESQL)
        {
            this.SqlEngine = SqlEngine; /// ConnectorConstants.ConnectorEngineEnum.POSTGRESQL; // par default
            sqlCommandType = SqlCommandTypeEnum.SELECT;
            this.TableName = tablename;
        }


        /// <summary>
        /// Nom de la table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Limitation
        /// </summary>
        public int LimitResults { get; set; }
        //public int OffsetResults { get; set; }

        /// <summary>
        /// Liste des paramètres, données INPUT
        /// </summary>
        public Dictionary<string, object> SqlInputParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Nom du SGBD
        /// </summary>
        public ConnectorConstants.ConnectorEngineEnum SqlEngine { get; set; }

        /// <summary>
        /// Type de commande SELECT
        /// </summary>
        public SqlCommandTypeEnum sqlCommandType { get; set; }



        /// <summary>
        /// Données pour SELECT/ Insert / Update
        /// </summary>
        public List<string> SqlFieldNames { get; set; } = new List<string>();



        /// <summary>
        /// Liste des conditions Where
        /// </summary>
        public List<string> WhereClauses { get; set; } = new List<string>();


        /// <summary>
        /// Order by clause
        /// </summary>
        public List<string> OrderClauses { get; set; } = new List<string>();

        /// <summary>
        /// Group By Clause
        /// </summary>
        public List<string> GroupClauses { get; set; } = new List<string>();


        ///// <summary>
        ///// Forcer les columnName en miniscule
        ///// </summary>
        //public bool ForceToLowerColumnName { get; set; } = true;

        ///// <summary>
        ///// Order by clause
        ///// </summary>
        ////public List<string> OrderClause = new List<string>();
        ////public Dictionary<string, string[]> Joins = new Dictionary<string, string[]>();





        /// <summary>
        /// Prépation d'une requette SELECT
        /// </summary>
        /// <param name="fields">Liste des champs, laisser vide pour * </param>
        /// <returns></returns>
        public SqlBuilder Select(params string[] fields)
        {
            this.sqlCommandType = SqlCommandTypeEnum.SELECT;
            if (fields != null) this.SqlFieldNames = fields.ToList();
            return this;
        }


        /// <summary>
        /// Mise à jour de données en base
        /// </summary>
        /// <param name="updateValues"></param>
        /// <returns></returns>
        public SqlBuilder Update(Dictionary<string, object> updateValues)
        {
            this.sqlCommandType = SqlCommandTypeEnum.UPDATE;
            this.SqlFieldNames = updateValues.Select(di => di.Key).ToList();
            this.SqlInputParameters.AddRange(updateValues, false);
            return this;
        }


        /// <summary>
        /// Supprimer des données
        /// L'utilisation de AddWhere sera obligatoire
        /// </summary>
        /// <returns></returns>
        public SqlBuilder Delete()
        {
            this.sqlCommandType = SqlCommandTypeEnum.DELETE;
            return this;
        }

        /// <summary>
        /// Insert data
        /// </summary>
        /// <returns></returns>
        public SqlBuilder Insert(Dictionary<string, object> insertValues)
        {
            this.sqlCommandType = SqlCommandTypeEnum.INSERT;
            this.SqlFieldNames = insertValues.Select(di => di.Key).ToList();
            this.SqlInputParameters.AddRange(insertValues, false);
            return this;
        }


        ///// <summary>
        ///// Jointures
        ///// </summary>
        //public Dictionary<string, SqlQueryBuilder> SqlJoins = new Dictionary<string, SqlQueryBuilder>();

        //internal SqlJoinTypeEnum SubJoin { get; set; }


        //public SqlQueryBuilder AddInnerJoin(string joinName, SqlQueryBuilder joinSql)
        //{
        //    return AddJoin(joinName, SqlJoinTypeEnum.INNER, joinSql);
        //}


        //public SqlQueryBuilder AddJoin(string joinName, SqlJoinTypeEnum joinType, SqlQueryBuilder joinSql)
        //{
        //    joinSql.SubJoin = joinType;
        //    SqlJoins.AddOrReplace(joinName, joinSql);
        //    return this;
        //}






        /// <summary>
        /// 
        /// </summary>
        /// <param name="parametername"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public SqlBuilder AddWhere(string parametername, object parameterValue)
        {
            return AddWhereClause($"{parametername}=@{parametername}", new Dictionary<string, object>() { { parametername, parameterValue } });
        }

        public SqlBuilder AddWhereBetween(string parametername, DateTime? DateMin, DateTime? DateMax)
        {

            if (!DateMin.HasValue && !DateMax.HasValue) return this;
            //if(DateMin.HasValue && DateMin.Value.Year<1900) 

            if (DateMin.HasValue && !DateMax.HasValue)
            {
                this.WhereClauses.Add(string.Format(" {0} >= @{0}Min ", parametername));
                this.SqlInputParameters.Add(parametername + "Min", DateMin.Value);
            }
            else if (DateMax.HasValue && !DateMin.HasValue)
            {
                this.WhereClauses.Add(string.Format(" {0} <= @{0}Max ", parametername));
                this.SqlInputParameters.Add(parametername + "Max", DateMax.Value);
            }
            else if (DateMin.HasValue && DateMax.HasValue)
            {
                this.WhereClauses.Add(string.Format(" {0} BETWEEN @{0}Min AND @{0}Max ", parametername));

                this.SqlInputParameters.Add(parametername + "Min", DateMin.Value);
                this.SqlInputParameters.Add(parametername + "Max", DateMax.Value);

            }
            return this;
        }


        public SqlBuilder AddWheres(Dictionary<string,object> wheresValues)
        {
            if(wheresValues!=null)
                wheresValues.ForEach(di => AddWhere(di.Key, di.Value));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlpart"></param>
        /// <param name="parametername"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public SqlBuilder AddWhereClause(string sqlpart, string parametername, object parameterValue)
        {
            Dictionary<string, object> Parameters = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(parametername)) Parameters.Add(parametername, parameterValue);
            return AddWhereClause(sqlpart, Parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlpart"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public SqlBuilder AddWhereClause(string sqlpart, Dictionary<string, object> Parameters)
        {
            if (string.IsNullOrWhiteSpace(sqlpart)) return this;
            this.WhereClauses.Add(sqlpart);
            if(Parameters!=null)
                this.SqlInputParameters.AddRange(Parameters);
            return this;
        }


        /// <summary>
        /// Ajoute la clause ORDER BY
        /// </summary>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public SqlBuilder OrderBy(params string[] orderby)
        {
            if (orderby != null) this.OrderClauses = orderby.ToList();
            return this;
        }

        /// <summary>
        /// Ajoute la clause GROUP BY
        /// </summary>
        /// <param name="groupby"></param>
        /// <returns></returns>
        public SqlBuilder GroupBy(params string[] groupby)
        {
            if (groupby != null) this.GroupClauses = groupby.ToList();
            if (SqlFieldNames.Count == 0) SqlFieldNames = this.GroupClauses; 
            return this;
        }


        public SqlBuilder Limit(int limitResult, int offsetResult=0)
        {
            this.LimitResults = limitResult;
            if (offsetResult != 0) ;
            return this;
        }



        //public SqlBuilder AddFormModel(MODELS.BASICS.ISearchForm searchForm)
        //{
        //    this.LimitResults = searchForm.LimitResults;
        //}




















        /// <summary>
        /// COMPOSE SQL STRING
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                return ComposeSQL(this);
            }
            catch (Exception ex)
            {
                return "";
            }
        }


   

        public static string ComposeSQL(SqlBuilder query)
        {
            try
            {
                // ------ CONTROLS --------
                if (string.IsNullOrWhiteSpace(query.TableName)) throw new Exception("TableName Empty");



                StringBuilder retoursql = new StringBuilder();

                // ------ SELECT PART --------
                if(query.sqlCommandType == SqlCommandTypeEnum.SELECT)
                {
                    retoursql.Append("SELECT ");
                    if (query.SqlEngine == ConnectorConstants.ConnectorEngineEnum.MSSQL && query.LimitResults > 0) retoursql.Append($"TOP {query.LimitResults} ");
                    if (query.SqlFieldNames.Count == 0) retoursql.Append(" * ");
                    else retoursql.Append(string.Join(" , ", query.SqlFieldNames) + " ");
                }
                else if (query.sqlCommandType == SqlCommandTypeEnum.UPDATE)
                {
                    if (query.SqlFieldNames.Count == 0) throw new Exception("No Update Values");
                    if(query.WhereClauses.Count == 0) throw new Exception("WhereClauses Required for UPDATE");
                    retoursql.Append($"UPDATE {query.TableName} SET "+ string.Join(" , ",query.SqlFieldNames.Select(valk => valk + "=@" + valk))+" " );
                }
                else if (query.sqlCommandType == SqlCommandTypeEnum.DELETE)
                {
                    if (query.WhereClauses.Count == 0) throw new Exception("WhereClauses Required for DELETE");
                    retoursql.Append($"DELETE ");
                }
                else if (query.sqlCommandType == SqlCommandTypeEnum.INSERT)
                {
                    if (query.SqlFieldNames.Count == 0) throw new Exception("No Insert Values");
                    retoursql.Append($"INSERT INTO {query.TableName} ");
                    retoursql.Append("( "+string.Join(" , ", query.SqlFieldNames) + ") ");
                    retoursql.AppendLine();
                    retoursql.Append("VALUES ( " + string.Join(" , ", query.SqlFieldNames.Select(valk=> "@"+valk)) + ") ");
                }


                // ------ FROM PART --------
                if (query.sqlCommandType == SqlCommandTypeEnum.SELECT || query.sqlCommandType == SqlCommandTypeEnum.DELETE)
                {
                    retoursql.AppendLine();
                    retoursql.Append($"FROM { query.TableName} ");
                }
                    

                // ------ JOINS PART --------

                //query.SqlJoins.ForEach(sqljoin => { 
                //    if(sqljoin.Value.WhereClauses.Count>0 || sqljoin.Value.SelectClauses.Count>0)
                //        retoursql.AppendLine($"{sqljoin.Value.SubJoin} JOIN "+ "{" + sqljoin.Value.ToString() + "}"+ $" AS {sqljoin.Key}");
                //    else if(!string.IsNullOrWhiteSpace(sqljoin.Value.TableName)) 
                //        retoursql.AppendLine($"{sqljoin.Value.SubJoin} JOIN {sqljoin.Value.TableName} AS {sqljoin.Key}");
                //});


                // ------ WHERE PART --------
                if (query.WhereClauses.Count > 0)
                {
                    retoursql.AppendLine();
                    retoursql.Append("WHERE " + string.Join(" AND ", query.WhereClauses) + " ");
                }

                // ------ GROUP PART --------
                if (query.GroupClauses.Count > 0)
                {
                    if (query.sqlCommandType != SqlCommandTypeEnum.SELECT) throw new Exception("GroupClauses only valid for SELECT");
                    retoursql.AppendLine();
                    retoursql.Append("GROUP BY " + string.Join(" , ", query.GroupClauses) + " ");
                }

                // ------ ORDER PART --------
                if (query.OrderClauses.Count > 0)
                {
                    if (query.sqlCommandType != SqlCommandTypeEnum.SELECT) throw new Exception("OrderClauses only valid for SELECT");
                    retoursql.AppendLine();
                    retoursql.Append("ORDER BY " + string.Join(" , ", query.OrderClauses) + " ");
                }
                   
                if (query.LimitResults > 0 && query.sqlCommandType == SqlCommandTypeEnum.SELECT)
                {
                    retoursql.AppendLine();
                    if (query.SqlEngine != ConnectorConstants.ConnectorEngineEnum.MSSQL)
                        retoursql.Append($"LIMIT {query.LimitResults} ");
                }

                // ------ END --------
                retoursql.AppendLine(";");
                return retoursql.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("SqlQueryBuilder " + ex.Message, ex);
            }
        }



    }
}
