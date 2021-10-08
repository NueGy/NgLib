using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.BASICS
{
    public class TextSearchForm : ISearchForm
    {
        public string SearchText { get; set; }
        public int CurrentPage { get; set; }
        public int LimitResults { get; set; }
        public string ShowOrderBy { get; set; }
    }
}
