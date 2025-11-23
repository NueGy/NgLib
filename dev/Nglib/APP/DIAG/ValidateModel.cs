using System;
using System.Collections.Generic;
using System.Linq;

namespace Nglib.APP.DIAG
{
    /// <summary>
    /// Validation result model with boolean status and error messages (Result/Either pattern).
    /// Documentation: <see href="https://github.com/NueGy/NgLib/docs/wiki_components_appdiag"/>
    /// </summary>
    public class ValidateModel
    {
        public ValidateModel() { }
        public ValidateModel(bool defaultValid) { this.IsValid = defaultValid; }



        /// <summary>
        /// The validation result status.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Specific error code (optional).
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Additional validation notes or multiple error messages.
        /// </summary>
        public List<string> Notes { get; private set; } = new List<string>();

        /// <summary>
        /// Group identifier for the validation.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Adds a validation note.
        /// </summary>
        /// <param name="note">The note to add</param>
        public void AddNote(string note)
        {
            if (string.IsNullOrEmpty(note)) return;
            Notes.Add(note);
        }

        /// <summary>
        /// Sets the result as invalid with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <returns>The current instance for fluent API</returns>
        public ValidateModel SetInvalid(string errorMessage)
        {
            IsValid = false;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                if(!string.IsNullOrWhiteSpace(this.Message))
                    this.AddNote(Message); // keep previous message as note
                this.Message = errorMessage;
            }
                
            return this;
        }

        /// <summary>
        /// Sets the result as invalid from an exception.
        /// </summary>
        /// <param name="exception">The exception to extract message from</param>
        /// <returns>The current instance for fluent API</returns>
        public ValidateModel SetInvalid(Exception exception)
        {
            if (exception == null) return this;
            SetInvalid(exception.Message);
            return this;
        }


        /// <summary>
        /// Sets the result as valid.
        /// </summary>
        /// <returns>The current instance for fluent API</returns>
        public ValidateModel SetValid()
        {
            IsValid = true;
            return this;
        }

        /// <summary>
        /// Throws an exception if the validation is invalid (fail-fast pattern).
        /// </summary>
        /// <param name="msgprefix">Optional message prefix</param>
        /// <exception cref="Exception">Thrown when IsValid is false</exception>
        public void EnsureIsValid(string msgprefix = null)
        {
            if (IsValid) return; // ok
            string msgex = $"{ErrorCode} {Message}";
            if (string.IsNullOrWhiteSpace(msgex)) msgex = "Validation Failed";
            if (!string.IsNullOrEmpty(Group)) msgex = $"[{Group}]{msgex}";
            if (msgprefix != null) msgex = $"{msgprefix} {msgex}";
            throw new Exception(msgex);
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static ValidateModel Success => new ValidateModel() { IsValid = true };

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        public static ValidateModel Fail => new ValidateModel() { IsValid = false };

        /// <summary>
        /// Creates a failed validation result with a message.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <returns>A new invalid ValidateModel</returns>
        public static ValidateModel Invalid(string message) => new ValidateModel() { IsValid = false, Message = message };

        /// <summary>
        /// Creates a failed validation result from an exception.
        /// </summary>
        /// <param name="exception">The exception to extract message from</param>
        /// <returns>A new invalid ValidateModel</returns>
        public static ValidateModel Invalid(Exception exception) => new ValidateModel() { IsValid = false, Message = exception.Message };

        /// <summary>
        /// Combines multiple validation results into one.
        /// The result is invalid if at least one validation is invalid.
        /// </summary>
        /// <param name="validations">The validation results to combine</param>
        /// <returns>A combined validation result</returns>
        public static ValidateModel Combine(params ValidateModel[] validations)
        {
            if (validations == null || validations.Length == 0)
                return Success;

            var result = new ValidateModel { IsValid = true };
            
            foreach (var validation in validations)
            {
                if (!validation.IsValid)
                {
                    result.IsValid = false;
                    if (string.IsNullOrEmpty(result.Message))
                        result.Message = validation.Message;
                    else if (!string.IsNullOrEmpty(validation.Message))
                        result.AddNote(validation.Message);
                }
                
                if (!string.IsNullOrEmpty(validation.ErrorCode) && string.IsNullOrEmpty(result.ErrorCode))
                    result.ErrorCode = validation.ErrorCode;
                
                if (validation.Notes != null)
                    foreach (var note in validation.Notes)
                        result.AddNote(note);
            }
            
            return result;
        }


        public override string ToString()
        {
            return this.IsValid?"VALID":"INVALID" 
                +   (string.IsNullOrEmpty(this.Message)?"":$"({this.Message})" )
                +   (string.IsNullOrEmpty(this.ErrorCode) ? "" : $"(code:{this.ErrorCode})");
        }


    }
}