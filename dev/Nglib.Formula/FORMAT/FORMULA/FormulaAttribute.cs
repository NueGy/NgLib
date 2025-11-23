//Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Pour indiquer qu'une methode statique est une fonction de formule
    /// </summary>
    public class FormulaAttribute : Attribute
    {
        /// <summary>
        /// Nom/commande UNIQUE
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Groupe, utilsera le nom de la classe si null
        /// </summary>
        public string Groupe { get; set; }

        /// <summary>
        /// Nombre d'arguments minimum requis pour cette fonction
        /// </summary>
        public int ReqArgs { get; set; }

        /// <summary>
        /// Description facultative
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Description facultative
        /// </summary>
        public string UseExample { get; set; }

        /// <summary>
        /// version
        /// </summary>
        public string Ver { get; set; }

        /// <summary>
        /// Indique qu'il faut passer le context à la methode
        /// </summary>
        public MethodModeEnum MethodMode { get; set; } = MethodModeEnum.StringArray;


        public FormulaAttribute(string name, int reqArgs, string description)
        {
            Name = name.ToLower();
            ReqArgs = reqArgs;
            Description = description;
        }
        public FormulaAttribute(string name, int reqArgs)
        {
            Name = name.ToLower();
            ReqArgs = reqArgs;
        }
        public FormulaAttribute(string name)
        {
            Name = name.ToLower();
        }

        public override string ToString()
        {
            return $"{Name}";
        }

    }
}
