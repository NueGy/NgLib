using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Nglib.DATA.BASICS
{
    /// <summary>
    /// Valeur de validation
    /// </summary>
    [Obsolete("BETA")]
    public struct ValidateModel
    {

        //public ValidateModel()
        //{
        //    this.Notes = new List<string>();
        //    this.MaxlevelError = 0;
        //}

        /// <summary>
        ///     Validation réussi
        ///     Default value= TRUE , Il faudra prouver le contraire
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!_IsValid.HasValue) _IsValid = true;
                return _IsValid.Value;
            }
            set => _IsValid = value;
        }
        private bool? _IsValid { get; set; }

        /// <summary>
        /// Message de validation (Erreur la plus haute)
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Informations complémentaires
        /// </summary>
        public List<string> Notes { get; private set; }

        /// <summary>
        /// Information de groupe pour le log (libre)
        /// </summary>
        public string LogCode { get; set; }  

        /// <summary>
        /// Niveau d'erreur de la plus haute erreur
        /// </summary>
        public int MaxlevelError { get; set; }


        public override string ToString()
        {
            var retour = $"{this.IsValid}({this.LogCode}):{this.ValidationMessage}";
            //if (Notes != null && this.Notes.Count > 0)
            //    retour += $"{Nglib.FORMAT.StringTools.Limit(this.Notes.FirstOrDefault(), 128)}";
            return retour;
        }



        /// <summary>
        /// Ajouter une note pour analyser la validation
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="changeValidState">Besoin de changer le statut de validation</param>
        public void AddNote(string msg, bool? changeValidState=null)
        {
            if (Notes == null) Notes = new List<string>();
            if(!string.IsNullOrEmpty(msg))Notes.Add(msg);
            if (changeValidState.HasValue) IsValid = changeValidState.Value;
        }

        /// <summary>
        /// Ajouter une erreur, note pour analyser la validation
        /// </summary>
        /// <param name="noteMessage">Message de l'erreur/note</param>
        /// <param name="maxlevelError">Niveau de l'erreur, On gardera la plus elevé</param>
        public void AddError(string noteMessage = null, int maxlevelError = 0)
        {
            IsValid = false;
            this.AddNote(noteMessage);
            if (this.ValidationMessage==null) this.ValidationMessage = noteMessage; //remplace si vide
            if (maxlevelError > 0 && maxlevelError > this.MaxlevelError)
            {
                if (noteMessage != null) this.ValidationMessage = noteMessage;   
                this.MaxlevelError = maxlevelError;
            }
        }
        public void AddError(Exception ex, int maxlevelError = 50) => this.AddError(ex.Message, maxlevelError);
    

        private ValidateModel Return(bool isValid, string noteMessage = null, int maxlevelError = 0)
        {
            IsValid = isValid;
            this.AddNote(noteMessage);
            if (this.ValidationMessage == null) this.ValidationMessage = noteMessage; //remplace si vide
            if (maxlevelError > 0 && maxlevelError > MaxlevelError)
            {
                if (noteMessage != null) this.ValidationMessage = noteMessage;
                MaxlevelError = maxlevelError;
            }
            return this;
        }


        /// <summary>
        /// Indique que la validation est réussi
        /// </summary>
        public ValidateModel ReturnTrue()
        {
            return Return(true);
        }

        /// <summary>
        /// Indique que la validation à échoué
        /// </summary>
        public ValidateModel ReturnFalse(string addnote = null, int maxlevelError = 0)
        {
            return Return(false, addnote, maxlevelError);
        }

 
        public static ValidateModel PrepareNew(string logcode = null)
        {
            var model = new ValidateModel();
            model.Notes = new List<string>();
            model.LogCode = logcode;
            return model;
        }

        /// <summary>
        ///     Permet de s'assurer qu'il est valide sinon déclenche une Exception
        /// </summary>
        public void EnsureIsValid(string ExceptionPrefixText = null)
        {
            if (!IsValid) throw new InvalidOperationException($"{ExceptionPrefixText} Invalid: {ToString()}");
        }

        /// <summary>
        /// Retour Valide Ok
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static ValidateModel ReturnTrue(string notes = null)
        {
            return PrepareNew().Return(true);
        }

        /// <summary>
        /// Retour Valide KO
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static ValidateModel ReturnFalse(string notes = null)
        {
            return PrepareNew().Return(false);
        }
    }
}