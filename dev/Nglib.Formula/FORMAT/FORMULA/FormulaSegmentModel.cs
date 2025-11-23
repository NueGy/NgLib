using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Segment d'une formule
    /// </summary>
    public class FormulaSegmentModel
    {
        /// <summary>
        /// Type de token
        /// </summary>
        public FormulaSegmentTypeEnum SegmentType { get; set; }

        /// <summary>
        /// Texte du token
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Position du token dans la formula d'origine
        /// </summary>
        public int Position { get; set; }


        /// <summary>
        /// Segments enfants (si parenthèses)
        /// </summary>
        public List<FormulaSegmentModel> SubSegments { get; set; } = new List<FormulaSegmentModel>();


        public override string ToString()
        {
            return $"{Text}";
        }

    }
}
