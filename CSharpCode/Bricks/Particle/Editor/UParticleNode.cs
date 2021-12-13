using EngineNS.EGui.Controls.NodeGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle.Editor
{
    public partial class UParticleNode : EGui.Controls.NodeGraph.NodeBase
    {
        internal UParticleEditor NebulaEditor;
        public override void OnLinkedFrom(PinIn iPin, NodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as UParticleGraph;
            if (funcGraph == null || oPin.Link == null || iPin.Link == null)
            {
                return;
            }
            if (iPin.Link.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
    }
    public class UEmitterNode : UParticleNode
    {
        public EGui.Controls.NodeGraph.PinOut Shapes { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public EGui.Controls.NodeGraph.PinOut Effectors { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public UEmitterNode()
        {
            Shapes.Link = UParticleEditor.NewInOutPinDesc();
            Effectors.Link = UParticleEditor.NewInOutPinDesc();
            Shapes.Name = "Shapes";
            Effectors.Name = "Effectors";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = 0xFF204020;
            Background.Color = 0x80808080;
            Name = "Emitter";

            AddPinOut(Shapes);
            AddPinOut(Effectors);
        }
    }
    public class UEmitShapeNode : UParticleNode
    {
        public EGui.Controls.NodeGraph.PinIn Left { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public EGui.Controls.NodeGraph.PinOut Right { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public UEmitShapeNode()
        {
            Left.Link = UParticleEditor.NewInOutPinDesc();
            Right.Link = UParticleEditor.NewInOutPinDesc();
            Left.Name = "in";
            Right.Name = "out";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = 0xFF204020;
            Background.Color = 0x80808080;
            Name = "EmitShape";

            AddPinIn(Left);
            AddPinOut(Right);
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UEffectorNode;
            if (nodeExpr != null)
                return false;

            if (oPin.Name == "Effectors")
                return false;
            return true;
        }
    }
    public class UEffectorNode : UParticleNode
    {
        public EGui.Controls.NodeGraph.PinIn Left { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public EGui.Controls.NodeGraph.PinOut Right { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public UEffectorNode()
        {
            Left.Link = UParticleEditor.NewInOutPinDesc();
            Right.Link = UParticleEditor.NewInOutPinDesc();
            Left.Name = "in";
            Right.Name = "out";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = 0xFF204020;
            Background.Color = 0x80808080;
            Name = "Effector";

            AddPinIn(Left);
            AddPinOut(Right);
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UEmitShapeNode;
            if (nodeExpr != null)
                return false;

            if (oPin.Name == "Shapes")
                return false;
            return true;
        }
    }

    public class UBoxEmitShapeNode : UEmitShapeNode
    {
        public UShapeBox Shape = new UShapeBox();
        public UBoxEmitShapeNode()
        {
            Name = "BoxShape";
        }
    }
    public class USphereEmitShapeNode : UEmitShapeNode
    {
        public UShapeSphere Shape = new UShapeSphere();
        public USphereEmitShapeNode()
        {
            Name = "SphereShape";
        }
    }

    public class UAcceleratedEffectorNode : UEffectorNode
    {
        public UAcceleratedEffectorNode()
        {
            Name = "Accelerated";
        }
    }
}
