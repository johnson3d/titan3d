using System;

using System.Reflection;
using System.ComponentModel;

namespace EngineNS.Design
{
    public class FieldPropertyDescriptor : PropertyDescriptor
    {
        private FieldInfo m_FieldInfo;

        public FieldPropertyDescriptor(FieldInfo fieldInfo)
            : base(fieldInfo.Name, Array.ConvertAll<object, Attribute>(fieldInfo.GetCustomAttributes(true), (obj => (Attribute)obj)))
	    {
		    m_FieldInfo = fieldInfo;
	    }

        public override Type ComponentType
	    {
            get
            {
                return m_FieldInfo.DeclaringType;
            }
	    }

        public override bool IsReadOnly
	    {
            get
            {
                return false;
            }
	    }

        public override Type PropertyType
	    {
            get
            {
                return m_FieldInfo.FieldType;
            }
	    }

        public override bool CanResetValue(Object obj)
	    {
		    return false;
	    }

        public override void ResetValue(Object obj)
	    {
		    //don't need to do anything
	    }

        public override object GetValue(Object component)
	    {
		    return m_FieldInfo.GetValue( component );
	    }

        public override void SetValue(Object component, Object value)
	    {
		    m_FieldInfo.SetValue( component, value );
	    }

        public override bool ShouldSerializeValue(Object obj)
	    {
		    return true;
	    }
    }
}
