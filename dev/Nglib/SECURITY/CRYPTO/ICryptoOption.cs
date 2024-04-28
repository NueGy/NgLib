namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Configuration pour du cryptage d'un objet
    /// Nécessite une autre DLL
    /// </summary>
    public interface ICryptoOption
    {
        byte[] GetCryptoIVBytes();
        byte[] GetCryptoKeyBytes();

        void SetInitializationVector(string IVvalue);

        bool IsEmptyIV();

        ICryptoOption CloneOption();
    }
}