using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.EGui.Controls.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class FunctionStartNode : INodeExpr
    {
        public static FunctionStartNode NewStartNode(FunctionGraph graph)
        {
            var result = new FunctionStartNode();
            result.FuncGraph = graph;
            result.Initialize(graph);
            return result;
        }
        public FunctionStartNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;

            Position = new Vector2(100, 100);
        }
        private void Initialize(FunctionGraph graph)
        {
            FuncGraph = graph;

            AddPinOut(AfterExec);
            UpdateFunctionDefine();
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var graph = hostObject as FunctionGraph;
            if (graph == null)
                return;
            
            Initialize(graph);
        }
        public FunctionGraph FuncGraph;
        public List<EGui.Controls.NodeGraph.PinOut> Arguments = new List<EGui.Controls.NodeGraph.PinOut>();
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

            var newArgs = new List<EGui.Controls.NodeGraph.PinOut>();
            foreach (var i in FuncGraph.Function.Arguments)
            {
                EGui.Controls.NodeGraph.PinOut argPin = null;
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
                    argPin = new EGui.Controls.NodeGraph.PinOut();
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
                            retNode.Initialize(this.ParentGraph as FunctionGraph);
                        }
                    }
                }
            }
        }
        public void BuildExpr(FunctionGraph funGraph, ICodeGen cGen)
        {
            var links = new List<PinLinker>();
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

        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
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
    public class FunctionGraph : EGui.Controls.NodeGraph.NodeGraph
    {
        public static FunctionGraph NewGraph(ClassGraph kls, DefineFunction func = null)
        {
            var result = new FunctionGraph();
            result.MacrossEditor = kls;
            //result.FunctionName = funName;
            //if (result.Function == null)
            //    return null;
            if (func == null)
                result.Function = new DefineFunction();
            else
            {
                result.Function = func;

                result.StartNode = FunctionStartNode.NewStartNode(result);
                result.StartNode.Graph = kls;
                result.AddNode(result.StartNode);
            }
            return result;
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var klsGraph = tagObject as ClassGraph;
            if (klsGraph == null)
                return;

            this.MacrossEditor = klsGraph;
        }
        public override void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {
            var klsGraph = tagObject as ClassGraph;
            if (klsGraph == null)
                return;
        }
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
                StartNode = this.FindNode(value) as FunctionStartNode;
                if (StartNode == null)
                {
                    StartNode = FunctionStartNode.NewStartNode(this);
                    AddNode(StartNode);
                }
            }
        }
        private FunctionStartNode StartNode;
        public ClassGraph MacrossEditor
        {
            get;
            private set;
        }
        public override string ToString()
        {
            return $"{Function.Name}";
        }
        public bool VisibleInClassGraphTables = false;
        [Rtti.Meta]
        public DefineFunction Function { get; set; }
        private uint _mCurSerialId = 0; 
        protected uint GenSerialId()
        {
            return _mCurSerialId++;
        }
        protected override void ShowAddNode(Vector2 posMenu)
        {
            if (ImGuiAPI.BeginMenu("Operation", true))
            {
                if (ImGuiAPI.MenuItem($"+", null, false, true))
                {
                    var node = new AddNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"-", null, false, true))
                {
                    var node = new SubNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"*", null, false, true))
                {
                    var node = new MulNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"/", null, false, true))
                {
                    var node = new DivNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"%", null, false, true))
                {
                    var node = new ModNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"&", null, false, true))
                {
                    var node = new BitAndNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"|", null, false, true))
                {
                    var node = new BitOrNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("Bool Operation", true))
            {
                if (ImGuiAPI.MenuItem($"==", null, false, true))
                {
                    var node = new EqualNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"!=", null, false, true))
                {
                    var node = new NotEqualNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($">", null, false, true))
                {
                    var node = new GreateNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($">=", null, false, true))
                {
                    var node = new GreateEqualNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"<", null, false, true))
                {
                    var node = new LessNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"<=", null, false, true))
                {
                    var node = new LessEqualNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"&&", null, false, true))
                {
                    var node = new AndNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"||", null, false, true))
                {
                    var node = new OrNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("Data", true))
            {
                if (ImGuiAPI.BeginMenu("POD", true))
                {
                    if (ImGuiAPI.MenuItem($"SByte", null, false, true))
                    {
                        var node = new SByteLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"Int16", null, false, true))
                    {
                        var node = new Int16LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"Int32", null, false, true))
                    {
                        var node = new Int32LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"Int64", null, false, true))
                    {
                        var node = new Int64LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"Byte", null, false, true))
                    {
                        var node = new ByteLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"UInt16", null, false, true))
                    {
                        var node = new UInt16LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"UInt32", null, false, true))
                    {
                        var node = new UInt32LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"UInt64", null, false, true))
                    {
                        var node = new UInt64LVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"Float", null, false, true))
                    {
                        var node = new FloatLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"Double", null, false, true))
                    {
                        var node = new DoubleLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.MenuItem($"String", null, false, true))
                    {
                        var node = new StringLVar();
                        node.Name = $"lVar_{GenSerialId()}";
                        node.Graph = MacrossEditor;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                    if (ImGuiAPI.BeginMenu("BaseData", true))
                    {
                        if (ImGuiAPI.MenuItem($"Vector2", null, false, true))
                        {
                            var node = new Vector2LVar();
                            node.Name = $"lVar_{GenSerialId()}";
                            node.Graph = MacrossEditor;
                            node.Position = View2WorldSpace(ref posMenu);
                            this.AddNode(node);
                        }
                        if (ImGuiAPI.MenuItem($"Vector3", null, false, true))
                        {
                            var node = new Vector3LVar();
                            node.Name = $"lVar_{GenSerialId()}";
                            node.Graph = MacrossEditor;
                            node.Position = View2WorldSpace(ref posMenu);
                            this.AddNode(node);
                        }
                        if (ImGuiAPI.MenuItem($"Vector4", null, false, true))
                        {
                            var node = new Vector4LVar();
                            node.Name = $"lVar_{GenSerialId()}";
                            node.Graph = MacrossEditor;
                            node.Position = View2WorldSpace(ref posMenu);
                            this.AddNode(node);
                        }
                        ImGuiAPI.EndMenu();
                    }
                    ImGuiAPI.EndMenu();
                }   
                if (ImGuiAPI.MenuItem($"AnyVar", null, false, true))
                {
                    var node = new AnyVar();
                    node.Name = $"lAnyVar_{GenSerialId()}";
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"TypeConverter", null, false, true))
                {
                    var type = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeStr(typeof(object)));
                    var node = TypeConverterVar.NewTypeConverterVar(type, type);
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"VarSetter", null, false, true))
                {
                    var node = new VarSetNode();
                    node.Name = $"VarSet_{GenSerialId()}";
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.BeginMenu("FlowControl", true))
            {
                if (ImGuiAPI.MenuItem($"Sequence", null, false, true))
                {
                    var node = new SequenceNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"If", null, false, true))
                {
                    var node = new IfNode();
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }
                if (ImGuiAPI.MenuItem($"Return", null, false, true))
                {
                    var node = ReturnNode.NewReturnNode(this);
                    node.Graph = MacrossEditor;
                    node.Position = View2WorldSpace(ref posMenu);
                    this.AddNode(node);
                }                
                ImGuiAPI.EndMenu();
            }
        }
        Bricks.CodeBuilder.MacrossNode.MethodSelector mMethodSelector = new Bricks.CodeBuilder.MacrossNode.MethodSelector();
        MacrossSelector KlassSelector = new MacrossSelector();
        protected override void OnAppendGraphMenuContent(Vector2 posMenu)
        {
            mMethodSelector.mSltMember = null;
            mMethodSelector.mSltField = null;
            mMethodSelector.mSltMethod = null;
            mMethodSelector.OnDrawTree();if (mMethodSelector.mSltMember != null)
            {
                mMenuType = EMenuType.None;
            }
            else if (mMethodSelector.mSltField != null)
            {
                mMenuType = EMenuType.None;
            }
            else if (mMethodSelector.mSltMethod != null)
            {
                mMenuType = EMenuType.None;
                var node = MethodNode.NewMethodNode(mMethodSelector.mSltMethod);
                node.Graph = MacrossEditor;
                node.Position = View2WorldSpace(ref posMenu);
                this.AddNode(node);
            }
        }
        public override void OnDrawAfter(NodeGraphStyles styles = null)
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
                MacrossEditor.DraggingMember.Position = this.View2WorldSpace(ref mousePt);
                MacrossEditor.DraggingMember.OnDraw(styles);

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
        protected override void OnLClicked()
        {
            //MacrossEditor.NodePropGrid.SingleTarget = null;
        }
        bool PopKlassSelector = false;
        protected override bool OnLinkingUp(LinkingLine linking, NodeBase pressNode)
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
                var oPin = linking.StartPin as EGui.Controls.NodeGraph.PinOut;
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
        protected override unsafe void OnDrawMenu(NodeGraphStyles styles)
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
                                        var posMenu = vPos - WindowPos;
                                        node.Position = View2WorldSpace(ref posMenu);
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
                        var posMenu = vPos - WindowPos;
                        node.Position = View2WorldSpace(ref posMenu);
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
                        var posMenu = vPos - WindowPos;
                        node.Position = View2WorldSpace(ref posMenu);
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
                        var posMenu = vPos - WindowPos;
                        node.Position = View2WorldSpace(ref posMenu);
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
            else
            {
                base.OnDrawMenu(styles);
            }
        }
    }
}

