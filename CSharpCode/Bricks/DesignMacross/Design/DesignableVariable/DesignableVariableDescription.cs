using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDesignableVariableDescription : IDesignableVariableDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public virtual string Name { get; set; } = "DesignableVariable";
        public string VariableName { get => TtASTBuildUtil.GenerateVariableName(this); }
        public string ClassName { get => TtASTBuildUtil.GenerateClassName(this); }
        [Rtti.Meta]
        public UTypeReference VariableType { get => new UTypeReference(ClassName); set { } }
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public IClassDescription DesignedClassDescription { get; set; }
        [Rtti.Meta]
        public UNamespaceDeclaration Namespace { get; set; }
        [Rtti.Meta]
        public bool IsStruct { get; set; }
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [Rtti.Meta]
        public List<IVariableDescription> Variables { get; set; } = new List<IVariableDescription>();
        [Rtti.Meta]
        public List<IMethodDescription> Methods { get; set; } = new List<IMethodDescription>();
        public IDescription Parent { get; set; }

        public virtual List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            List<UClassDeclaration> classDeclarationsBuilded = new();
            classDeclarationsBuilded.AddRange(DesignedClassDescription.BuildClassDeclarations(ref classBuildContext));
            return classDeclarationsBuilded;
        }

        public virtual UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        public virtual void GenerateCodeInClass(UClassDeclaration classDeclaration)
        {
            
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
