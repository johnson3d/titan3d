using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Particle.Editor
{
    public partial class UParticleNode : NodeGraph.UNodeBase
    {
        internal UParticleEditor NebulaEditor;
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as UParticleGraph;
            if (funcGraph == null || oPin.LinkDesc == null || iPin.LinkDesc == null)
            {
                return;
            }
            if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
    }
    public class UEmitterNode : UParticleNode
    {
        public PinOut Shapes { get; set; } = new PinOut();
        public PinOut Effectors { get; set; } = new PinOut();
        public UEmitterNode()
        {
            Shapes.LinkDesc = UParticleEditor.NewInOutPinDesc();
            Effectors.LinkDesc = UParticleEditor.NewInOutPinDesc();
            Shapes.Name = "Shapes";
            Effectors.Name = "Effectors";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            Name = "Emitter";

            AddPinOut(Shapes);
            AddPinOut(Effectors);
        }
    }
    public class UEmitShapeNode : UParticleNode
    {
        public PinIn Left { get; set; } = new PinIn();
        public PinOut Right { get; set; } = new PinOut();
        public UEmitShapeNode()
        {
            Left.LinkDesc = UParticleEditor.NewInOutPinDesc();
            Right.LinkDesc = UParticleEditor.NewInOutPinDesc();
            Left.Name = "in";
            Right.Name = "out";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            Name = "EmitShape";

            AddPinIn(Left);
            AddPinOut(Right);
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
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
        public PinIn Left { get; set; } = new PinIn();
        public PinOut Right { get; set; } = new PinOut();
        public UEffectorNode()
        {
            Left.LinkDesc = UParticleEditor.NewInOutPinDesc();
            Right.LinkDesc = UParticleEditor.NewInOutPinDesc();
            Left.Name = "in";
            Right.Name = "out";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            Name = "Effector";

            AddPinIn(Left);
            AddPinOut(Right);
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
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
