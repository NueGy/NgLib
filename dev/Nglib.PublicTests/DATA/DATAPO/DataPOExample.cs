using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Classe de démo basé sur les DATAPO
    /// </summary>
    public class DataPOExample : Nglib.DATA.DATAPO.DataPO
    {
        public override DataTable CreateSchema()
        {
            DataColumn[] keys = new List<DataColumn>() { new DataColumn("monid", typeof(int)) { AutoIncrement = true, AutoIncrementSeed = 1, AutoIncrementStep = 1 } }.ToArray();
            return Nglib.DATA.COLLECTIONS.DataSetTools.DefineDataTable("demotable", null, keys);
        }

        public Nglib.DATA.PARAMVALUES.ParamValuesPOFlux Flux => base.GetOrDefineFlow<Nglib.DATA.PARAMVALUES.ParamValuesPOFlux>("fluxjson", Nglib.DATA.ACCESSORS.FlowTypeEnum.JSON, false);


        public int MonId { get { return this.GetInt("monid"); } set { this["monid"] = value; } }

        public string MaValeur { get { return this.GetString("mavaleur"); } set { this["mavaleur"] = value; } }

        public string MaValeurNosql { get { return this.Flux.GetString("maval"); } set { this.Flux["maval"] = value; } }



    }
}
