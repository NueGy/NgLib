using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.WEB.PIPELINE
{
    public class RequestLogSearchModel : Nglib.DATA.BASICS.ISearchForm
    {

        public string DateMin { get; set; }
        public string DateMax { get; set; }

        public int? TenantId { get; set; }

        public bool OnlyError { get; set; }


        public string RoutePathLike { get; set; }   

        public int CurrentPage { get; set; }
        public int LimitResults { get; set; }
        public string ShowOrderBy { get; set; }

    }
}