using EngineNS.Bricks.CodeBuilder.ShaderNode.Operator;
using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public class UMaterialGraph : EGui.Controls.NodeGraph.NodeGraph
    {
        public UShaderEditor ShaderEditor;
        uint CurSerialId = 0;
        public uint GenSerialId()
        {
            return CurSerialId++;
        }
        protected override void ShowAddNode(Vector2 posMenu)
        {
            if (ImGuiAPI.BeginMenu("Operation", true))
            {
                if (ImGuiAPI.MenuItem($"+", null, false, true))
                {
                    var node = new AddNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"-", null, false, true))
                {
                    var node = new SubNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"*", null, false, true))
                {
                    var node = new MulNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"/", null, false, true))
                {
                    var node = new DivNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"%", null, false, true))
                {
                    var node = new ModNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"&", null, false, true))
                {
                    var node = new BitAndNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"|", null, false, true))
                {
                    var node = new BitOrNode();
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("Data", true))
            {
                if (ImGuiAPI.MenuItem($"float", null, false, true))
                {
                    var node = new Var.VarDimF1();
                    node.Name = $"f1_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"float2", null, false, true))
                {
                    var node = new Var.VarDimF2();
                    node.Name = $"f2_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"float3", null, false, true))
                {
                    var node = new Var.VarDimF3();
                    node.Name = $"f3_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"float4", null, false, true))
                {
                    var node = new Var.VarDimF4();
                    node.Name = $"f4_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"Color3", null, false, true))
                {
                    var node = new Var.VarColor3();
                    node.Name = $"clr3_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"Color4", null, false, true))
                {
                    var node = new Var.VarColor4();
                    node.Name = $"clr4_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"texture2d", null, false, true))
                {
                    var node = new Var.Texture2D();
                    node.Name = $"tex2d_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"sampler", null, false, true))
                {
                    var node = new Var.SamplerState();
                    node.Name = $"sampler_{GenSerialId()}";
                    node.Graph = this;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                ImGuiAPI.EndMenu();
            }
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
                                    node.Position = View2WorldSpace(ref posMenu);
                                    this.AddNode(node);
                                }
                            }
                            else
                            {
                                var node = Control.CallNode.NewMethodNode(i);
                                node.Name = i.Method.Name;
                                node.Graph = this;
                                node.Position = View2WorldSpace(ref posMenu);
                                this.AddNode(node);
                            }
                        }
                        else
                        {
                            var node = Control.CallNode.NewMethodNode(i);
                            node.Name = i.Method.Name;
                            node.Graph = this;
                            node.Position = View2WorldSpace(ref posMenu);
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
                            node.Position = View2WorldSpace(ref posMenu);
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
                    System.Reflection.FieldInfo[] members = typeof(Graphics.Pipeline.Shader.UMaterial.PSInput).GetFields();
                    foreach (var i in members)
                    {
                        if (ImGuiAPI.MenuItem(i.Name, null, false, true))
                        {
                            var node = new UUniformVar();
                            node.VarType = Rtti.UTypeDesc.TypeOf(i.FieldType);
                            node.Name = "input." + i.Name;
                            node.Graph = this;
                            node.Position = View2WorldSpace(ref posMenu);
                            this.AddNode(node);
                        }
                    }
                    ImGuiAPI.EndMenu();
                }
                ImGuiAPI.EndMenu();
            }
        }

        //int Seed = 5;
        public unsafe override void OnDrawAfter(EGui.Controls.NodeGraph.NodeGraphStyles styles = null)
        {
            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());

            var O = WindowPos + new Vector2(GraphViewSize.X / 2, GraphViewSize.Y);

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

            cmdlist.AddLine(ref O, ref p, 0xFFFFFFFF, width);

            Tree(ref p, angle + Math.PI / 18 * (0.9f + mRand.NextDouble() * 0.2), length * 0.8 * (0.9f + mRand.NextDouble() * 0.2), width * 0.8f, ref cmdlist);
            Tree(ref p, angle - Math.PI / 18 * (0.9f + mRand.NextDouble() * 0.2), length * 0.8 * (0.9f + mRand.NextDouble() * 0.2), width * 0.8f, ref cmdlist);
        }

        protected override void OnLClicked()
        {
            base.OnLClicked();
        }
    }
}
