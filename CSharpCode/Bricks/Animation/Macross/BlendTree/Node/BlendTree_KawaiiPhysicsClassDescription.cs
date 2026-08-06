using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.KawaiiPhysics;
using EngineNS.Bricks.Animation.Macross.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using Jither.OpenEXR.Attributes;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;

namespace EngineNS.Animation.Macross.BlendTree
{
    [AnimBlendTreeContextMenu("KawaiiPhysics", "BlendTreeNode\\KawaiiPhysics", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_BlendTree_KawaiiPhysics))]
    [TtPropertyOrder(Order = TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtBlendTree_KawaiiPhysicsClassDescription : TtBlendTreeNodeClassDescription
    {
        public override string Name { get => "KawaiiPhysics"; }
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public List<TtKawaiiChainSetup> ChainSetups { get; set; } = new();
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public List<TtKawaiiClothSetup> ClothSetups { get; set; } = new();
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public List<TtKawaiiRodSetup> RodSetups { get; set; } = new();

        /// <summary>
        /// When true, the uniform ComponentPhysicsSettings below overrides every chain/rod's
        /// own per-setup PhysicsSettings at runtime (KawaiiComponent.Initialize applies it to
        /// each setup). Set false to let each chain's PhysicsSettings (under ChainSetups) drive
        /// the simulation.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public bool ApplyComponentPhysicsSettings { get; set; } = true;

        /// <summary>
        /// When true, the component's world-space movement (actor placement change) drives
        /// hair inertia: dynamic particles lag behind the moving root via length constraints,
        /// producing flow/swing effects. When false, particles are unaffected by actor movement.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public bool EnableComponentMovementPhysics { get; set; } = true;

        /// <summary>
        /// Uniform physics settings applied to all chains/rods when ApplyComponentPhysicsSettings is true.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public TtKawaiiPhySettings ComponentPhysicsSettings { get; set; } = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettings();

        /// <summary>
        /// Randomization range for the component-level physics settings.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public TtKawaiiPhySettings ComponentPhysicsSettingsRandom { get; set; } = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettingsRandom();

        /// <summary>
        /// 网格空间重力 (m/s²)。引擎是 Y-up + 米制(人高约 1.5、PhysX 主场景重力 -9.8),
        /// 所以默认是 (0,-9.8,0)，而不是上游 KawaiiPhysics 的厘米值 -980。
        /// </summary>
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public Vector3 Gravity { get; set; } = new Vector3(0, -9.8f, 0);

        /// <summary>
        /// 乘在 <see cref="Gravity"/> 上的系数: 1 = 引擎重力, 0 = 失重头发, &gt;1 = 更重。
        /// </summary>
        [Rtti.Meta("")]
        [Category("Kawaii")]
        public float GravityScale { get; set; } = 1.0f;

        public TtPoseInPinDescription InPin
        {
            get
            {
                return PoseInPins[0];
            }
        }

        [Category("Pins"), DisplayName("Alpha")]
        public TtDataInPinDescription AlphaPin { get => DataInPins[0]; }

        //[Category("Pins"), DisplayName("PhysicsSettings")]
        //public TtDataInPinDescription PhysicsSettingsPin { get => DataInPins[1]; }
        
        //[Category("Pins"), DisplayName("PhysicsSettingsRandom")]
        //public TtDataInPinDescription PhysicsSettingsRandomPin { get => DataInPins[2]; }
        public TtBlendTree_KawaiiPhysicsClassDescription()
        {
            AddPoseInPin(new TtPoseInPinDescription());
            AddPoseOutPin(new TtPoseOutPinDescription());

            AddDataInPin(new() { Name = "Alpha", TypeDesc = TtTypeDescGetter<Single>.TypeDesc, TypeVaule = "1" });
            //AddDataInPin(new() { 
            //                     Name = "Physics Settings", 
            //                     TypeDesc = TtTypeDescGetter<Vector3>.TypeDesc, 
            //                     TypeVaule = TtDataInPinDescription.GetDefaultVale(TtTypeDescGetter<Vector3>.TypeDesc).ToString() 
            //                   });
            //AddDataInPin(new() { Name = "Physics Settings Random", TypeDesc = TtTypeDescGetter<float>.TypeDesc, TypeVaule = "1" });
        }
        
        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            var mainClass = classBuildContext.MainClassDescription as TtClassDescription;
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_KawaiiPhysics<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod(ref classBuildContext));
            thisClassDeclaration.AddMethod(BuildOverrideTickMethod(ref classBuildContext));
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
            else
            {
                //empty method
            }
            return null;
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();
            
            //ChainSetups.Add(new TtKawaiiChainSetup());
            TtASTBuildUtil.CreateList("TempChainSetups", ChainSetups, methodDeclaration);
            TtASTBuildUtil.CreateList("TempClothSetups", ClothSetups, methodDeclaration);
            TtASTBuildUtil.CreateList("TempRodSetups", RodSetups, methodDeclaration);

            var chainSetupsAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                                               new TtVariableReferenceExpression("ChainSetups"),
                                               new TtVariableReferenceExpression("TempChainSetups"));
            var clothSetupsAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                                               new TtVariableReferenceExpression("ClothSetups"),
                                               new TtVariableReferenceExpression("TempClothSetups"));
            var rodSetupsAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                                               new TtVariableReferenceExpression("RodSetups"),
                                               new TtVariableReferenceExpression("TempRodSetups"));

            methodDeclaration.MethodBody.Sequence.Add(chainSetupsAssin);
            methodDeclaration.MethodBody.Sequence.Add(clothSetupsAssin);
            methodDeclaration.MethodBody.Sequence.Add(rodSetupsAssin);

            // Component-level physics settings -> runtime KawaiiComponent. These are emitted
            // BEFORE base.Initialize so KawaiiComponent.Initialize can propagate them to setups.
            // NOTE: must use the single-arg TtPrimitiveExpression(bool) overload. The two-arg
            // (TtTypeDesc, bool) overload is a typeof() generator and would emit "typeof(System.Boolean)".
            var applyComponentAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("ApplyComponentPhysicsSettings", new TtVariableReferenceExpression("KawaiiComponent")),
                new TtPrimitiveExpression(ApplyComponentPhysicsSettings));
            methodDeclaration.MethodBody.Sequence.Add(applyComponentAssign);

            // EnableComponentMovementPhysics -> KawaiiComponent
            // NOTE: must use single-arg TtPrimitiveExpression(bool) overload (same trap as ApplyComponentPhysicsSettings)
            var enableMovementAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("EnableComponentMovementPhysics", new TtVariableReferenceExpression("KawaiiComponent")),
                new TtPrimitiveExpression(EnableComponentMovementPhysics));
            methodDeclaration.MethodBody.Sequence.Add(enableMovementAssign);

            // Gravity / GravityScale -> KawaiiComponent (米制、网格空间)
            var gravityAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("Gravity", new TtVariableReferenceExpression("KawaiiComponent")),
                new TtPrimitiveExpression(Gravity));
            methodDeclaration.MethodBody.Sequence.Add(gravityAssign);

            var gravityScaleAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("GravityScale", new TtVariableReferenceExpression("KawaiiComponent")),
                new TtPrimitiveExpression(GravityScale));
            methodDeclaration.MethodBody.Sequence.Add(gravityScaleAssign);

            TtASTBuildUtil.CreateItem(typeof(TtKawaiiPhySettings), ComponentPhysicsSettings, "TempComponentPhysicsSettings", methodDeclaration);
            var componentPhyAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("PhysicsSettings", new TtVariableReferenceExpression("KawaiiComponent")),
                new TtVariableReferenceExpression("TempComponentPhysicsSettings"));
            methodDeclaration.MethodBody.Sequence.Add(componentPhyAssign);

            TtASTBuildUtil.CreateItem(typeof(TtKawaiiPhySettings), ComponentPhysicsSettingsRandom, "TempComponentPhysicsSettingsRandom", methodDeclaration);
            var componentPhyRandomAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("PhysicsSettingsRandom", new TtVariableReferenceExpression("KawaiiComponent")),
                new TtVariableReferenceExpression("TempComponentPhysicsSettingsRandom"));
            methodDeclaration.MethodBody.Sequence.Add(componentPhyRandomAssign);

            var linkedPin = IDataLineOperator.GetLinkedDataPin(Parent, AlphaPin);
            if(linkedPin == null)
            {
                var alphaAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression("Alpha", new TtVariableReferenceExpression("CommandDesc")), new TtPrimitiveExpression(AlphaPin.TypeDesc, AlphaPin.GetTypeValue()));
                methodDeclaration.MethodBody.Sequence.Add(alphaAssign);
            }

            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);
            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        private TtMethodDeclaration BuildOverrideTickMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedTickMethodStatement();
            TtAnimASTBuildUtil.CreateBaseTickInvokeStatementRefContext(methodDeclaration);

            var linkedPin = IDataLineOperator.GetLinkedDataPin(Parent, AlphaPin);

            if (linkedPin != null)
            {
                TtExecuteSequenceStatement sequence = new TtExecuteSequenceStatement();
                TtExpressionBase rightSide = null;
                if (linkedPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { OwnerDescription = Parent, ClassBuildContext = classBuildContext , Sequence = sequence };
                    rightSide = expressionDescription.BuildExpression(ref buildContext);
                }
                if (linkedPin.Parent is TtPureMethodInvokeDescription pureStatementDescription)
                {
                    FStatementBuildContext buildContext = new()
                    {
                        ExecuteSequenceStatement = sequence,
                        OwnerDescription = Parent,
                        ClassBuildContext = classBuildContext
                    };
                    pureStatementDescription.BuildStatement(ref buildContext);
                    methodDeclaration.MethodBody.Sequence.Add(sequence);
                    rightSide = pureStatementDescription.BuildExpressionForOutPin(linkedPin);
                }
                else if(linkedPin.Parent is TtMethodInvokeDescription statementDescription)
                {

                }
                methodDeclaration.MethodBody.Sequence.Add(sequence);
                var alphaAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression("Alpha", new TtVariableReferenceExpression("CommandDesc")), rightSide);


                methodDeclaration.MethodBody.Sequence.Add(alphaAssign);
            }

            var fromNodeTickInvoke = new TtMethodInvokeStatement("Tick",
            null, new TtVariableReferenceExpression("FromNode"),
            new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("elapseSecond") },
            new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("context"), OperationType = EMethodArgumentAttribute.Ref });
            methodDeclaration.MethodBody.Sequence.Add(fromNodeTickInvoke);

            return methodDeclaration;
        }
        #endregion

        #region ISerializer
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
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


        #endregion ISerializer
    }
}
