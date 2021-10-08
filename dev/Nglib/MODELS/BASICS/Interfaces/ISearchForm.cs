using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.BASICS
{
    /// <summary>
    /// Formulaire de recherche
    /// </summary>
    public interface ISearchForm
    {

        int CurrentPage { get; set; }
        int LimitResults { get; set; }
        string ShowOrderBy { get; set; }




    }
}
