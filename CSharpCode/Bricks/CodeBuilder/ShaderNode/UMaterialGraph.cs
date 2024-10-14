using EngineNS.Bricks.CodeBuilder.ShaderNode.Operator;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.Linq;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class TtMaterialGraphBase : UNodeGraph
    {
        public virtual string EditorKeyword { get; }
        public TtMaterialGraphBase()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(ContextMenuAttribute), true);
                    if (atts.Length > 0)
                    {
                        var parentMenu = CanvasMenus;
                        var att = atts[0] as ContextMenuAttribute;
                        if (!att.HasKeyString(EditorKeyword))
                            continue;
                        for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                        {
                            var menuStr = att.MenuPaths[menuIdx];
                            var menuName = GetMenuName(menuStr);
                            if (menuIdx < att.MenuPaths.Length - 1)
                                parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                            else
                            {
                                parentMenu.AddMenuItem(menuName, att.FilterStrings, null,
                                    (TtMenuItem item, object sender) =>
                                    {
                                        var node = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as UNodeBase;
                                        var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                        if (nodeName != null)
                                            node.Name = nodeName;
                                        node.UserData = this;
                                        node.Position = PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);
                                    });
                            }
                        }
                    }
                }
            }
            //var kls = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(typeof(Control.HLSLMethod).FullName);
            var methods = TtEngine.Instance.HLSLMethodManager.Methods;
            var funcMenu = CanvasMenus.AddMenuItem("Function", null, null);
            foreach (var i in methods.Values)
            {
                var attrs = i.GetMethod().GetCustomAttributes(typeof(Bricks.CodeBuilder.ShaderNode.Control.UserCallNodeAttribute), false);
                var menuAtts = i.GetMethod().GetCustomAttributes(typeof(ContextMenuAttribute), true);
                TtMenuItem.FMenuAction action = (TtMenuItem item, object sender) =>
                {
                    Control.CallNode node;
                    if (attrs.Length > 0)
                    {
                        var data = (attrs[0] as Bricks.CodeBuilder.ShaderNode.Control.UserCallNodeAttribute);
                        node = Rtti.TtTypeDescManager.CreateInstance(data.CallNodeType, null) as Control.CallNode;
                        node.Initialize(i);
                    }
                    else
                    {
                        node = Control.CallNode.NewMethodNode(i);
                    }
                    node.Name = i.MethodName;
                    node.UserData = this;
                    node.Position = PopMenuPosition;
                    SetDefaultActionForNode(node);
                    this.AddNode(node);
                };
                if (menuAtts.Length > 0)
                {
                    var parentMenu = funcMenu;
                    var att = menuAtts[0] as ContextMenuAttribute;
                    if (!att.HasKeyString(EditorKeyword))
                        continue;
                    for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                    {
                        var menuStr = att.MenuPaths[menuIdx];
                        var menuName = GetMenuName(menuStr);
                        if (menuIdx < att.MenuPaths.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                        else
                        {
                            parentMenu.AddMenuItem(menuName, att.FilterStrings, null, action);
                        }
                    }
                }
                else
                {
                    funcMenu.AddMenuItem(i.MethodName, null, action);
                }
            }
            var uniformVarMenus = CanvasMenus.AddMenuItem("UniformVars", null, null);
            var perFrameMenus = uniformVarMenus.AddMenuItem("PerFrame", null, null);
            var members = TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerFrame.GetType().GetFields();
            foreach (var i in members)
            {
                var attrs = i.GetCustomAttributes(typeof(NxRHI.TtShader.UShaderVarAttribute), false);
                if (attrs.Length == 0)
                    continue;
                perFrameMenus.AddMenuItem(i.Name, null,
                    (TtMenuItem item, object sender) =>
                    {
                        var node = new UUniformVar();
                        node.VarType = Rtti.TtTypeDesc.TypeOf((attrs[0] as NxRHI.TtShader.UShaderVarAttribute).VarType);
                        node.Name = i.Name;
                        node.UserData = this;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }
            var perCameraMenus = uniformVarMenus.AddMenuItem("PerCamera", null, null);
            var cameraMembers = TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerCamera.GetType().GetFields();
            foreach (var i in cameraMembers)
            {
                var attrs = i.GetCustomAttributes(typeof(NxRHI.TtShader.UShaderVarAttribute), false);
                if (attrs.Length == 0)
                    continue;
                perCameraMenus.AddMenuItem(i.Name, null,
                    (TtMenuItem item, object sender) =>
                    {
                        var node = new UUniformVar();
                        node.VarType = Rtti.TtTypeDesc.TypeOf((attrs[0] as NxRHI.TtShader.UShaderVarAttribute).VarType);
                        node.Name = i.Name;
                        node.UserData = this;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }
            var inputVarMenus = CanvasMenus.AddMenuItem("InputVars", null, null);
            var psInputMenus = inputVarMenus.AddMenuItem("PSInput", null, null);
            psInputMenus.AddMenuItem("Input", null,
                (TtMenuItem item, object sender) =>
                {
                    var node = new UUniformVar();
                    node.VarType = Rtti.TtTypeDesc.TypeOf(typeof(Graphics.Pipeline.Shader.PS_INPUT));
                    node.Name = "input";
                    node.UserData = this;
                    node.Position = PopMenuPosition;
                    SetDefaultActionForNode(node);
                    this.AddNode(node);
                });
            System.Reflection.FieldInfo[] psInputmembers = typeof(Graphics.Pipeline.Shader.PS_INPUT).GetFields();
            foreach (var i in psInputmembers)
            {
                psInputMenus.AddMenuItem(i.Name, null,
                    (TtMenuItem item, object sender) =>
                    {
                        var node = new UUniformVar();
                        node.VarType = Rtti.TtTypeDesc.TypeOf(i.FieldType);
                        if (i.Name == "bIsFrontFace")
                        {
                            node.Name = "input.GetIsFrontFace()";
                        }
                        else
                            node.Name = "input." + i.Name;
                        node.UserData = this;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }
        }
        public override void SetDefaultActionForNode(UNodeBase node)
        {
            node.OnLButtonClickedAction = NodeLButtonClickedAction;
            node.OnLinkedFromAction = NodeOnLinkedFrom;
            //node.OnPreReadAction = NodeOnPreRead;

            node.UserData = this;
        }
        //int Seed = 5;
        public override void OnDrawAfter(Bricks.NodeGraph.TtGraphRenderer renderer, UNodeGraphStyles styles, ImDrawList cmdlist)
        {
            //var O = WindowPos + new Vector2(GraphViewSize.X / 2, GraphViewSize.Y);

            //mRand = new System.Random(Seed);
            //Tree(ref O, Math.PI / 2, 100, 10, ref cmdlist);
        }
        System.Random mRand = new System.Random(1);
        private void Tree(ref Vector2 O, double angle, double length, float width, ref ImDrawList cmdlist)
        {
            if (width < 1)
                width = 1;
            if (length < 10)//差不多树枝尽头的时候停止绘制
            {
                return;
            }
            Vector2 p = new Vector2(O.X + (int)(length * Math.Cos(angle)), O.Y - (int)(length * Math.Sin(angle)));

            cmdlist.AddLine(in O, in p, 0xFFFFFFFF, width);

            Tree(ref p, angle + Math.PI / 18 * (0.9f + mRand.NextDouble() * 0.2), length * 0.8 * (0.9f + mRand.NextDouble() * 0.2), width * 0.8f, ref cmdlist);
            Tree(ref p, angle - Math.PI / 18 * (0.9f + mRand.NextDouble() * 0.2), length * 0.8 * (0.9f + mRand.NextDouble() * 0.2), width * 0.8f, ref cmdlist);
        }

        private void NodeLButtonClickedAction(UNodeBase node, NodePin clickedPin)
        {

        }
        private void NodeOnLinkedFrom(UNodeBase node, PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as TtMaterialGraph;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.ShaderNode.UMaterialGraph@EngineCore", "EngineNS.Bricks.CodeBuilder.ShaderNode.UMaterialGraph" })]
    public partial class TtMaterialGraph : TtMaterialGraphBase
    {
        public const string MaterialEditorKeyword = "Material";
        public override string EditorKeyword { get => MaterialEditorKeyword; }
        public TtMaterialEditor ShaderEditor;
    }

    public partial class TtMaterialFunctionGraph : TtMaterialGraphBase
    {
        public const string MaterialEditorKeyword = "MaterialFunction";
        public override string EditorKeyword { get => MaterialEditorKeyword; }
        public TtMaterialFunctionEditor ShaderEditor;
    }
}
