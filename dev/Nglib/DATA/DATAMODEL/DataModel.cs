using System;
using System.Collections.Generic;
using Nglib.DATA.BASICS;

namespace Nglib.DATA.DATAMODEL
{
    /// <summary>
    ///     Un objet de données complexe pour une manipulation poussé avec un front(webapp)
    ///     Permet d'avoir un model de données plus complet pour gérer une génération automatique du formulaire html
    /// </summary>
    /// <typeparam name="Tmodel"></typeparam>
    [Obsolete("SOON")]
    public class DataModel<Tmodel> : IDataModel where Tmodel : IModel, new()
    {
        /// <summary>
        ///     Les données
        /// </summary>
        public Tmodel Model { get; set; }

        /// <summary>
        ///     Donées suplémentaires
        /// </summary>
        public List<ModelValue> InfoValues { get; set; } = new();


        /// <summary>
        ///     Pour la génération du formulaire
        /// </summary>
        public List<ModelValue> FormValues { get; set; } = new();
    }
}