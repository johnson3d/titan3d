using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace EngineNS.DesignMacross.Nodes
{
    public class TtGraphElement_Binocular : TtGraphElement_NodeBase
    {
    }

    public class TtBinocularDescription : IDescription
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public UBinaryOperatorExpression.EBinaryOperation Op { get; set; }

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }

    [ContextMenu("add,+", "Operation\\+", UMacross.MacrossEditorKeyword)]
    public class TtAddDescription : TtBinocularDescription
    {

    }
}
