namespace Nglib.DATA.BASICS
{
    public interface IPostalAddress
    {
        string AddressName { get; set; }
        string AddressLines { get; set; }
        string AddressPostcode { get; set; }
        string AddressCity { get; set; }
        string AddressCountryCode { get; set; }
    }
}