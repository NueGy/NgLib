using System;

namespace Nglib.APP.ENV
{
    /// <summary>
    /// Environnement d'execution global (obsolète, utiliser IMasterEnv)
    /// </summary>
    [Obsolete("Use IMasterEnv instead")]
    public interface IGlobalEnv : IMasterEnv
    {
        // Interface conservée pour compatibilité ascendante
        // Toutes les fonctionnalités sont désormais dans IMasterEnv
    }
}
