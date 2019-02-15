using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    [Table("tests")]
    public class DataModelSample
    {
        //protected override void DefineStructRow()
        //{
        //    this.POManager().DefineRow("Banques", new System.Data.DataColumn("IDBanque", typeof(string)));
        //}


        [Column("fluxxml")]
        public string FluxXml { get; set; }
        

        [Column("fluxxml2")]
        public string FluxXml2 { get; set; }



        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestId { get; set; }

        [Column]
        public string Pseudo { get; set; }

    }
}
