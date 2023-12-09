using System;
using System.Collections.Generic;

namespace Nglib.DATA.BASICS
{
    public struct ValidateModel
    {
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

        public List<string> Notes { get; set; }

        public Dictionary<string, object> Datas { get; set; }

        public Exception Exception { get; set; }

        public string LogCode { get; set; }


        public int MaxlevelError { get; set; }


        public override string ToString()
        {
            var retour = $"{IsValid}:";
            if (Notes != null && Notes.Count > 0) retour += string.Join("; ", Notes);
            return retour;
        }


        public void AddNote(string msg, bool? changeisvalid)
        {
            if (Notes == null) Notes = new List<string>();
            Notes.Add(msg);
            if (changeisvalid.HasValue) IsValid = changeisvalid.Value;
        }


        public void AddError(string noteMessage = null, int maxlevelError = 0)
        {
            if (Notes == null) Notes = new List<string>();
            if (noteMessage != null) Notes.Add(noteMessage);
            IsValid = false;
            if (maxlevelError > 0 && maxlevelError > MaxlevelError) MaxlevelError = maxlevelError;
        }

        public ValidateModel Return(bool isValid, string addnote = null, int maxlevelError = 0)
        {
            IsValid = isValid;
            if (Notes == null) Notes = new List<string>();
            if (addnote != null) Notes.Add(addnote);
            if (maxlevelError > 0 && maxlevelError > MaxlevelError) MaxlevelError = maxlevelError;
            return this;
        }

        public ValidateModel ReturnTrue()
        {
            return Return(true);
        }

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
            if (!IsValid) throw new Exception($"{ExceptionPrefixText} Objet Invalid: {ToString()}");
        }


        public static ValidateModel ReturnTrue(string notes = null)
        {
            return PrepareNew().Return(true);
        }

        public static ValidateModel ReturnFalse(string notes = null)
        {
            return PrepareNew().Return(false);
        }
    }
}