using Nglib.DATA.ACCESSORS;
using Nglib.DATA.DATAPO;

namespace Nglib.DATA.KEYVALUES
{
    public class KeyValuesPOFlow : KeyValues, IDataPOFlow
    {
        private string fieldName;
        private FlowTypeEnum fieldType = FlowTypeEnum.AUTO;
        private bool isFullEncrypted;


        public void DefineField(string fieldName, FlowTypeEnum fieldType, bool isFullEncrypted = false)
        {
            this.fieldName = fieldName;
            this.isFullEncrypted = isFullEncrypted;
            this.fieldType = fieldType;
        }


        public string GetFieldName()
        {
            return fieldName;
        }

        public FlowTypeEnum GetFieldType()
        {
            return fieldType;
        }

        public bool IsFieldEncrypted()
        {
            return isFullEncrypted;
        }


        public void DeSerializeField(string dataField)
        {
            var isxml = fieldType == FlowTypeEnum.XML;
            var serializer = KeyValueTools.SerializerFactory(isxml);
            var edes = serializer.DeSerialize(dataField);
            Clear();
            AddRange(edes);
        }

        public string SerializeField()
        {
            var isxml = fieldType == FlowTypeEnum.XML;
            var serializer = KeyValueTools.SerializerFactory(isxml);
            var json = serializer.Serialize(this);
            if (string.IsNullOrWhiteSpace(json)) json = null;
            return json;
        }
    }
}