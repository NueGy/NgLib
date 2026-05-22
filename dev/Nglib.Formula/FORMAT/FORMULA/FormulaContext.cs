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
        /// Gets a parameter value by name, case-insensitive
        /// </summary>
        public object GetParameterValue(string name)
        {
            if (Parameters == null) return null;
            var key = Parameters.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
            return key != null ? Parameters[key] : null;
        }

        /// <summary>
        /// Returns true if the parameter exists, case-insensitive
        /// </summary>
        public bool HasParameter(string name)
        {
            if (Parameters == null) return false;
            return Parameters.Keys.Any(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


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
