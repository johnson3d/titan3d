using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public class PGHideBaseClassPropertiesAttribute : Attribute { }
    public class TtShowInPropertyGridAttribute : Attribute { }
    public class TtCategoryFilters : Attribute 
    {
        public string[] ExcludeFilters = null;
    }
    public class TtValueRange : Attribute
    {
        public double Max;
        public double Min;
        public TtValueRange(double min, double max)
        {
            Max = max;
            Min = min;
        }
    }
    public class TtValueChangeStep : Attribute
    {
        public float Step = 1.0f;
        public TtValueChangeStep(float step)
        {
            Step = step;
        }
    }
    public class TtValueFormat : Attribute
    {
        public string Format = null;
        public TtValueFormat(string format)
        {
            Format = format;
        }
    }
    public class TtBaseType : Attribute
    {
        public Type BaseType;
        public TtBaseType(Type baseType)
        {
            BaseType = baseType;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PGShowWithProperty<T> : Attribute
    {
        public T PropertyValue;
        public string PropertyName;
        public enum EValueType
        {
            Equal           = 0,
            NotEqual        = 1,
            LessThan        = 2,
            LessThanOrEqual = 3,
            MoreThan        = 4,
            MoreThanOrEqual = 5,
        }
        public EValueType ValueType = EValueType.Equal;
    }

    // Operation when list add, remove or value changed
    public class PGListOperationCallbackAttribute : Attribute
    {
        public virtual void OnPreInsert(int index, object value, object objInstance) { }
        public virtual void OnAfterInsert(int index, object value, object objInstance) { }
        public virtual void OnPreRemoveAt(int index, object objInstance) { }
        public virtual void OnAfterRemoveAt(int index, object objInstance) { }
        public virtual void OnPreValueChanged(int index, object value, object objInstance) { }
        public virtual void OnAfterValueChanged(int index, object value, object objInstance) { }
    }
}
