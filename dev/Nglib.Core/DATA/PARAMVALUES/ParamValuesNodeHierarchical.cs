using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.PARAMVALUES
{
    public class ParamValuesNodeHierarchical 
    {
        /// <summary>
        /// Chemin complet
        /// </summary>
        public string NodePath { get; set; }

        /// <summary>
        /// Nom du node
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Noeud avec une valeur
        /// </summary>
        public ParamValuesNode ValueNode { get; set; }


        /// <summary>
        /// Sous noeuds (sans valeur)
        /// </summary>
        public List<ParamValuesNodeHierarchical> ChildrenNodes { get; set; } = new List<ParamValuesNodeHierarchical>();

        public override string ToString()
        {
            return this.NodePath;
        }

    }
}
