using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO.Serializer
{
    public abstract class CustomSerializer : FieldSerializer
    {
        public override void ReadValueList(System.Collections.IList lst, IReader pkg)
        {

        }
        public override void WriteValueList(System.Collections.IList lst, IWriter pkg)
        {

        }

        #region XML
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            return o.ToString();
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            return null;
        }
        #endregion
    }

    public class CustomFieldSerializerAttribute : System.Attribute
    {
        public System.Type SerializerType
        {
            get;
            protected set;
        } = null;
        public CustomFieldSerializerAttribute(System.Type stype)
        {
            SerializerType = stype;
        }
    }
}
