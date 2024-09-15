using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace EngineNS.Bricks.Particle.Editor
{
    public class TtParticleGraph : NodeGraph.UNodeGraph
    {
        public const string NebulaEditorKeyword = "Nebula";
        public TtParticleGraph()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        public TtParticleEditor NebulaEditor { get => Editor as TtParticleEditor; }
        static void GetNodeNameAndMenuStr(in string menuString, TtParticleGraph graph, ref string nodeName, ref string menuName)
        {
            menuName = menuString;
            nodeName = menuName;
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(Bricks.CodeBuilder.ContextMenuAttribute), true);
                    if (atts.Length > 0)
                    {
                        var parentMenu = CanvasMenus;
                        var att = atts[0] as Bricks.CodeBuilder.ContextMenuAttribute;
                        if (!att.HasKeyString(NebulaEditorKeyword))
                            continue;
                        for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                        {
                            var menuStr = att.MenuPaths[menuIdx];
                            string nodeName = null;
                            GetNodeNameAndMenuStr(menuStr, this, ref nodeName, ref menuStr);
                            if (menuIdx < att.MenuPaths.Length - 1)
                                parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                            else
                            {
                                parentMenu.AddMenuItem(menuStr, att.FilterStrings, null,
                                    (TtMenuItem item, object sender) =>
                                    {
                                        var node = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as TtParticleNode;
                                        if (nodeName != null)
                                            node.Name = nodeName;
                                        node.UserData = this;
                                        node.NebulaEditor = NebulaEditor;
                                        node.Position = PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);
                                    });
                            }
                        }
                    }
                }
            }

            //CanvasMenus.SubMenuItems.Clear();
            //CanvasMenus.Text = "Canvas";

            //CanvasMenus.AddMenuItem(
            //    "SimpleEmitter", null,
            //    (TtMenuItem item, object sender) =>
            //    {
            //        var node = new Simple.TtSimpleEmitterNode();
            //        node.NebulaEditor = NebulaEditor;
            //        node.Position = PopMenuPosition;
            //        this.AddNode(node);
            //    });
            //var shapes = CanvasMenus.AddMenuItem("Shape", null, null);
            //{
            //    shapes.AddMenuItem("Box", null, (TtMenuItem item, object sender) =>
            //    {
            //        var node = new TtBoxEmitShapeNode();
            //        node.NebulaEditor = NebulaEditor;
            //        node.Position = PopMenuPosition;
            //        this.AddNode(node);
            //    });
            //    shapes.AddMenuItem("Sphere", null, (TtMenuItem item, object sender) =>
            //    {
            //        var node = new TtSphereEmitShapeNode();
            //        node.NebulaEditor = NebulaEditor;
            //        node.Position = PopMenuPosition;
            //        this.AddNode(node);
            //    });
            //}
            //CanvasMenus.AddMenuItem(
            //    "EffectorQueue", null,
            //    (TtMenuItem item, object sender) =>
            //    {
            //        var node = new TtEffectorQueueNode();
            //        node.NebulaEditor = NebulaEditor;
            //        node.Position = PopMenuPosition;
            //        this.AddNode(node);
            //    });
            //var effectors = CanvasMenus.AddMenuItem("Effector", null, null);
            //{
            //    effectors.AddMenuItem("Accelerated", null, (TtMenuItem item, object sender) =>
            //    {
            //        var node = new TtAcceleratedEffectorNode();
            //        node.NebulaEditor = NebulaEditor;
            //        node.Position = PopMenuPosition;
            //        this.AddNode(node);
            //    });
            //}
        }
    }
}
