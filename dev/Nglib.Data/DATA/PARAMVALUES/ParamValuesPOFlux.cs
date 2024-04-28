// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.PARAMVALUES
{
    /// <summary>
    /// Représente un champ flux (JSON ou XML) dans la base
    /// </summary>
    public class ParamValuesPOFlux : ParamValues, DATA.DATAPO.IDataPOFlow
    {
        string fieldName = null;
        bool isFullEncrypted = false;
        FlowTypeEnum fieldType = FlowTypeEnum.AUTO;

        public void DefineField(string fieldName, FlowTypeEnum fieldType, bool isFullEncrypted = false)
        {
            this.fieldName = fieldName;
            this.isFullEncrypted = isFullEncrypted;
            this.fieldType = fieldType;
        }



        public string GetFieldName()
        {
            return this.fieldName;
        }

        public FlowTypeEnum GetFieldType()
        {
            return this.fieldType;
        }

        public bool IsFieldEncrypted()
        {
            return isFullEncrypted;
        }

  
        /// <summary>
        /// Serialisation du FLUX ToFlux
        /// </summary>
        /// <returns></returns>
        public string SerializeField()
        {
            return ParamValuesTools.ToFlux(this, fieldType == FlowTypeEnum.JSON ? false:true);
        }

        /// <summary>
        /// Deserialisation, FromFlux
        /// </summary>
        /// <param name="fluxContent"></param>
        public void DeSerializeField(string fluxContent)
        {
            ParamValuesTools.FromFlux(this, fluxContent);
        }
    }
}
