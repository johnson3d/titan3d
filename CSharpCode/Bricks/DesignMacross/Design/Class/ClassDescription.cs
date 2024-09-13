﻿using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Outline;
using System.Reflection;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.Animation.Macross.Postprocessing;

namespace EngineNS.DesignMacross.Design
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtClassDescription : IClassDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "ClassDescription";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public string ClassName { get => Name; }
        [Rtti.Meta]
        public TtCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public TtNamespaceDeclaration Namespace { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public bool IsStruct { get; set; } = false;
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [OutlineElement_List(typeof(TtOutlineElementsList_Variables))]
        [Rtti.Meta]
        public List<IVariableDescription> Variables { get; set; } = new List<IVariableDescription>();
        [Rtti.Meta]
        [OutlineElement_List(typeof(TtOutlineElementsList_Methods))]
        public List<IMethodDescription> Methods { get; set; } = new List<IMethodDescription>();
        [OutlineElement_List(typeof(TtOutlineElementsList_DesignableVariables))]
        [Rtti.Meta]
        public List<IDesignableVariableDescription> DesignableVariables { get; set; } = new List<IDesignableVariableDescription>();
        public IDescription Parent { get; set; }

        public List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            classBuildContext.ClassDeclaration = thisClassDeclaration;
            foreach (var designVarDesc in DesignableVariables)
            {
                if (designVarDesc is TtAnimFinalBlendTreeClassDescription)
                {
                    continue;
                }
                classDeclarationsBuilded.AddRange(designVarDesc.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(designVarDesc.BuildVariableDeclaration(ref classBuildContext));
                designVarDesc.GenerateCodeInClass(thisClassDeclaration, ref classBuildContext);
            }
            foreach (var designVarDesc in DesignableVariables)
            {
                if(designVarDesc is TtAnimFinalBlendTreeClassDescription finalBlendTreeClassDescription)
                {
                    classDeclarationsBuilded.AddRange(designVarDesc.BuildClassDeclarations(ref classBuildContext));
                    thisClassDeclaration.Properties.Add(designVarDesc.BuildVariableDeclaration(ref classBuildContext));
                    designVarDesc.GenerateCodeInClass(thisClassDeclaration, ref classBuildContext);
                }
            }
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
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
}
