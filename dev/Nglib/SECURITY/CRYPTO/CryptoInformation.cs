using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Nglib.SECURITY.CRYPTO
{
    public interface ICryptoOption
    {

        byte[] GetCryptoIVBytes();
        byte[] GetCryptoKeyBytes();

        void SetInitializationVector(string IVvalue);

        bool IsEmptyIV();

        ICryptoOption CloneOption();

    }


    public class CryptoOption : ICryptoOption
    {


        /// <summary>
        /// Vecteur d'initialisation nécessaire. Valeur en clair et généré aléatoirement
        /// </summary>
        public byte[] InitializationVectorBytes { get; set; }

        /// <summary>
        /// Clef de cryptage/décryptage
        /// </summary>
        private byte[] CryptoKeyBytes { get; set; } // //add SecureString !!!


        /// <summary>
        /// Cryptage par RSA ou AES
        /// </summary>
        public Nglib.SECURITY.CRYPTO.CryptoModeEnum CryptoMode { get; set; } = CryptoModeEnum.AES256;


        /// <summary>
        /// Nombre de caractères aléatoires en début de cryptage (A Ajouter ou supprimer)
        /// </summary>
        public int RandomStartSalt { get; set; } = 0; 



  





        /// <summary>
        /// Sera Haché en SHA256
        /// </summary>
        /// <param name="PasswordValue"></param>
        /// <returns></returns>
        public void SetCryptoPassword(string PasswordValue)
        {
            if (string.IsNullOrWhiteSpace(PasswordValue)) throw new ArgumentNullException("PasswordValue", "SetCryptoPassword EmptyValue");
            // Hash the password with SHA256
            byte[] passwordBytes = Encoding.UTF8.GetBytes(PasswordValue);
            passwordBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(passwordBytes); // permet d'avoir la clef sur 32 byte
            this.SetCryptoKey(passwordBytes);
        }
        public void SetCryptoKey(string KeyValue)
        {
            if (string.IsNullOrWhiteSpace(KeyValue)) throw new ArgumentNullException("KeyValue", "SetCryptoKey EmptyValue");
            byte[] KeyValueBytes = Encoding.UTF8.GetBytes(KeyValue);
            this.SetCryptoKey(KeyValueBytes);
        }


        public void SetCryptoKey(byte[] KeyValue)
        {
            if (KeyValue==null) throw new ArgumentNullException("KeyValue", "SetCryptoKey EmptyValue");
           
            this.CryptoKeyBytes = KeyValue;
        }

        public string GetCryptoKeyString()
        {
            if (this.CryptoKeyBytes == null) return null;
            return Encoding.UTF8.GetString(this.CryptoKeyBytes);
        }

        public byte[] GetCryptoKeyBytes()
        {
            return this.CryptoKeyBytes;
        }

        public byte[] GetCryptoIVBytes()
        {
            if(this.InitializationVectorBytes==null)return new byte[16]; // retourne un IV vide
            return this.InitializationVectorBytes;
        }
        

        public bool IsEmptyIV()
        {
            if (this.InitializationVectorBytes == null) return true;
            return false;
        }


        public void SetInitializationVector(string IVvalue)
        {
            if (string.IsNullOrWhiteSpace(IVvalue)) { this.InitializationVectorBytes = null; return; }
           this.InitializationVectorBytes = Encoding.UTF8.GetBytes(IVvalue).Take(16).ToArray();
            ////if (IVbyte == null) cryptoInformation.InitializationVectorBytes = GenerateRandomBytes(16);
        }


        public ICryptoOption CloneOption()
        {
            CryptoOption retour = new CryptoOption();
            retour.CryptoKeyBytes = this.CryptoKeyBytes;
            retour.InitializationVectorBytes = this.InitializationVectorBytes;
            retour.RandomStartSalt = this.RandomStartSalt;
            return retour;
        }




    }
}
