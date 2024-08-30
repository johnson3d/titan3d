using EngineNS.Animation.BlendTree;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.Bricks.StateMachine;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Linq.Expressions;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.DesignMacross.Base.Description;

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
    public class TtBlendTreeClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "BlendTree";
        [Rtti.Meta]
        [DrawInGraph]
        public TtBlendTree_PoseOutputClassDescription PoseOutput { get; set; } = new TtBlendTree_PoseOutputClassDescription();
        [Rtti.Meta]
        [DrawInGraph]
        public List<TtBlendTreeNodeClassDescription> Nodes { get; set; } = new List<TtBlendTreeNodeClassDescription>();
        [Rtti.Meta, DrawInGraph]
        public List<TtPoseLineDescription> PoseLines { get; set; } = new();
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
        public void AddPoseLine(TtPoseLineDescription poseLine)
        {
            PoseLines.Add(poseLine);
            poseLine.Parent = this;
        }

        public bool RemovePoseLine(TtPoseLineDescription poseLine)
        {
            PoseLines.Remove(poseLine);
            poseLine.Parent = null;
            return true;
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

        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.TtLocalSpacePoseBlendTree");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }
        public override void GenerateCodeInClass(UClassDeclaration classDeclaration)
        {
            base.GenerateCodeInClass(classDeclaration);


        }
    }
}
