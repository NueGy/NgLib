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
}