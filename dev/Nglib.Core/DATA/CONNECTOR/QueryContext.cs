using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{
    public delegate void QueryCompletedHandler(QueryContext queryContext); //object sender,

    /// <summary>
    /// Contient toutes les informations pour le suivis de la requette SQL
    /// </summary>
    public class QueryContext
    {
        /// <summary>
        /// Context SQL
        /// </summary>
        public QueryContext() { this.parameters = new Dictionary<string, object>(); }

        /// <summary>
        /// COntext SQL
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        public QueryContext(string sqlQuery, Dictionary<string, object> parameters)
        {
            this.sqlQuery = sqlQuery;
            this.parameters = parameters;
            if (this.parameters == null) this.parameters = new Dictionary<string, object>();
        }

        ///// <summary>
        ///// Context SQl
        ///// </summary>
        ///// <param name="sqlQuery">Requette SQL</param>
        ///// <param name="parampx">parametres @p1, @p2, @px</param>
        //public QueryContext(string sqlQuery, params object[] parampx)
        //{
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    int ii = 1;
        //    if (parampx != null)
        //        foreach (var item in parampx)
        //        {
        //            parameters.Add("p" + ii, item);
        //            ii++;
        //        }

        //    this.sqlQuery = sqlQuery;
        //    this.parameters = parameters;
        //}

        /// <summary>
        /// Nombre de tentatives
        /// </summary>
        public int QueryTry { get; set; }

        /// <summary>
        /// Temps de chargement de la requette (Hors Open/Close)
        /// </summary>
        public System.Diagnostics.Stopwatch watchExecute = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Temp total incluant le OPEN/Close
        /// </summary>
        public System.Diagnostics.Stopwatch watchAll = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Date d'initialisation de la requette
        /// </summary>
        public DateTime InitDate = DateTime.Now;

        /// <summary>
        /// Date ou le requette à commencer à etre executé
        /// </summary>
        public DateTime? ExecuteDate { get; set; } 

        /// <summary>
        /// Requette SQl complete
        /// </summary>
        public string sqlQuery { get; set; }

        /// <summary>
        /// Parametres
        /// </summary>
        public Dictionary<string, object> parameters { get; set; }

        /// <summary>
        /// Erreur retour eventuel
        /// </summary>
        public string error { get; set; }



        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(sqlQuery)) throw new Exception("sqlQuery is empty");
        }



        public override string ToString()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(error))
                    return string.Format("[{2}--error] {1}", "", this.sqlQuery, InitDate.ToString("HH:mm:ss:ff"));
                else
                    return string.Format("[{2}--{0}/{3}ms] {1}", this.watchExecute.ElapsedMilliseconds, this.sqlQuery, InitDate.ToString("HH:mm:ss:ff"), this.watchAll.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                return base.ToString();
            }
        }

    }
}
