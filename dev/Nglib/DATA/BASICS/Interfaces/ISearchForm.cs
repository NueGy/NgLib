namespace Nglib.DATA.BASICS
{
    /// <summary>
    ///     Formulaire de recherche
    /// </summary>
    public interface ISearchForm
    {
        /// <summary>
        ///     Page
        /// </summary>
        int CurrentPage { get; set; }

        /// <summary>
        ///     Résultats maximum
        /// </summary>
        int LimitResults { get; set; }

        /// <summary>
        ///     OrderBy
        /// </summary>
        string ShowOrderBy { get; set; }
    }
}