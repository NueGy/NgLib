using System.Collections.Generic;
using Nglib.DATA.BASICS;

namespace Nglib.DATA.DATAMODEL
{
    /// <summary>
    ///     Une liste de datamodel , rarement utilisé dans les API
    /// </summary>
    /// <typeparam name="Tmodel"></typeparam>
    public class DataModelCollection<Tmodel> : List<DataModel<Tmodel>> where Tmodel : IModel, new()
    {
    }
}