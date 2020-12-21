using Nglib.DATA.ACCESSORS;
using Nglib.SECURITY.CRYPTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesPOFlow : KeyValues, Nglib.DATA.DATAPO.IDataPOFlow
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


        public void DeSerializeField(string dataField)
        {
            bool isxml = fieldType == FlowTypeEnum.XML;
            KEYVALUES.IKeyValuesSerializer serializer = KeyValueTools.SerializerFactory(isxml);
            KeyValues edes =serializer.DeSerialize(dataField);
            this.Clear();
            this.AddRange(edes);
        }

        public string SerializeField()
        {
            bool isxml = fieldType == FlowTypeEnum.XML;
            KEYVALUES.IKeyValuesSerializer serializer = KeyValueTools.SerializerFactory(isxml);
            string json = serializer.Serialize(this);
            return json;
        }


    }
}
