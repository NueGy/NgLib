using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.BASICS
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModelConfigAttribute : Attribute
    {

        public ModelConfigAttribute(string ServicePartUrl)
        {
            this.ApiPartUrl = ServicePartUrl;
        }

         
        /// <summary>
        /// Url du webservice
        /// </summary>
        public string ApiPartUrl { get; set; }

        /// <summary>
        /// Nom du model/singulier
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Icone du model
        /// </summary>
        public string Ico { get; set; }
 

        public Dictionary<string, string> PageBreadcrums { get; set; }

    }
  
}
