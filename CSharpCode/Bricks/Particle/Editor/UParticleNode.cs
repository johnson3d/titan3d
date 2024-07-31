using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxPhysics;
using EngineNS.Rtti;

namespace EngineNS.Bricks.Particle.Editor
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtParticleNode : NodeGraph.UNodeBase
    {
        internal TtParticleEditor NebulaEditor;
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as TtParticleGraph;
            if (funcGraph == null || oPin.LinkDesc == null || iPin.LinkDesc == null)
            {
                return;
            }
            //if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        public override void OnLButtonClicked(NodePin hitPin)
        {
            var funcGraph = ParentGraph as TtParticleGraph;
            var editor = funcGraph.Editor as TtParticleEditor;
            editor.NodePropGrid.Target = this;
        }
    }
    [Bricks.CodeBuilder.ContextMenu(filterStrings: "McEmitter", "Emitter\\McEmitter", Editor.TtParticleGraph.NebulaEditorKeyword)]
    public class TtEmitterNode : TtParticleNode
    {
        public PinOut Shapes { get; set; } = new PinOut()
        {
            MultiLinks = false,
        };
        public PinOut Effectors { get; set; } = new PinOut()
        {
            MultiLinks = true,
        };
        public TtEmitterNode()
        {
            Shapes.LinkDesc = TtParticleEditor.NewInOutPinDesc("Shape");
            Effectors.LinkDesc = TtParticleEditor.NewInOutPinDesc("EffectorQueue");
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
        [Rtti.Meta]
        public bool Enable { get; set; } = true;
        [Category("Option")]
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
        public RName MeshName
        {
            get; set;
        }
        [Category("Option")]
        [Rtti.Meta]
        [RName.PGRName(FilterExts = CodeBuilder.UMacross.AssetExt, MacrossType = typeof(TtEmitterMacross))]
        public RName McName
        {
            get;
            set;
        } = null;
        [Category("Option")]
        [Rtti.Meta]
        public bool IsGpuDriven { get; set; } = true;
        [Category("Option")]
        [Rtti.Meta]
        public uint MaxParticle { get; set; } = 1024;
        [Category("Option")]
        [Rtti.Meta]
        public string DefaultCurrentQueue { get; set; } = "Default";
        [Category("Option")]
        [Rtti.Meta]
        public string EmitterName { get; set; } = "Default";

        public TtEmitter EditingObject = null;
        Vector3 mLocation;
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 Location
        {
            get => mLocation;
            set
            {
                mLocation = value;
                if (EditingObject != null)
                {
                    EditingObject.Location = value;
                }
            }
        }
        Vector3 mVelocity;
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 Velocity
        {
            get => mVelocity;
            set
            {
                mVelocity = value;
                if (EditingObject != null)
                {
                    EditingObject.Velocity = value;
                }
            }
        }
        public virtual UTypeDesc CreateEmitterType()
        {
            return UTypeDescGetter<TtEmitter>.TypeDesc;
        }
        public virtual void InitEmitter(TtEmitter emt)
        {
            emt.EmitterData.Location = Location;
            emt.EmitterData.Velocity = Velocity;
            emt.McName = McName;
            //emt.EmitterData.Flags = Flags;
        }
    }
    public class TtEmitShapeNode : TtParticleNode
    {
        public PinIn Left { get; set; } = new PinIn()
        {
            MultiLinks = false,
        };
        public PinOut Right { get; set; } = new PinOut()
        {
            MultiLinks = false,
        };
        public TtEmitShapeNode()
        {
            Left.LinkDesc = TtParticleEditor.NewInOutPinDesc("Shape");
            Right.LinkDesc = TtParticleEditor.NewInOutPinDesc("Shape");
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

            var nodeExpr = OutNode as TtEffectorNode;
            if (nodeExpr != null)
                return false;

            return true;
        }
        public virtual TtShape CreateShape()
        {
            return null;
        }
        public TtShape EditingObject = null;
    }

    [Bricks.CodeBuilder.ContextMenu(filterStrings: "EffectorQueue", "EffectorQueue", TtParticleGraph.NebulaEditorKeyword)]
    public class TtEffectorQueueNode : TtParticleNode
    {
        [Category("Option")]
        [Rtti.Meta]
        public string QueueName { get; set; } = "Default";
        public PinIn Left { get; set; } = new PinIn()
        {
            MultiLinks = false,
        };
        public PinOut Right { get; set; } = new PinOut()
        {
            MultiLinks = false,
        };
        public TtEffectorQueueNode()
        {
            Left.LinkDesc = TtParticleEditor.NewInOutPinDesc("EffectorQueue");
            Right.LinkDesc = TtParticleEditor.NewInOutPinDesc("Effector");
            Left.Name = "in";
            Right.Name = "out";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            Name = "Queue:" + QueueName;

            AddPinIn(Left);
            AddPinOut(Right);
        }
    }
    public class TtEffectorNode : TtParticleNode
    {
        public PinIn Left { get; set; } = new PinIn()
        {
            MultiLinks = false,
        };
        public PinOut Right { get; set; } = new PinOut()
        {
            MultiLinks = false,
        };
        public TtEffectorNode()
        {
            Left.LinkDesc = TtParticleEditor.NewInOutPinDesc("Effector");
            Right.LinkDesc = TtParticleEditor.NewInOutPinDesc("Effector");
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

            var nodeExpr = OutNode as TtEmitShapeNode;
            if (nodeExpr != null)
                return false;

            return true;
        }
        public virtual TtEffector CreateEffector()
        {
            return null;
        }
        public TtEffector EditingObject = null;
    }

    [Bricks.CodeBuilder.ContextMenu(filterStrings: "BoxShape", "Shape\\BoxShape", TtParticleGraph.NebulaEditorKeyword)]
    public class TtBoxEmitShapeNode : TtEmitShapeNode
    {
        public TtShapeBox Shape = new TtShapeBox();
        public TtBoxEmitShapeNode()
        {
            Name = "BoxShape";
        }
        Vector3 mCenter = Vector3.Zero;
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 Center 
        { 
            get => mCenter;
            set
            {
                mCenter = value;
                var shape = EditingObject as Bricks.Particle.TtShapeBox;
                if (shape != null)
                {
                    shape.Center = value;
                }
            }
        }
        Vector3 mHalfExtent = Vector3.One;
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 HalfExtent 
        { 
            get => mHalfExtent;
            set
            {
                mHalfExtent = value;
                var shape = EditingObject as Bricks.Particle.TtShapeBox;
                if (shape != null)
                {
                    shape.HalfExtent = value;
                }
            }
        }
        float mThinness = 1.0f;
        [Category("Option")]
        [Rtti.Meta]
        public float Thinness 
        { 
            get => mThinness;
            set
            {
                mThinness = value;
                var shape = EditingObject as Bricks.Particle.TtShapeBox;
                if (shape != null)
                {
                    shape.Thinness = value;
                }
            }
        }
        public override TtShape CreateShape()
        {
            var boxShape = new Bricks.Particle.TtShapeBox();
            boxShape.Center = Center;
            boxShape.HalfExtent = HalfExtent;
            boxShape.Thinness = Thinness;
            return boxShape;
        }
    }
    [Bricks.CodeBuilder.ContextMenu(filterStrings: "SphereShape", "Shape\\SphereShape", TtParticleGraph.NebulaEditorKeyword)]
    public class TtSphereEmitShapeNode : TtEmitShapeNode
    {
        public TtShapeSphere Shape = new TtShapeSphere();
        public TtSphereEmitShapeNode()
        {
            Name = "SphereShape";
        }
        Vector3 mCenter = Vector3.Zero;
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 Center
        {
            get => mCenter;
            set
            {
                mCenter = value;
                var shape = EditingObject as Bricks.Particle.TtShapeSphere;
                if (shape != null)
                {
                    shape.Center = value;
                }
            }
        }
        float mRadius = 1.0f;
        [Category("Option")]
        [Rtti.Meta]
        public float Radius
        {
            get => mRadius;
            set
            { 
                mRadius = value;
                var shape = EditingObject as Bricks.Particle.TtShapeSphere;
                if (shape != null)
                {
                    shape.Radius = value;
                }
            }
        }
        [Category("Option")]
        [Rtti.Meta]
        public float Thinness { get; set; } = 1.0f;
        public override TtShape CreateShape()
        {
            var sphereShape = new Bricks.Particle.TtShapeSphere();
            sphereShape.Center = Center;
            sphereShape.Radius = Radius;
            sphereShape.Thinness = Thinness;
            return sphereShape;
        }
    }

    [Bricks.CodeBuilder.ContextMenu(filterStrings: "Accelerated", "Effector\\Accelerated", TtParticleGraph.NebulaEditorKeyword)]
    public class TtAcceleratedEffectorNode : TtEffectorNode
    {
        public TtAcceleratedEffectorNode()
        {
            Name = "Accelerated";
        }
        Vector3 mAcceleration = new Vector3(0, -0.1f, 0);
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 Acceleration 
        { 
            get => mAcceleration;
            set
            {
                mAcceleration = value;
                var effector = EditingObject as TtAcceleratedEffector;
                if (effector != null)
                {
                    effector.Acceleration = mAcceleration;
                }
            }
        }
        public override TtEffector CreateEffector()
        {
            var result = new TtAcceleratedEffector();
            result.Acceleration = Acceleration;
            return result;
        }
    }
}
