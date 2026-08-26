using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.RBF;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace EngineNS.Animation.Macross.BlendTree
{
    /// <summary>
    /// PoseDriver 节点描述: 用一根源骨骼的当前姿态, 经 RBF 求出各 target 权重,
    /// 再把 PoseAsset 中对应的命名 pose 按权重混合进来。对标 UE AnimNode_PoseDriver。
    ///
    /// 首版为单源骨骼 + Additive/Interpolative 均可; 多源骨骼见后续扩展。
    /// </summary>
    [AnimBlendTreeContextMenu("PoseDriver", "BlendTreeNode\\PoseDriver", UDesignMacross.MacrossAnimEditorKeyword)]
    // [GraphElement] 不是可选的: TtGraph_BlendTree.ConstructElements 会对每个节点 description
    // 取 GraphElementAttribute 并 Debug.Assert(!= null), 缺了就会在新建节点时断言失败。
    // 本节点无特殊渲染需求, 直接用通用的 TtGraphElement_BlendTreeNode。
    [GraphElement(typeof(TtGraphElement_BlendTreeNode))]
    [TtPropertyOrder(Order = TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtBlendTree_PoseDriverClassDescription : TtBlendTreeNodeClassDescription
    {
        public override string Name { get => "PoseDriver"; }

        /// <summary> 提供 driven pose 的 PoseAsset。 </summary>
        [Rtti.Meta("")]
        [Category("PoseDriver")]
        [RName.PGRName(FilterExts = EngineNS.Animation.Asset.TtPoseAsset.AssetExt)]
        public RName PoseAsset { get; set; }

        /// <summary> 源骨骼: 用它的当前姿态作为 RBF 输入。 </summary>
        [Rtti.Meta("")]
        [Category("PoseDriver")]
        [TtSkeletonBoneIndexPickerEditor]
        public LimbIndexInSkeleton SourceBone { get; set; } = LimbIndexInSkeleton.CrteateDefault();

        /// <summary>
        /// 第 2 根源骨骼(可选)。多源骨骼时, 每根向 RBF 输入贡献一组三维量,
        /// target 上需相应填写 TargetEulerDegrees2 / TargetTranslation2。
        /// 上限 3 根: target 内不能用集合字段(GenCode 不支持), 故用固定字段。
        /// </summary>
        [Rtti.Meta("")]
        [Category("PoseDriver")]
        [TtSkeletonBoneIndexPickerEditor]
        public LimbIndexInSkeleton SourceBone2 { get; set; } = LimbIndexInSkeleton.CrteateDefault();

        /// <summary> 第 3 根源骨骼(可选)。 </summary>
        [Rtti.Meta("")]
        [Category("PoseDriver")]
        [TtSkeletonBoneIndexPickerEditor]
        public LimbIndexInSkeleton SourceBone3 { get; set; } = LimbIndexInSkeleton.CrteateDefault();

        /// <summary> 用源骨骼的旋转还是位移作为驱动量。 </summary>
        [Rtti.Meta("")]
        [Category("PoseDriver")]
        public EPoseDriverSource DriveSource { get; set; } = EPoseDriverSource.Rotation;

        /// <summary> RBF target 列表: 每个 target 是一个采样姿态 + 它驱动的 pose 名。 </summary>
        [Rtti.Meta("")]
        [Category("PoseDriver")]
        public List<TtPoseDriverTarget> PoseTargets { get; set; } = new();

        // ─── RBF 参数 ─────────────────────────────────────────────────────
        [Rtti.Meta("")]
        [Category("RBF")]
        public ERBFSolverType SolverType { get; set; } = ERBFSolverType.Additive;

        /// <summary> 各 target 的影响半径, 单位为度。AutomaticRadius 为真时忽略。 </summary>
        [Rtti.Meta("")]
        [Category("RBF")]
        public float Radius { get; set; } = 45.0f;

        [Rtti.Meta("")]
        [Category("RBF")]
        public bool AutomaticRadius { get; set; } = false;

        [Rtti.Meta("")]
        [Category("RBF")]
        public ERBFFunctionType Function { get; set; } = ERBFFunctionType.Gaussian;

        [Rtti.Meta("")]
        [Category("RBF")]
        public ERBFDistanceMethod DistanceMethod { get; set; } = ERBFDistanceMethod.SwingAngle;

        /// <summary> SwingAngle / TwistAngle 使用的扭转轴。 </summary>
        [Rtti.Meta("")]
        [Category("RBF")]
        public ERBFTwistAxis TwistAxis { get; set; } = ERBFTwistAxis.X;

        [Rtti.Meta("")]
        [Category("RBF")]
        public float WeightThreshold { get; set; } = 0.0001f;

        [Rtti.Meta("")]
        [Category("RBF")]
        public ERBFNormalizeMethod NormalizeMethod { get; set; } = ERBFNormalizeMethod.OnlyNormalizeAboveOne;

        // ─── 驱动范围(bone mask) ─────────────────────────────────
        // 对标 UE 的 bOnlyDriveSelectedBones + OnlyDriveBones。这里用"根骨骼 + 子树"
        // 而不是逐骨骼列表: 逐骨骼需要集合字段(GenCode 不支持), 而实际用法里
        // "只驱动某条肢体/某部分"本质就是一个骨骼子树。

        /// <summary>
        /// 为 true 时只驱动下面指定的骨骼子树, 其它骨骼保持输入姿势。
        /// 用于矫正型 pose(例如只影响肩部)不干扰全身动作。
        /// </summary>
        [Rtti.Meta("")]
        [Category("DriveScope")]
        public bool OnlyDriveSelectedBones { get; set; } = false;

        /// <summary> 驱动范围根骨骼 1(含其全部子骨骼)。 </summary>
        [Rtti.Meta("")]
        [Category("DriveScope")]
        [TtSkeletonBoneIndexPickerEditor]
        public LimbIndexInSkeleton DriveBoneRoot1 { get; set; } = LimbIndexInSkeleton.CrteateDefault();

        /// <summary> 驱动范围根骨骼 2。 </summary>
        [Rtti.Meta("")]
        [Category("DriveScope")]
        [TtSkeletonBoneIndexPickerEditor]
        public LimbIndexInSkeleton DriveBoneRoot2 { get; set; } = LimbIndexInSkeleton.CrteateDefault();

        /// <summary> 驱动范围根骨骼 3。 </summary>
        [Rtti.Meta("")]
        [Category("DriveScope")]
        [TtSkeletonBoneIndexPickerEditor]
        public LimbIndexInSkeleton DriveBoneRoot3 { get; set; } = LimbIndexInSkeleton.CrteateDefault();

        public TtPoseInPinDescription InPin
        {
            get { return PoseInPins[0]; }
        }

        [Category("Pins"), DisplayName("Alpha")]
        public TtDataInPinDescription AlphaPin { get => DataInPins[0]; }

        public TtBlendTree_PoseDriverClassDescription()
        {
            AddPoseInPin(new TtPoseInPinDescription());
            AddPoseOutPin(new TtPoseOutPinDescription());
            AddDataInPin(new() { Name = "Alpha", TypeDesc = TtTypeDescGetter<System.Single>.TypeDesc, TypeVaule = "1" });
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseDriver<{classBuildContext.MainClassDescription.ClassName}>");
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
            return null;
        }

        #region Internal AST Build

        /// <summary>
        /// 生成一句“运行时字段 = 骨骼名字符串”。骨骼引用在编辑期是
        /// LimbIndexInSkeleton(带选择器 UI), 但运行时按名查找就够了。
        /// </summary>
        void EmitBoneNameAssign(TtMethodDeclaration method, string runtimeFieldName, LimbIndexInSkeleton bone)
        {
            var boneName = (bone != null && bone.Name != null) ? bone.Name : "";
            method.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression(runtimeFieldName),
                new TtPrimitiveExpression(boneName)));
        }

        private TtMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();

            // target 列表: List<复杂对象> 必须手动 CreateList(逐项走 CreateItem)
            TtASTBuildUtil.CreateList("TempPoseTargets", PoseTargets, methodDeclaration);
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("PoseTargets"),
                new TtVariableReferenceExpression("TempPoseTargets")));

            // PoseAsset: RName 没有 TtPrimitiveExpression 重载, 走 RName.ParseFrom(字符串)
            if (PoseAsset != null)
            {
                var parseRName = new TtMethodInvokeStatement("ParseFrom",
                    TtASTBuildUtil.CreateVariableDeclaration("TempPoseAssetName", new TtTypeReference(typeof(RName)), null),
                    new TtClassReferenceExpression(TtTypeDesc.TypeOf<RName>()),
                    new TtMethodInvokeArgumentExpression { Expression = new TtPrimitiveExpression(PoseAsset.ToString()) });
                methodDeclaration.MethodBody.Sequence.Add(parseRName);
                methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("PoseAssetName"),
                    new TtVariableReferenceExpression("TempPoseAssetName")));
            }

            // 源骨骼: 编辑期用带选择器的 LimbIndexInSkeleton, 运行时只需要名字
            EmitBoneNameAssign(methodDeclaration, "SourceBoneName", SourceBone);
            EmitBoneNameAssign(methodDeclaration, "SourceBoneName2", SourceBone2);
            EmitBoneNameAssign(methodDeclaration, "SourceBoneName3", SourceBone3);

            // 驱动范围(bone mask)
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("OnlyDriveSelectedBones"),
                new TtPrimitiveExpression(OnlyDriveSelectedBones)));
            EmitBoneNameAssign(methodDeclaration, "DriveBoneRoot1", DriveBoneRoot1);
            EmitBoneNameAssign(methodDeclaration, "DriveBoneRoot2", DriveBoneRoot2);
            EmitBoneNameAssign(methodDeclaration, "DriveBoneRoot3", DriveBoneRoot3);

            // 枚举有专用的 TtPrimitiveExpression(Enum) 重载
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("DriveSource"),
                new TtPrimitiveExpression(DriveSource)));

            // RBF 参数
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("SolverType"), new TtPrimitiveExpression(SolverType)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("Radius"), new TtPrimitiveExpression(Radius)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("AutomaticRadius"), new TtPrimitiveExpression(AutomaticRadius)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("Function"), new TtPrimitiveExpression(Function)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("DistanceMethod"), new TtPrimitiveExpression(DistanceMethod)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("TwistAxis"), new TtPrimitiveExpression(TwistAxis)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("WeightThreshold"), new TtPrimitiveExpression(WeightThreshold)));
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("NormalizeMethod"), new TtPrimitiveExpression(NormalizeMethod)));

            // Alpha: 未连线时用 pin 上的常量值
            var linkedPin = IDataLineOperator.GetLinkedDataPin(Parent, AlphaPin);
            if (linkedPin == null)
            {
                methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("Alpha", new TtVariableReferenceExpression("CommandDesc")),
                    new TtPrimitiveExpression(AlphaPin.TypeDesc, AlphaPin.GetTypeValue())));
            }

            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);
            methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                new TtPrimitiveExpression(true)));
            return methodDeclaration;
        }

        private TtMethodDeclaration BuildOverrideTickMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedTickMethodStatement();
            TtAnimASTBuildUtil.CreateBaseTickInvokeStatementRefContext(methodDeclaration);

            // Alpha 连线时每帧取值
            var linkedPin = IDataLineOperator.GetLinkedDataPin(Parent, AlphaPin);
            if (linkedPin != null)
            {
                TtExecuteSequenceStatement sequence = new TtExecuteSequenceStatement();
                TtExpressionBase rightSide = null;
                if (linkedPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { OwnerDescription = Parent, ClassBuildContext = classBuildContext, Sequence = sequence };
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
                methodDeclaration.MethodBody.Sequence.Add(sequence);
                methodDeclaration.MethodBody.Sequence.Add(TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("Alpha", new TtVariableReferenceExpression("CommandDesc")), rightSide));
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
