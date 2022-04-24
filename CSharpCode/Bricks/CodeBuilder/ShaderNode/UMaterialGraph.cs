using EngineNS.Bricks.CodeBuilder.ShaderNode.Operator;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class UMaterialGraph : UNodeGraph
    {
        public UMaterialGraph()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        public UShaderEditor ShaderEditor;
        uint CurSerialId = 0;
        public uint GenSerialId()
        {
            return CurSerialId++;
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";
            var oprations = CanvasMenus.AddMenuItem("Operation", null, null);
            {
                oprations.AddMenuItem("+", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new AddNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("-", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new SubNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("*", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new MulNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("/", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new DivNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("%", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new ModNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("&", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new BitAndNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("|", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new BitOrNode();
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
            }
            var Datas = CanvasMenus.AddMenuItem("Data", null, null);
            {
                Datas.AddMenuItem("float", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.VarDimF1();
                        node.Name = $"f1_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("float2", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.VarDimF2();
                        node.Name = $"f2_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("float3", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.VarDimF3();
                        node.Name = $"f3_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("float4", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.VarDimF4();
                        node.Name = $"f3_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("Color3", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.VarColor3();
                        node.Name = $"clr3_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("Color4", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.VarColor4();
                        node.Name = $"clr4_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("texture2d", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.Texture2D();
                        node.Name = $"tex2d_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("texture2dArray", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.Texture2DArray();
                        node.Name = $"tex2dArray_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                Datas.AddMenuItem("sampler", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Var.SamplerState();
                        node.Name = $"sampler_{GenSerialId()}";
                        node.Graph = this;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
            }
        }
        public override void OnAfterDrawMenu(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            if (ImGuiAPI.BeginMenu("Function", true))
            {
                var kls = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(typeof(Control.HLSLMethod).FullName);
                foreach(var i in kls.Methods)
                {
                    if (ImGuiAPI.MenuItem(i.Method.Name, null, false, true))
                    {
                        var attrs = i.Method.GetCustomAttributes(typeof(Control.UserCallNodeAttribute), false);
                        if (attrs.Length > 0)
                        {
                            var usr = attrs[0] as Control.UserCallNodeAttribute;
                            if (usr.CallNodeType != null)
                            {
                                var node = Rtti.UTypeDescManager.CreateInstance(usr.CallNodeType) as Control.CallNode;
                                if (node != null)
                                {
                                    node.Initialize(i);
                                    node.Name = i.Method.Name;
                                    node.Graph = this;
                                    node.Position = PopMenuPosition;
                                    this.AddNode(node);
                                }
                            }
                            else
                            {
                                var node = Control.CallNode.NewMethodNode(i);
                                node.Name = i.Method.Name;
                                node.Graph = this;
                                node.Position = PopMenuPosition;
                                this.AddNode(node);
                            }
                        }
                        else
                        {
                            var node = Control.CallNode.NewMethodNode(i);
                            node.Name = i.Method.Name;
                            node.Graph = this;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        }
                    }
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("UniformVars", true))
            {
                if (ImGuiAPI.BeginMenu("PerFrame", true))
                {
                    System.Reflection.FieldInfo[] members = RHI.CConstantBuffer.PerFrameType.GetFields();
                    foreach (var i in members)
                    {
                        var attrs = i.GetCustomAttributes(typeof(RHI.CConstantBuffer.UShaderTypeAttribute), false);
                        if (attrs.Length == 0)
                            continue;
                        if (ImGuiAPI.MenuItem(i.Name, null, false, true))
                        {
                            var node = new UUniformVar();
                            node.VarType = Rtti.UTypeDesc.TypeOf((attrs[0] as RHI.CConstantBuffer.UShaderTypeAttribute).ShaderType);
                            node.Name = i.Name;
                            node.Graph = this;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        }
                    }
                    ImGuiAPI.EndMenu();
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("InputVars", true))
            {
                if (ImGuiAPI.BeginMenu("PSInput", true))
                {
                    {
                        if (ImGuiAPI.MenuItem("Input", null, false, true))
                        {
                            var node = new UUniformVar();
                            node.VarType = Rtti.UTypeDesc.TypeOf(typeof(Graphics.Pipeline.Shader.UMaterial.PSInput));
                            node.Name = "input";
                            node.Graph = this;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        }   
                    }
                    System.Reflection.FieldInfo[] members = typeof(Graphics.Pipeline.Shader.UMaterial.PSInput).GetFields();
                    foreach (var i in members)
                    {
                        if (ImGuiAPI.MenuItem(i.Name, null, false, true))
                        {
                            var node = new UUniformVar();
                            node.VarType = Rtti.UTypeDesc.TypeOf(i.FieldType);
                            node.Name = "input." + i.Name;
                            node.Graph = this;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        }
                    }
                    ImGuiAPI.EndMenu();
                }
                ImGuiAPI.EndMenu();
            }
        }

        //int Seed = 5;
        public override void OnDrawAfter(Bricks.NodeGraph.UGraphRenderer renderer, EGui.Controls.NodeGraph.NodeGraphStyles styles, ImDrawList cmdlist)
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
    }
}
