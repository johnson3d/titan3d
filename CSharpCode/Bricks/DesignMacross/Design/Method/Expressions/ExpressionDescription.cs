using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtClassReferenceExpressionDescription : IClassDescription
    {
        public TtDataPinDescription DataPin;

        public string ClassName => throw new NotImplementedException();
        public EVisisMode VisitMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UCommentStatement Comment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UNamespaceDeclaration Namespace { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsStruct { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<string> SupperClassNames { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ObservableCollection<IVariableDescription> Variables { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ObservableCollection<IMethodDescription> Methods { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public List<UClassDeclaration> BuildClassDeclarations()
        {
            throw new NotImplementedException();
        }

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
    public class TtVariableReferenceExpressionDescription
    {

    }
    public class TtSelfReferenceExpressionDescription
    {

    }
    public class TtMethodInvokeDescription
    {

    }
    public class TtLambdaExpressionDescription
    {

    }
    public class TtAssignOperatorExpressionDescription
    {

    }
    public class TtUnaryOperatorExpressionDescription
    {

    }
    public class TtIndexerOperatorExpressionDescription
    {

    }
    public class TtPrimitiveExpressionDescription
    {

    }
    public class TtCastExpressionDescription
    {

    }
    public class TtCreateObjectExpressionDescription
    {

    }
    public class TtDefaultValueExpressionDescription
    {

    }
    public class TtNullValueExpressionDescription
    {

    }
    public class TtExecuteSequenceStatementDescription
    {

    }
    public class TtReturnStatementDescription
    {

    }
    public class TtIfStatementDescription
    {

    }
    public class TtForLoopStatementDescription
    {

    }
    public class TtWhileLoopStatementDescription
    {

    }
    public class TtContinueStatementDescription
    {

    }
    public class TtBreakStatementDescription
    {

    }
}
