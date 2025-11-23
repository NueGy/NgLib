namespace Nglib.DATA.KEYVALUES
{
    public interface IKeyValuesSerializer
    {
        string Serialize(KeyValues values);

        KeyValues DeSerialize(string fluxstring);
    }
}