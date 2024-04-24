using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;

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
                (TtMenuItem item, object sender) =>
                {
                    var node = new UEmitterNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
            var shapes = CanvasMenus.AddMenuItem("Shape", null, null);
            {
                shapes.AddMenuItem("Box", null, (TtMenuItem item, object sender) =>
                {
                    var node = new UBoxEmitShapeNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
                shapes.AddMenuItem("Sphere", null, (TtMenuItem item, object sender) =>
                {
                    var node = new USphereEmitShapeNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
            }
            var effectors = CanvasMenus.AddMenuItem("Effector", null, null);
            {
                effectors.AddMenuItem("Accelerated", null, (TtMenuItem item, object sender) =>
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
