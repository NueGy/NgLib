using Nglib.SECURITY.CRYPTO;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Permet de gérer le chiffrement/déchiffrement des objets dataPO
    /// </summary>
    public interface IDataAccessorCryptoContext
    {

        /// <summary>
        /// Obtenir le vecteur d'initialisation
        /// </summary>
        /// <param name="datapo"></param>
        /// <returns></returns>
        string GetIV(IDataAccessor obj);

        /// <summary>
        /// Option de cryptage si disponible
        /// </summary>
        ICryptoOption OptionForEncrypt { get; set; }

        /// <summary>
        /// Option de décryptage si disponible
        /// </summary>
        ICryptoOption OptionForDecrypt { get; set; }


        object EncryptObjectValue(IDataAccessor dataAccessor, string nameValue, object value, DataAccessorOptionEnum options);
        object DecryptObjectValue(IDataAccessor dataAccessor, string nameValue, object valuecc, DataAccessorOptionEnum options);


    }





    /// <summary>
    /// Permet de gérer le chiffrement/déchiffrement des objets dataPO
    /// </summary>
    public class DataAccessorCryptoContext : IDataAccessorCryptoContext
    {
        public ICryptoOption OptionForEncrypt { get; set; }
        public ICryptoOption OptionForDecrypt { get; set; }



        public virtual string GetIV(IDataAccessor obj)
        {
            throw new NotImplementedException();

            //ICryptoOption retour = keyForEncrypt ? this.CryptoOptionForEncrypt : this.CryptoOptionForDecrypt;

            //// appel du vecteur d'initialisation
            //if (this.CryptoReplaceIv && retour.IsEmptyIV())
            //{
            //    retour = retour.CloneOption(); // il faut un nouvel objet car la référence du précédent objet à peut être été utilisé pour plusieurs PO
            //    string iv = this.GetCryptoIV();
            //    if (!string.IsNullOrEmpty(iv) && iv.Length < 16) throw new Exception("Invalid IV : Minimum length 16");
            //    retour.SetInitializationVector(iv);
            //    if (keyForEncrypt) this.CryptoOptionForEncrypt = retour; else this.CryptoOptionForDecrypt = retour; // remplace l'objet pour les prochaines fois
            //}


            //define

            // spécifique pour le cryptage partiel dans le flux, on y ajoute les memes clefs de cryptage
            //if ((this.CryptoOptionForEncrypt != null || this.CryptoOptionForDecrypt != null) && flow is Nglib.DATA.PARAMVALUES.ParamValues)
            //{
            //    if (this.CryptoReplaceIv) { this.GetCryptoOption(fieldName, false); this.GetCryptoOption(fieldName, true); } // permet de forcer la génération des IV si nécessaire
            //    (flow as Nglib.DATA.PARAMVALUES.ParamValues).SetCryptoOptions(this.CryptoOptionForEncrypt, this.CryptoOptionForDecrypt);
            //}


        }




        public virtual object EncryptObjectValue(IDataAccessor dataAccessor, string nameValue, object value, DataAccessorOptionEnum options)
        {
            if (!options.HasFlag(DataAccessorOptionEnum.Encrypted) || dataAccessor == null || value == null) return value; // rien à faire
            if (this.OptionForEncrypt == null) throw new NullReferenceException($"DataAccessorEncrypt OptionForEncrypt NULL (objectType: {dataAccessor.GetType().Name})");
            if (value.GetType().IsArray) return EncryptArrayValue(dataAccessor, nameValue, value, options); // spécifique pour gérer les array

            string strvalue = value.ToString(); // la valeur en base sera forcément un string
            string encryptedVal = Nglib.FORMAT.CryptHashTools.Encrypt(strvalue, this.OptionForEncrypt);
            return encryptedVal;
        }

        protected virtual object EncryptArrayValue(IDataAccessor dataAccessor, string nameValue, object value, DataAccessorOptionEnum options)
        {
            if (!options.HasFlag(DataAccessorOptionEnum.Encrypted) || dataAccessor == null || value == null) return value; // rien à faire
            if (this.OptionForEncrypt == null) throw new NullReferenceException($"DataAccessorEncrypt OptionForEncrypt NULL (objectType: {dataAccessor.GetType().Name})");
            string[] adata = DataAccessorTools.ConvertoArrayString(value);
            for (int i = 0; i < adata.Length; i++) // on encrypte les valeurs une par une
                adata[i] = Nglib.FORMAT.CryptHashTools.Encrypt(adata[i], this.OptionForEncrypt);

            return adata;
        }


        public virtual object DecryptObjectValue(IDataAccessor dataAccessor, string nameValue, object valuecc, DataAccessorOptionEnum options)
        {
            if (!options.HasFlag(DataAccessorOptionEnum.Encrypted) || dataAccessor == null || valuecc == null) return valuecc; // rien à faire
            if (this.OptionForDecrypt == null) throw new NullReferenceException($"DataAccessorDecrypt OptionForDecrypt NULL (objectType: {dataAccessor.GetType().Name})");
            if (valuecc.GetType().IsArray) return DecryptArrayValue(dataAccessor, nameValue, valuecc, options); // spécifique pour gérer les array


            if (!(valuecc is string)) throw new Exception("DataAccessorDecrypt Only string input allowed");
            string strvalue = valuecc.ToString(); // la valeur en base sera forcément un string
            string decryptedVal = Nglib.FORMAT.CryptHashTools.Decrypt(strvalue, this.OptionForDecrypt);
            return decryptedVal;
        }

        protected virtual object DecryptArrayValue(IDataAccessor dataAccessor, string nameValue, object valuecc, DataAccessorOptionEnum options)
        {
            if (!options.HasFlag(DataAccessorOptionEnum.Encrypted) || dataAccessor == null || valuecc == null) return valuecc; // rien à faire
            if (this.OptionForDecrypt == null) throw new NullReferenceException($"DataAccessorDecrypt OptionForDecrypt NULL (objectType: {dataAccessor.GetType().Name})");
            if (valuecc == null) return null;
            string[] adata = valuecc as string[];
            if (adata == null) throw new Exception("DecryptArrayValue Only string[] input allowed");

            for (int i = 0; i < adata.Length; i++) //une par une
                adata[i] = Nglib.FORMAT.CryptHashTools.Decrypt(adata[i], this.OptionForDecrypt);
            return adata;
        }




        public static DataAccessorCryptoContext PrepareWithAESKey(string aesKeyString)
        {
           var crypto= new Nglib.SECURITY.CRYPTO.CryptoOption() { CryptoMode = CryptoModeEnum.AES256 };
            crypto.SetCryptoKey(aesKeyString);

            DataAccessorCryptoContext retour = new DataAccessorCryptoContext();
            retour.OptionForEncrypt = crypto;
            retour.OptionForDecrypt = crypto;
            return retour;
        }


    }
}
