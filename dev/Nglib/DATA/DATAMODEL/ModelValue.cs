using System.Collections.Generic;

namespace Nglib.DATA.DATAMODEL
{
    /// <summary>
    ///     Utile pour générer des formulaire Html
    /// </summary>
    public class ModelValue
    {
        /// <summary>
        ///     Nom de la valeur (CLEF)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Libelle de la valeur
        /// </summary>
        public string Label { get; set; }


        /// <summary>
        ///     Titre, permet l'affichage d'un title dans les rendus HTML
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        ///     PlaceHolder, permet l'affichage d'un PlaceHolder dans les rendus HTML
        /// </summary>
        public string PlaceHolder { get; set; }

        /// <summary>
        ///     Permet permet l'affichage d'un icon dans les rendus HTML
        /// </summary>
        public string Ico { get; set; }

        /// <summary>
        ///     Type de la valeur
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     VALEUR
        /// </summary>
        public string Value { get; set; }


        public string DefaultValue { get; set; }

        /// <summary>
        ///     Indiquer si la valeur à été modifié
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        ///     Sera affiché dans la vue edit principale
        /// </summary>
        public bool Important { get; set; }


        /// <summary>
        ///     Style suplémentaire dans le rendu html
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        ///     Style suplémentaire dans le rendu html
        /// </summary>
        public string StyleClass { get; set; }

        /// <summary>
        ///     valeurs possibles
        /// </summary>
        public Dictionary<string, string> PossibleValues { get; set; }

        public Dictionary<string, string> ValueOptions { get; set; }

        /// <summary>
        ///     Regex de controle
        /// </summary>
        public string Regex { get; set; }


        public bool Required { get; set; }

        // !!! public object Value { get; set; }


        /// <summary>
        ///     Groupe pour affichage distinct
        /// </summary>
        public string Group { get; set; }


        public string GetTypeValue()
        {
            return Type == null ? "" : Type.ToUpper().Split(':')[0];
        }
    }
}