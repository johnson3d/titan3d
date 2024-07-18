using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.Particle.Editor
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class UParticleNode : NodeGraph.UNodeBase
    {
        internal UParticleEditor NebulaEditor;
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as TtParticleGraph;
            if (funcGraph == null || oPin.LinkDesc == null || iPin.LinkDesc == null)
            {
                return;
            }
            if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        public override void OnLButtonClicked(NodePin hitPin)
        {
            var funcGraph = ParentGraph as TtParticleGraph;
            var editor = funcGraph.Editor as UParticleEditor;
            editor.NodePropGrid.Target = this;
        }
    }
    public class UEmitterNode : UParticleNode
    {
        public PinOut Shapes { get; set; } = new PinOut()
        {
            MultiLinks = true,
        };
        public PinOut Effectors { get; set; } = new PinOut()
        {
            MultiLinks = true,
        };
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
        [Category("Option")]
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
        public RName MeshName
        {
            get; set;
        }
        [Category("Option")]
        public bool IsGpuDriven { get; set; } = true;
        [Category("Option")]
        public int MaxParticle { get; set; } = 1024;
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
        public TtShapeBox Shape = new TtShapeBox();
        public UBoxEmitShapeNode()
        {
            Name = "BoxShape";
        }
    }
    public class USphereEmitShapeNode : UEmitShapeNode
    {
        public TtShapeSphere Shape = new TtShapeSphere();
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
