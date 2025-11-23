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
        public QueryContext()
        {
            this.Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// COntext SQL
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        public QueryContext(string sqlQuery, Dictionary<string, object> parameters=null)
        {
            if (string.IsNullOrWhiteSpace(sqlQuery)) 
                throw new ArgumentNullException(nameof(sqlQuery), "La requête SQL ne peut pas être vide");
            this.SqlQuery = sqlQuery;
            this.Parameters = parameters ?? new Dictionary<string, object>();
        }


        /// <summary>
        /// Temps de chargement de la requette (Hors Open/Close)
        /// </summary>
        public System.Diagnostics.Stopwatch watchExecute { get; }  = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Temp total incluant le OPEN/Close
        /// </summary>
        public System.Diagnostics.Stopwatch watchAll { get; }  = new System.Diagnostics.Stopwatch();

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
        public string SqlQuery { get; set; }

        /// <summary>
        /// Parametres
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Erreur retour eventuel
        /// </summary>
        public string Error { get; set; }



        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SqlQuery)) throw new Exception("sqlQuery is empty");
        }



        public override string ToString()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Error))
                    return string.Format("[{2}--error] {1}", "", this.SqlQuery, InitDate.ToString("HH:mm:ss:ff"));
                else
                    return string.Format("[{2}--{0}/{3}ms] {1}", this.watchExecute.ElapsedMilliseconds, this.SqlQuery, InitDate.ToString("HH:mm:ss:ff"), this.watchAll.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                return base.ToString();
            }
        }

    }
}
