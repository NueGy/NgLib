using System;
using System.Collections.Generic;

namespace Nglib.DATA.DATAMODEL
{
    /// <summary>
    ///     permet de Configurer un model
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataModelConfigAttribute : Attribute
    {
        public DataModelConfigAttribute(string ServicePartUrl)
        {
            ApiPartUrl = ServicePartUrl;
        }


        /// <summary>
        ///     Url du webservice
        /// </summary>
        public string ApiPartUrl { get; set; }

        /// <summary>
        ///     Nom du model/singulier
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        ///     Icone du model
        /// </summary>
        public string Ico { get; set; }

        /// <summary>
        ///     Definir le fil d'arriane
        /// </summary>
        public Dictionary<string, string> PageBreadcrums { get; set; }

        /// <summary>
        ///     Le type du wrapper api à utiliser pour realiser les webservices
        /// </summary>
        public Type WrapperApiType { get; set; }


        /// <summary>
        ///     Le type du front manager si il y en as un
        /// </summary>
        public Type FrontManagerType { get; set; }

        /// <summary>
        ///     Chargera automatiquement le type du frontManager par reflexion lors du chargement (soon)
        /// </summary>
        public string FrontManagerTypeName { get; set; }


        /// <summary>
        ///     Utilisation de Nglib.MODELS.BASICS.EditModelComplex
        /// </summary>
        public bool UseEditModelComplex { get; set; }


        /// <summary>
        ///     Utilise xx/search[POST]
        /// </summary>
        public bool UseCustomSearchEndpoint { get; set; }


        /// <summary>
        ///     Configuration de l'objet pour un affichage dynamique du tableau de recherche
        /// </summary>
        public object FrontResultsConfig { get; set; }

        /// <summary>
        ///     Configuration de l'objet pour un affichage dynamique de la page de consultation/edit
        /// </summary>
        public object FrontEditConfig { get; set; }

        /// <summary>
        ///     Regles de formatage des valeurs
        /// </summary>
        public List<ModelValue> FormatValues { get; set; }
    }
}