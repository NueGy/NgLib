using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.CONNECTOR
{
    public enum SqlCommandTypeEnum
    {
        SELECT,
        UPDATE,
        DELETE,
        INSERT
    }

    public enum SqlJoinTypeEnum
    {
        INNER,
        LEFT,
        RIGHT
        //,UNION
    }
}
