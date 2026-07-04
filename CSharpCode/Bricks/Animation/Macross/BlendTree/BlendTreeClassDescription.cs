using Assimp;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree
{
    public class AnimBlendTreeContextMenuAttribute : ContextMenuAttribute
    {
        public AnimBlendTreeContextMenuAttribute(string filterStrings, string menuPaths, params string[] keyStrings) : base(filterStrings, menuPaths, keyStrings)
        {
        }
    }

    [OutlineElement_Leaf(typeof(TtOutlineElement_BlendTreeGraph))]
    [Designable(typeof(TtLocalSpacePoseBlendTree), "BlendTree")]
    [Graph(typeof(TtGraph_BlendTree))]
    public class TtBlendTreeClassDescription : TtDesignableVariableDescription, IDataLineOperator, IPoseLineOperator, IExpressionOperator, IStatementOperator
    {
        [Rtti.Meta("")]
        [Category("Option")]
        public override string Name { get; set; } = "BlendTree";
        [Rtti.Meta("")]
        [DrawInGraph]
        public TtBlendTree_PoseOutputClassDescription PoseOutput { get; set; } = new TtBlendTree_PoseOutputClassDescription();
        [Rtti.Meta("")]
        [DrawInGraph]
        public List<TtBlendTreeNodeClassDescription> Nodes { get; set; } = new List<TtBlendTreeNodeClassDescription>();
        [Rtti.Meta, DrawInGraph]
        public List<TtPoseLineDescription> PoseLines { get; set; } = new();
        [Rtti.Meta, DrawInGraph]
        public List<TtDataLineDescription> DataLines { get; set; } = new();
        [Rtti.Meta, DrawInGraph]
        public List<TtExpressionDescription> Expressions { get; set; } = new();
        [Rtti.Meta, DrawInGraph]
        public List<TtStatementDescription> Statements { get; set; } = new();

        #region Node
        public bool AddNode(TtBlendTreeNodeClassDescription node)
        {
            Nodes.Add(node);
            node.Parent = this;
            return true;
        }
        public bool RemoveNode(TtBlendTreeNodeClassDescription node)
        {
            Nodes.Remove(node);
            node.Parent = null;
            return true;
        }
        public TtBlendTreeNodeClassDescription GetLinkedBlendTreeNode(TtPosePinDescription pin)
        {
            Guid linkedPinId = Guid.Empty;
            foreach (var line in PoseLines)
            {
                if (line.FromId == pin.Id)
                {
                    linkedPinId = line.ToId;
                }
                if (line.ToId == pin.Id)
                {
                    linkedPinId = line.FromId;
                }
            }
            if (linkedPinId == Guid.Empty)
            {
                return null;
            }
            foreach (var node in Nodes)
            {
                if (node.TryGetPosePin(linkedPinId, out var linkedPin))
                {
                    return node;
                }
            }
            return null;
        }
        #endregion

        #region IPoseLineOperator
        public void AddPoseLine(TtPoseLineDescription poseLine)
        {
            if(ContainsDataLineBetweenPins(poseLine.FromId, poseLine.ToId))
            {
                return;
            };
            PoseLines.Add(poseLine);
            poseLine.Parent = this;
        }

        public bool RemovePoseLine(TtPoseLineDescription poseLine)
        {
            PoseLines.Remove(poseLine);
            poseLine.Parent = null;
            return true;
        }
        public TtPoseLineDescription GetPoseLineWithPin(TtPosePinDescription dataPin)
        {
            foreach (var poseLine in PoseLines)
            {
                if (poseLine.FromId == dataPin.Id)
                {
                    return poseLine;
                }
                if (poseLine.ToId == dataPin.Id)
                {
                    return poseLine;
                }
            }
            return null;
        }


        public TtPosePinDescription GetLinkedPosePin(TtPosePinDescription posePin)
        {
            var linkedPinId = Guid.Empty;
            foreach (var dataLine in PoseLines)
            {
                if (dataLine.FromId == posePin.Id)
                {
                    linkedPinId = dataLine.ToId;
                    break;
                }
                if (dataLine.ToId == posePin.Id)
                {
                    linkedPinId = dataLine.FromId;
                    break;
                }
            }
            foreach (var node in Nodes)
            {
                if (node.TryGetPosePin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            return null;
        }
        public bool ContainsPoseLineBetweenPins(TtPosePinDescription pinA, TtPosePinDescription pinB)
        {
            return ContainsPoseLineBetweenPins(pinA.Id, pinB.Id);
        }
        public bool ContainsPoseLineBetweenPins(Guid pinAId, Guid pinBId)
        {
            foreach (var poseLine in PoseLines)
            {
                if ((poseLine.FromId == pinAId && poseLine.ToId == pinBId) ||
                    (poseLine.FromId == pinBId && poseLine.ToId == pinAId))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IDataLineOperator
        public void AddDataLine(TtDataLineDescription dataLine)
        {
            if(ContainsDataLineBetweenPins(dataLine.FromId, dataLine.ToId))
            {
                return;
            }
            DataLines.Add(dataLine);
            dataLine.Parent = this;

            var fromPin = GetDataPinById(dataLine.FromId);
            var toPin = GetDataPinById(dataLine.ToId);
            System.Diagnostics.Debug.Assert(fromPin != null && toPin != null);
            IDataPinOperator.OnDataPinConnected(fromPin.Parent, fromPin, toPin, this);
            IDataPinOperator.OnDataPinConnected(toPin.Parent, toPin, fromPin, this);
        }
        public TtDataPinDescription GetDataPinById(Guid dataPinId)
        {
            foreach (var node in Nodes)
            {
                if (node.TryGetDataPin(dataPinId, out var dataPin))
                {
                    return dataPin;
                }
            }
            foreach (var statement in Statements)
            {
                if (statement.TryGetDataPin(dataPinId, out var dataPin))
                {
                    return dataPin;
                }
            }

            foreach (var exp in Expressions)
            {
                if (exp.TryGetDataPin(dataPinId, out var dataPin))
                {
                    return dataPin;
                }
            }
            return null;
        }
        public bool RemoveDataLine(TtDataLineDescription dataLine)
        {
            DataLines.Remove(dataLine);
            dataLine.Parent = null;
            var fromPin = GetDataPinById(dataLine.FromId);
            var toPin = GetDataPinById(dataLine.ToId);

            if (fromPin != null && fromPin.Parent != null)
            {
                IDataPinOperator.OnDataPinDisConnected(fromPin.Parent, fromPin, toPin, this);
            }
            if (toPin != null && toPin.Parent != null)
            {
                IDataPinOperator.OnDataPinDisConnected(toPin.Parent, toPin, fromPin, this);
            }
            return true;
        }
        public TtDataPinDescription GetLinkedDataPin(TtDataPinDescription dataPin)
        {
            var linkedPinId = Guid.Empty;
            foreach (var dataLine in DataLines)
            {
                if (dataLine.FromId == dataPin.Id)
                {
                    linkedPinId = dataLine.ToId;
                    break;
                }
                if (dataLine.ToId == dataPin.Id)
                {
                    linkedPinId = dataLine.FromId;
                    break;
                }
            }
            foreach (var statement in Statements)
            {
                if (statement.TryGetDataPin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }

            foreach (var exp in Expressions)
            {
                if (exp.TryGetDataPin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            foreach (var node in Nodes)
            {
                if (node.TryGetDataPin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            return null;
        }

        public TtDataLineDescription GetDataLineWithPin(TtDataPinDescription dataPin)
        {
            foreach (var dataLine in DataLines)
            {
                if (dataLine.FromId == dataPin.Id)
                {
                    return dataLine;
                }
                if (dataLine.ToId == dataPin.Id)
                {
                    return dataLine;
                }
            }
            return null;
        }
        public List<TtDataLineDescription> GetDataLinesWithPin(TtDataPinDescription dataPin)
        {
            List<TtDataLineDescription> result = new();
            foreach (var dataLine in DataLines)
            {
                if (dataLine.FromId == dataPin.Id)
                {
                    result.Add(dataLine);
                }
                if (dataLine.ToId == dataPin.Id)
                {
                    result.Add(dataLine);
                }
            }
            return result;
        }

        public bool ContainsDataLineBetweenPins(TtDataPinDescription pinA, TtDataPinDescription pinB)
        {
            return ContainsDataLineBetweenPins(pinA.Id, pinB.Id);
        }
        public bool ContainsDataLineBetweenPins(Guid pinAId, Guid pinBId)
        {
            foreach (var dataLine in DataLines)
            {
                if ((dataLine.FromId == pinAId && dataLine.ToId == pinBId) ||
                    (dataLine.FromId == pinBId && dataLine.ToId == pinAId))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion


        #region IExepressionOperator 
        public void AddExpression(TtExpressionDescription expression)
        {
            Expressions.Add(expression);
            expression.Parent = this;
        }
        public bool RemoveExpression(TtExpressionDescription expression)
        {
            Expressions.Remove(expression);
            expression.Parent = null;
            return true;
        }
        #endregion

        #region IStatementOperator
        public void AddStatement(TtStatementDescription statement)
        {
            Statements.Add(statement);
            statement.Parent = this;
        }
        public bool RemoveStatement(TtStatementDescription statement)
        {
            Statements.Remove(statement);
            statement.Parent = null;
            return true;
        }
        #endregion

        public override void UpdateData(ref FDescriptionUpdateContext updateContext)
        {
            foreach (var node in Nodes)
            {
                node.UpdateData(ref updateContext);
            }
            foreach (var expression in Expressions)
            {
                expression.UpdateData(ref updateContext);
            }
            foreach (var statement in Statements)
            {
                statement.UpdateData(ref updateContext);
            }
            foreach (var poseLine in PoseLines)
            {
                poseLine.UpdateData(ref updateContext);
            }
            foreach (var dataLines in DataLines)
            {
                dataLines.UpdateData(ref updateContext);
            }
            base.UpdateData(ref updateContext);
        }


        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

            classDeclarationsBuilded.AddRange(PoseOutput.BuildClassDeclarations(ref classBuildContext));
            thisClassDeclaration.Properties.Add(PoseOutput.BuildVariableDeclaration(ref classBuildContext));
            foreach (var blendTreeNode in Nodes)
            {
                var decl = blendTreeNode.BuildClassDeclarations(ref classBuildContext);
                if (decl == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Error, $"BuildClassDeclarations: Node ({blendTreeNode.Name}) build failed");
                    return null;
                }
                classDeclarationsBuilded.AddRange(decl);
                thisClassDeclaration.Properties.Add(blendTreeNode.BuildVariableDeclaration(ref classBuildContext));
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }
        void GenerateCodeInMainClassInitMethod(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            var initMethod = classDeclaration.FindMethod("Initialize");
            if (initMethod == null)
            {
                initMethod = TtASTBuildUtil.CreateInitMethodDeclaration();
                classDeclaration.AddMethod(initMethod);
            }

            var blendTreeContext_VarName = "blendTreeContext" + VariableName;

            var blendTreeContextCreate = TtASTBuildUtil.CreateVariableDeclaration(blendTreeContext_VarName,
                new TtTypeReference(typeof(FAnimBlendTreeContext)),
                new TtCreateObjectExpression(typeof(FAnimBlendTreeContext).FullName));
            initMethod.MethodBody.Sequence.Add(blendTreeContextCreate);

            var animatableSkeletonPose_VarName = "animatableSkeletonPose" + VariableName;

            var animatableSkeletonPoseCreate = TtASTBuildUtil.CreateVariableDeclaration(animatableSkeletonPose_VarName,
                new TtTypeReference(typeof(TtAnimatableSkeletonPose)), new TtNullValueExpression());
            initMethod.MethodBody.Sequence.Add(animatableSkeletonPoseCreate);

            var getAnimatablePoseFromNode = new TtMethodInvokeStatement("CreateAnimatableSkeletonPoseFromeNode",
                animatableSkeletonPoseCreate,
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<TtAnimUtil>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(getAnimatablePoseFromNode);


            var blendTreeContextAnimatablePoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("AnimatableSkeletonPose", new TtVariableReferenceExpression(blendTreeContext_VarName)),
                new TtVariableReferenceExpression(animatableSkeletonPose_VarName));
            initMethod.MethodBody.Sequence.Add(blendTreeContextAnimatablePoseAssign);

            var finalPoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(VariableName), new TtCreateObjectExpression(VariableType.TypeFullName));
            initMethod.MethodBody.Sequence.Add(finalPoseAssign);

            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(VariableName)),
                new TtSelfReferenceExpression());
            initMethod.MethodBody.Sequence.Add(centerDataAssign);

            var finalPoseInitializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression(blendTreeContext_VarName) });
            finalPoseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(finalPoseInitializeInvoke);

            var runtimePose_VarName = "runtimePose";
            var runtimePoseCreate = TtASTBuildUtil.CreateVariableDeclaration(runtimePose_VarName, new TtTypeReference(typeof(TtLocalSpaceRuntimePose)), new TtNullValueExpression());
            initMethod.MethodBody.Sequence.Add(runtimePoseCreate);

            var bindRuntimeSkeletonPoseToNode = new TtMethodInvokeStatement("BindRuntimeSkeletonPoseToNode",
                runtimePoseCreate,
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<TtAnimUtil>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(bindRuntimeSkeletonPoseToNode);

            var setPose = new TtMethodInvokeStatement("SetPose",
                null,
                new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(runtimePose_VarName) }
                );
            initMethod.MethodBody.Sequence.Add(setPose);

            var baseInitializeInvoke = new TtMethodInvokeStatement("Initialize", null, new TtBaseReferenceExpression());
            baseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(baseInitializeInvoke);
        }

        void GenerateCodeInMainClassAfterTickMethod(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            var afterTickMethod = classDeclaration.FindMethod("AfterTick");
            if (afterTickMethod == null)
            {
                afterTickMethod = TtASTBuildUtil.CreateAfterTickMethodDeclaration();
                classDeclaration.AddMethod(afterTickMethod);
            }
            var blendTreeContext_VarName = "blendTreeContext" + VariableName;

            var blendTreeContextCreate = TtASTBuildUtil.CreateVariableDeclaration(blendTreeContext_VarName,
                new TtTypeReference(typeof(FAnimBlendTreeContext)),
                new TtCreateObjectExpression(typeof(FAnimBlendTreeContext).FullName));
            afterTickMethod.MethodBody.Sequence.Add(blendTreeContextCreate);

            var tickInvoke = new TtMethodInvokeStatement("Tick",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression("elapseSecond") },
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Ref, Expression = new TtVariableReferenceExpression(blendTreeContext_VarName) });
            afterTickMethod.MethodBody.Sequence.Add(tickInvoke);

        }
        public override void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            base.GenerateCodeInClass(classDeclaration, ref classBuildContext);

            GenerateCodeInMainClassInitMethod(classDeclaration, ref classBuildContext);

            GenerateCodeInMainClassAfterTickMethod(classDeclaration, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();
            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);
            TtAnimASTBuildUtil.CreateNewThenCenterDataAssignThenInitInvokeStatement(PoseOutput, methodDeclaration);
            foreach (var node in Nodes)
            {
                TtAnimASTBuildUtil.CreateNewThenCenterDataAssignThenInitInvokeStatement(node, methodDeclaration);
            }

            FBlendTreeBuildContext buildContext = new() { ExecuteSequenceStatement = new(), BlendTreeDescription = this };
            PoseOutput.BuildBlendTreeStatement(ref buildContext);
            methodDeclaration.MethodBody.Sequence.Add(buildContext.ExecuteSequenceStatement);
            var fromNodeAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("FromNode"),
                    new TtVariableReferenceExpression(PoseOutput.VariableName));
            methodDeclaration.MethodBody.Sequence.Add(fromNodeAssin);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion

       
    }
}
