using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.DesignMacross.Base.Description;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    public class TtDesignableVariableDescription : IDesignableVariableDescription
    {
        [Rtti.Meta]
        [Browsable(false)]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public virtual string Name { get; set; } = "DesignableVariable";
        [Browsable(false)]
        public string VariableName { get => TtDescriptionASTBuildUtil.GenerateVariableName(this); }
        [Browsable(false)]
        public string ClassName { get => TtDescriptionASTBuildUtil.GenerateClassName(this); }
        [Rtti.Meta]
        [Browsable(false)]
        public UTypeReference VariableType { get => new UTypeReference(ClassName); set { } }
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        [Browsable(false)]
        public IClassDescription DesignedClassDescription { get; set; }
        [Rtti.Meta]
        public UNamespaceDeclaration Namespace { get; set; }
        [Rtti.Meta]
        public bool IsStruct { get; set; }
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [Rtti.Meta]
        [Browsable(false)]
        public List<IVariableDescription> Variables { get; set; } = new List<IVariableDescription>();
        [Rtti.Meta]
        [Browsable(false)]
        public List<IMethodDescription> Methods { get; set; } = new List<IMethodDescription>();
        [Browsable(false)]
        public IDescription Parent { get; set; }

        public virtual List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            List<UClassDeclaration> classDeclarationsBuilded = new();
            classDeclarationsBuilded.AddRange(DesignedClassDescription.BuildClassDeclarations(ref classBuildContext));
            return classDeclarationsBuilded;
        }

        public virtual UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtDescriptionASTBuildUtil.BuildDefaultPartForVariableDeclaration(this, ref classBuildContext);
        }
        #region ISerializer
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
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
