using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Particle.Editor
{
    public class UParticleGraph : NodeGraph.UNodeGraph
    {
        public UParticleGraph()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        public UParticleEditor NebulaEditor;
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            CanvasMenus.AddMenuItem(
                "AddEmitter", null,
                (UMenuItem item, object sender) =>
                {
                    var node = new UEmitterNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
            var shapes = CanvasMenus.AddMenuItem("Shape", null, null);
            {
                shapes.AddMenuItem("Box", null, (UMenuItem item, object sender) =>
                {
                    var node = new UBoxEmitShapeNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
                shapes.AddMenuItem("Sphere", null, (UMenuItem item, object sender) =>
                {
                    var node = new USphereEmitShapeNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
            }
            var effectors = CanvasMenus.AddMenuItem("Effector", null, null);
            {
                effectors.AddMenuItem("Accelerated", null, (UMenuItem item, object sender) =>
                {
                    var node = new UAcceleratedEffectorNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
            }
        }
    }
}
