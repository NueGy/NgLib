using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Context object that carries all parameters for formula processing.
    /// Documentation: <see href="https://github.com/NueGy/NgLibComponents/wiki/wiki_components_formula"/>
    /// </summary>
    public class FormulaContext
    {

        /// <summary>
        /// Calculation options (Development in progress...)
        /// </summary>
        [Obsolete("SOON")]
        public FormulaOptionModel Option { get; set; } = new FormulaOptionModel();


        /// <summary>
        /// Segmented formula tree
        /// </summary>
        public FormulaSegmentModel RootSegment { get; set; }


        /// <summary>
        /// Calculated values for each segment
        /// </summary>
        public Dictionary<FormulaSegmentModel, object> CalcValues { get; private set; } = new Dictionary<FormulaSegmentModel, object>();



        /// <summary>
        /// Parameters for the formula (will only be in the first context)
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }


        /// <summary>
        /// Formula result (directly if constants, otherwise after calculation)
        /// </summary>
        public object ResultValue { get; set; }


        /// <summary>
        /// Formula execution time in milliseconds
        /// </summary>
        public long ElapsedTime { get; set; }



 





        public override string ToString()
        {
            return $"{RootSegment?.ToString()}";
        }
    }
}
