using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Fournis des accesseurs pour la transformation de valeurs
    /// </summary>
    public interface IDataAccessor //<TRetour>
    {

        /// <summary>
        /// Obtenir un objet (Methode principale)
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        object GetObject(string nameValue, DataAccessorOptionEnum AccesOptions);

        /// <summary>
        /// Définir un Objet (Methode principale)
        /// </summary>
        /// <param name="nameValue"></param>
        /// <param name="obj"></param>
        /// <param name="AccesOptions"></param>
        /// <returns></returns>
        bool SetObject(string nameValue, object obj, DataAccessorOptionEnum AccesOptions);


        /// <summary>
        /// Obtient la liste de tous les champs de l'objet
        /// </summary>
        /// <returns></returns>
        string[] ListFieldsKeys();








    }


}
