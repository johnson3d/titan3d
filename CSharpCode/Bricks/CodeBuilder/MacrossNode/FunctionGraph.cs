using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class UFunctionStartNode : INodeExpr
    {
        public static UFunctionStartNode NewStartNode(UMacrossFunctionGraph graph)
        {
            var result = new UFunctionStartNode();
            result.FuncGraph = graph;
            result.Initialize(graph);
            return result;
        }
        public UFunctionStartNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            Position = new Vector2(100, 100);
        }
        private void Initialize(UMacrossFunctionGraph graph)
        {
            FuncGraph = graph;

            AddPinOut(AfterExec);
            UpdateFunctionDefine();
            OnPositionChanged();
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var graph = hostObject as UMacrossFunctionGraph;
            if (graph == null)
                return;
            
            Initialize(graph);
        }
        public UMacrossFunctionGraph FuncGraph;
        public List<PinOut> Arguments = new List<PinOut>();
        public void UpdateFunctionDefine()
        {
            for (int i = 0; i < FuncGraph.Function.Arguments.Count; i++)
            {
                if(FuncGraph.Function.Arguments[i].VarName == null)
                {
                    FuncGraph.Function.Arguments[i].VarName = $"arg{i}";
                }
            }
            Name = FuncGraph.Function.Name;

            var newArgs = new List<PinOut>();
            foreach (var i in FuncGraph.Function.Arguments)
            {
                PinOut argPin = null;
                foreach (var j in Arguments)
                {
                    var defType = j.Tag as string;
                    if (j.Name == i.VarName && defType == i.DefType)
                    {
                        argPin = j;
                        Arguments.Remove(j);
                        Outputs.Remove(j);//非常危险的操作，这里有把握才能操作
                        break;
                    }
                }
                if (argPin == null)
                {
                    argPin = new PinOut();
                    argPin.Tag = i.DefType;
                    argPin.Name = i.VarName;
                }
                newArgs.Add(argPin);
            }

            foreach (var i in Arguments)
            {
                FuncGraph.RemoveLinkedOut(i);
                RemovePinOut(i);
            }
            Arguments.Clear();

            foreach (var i in newArgs)
            {
                Arguments.Add(i);
                AddPinOut(i);
            }

            if (this.ParentGraph != null)
            {
                foreach (var i in this.ParentGraph.Nodes)
                {
                    var retNode = i as ReturnNode;
                    if (retNode == null)
                        continue;

                    if (retNode.ReturnValuePin != null && retNode.ReturnType != FuncGraph.Function.ReturnType)
                    {
                        if (retNode.ReturnValuePin.HasLinker())
                        {
                            //FuncGraph.RemoveLinkedIn(retNode.ReturnValuePin);
                            retNode.HasError = true;
                        }
                        else
                        {
                            retNode.ReturnType = FuncGraph.Function.ReturnType;
                            retNode.Initialize(this.ParentGraph as UMacrossFunctionGraph);
                        }
                    }
                }
            }
        }
        public void BuildExpr(UMacrossFunctionGraph funGraph, ICodeGen cGen)
        {
            var links = new List<UPinLinker>();
            funGraph.FindOutLinker(AfterExec, links);
            foreach (var i in links)
            {
                var nextNode = i.InNode as INodeExpr;
                funGraph.Function.Body.PushExpr(nextNode.GetExpr(funGraph, cGen, false));
            }

            if (funGraph.Function.ReturnType != typeof(void).FullName)
            {
                var retOp = new ReturnOp();
                var newOp = new NewObjectOp();
                newOp.Type = funGraph.Function.ReturnType;
                retOp.ReturnExpr = newOp;
                funGraph.Function.Body.PushExpr(retOp);
            }
        }

        public override System.Type GetOutPinType(PinOut pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    var typeDesc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(FuncGraph.Function.Arguments[i].DefType);
                    if (typeDesc != null)
                        return typeDesc.SystemType;
                }
            }
            return null;
        }
    }
    public partial class UMacrossFunctionGraph : UNodeGraph
    {
        public static UMacrossFunctionGraph NewGraph(UMacrossEditor kls, DefineFunction func = null)
        {
            var result = new UMacrossFunctionGraph();
            result.MacrossEditor = kls;
            result.Initialize();
            //result.FunctionName = funName;
            //if (result.Function == null)
            //    return null;
            if (func == null)
                result.Function = new DefineFunction();
            else
            {
                result.Function = func;

                result.StartNode = UFunctionStartNode.NewStartNode(result);
                result.StartNode.Graph = kls;
                result.AddNode(result.StartNode);
            }
            return result;
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var klsGraph = tagObject as UMacrossEditor;
            if (klsGraph == null)
                return;

            this.MacrossEditor = klsGraph;
        }
        public override void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {
            var klsGraph = tagObject as UMacrossEditor;
            if (klsGraph == null)
                return;
        }
        public Bricks.NodeGraph.UGraphRenderer GraphRenderer = new NodeGraph.UGraphRenderer();
        public void BuildCodeExpr(ICodeGen cGen)
        {
            Function.LocalVars.Clear();
            Function.Body.Lines.Clear();
            foreach(INodeExpr i in this.Nodes)
            {
                i.HasError = false;
                i.CodeExcept = null;
            }
            StartNode.BuildExpr(this, cGen);
        }
        //[Rtti.Meta]
        public string FunctionName
        {
            get { return Function.GetFunctionDeclType(); }
        }
        [Rtti.Meta]
        public Guid StartNodeId
        {
            get { return StartNode.NodeId; }
            set
            {
                StartNode = this.FindNode(value) as UFunctionStartNode;
                if (StartNode == null)
                {
                    StartNode = UFunctionStartNode.NewStartNode(this);
                    AddNode(StartNode);
                }
            }
        }
        private UFunctionStartNode StartNode;
        public UMacrossEditor MacrossEditor
        {
            get;
            private set;
        }
        public override string ToString()
        {
            var preStr = Function.IsOverride ? "[O] " : "    ";
            return $"{preStr}{Function.Name}";
        }
        public bool VisibleInClassGraphTables = false;
        [Rtti.Meta]
        public DefineFunction Function { get; set; }
        private uint _mCurSerialId = 0; 
        protected uint GenSerialId()
        {
            return _mCurSerialId++;
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
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("-", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new SubNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("*", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new MulNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("/", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new DivNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("%", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new ModNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("&", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new BitAndNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                oprations.AddMenuItem("|", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new BitOrNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
            }
            var boolOp = CanvasMenus.AddMenuItem("Operation", null, null);
            {
                boolOp.AddMenuItem("==", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new EqualNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                boolOp.AddMenuItem("!=", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new NotEqualNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
                boolOp.AddMenuItem(">", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new GreateNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
                boolOp.AddMenuItem(">=", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new GreateEqualNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
                boolOp.AddMenuItem("<", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new LessNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
                boolOp.AddMenuItem("<=", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new LessEqualNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
                boolOp.AddMenuItem("&&", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new AndNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
                boolOp.AddMenuItem("||", null,
                   (UMenuItem item, object sender) =>
                   {
                       var node = new OrNode();
                       node.Graph = MacrossEditor;
                       node.Position = PopMenuPosition;
                       this.AddNode(node);
                   });
            }
            var Datas = CanvasMenus.AddMenuItem("Data", null, null);
            {
                var POD = Datas.AddMenuItem("POD", null, null);
                {
                    POD.AddMenuItem("Int8", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new SByteLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Int16", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Int16LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Int32", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Int32LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Int64", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Int64LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("UInt8", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new ByteLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("UInt16", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new UInt16LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("UInt32", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new UInt32LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("UInt64", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new UInt64LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Single", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new FloatLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Double", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new DoubleLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("String", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new StringLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Vector2", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Vector2LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Vector3", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Vector3LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                    POD.AddMenuItem("Vector4", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new Vector4LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                }
                {
                    Datas.AddMenuItem("AnyVar", null,
                        (UMenuItem item, object sender) =>
                        {
                            var node = new AnyVar();
                            node.Name = $"lVar_{GenSerialId()}";
                            node.Graph = MacrossEditor;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        });
                    Datas.AddMenuItem("TypeConverter", null,
                        (UMenuItem item, object sender) =>
                        {
                            var type = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeStr(typeof(object)));
                            var node = TypeConverterVar.NewTypeConverterVar(type, type);
                            node.Name = $"lAnyVar_{GenSerialId()}";
                            node.Graph = MacrossEditor;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        });
                    Datas.AddMenuItem("VarSetter", null,
                        (UMenuItem item, object sender) =>
                        {
                            var node = new VarSetNode();
                            node.Name = $"VarSet_{GenSerialId()}";
                            node.Graph = MacrossEditor;
                            node.Position = PopMenuPosition;
                            this.AddNode(node);
                        });
                }
            }
            var flowControls = CanvasMenus.AddMenuItem("FlowControl", null, null);
            {
                flowControls.AddMenuItem("Sequence", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new SequenceNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                flowControls.AddMenuItem("If", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new IfNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
                flowControls.AddMenuItem("Return", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = new ReturnNode();
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);
                    });
            }
        }
        Bricks.CodeBuilder.MacrossNode.MethodSelector mMethodSelector = new Bricks.CodeBuilder.MacrossNode.MethodSelector();
        MacrossSelector KlassSelector = new MacrossSelector();
        public override void OnAfterDrawMenu(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            mMethodSelector.mSltMember = null;
            mMethodSelector.mSltField = null;
            mMethodSelector.mSltMethod = null;
            mMethodSelector.OnDrawTree();
            if (mMethodSelector.mSltMember != null)
            {
                CurMenuType = EGraphMenu.None;
            }
            else if (mMethodSelector.mSltField != null)
            {
                CurMenuType = EGraphMenu.None;
            }
            else if (mMethodSelector.mSltMethod != null)
            {
                CurMenuType = EGraphMenu.None;
                var node = MethodNode.NewMethodNode(mMethodSelector.mSltMethod);
                node.Graph = MacrossEditor;
                node.Position = PopMenuPosition;
                this.AddNode(node);
            }
        }
        public override void OnDrawAfter(Bricks.NodeGraph.UGraphRenderer renderer, EGui.Controls.NodeGraph.NodeGraphStyles styles, ImDrawList cmdlist)
        {
            var mousePt = ImGuiAPI.GetMousePos() - ImGuiAPI.GetWindowPos();
            if (mousePt.X < 0 || mousePt.Y < 0)
                return;
            var winSize = ImGuiAPI.GetWindowSize();
            if (mousePt.X > winSize.X || mousePt.Y > winSize.Y)
                return;

            if (MacrossEditor.IsDraggingMember && MacrossEditor.DraggingMember != null)
            {
                MacrossEditor.DraggingMember.ParentGraph = this;
                var screenPt = this.ToScreenPos(mousePt.X, mousePt.Y);
                MacrossEditor.DraggingMember.Position = this.ViewportRateToCanvas(in screenPt);
                //MacrossEditor.DraggingMember.Position = this.View2WorldSpace(ref mousePt);
                //MacrossEditor.DraggingMember.OnDraw(styles);
                renderer.DrawNode(cmdlist, MacrossEditor.DraggingMember);

                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
                {
                    this.AddNode(MacrossEditor.DraggingMember);
                    MacrossEditor.IsDraggingMember = false;
                    MacrossEditor.DraggingMember = null;
                }
            }

            if (Function.IsFunctionDefineChanged)
            {
                StartNode.UpdateFunctionDefine();
                Function.IsFunctionDefineChanged = false;
            }
        }
        public override void OnLButtonClicked()
        {
            //MacrossEditor.NodePropGrid.SingleTarget = null;
        }
        bool PopKlassSelector = false;
        public override bool OnLinkingUp(ULinkingLine linking, UNodeBase pressNode)
        {
            if (linking.StartPin == null)
            {
                return true;
            }
            var nodeExpr = linking.StartPin.HostNode as INodeExpr;
            if (nodeExpr == null)
                return true;

            if (linking.StartPin != null && pressNode == null)
            {
                var oPin = linking.StartPin as PinOut;
                if (oPin != null)
                {
                    var type = nodeExpr.GetOutPinType(oPin);
                    if (type != null)
                    {
                        var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(type);
                        KlassSelector.KlsMeta = Rtti.UClassMetaManager.Instance.GetMeta(typeStr);

                        PopKlassSelector = true;
                        LinkingOp.IsBlocking = true;
                        return false;
                    }
                }
            }
            return true;
        }
        public override unsafe void OnBeforeDrawMenu(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            if (PopKlassSelector)
            {
                var vPos = ImGuiAPI.GetMousePos();
                if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_None))  
                {
                    KlassSelector.mSltMethod = null;
                    KlassSelector.mSltField = null;
                    KlassSelector.mSltMember = null;
                    KlassSelector.mSltSubClass = null;
                    KlassSelector.OnDraw(vPos);
                    if (KlassSelector.mSltSubClass != null)
                    {
                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            var oNode = oPin.HostNode as INodeExpr;
                            var type = oNode.GetOutPinType(oPin);
                            if (type != null)
                            {
                                var srcType = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeStr(type));
                                if (srcType != null)
                                {
                                    var node = TypeConverterVar.NewTypeConverterVar(srcType, KlassSelector.mSltSubClass);
                                    if (node != null)
                                    {
                                        node.Graph = MacrossEditor;
                                        node.Position = this.PopMenuPosition;
                                        this.AddNode(node);

                                        this.AddLink(oPin.HostNode, oPin.Name, node, node.Left.Name);
                                    }
                                }
                            }
                        }

                        LinkingOp.StartPin = null;
                        LinkingOp.HoverPin = null;
                        LinkingOp.IsBlocking = false;
                        PopKlassSelector = false;
                    }
                    else if (KlassSelector.mSltMember != null)
                    {
                        var defKls = new DefineClass();
                        defKls.ClassName = KlassSelector.KlsMeta.ClassType.Name;
                        defKls.NameSpace = KlassSelector.KlsMeta.ClassType.Namespace;
                        var node = ClassMemberVar.NewClassMemberVar(defKls, KlassSelector.mSltMember.FieldName);
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            this.AddLink(oPin.HostNode, oPin.Name, node, node.Self.Name);
                        }

                        LinkingOp.StartPin = null;
                        LinkingOp.HoverPin = null;
                        LinkingOp.IsBlocking = false;
                        PopKlassSelector = false;
                    }
                    else if (KlassSelector.mSltField != null)
                    {
                        var defKls = new DefineClass();
                        defKls.ClassName = KlassSelector.KlsMeta.ClassType.Name;
                        defKls.NameSpace = KlassSelector.KlsMeta.ClassType.Namespace;
                        var node = ClassMemberVar.NewClassMemberVar(defKls, KlassSelector.mSltField.Field.Name);
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            this.AddLink(oPin.HostNode, oPin.Name, node, node.Self.Name);
                        }

                        LinkingOp.StartPin = null;
                        LinkingOp.HoverPin = null;
                        LinkingOp.IsBlocking = false;
                        PopKlassSelector = false;
                    }
                    else if (KlassSelector.mSltMethod != null)
                    {
                        var node = MethodNode.NewMethodNode(KlassSelector.mSltMethod);
                        node.Graph = MacrossEditor;
                        node.Position = PopMenuPosition;
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            if (KlassSelector.mSltMethod.Method.IsStatic == false)
                            {
                                this.AddLink(oPin.HostNode, oPin.Name, node, node.Self.Name);
                            }
                        }
                        
                        LinkingOp.StartPin = null;
                        LinkingOp.HoverPin = null;
                        LinkingOp.IsBlocking = false;
                        PopKlassSelector = false;
                    }
                    ImGuiAPI.EndPopup();
                }
                else
                {
                    LinkingOp.StartPin = null;
                    LinkingOp.HoverPin = null;
                    LinkingOp.IsBlocking = false;
                    PopKlassSelector = false;
                }
            }
            //else
            //{
            //    base.OnBeforeDrawMenu(styles);
            //}
        }
    }
}

