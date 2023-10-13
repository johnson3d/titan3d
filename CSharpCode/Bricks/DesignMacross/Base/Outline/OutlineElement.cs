using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Outline
{
    public class OutlineElementAttribute : Attribute
    {
        public Type ClassType { get; set; }
        public OutlineElementAttribute(Type type)
        {
            ClassType = type;
        }
    }
    public class OutlineElementsListAttribute : Attribute
    {
        public Type ClassType { get; set; }
        public OutlineElementsListAttribute(Type type)
        {
            ClassType = type;
        }
    }


    public interface IDesignableVariableOutlineElement : IOutlineElement
    {

    }

    //public interface IVariableOutlinerElement : IOutlinerElement
    //{

    //}
    //public interface IMethodOutlinerElement : IOutlinerElement
    //{
    //    //For code generate
    //}
}
