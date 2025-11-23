//Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{


    /// <summary>
    /// Token/segment type in the formula
    /// </summary>
    public enum FormulaSegmentTypeEnum
    {
        /// <summary>
        /// Number: 123, 3.14
        /// </summary>
        ValNumber,

        /// <summary>
        /// String: 'hello', "world"
        /// </summary>
        ValString,      

        /// <summary>
        /// Parameter: @price, @user
        /// </summary>
        ValParameter,

        /// <summary>
        /// Function: add(), mul(), ceil()
        /// </summary>
        Function,    

        /// <summary>
        /// Operator: +, -, *, /, ==, >=
        /// </summary>
        Operator,


        /// <summary>
        /// Raw formula content
        /// </summary>
        FormulaContent,
    }



    /// <summary>
    /// How to pass arguments to the method
    /// </summary>
    public enum MethodModeEnum
    {
        /// <summary>
        /// Passes a string array to the method, DEFAULT
        /// </summary>
        StringArray,
        /// <summary>
        /// Passes an object array to the method
        /// </summary>
        ObjectArray,
        /// <summary>
        /// Passes the context directly to the method
        /// </summary>
        Context,

        /// <summary>
        /// It's a constant, there is no method/command
        /// </summary>
        Constant
    }

    /// <summary>
    /// Data type returned by the method
    /// </summary>
    public enum FormulaDataType
    {
        /// <summary>
        /// Chaine = Standard
        /// </summary>
        STRING,

        /// <summary>
        /// Nombre Int64 = Long
        /// </summary>
        INT,

        /// <summary>
        /// INT,LONG, FLOAT,... Est stocké au format double
        /// </summary>
        NUMERIC,

        /// <summary>
        /// Array de chaine, string[]
        /// </summary>
        ARRAY,

        /// <summary>
        /// Objet au format System.Data.DataRow
        /// </summary>
        DATAROW,

        /// <summary>
        /// Objet divers (Do Not Use)
        /// </summary>
        OBJECT





    }

}
