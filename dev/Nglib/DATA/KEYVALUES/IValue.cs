namespace Nglib.DATA.KEYVALUES
{
    public interface IValue
    {
        object GetData();

        void AcceptChanges();

        bool IsChanges();
    }
}