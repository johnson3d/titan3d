using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    [Graph(typeof(TtGraph_Method))]
    [OutlineElement_Leaf(typeof(TtOutlineElement_Method))]
    public class TtMethodDescription : IMethodDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Method";
        [Rtti.Meta]

        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]

        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]

        public UVariableDeclaration ReturnValue { get; set; }
        [Rtti.Meta]

        public virtual string MethodName { get=> TtDescriptionASTBuildUtil.GenerateMethodName(this);}
        [Rtti.Meta]

        public List<UMethodArgumentDeclaration> Arguments { get; set; } = new List<UMethodArgumentDeclaration>();
        [Rtti.Meta]

        public List<UVariableDeclaration> LocalVariables { get; set; } = new List<UVariableDeclaration>();
        [Rtti.Meta]

        public bool IsOverride { get; set; } = false;
        [Rtti.Meta]

        public bool IsAsync { get; set; } = false;

        [Rtti.Meta]
        public TtDesignMacrossMethodGraph MethodGraph
        { 
            get; 
            set; 
        } = null;
        public virtual UMethodDeclaration BuildMethodDeclaration(ref FClassBuildContext classBuildContext)
        {
            if(MethodGraph != null)
            {
                if (MethodGraph.MacrossEditor == null)
                {
                    var macrossHolder = new TtTransitionMacrossHolder();
                    macrossHolder.DefClass = classBuildContext.ClassDeclaration;
                    MethodGraph.SetMacrossEditor(macrossHolder);
                }
                MethodGraph?.BuildExpression(classBuildContext.ClassDeclaration);
                return MethodGraph.MethodDatas[0].MethodDec;
            }
            return TtDescriptionASTBuildUtil.BuildDefaultPartForMethodDeclaration(this, ref classBuildContext);
        }

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if (hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }


}
