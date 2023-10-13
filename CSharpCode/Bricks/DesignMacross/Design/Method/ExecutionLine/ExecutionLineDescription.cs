using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    [GraphElement(typeof(TtGraphElement_ExecutionPin))]
    public class TtExecutionPinDescription : IDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "ExecPin";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
    [GraphElement(typeof(TtGraphElement_ExecutionLine))]
    public class TtExecutionLineDescription : IDescription
    {
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
