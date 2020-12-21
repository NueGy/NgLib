using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nglib.DATA.ACCESSORS;

namespace Nglib.DATA.DATAPO
{
    [Table("tests")]
    public class DataPoSample : DATAPO.DataPO
    {

        //protected override void DefineStructRow()
        //{
        //    this.POManager().DefineRow("Banques", new System.Data.DataColumn("IDBanque", typeof(string)));
        //}


        [Column("fluxxml")]
        public DATA.PARAMVALUES.ParamValuesPOFlux FluxXml
        {
            get { return this.GetOrDefineFlow<DATA.PARAMVALUES.ParamValuesPOFlux>("fluxxml", FlowTypeEnum.XML); }
        }

        [Column("fluxjson")]
        public DATA.KEYVALUES.KeyValuesPOFlow FluxJson
        {
            get { return this.GetOrDefineFlow<DATA.KEYVALUES.KeyValuesPOFlow>("fluxjson", FlowTypeEnum.JSON); }
        }



        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestId
        {
            get { return this.GetInt("testid"); }
            set { this.SetObject("testid", value); }
        }

        [Column]
        public string Pseudo
        {
            get { return this.GetString("pseudo"); }
            set { this.SetObject("pseudo", value); }
        }


        //[Column]
        //public string Pseudo2
        //{
        //    get { return this.GetString("pseudo2"); }
        //    set { this.SetObject("pseudo2", value); }
        //}



        //public List<string> multicols1
        //{
        //    get { return this.GetString("multicols1"); }
        //    set { this.SetObject("pseudo", value); }
        //}

        //public List<int> multicols2
        //{
        //    get { return this.GetString("multicols2"); }
        //    set { this.SetObject("pseudo", value); }
        //}

    }
}
