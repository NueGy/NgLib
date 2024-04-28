using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.ACCESSORS
{
    public static class DataAccessorExtentionsPlus
    {

        [Obsolete("BETA")]
        public static string DynamicValues(this IDataAccessor dataAccessor, string formatingValues, Dictionary<string, string> transformFuctions = null, DataAccessorOptionEnum AccesOptions = DataAccessorOptionEnum.None)
        {
            try
            {
                // dynamisation de la chaine de caratère
                string retour = FORMAT.StringTransform.DynamiseWithAccessor(formatingValues, dataAccessor);
                // Transformation de la chaine finale selon une série d'instruction de transformation
                if (transformFuctions != null)
                    retour = FORMAT.StringTransform.Transform(retour, transformFuctions);
                return retour;
            }
            catch (Exception ex)
            {
                if (!AccesOptions.HasFlag(DataAccessorOptionEnum.Safe)) throw new DataAccessorException("FormatingMultiValues " + ex.Message, ex);
                return string.Empty;
            }
        }




    }
}
