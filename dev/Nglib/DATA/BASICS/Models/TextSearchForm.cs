namespace Nglib.DATA.BASICS
{
    public class TextSearchForm : ISearchForm
    {
        public string SearchText { get; set; }

        public int ActionIncrement { get; set; }
        public int CurrentPage { get; set; }
        public int LimitResults { get; set; }
        public string ShowOrderBy { get; set; }
    }
}