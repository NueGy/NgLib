using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Formulaire pour modification d'un datapo
    /// </summary>
    /// <typeparam name="Tpo"></typeparam>
    public interface IFormPO<Tpo> where Tpo : DataPO
    {



        void ToPO(Tpo item);
        void FromPO(Tpo item);


    }
}
