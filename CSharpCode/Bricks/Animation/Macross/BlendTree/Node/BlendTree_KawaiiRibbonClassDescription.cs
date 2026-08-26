using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Bricks.Animation.KawaiiPhysics;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.Animation.Macross.BlendTree
{
    /// <summary>
    /// DMC node description for the ribbon wave overlay.
    ///
    /// Deliberately a peer of KawaiiPhysics rather than a list inside it: the ribbon only
    /// generates a procedural wave, and physics is obtained by wiring its pose output into a
    /// downstream KawaiiPhysics node. That node then treats the waving pose as the animated pose,
    /// so its Stiffness / collision / WorldDamping act as corrections on top of the wave. Because
    /// composition already gives that for free, this node exposes no physics parameters at all.
    /// </summary>
    [AnimBlendTreeContextMenu("KawaiiRibbon", "BlendTreeNode\\KawaiiRibbon", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BlendTree_KawaiiRibbon))]
    [TtPropertyOrder(Order = TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtBlendTree_KawaiiRibbonClassDescription : TtBlendTreeNodeClassDescription
    {
        public override string Name { get => "KawaiiRibbon"; }

        [Rtti.Meta("")]
        [Category("Ribbon")]
        public List<TtKawaiiRibbonSetup> RibbonSetups { get; set; } = new();

        public TtPoseInPinDescription InPin
        {
            get
            {
                return PoseInPins[0];
            }
        }

        [Category("Pins"), DisplayName("Alpha")]
        public TtDataInPinDescription AlphaPin { get => DataInPins[0]; }

        public TtBlendTree_KawaiiRibbonClassDescription()
        {
            AddPoseInPin(new TtPoseInPinDescription());
            AddPoseOutPin(new TtPoseOutPinDescription());

            AddDataInPin(new() { Name = "Alpha", TypeDesc = TtTypeDescGetter<Single>.TypeDesc, TypeVaule = "1" });
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            var mainClass = classBuildContext.MainClassDescription as TtClassDescription;
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_KawaiiRibbon<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod(ref classBuildContext));
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        public override void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            base.GenerateCodeInClass(classDeclaration, ref classBuildContext);
        }

        public override TtStatementBase BuildBlendTreeStatement(ref FBlendTreeBuildContext blendTreeBuildContext)
        {
            var blendTreeNode = blendTreeBuildContext.BlendTreeDescription;
            var linkedNode = blendTreeNode.GetLinkedBlendTreeNode(InPin);
            if (linkedNode != null)
            {
                var fromNodeAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("FromNode", new TtVariableReferenceExpression(VariableName)),
                    new TtVariableReferenceExpression(linkedNode.VariableName));
                blendTreeBuildContext.AddStatement(fromNodeAssin);

                FBlendTreeBuildContext buildContext = new() { ExecuteSequenceStatement = new(), BlendTreeDescription = blendTreeBuildContext.BlendTreeDescription };
                var statement = linkedNode.BuildBlendTreeStatement(ref blendTreeBuildContext);
                blendTreeBuildContext.AddStatement(buildContext.ExecuteSequenceStatement);
                return statement;
            }
            return null;
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();

            TtASTBuildUtil.CreateList("TempRibbonSetups", RibbonSetups, methodDeclaration);

            var ribbonSetupsAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                                               new TtVariableReferenceExpression("RibbonSetups"),
                                               new TtVariableReferenceExpression("TempRibbonSetups"));
            methodDeclaration.MethodBody.Sequence.Add(ribbonSetupsAssin);

            var linkedPin = IDataLineOperator.GetLinkedDataPin(Parent, AlphaPin);
            if (linkedPin == null)
            {
                var alphaAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("Alpha", new TtVariableReferenceExpression("CommandDesc")),
                    new TtPrimitiveExpression(AlphaPin.TypeDesc, AlphaPin.GetTypeValue()));
                methodDeclaration.MethodBody.Sequence.Add(alphaAssign);
            }

            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);
            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}
