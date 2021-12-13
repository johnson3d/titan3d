using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle.Editor
{
    public class UParticleGraph : EGui.Controls.NodeGraph.NodeGraph
    {
        public UParticleEditor NebulaEditor;
        protected override void ShowAddNode(Vector2 posMenu)
        {
            if (ImGuiAPI.MenuItem($"AddEmitter", null, false, true))
            {
                var node = new UEmitterNode();
                node.NebulaEditor = NebulaEditor;
                node.Position = View2WorldSpace(ref posMenu);
                this.AddNode(node);
            }
            if (ImGuiAPI.BeginMenu("Shapes", true))
            {
                if (ImGuiAPI.MenuItem($"Box", null, false, true))
                {
                    var node = new UBoxEmitShapeNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"Sphere", null, false, true))
                {
                    var node = new USphereEmitShapeNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("Effector", true))
            {
                if (ImGuiAPI.MenuItem($"Accelerated", null, false, true))
                {
                    var node = new UAcceleratedEffectorNode();
                    node.NebulaEditor = NebulaEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node); 
                }
                ImGuiAPI.EndMenu();
            }
        }
    }
}
