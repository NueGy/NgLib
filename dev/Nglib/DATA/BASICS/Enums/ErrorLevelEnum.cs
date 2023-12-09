namespace Nglib.DATA.BASICS
{
    /// <summary>
    ///     Niveau d'erreur
    /// </summary>
    public enum ErrorLevelEnum
    {
        /// <summary>
        ///     Ne sera pas enregistré
        /// </summary>
        DEBUG = 0,

        /// <summary>
        ///     pour envoyer un message
        /// </summary>
        NOTIFICATION = 1,

        /// <summary>
        ///     OK
        /// </summary>
        SUCCESS = 2,

        /// <summary>
        ///     Attention
        /// </summary>
        WARNING = 3,

        /// <summary>
        ///     Une erreur
        /// </summary>
        ERROR = 4,

        /// <summary>
        ///     Une erreur importante (ATTENTION, limité son utilisation au strict nécessaire)
        /// </summary>
        CRITIQ = 5
    }
}