// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using Nglib.DATA.ACCESSORS;
using Nglib.DATA.DATAVALUES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Représente un champ flux (JSON ou XML) dans la base
    /// </summary>
    public class DataPOFlux : DATAVALUES.DataValues, IDataPOFlow
    {
        string fieldName = null;
        string encryptedKey = null;
        FlowTypeEnum fieldType = FlowTypeEnum.AUTO;

        public void DefineField(string fieldName, FlowTypeEnum fieldType, string encryptedKey = null)
        {
            this.fieldName = fieldName;
            this.encryptedKey = encryptedKey;
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
            return !string.IsNullOrWhiteSpace(encryptedKey);
        }

  

        public string SerializeField()
        {
            return DataValuesTools.ToFlux(this);
        }

        public void UnSerializeField(string fluxContent)
        {
            DataValuesTools.FromFlux(this, fluxContent);
        }
    }
}
