using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class UMethodStartNode : UNodeBase
    {
        public PinOut AfterExec { get; set; } = new PinOut();
        string mMethodDecKeyword;
        [Rtti.Meta]
        public string MethodDecKeyword 
        {
            get => mMethodDecKeyword;
            set
            {
                mMethodDecKeyword = value;
                UMethodDeclaration method = null;
                for (int i = 0; i < MethodGraph.MethodDatas.Count; i++)
                {
                    if (MethodGraph.MethodDatas[i].MethodDec.GetKeyword() == value)
                    {
                        MethodGraph.MethodDatas[i].StartNode = this;
                        method = MethodGraph.MethodDatas[i].MethodDec;
                        break;
                    }
                }
                if(method == null)
                {
                    HasError = true;
                    CodeExcept = new GraphException(this, null, $"Can not find method with {value}");
                }
                else
                    Initialize(MethodGraph, method);
            }
        }

        public static UMethodStartNode NewStartNode(UMacrossMethodGraph graph, UMethodDeclaration methodDec)
        {
            var result = new UMethodStartNode();
            result.MethodGraph = graph;
            result.Initialize(graph, methodDec);
            return result;
        }
        public UMethodStartNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            AfterExec.Name = ">> ";
            AfterExec.Link = MacrossStyles.Instance.NewExecPinDesc();
            Position = new Vector2(100, 100);
        }
        private void Initialize(UMacrossMethodGraph graph, UMethodDeclaration methodDec)
        {
            MethodGraph = graph;
            mMethodDecKeyword = methodDec.GetKeyword();

            AddPinOut(AfterExec);
            UpdateMethodDefine(methodDec);
            OnPositionChanged();
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var graph = hostObject as UMacrossMethodGraph;
            if (graph == null)
                return;

            MethodGraph = graph;
        }
        public UMacrossMethodGraph MethodGraph;
        public List<PinOut> Arguments = new List<PinOut>();
        public void UpdateMethodDefine(UMethodDeclaration methodDec)
        {
            for (int i = 0; i < methodDec.Arguments.Count; i++)
            {
                if(methodDec.Arguments[i].VariableName == null)
                {
                    methodDec.Arguments[i].VariableName = $"arg{i}";
                }
            }
            Name = methodDec.MethodName;

            var newArgs = new List<PinOut>();
            foreach (var i in methodDec.Arguments)
            {
                PinOut argPin = null;
                foreach (var j in Arguments)
                {
                    var defType = j.Tag as UTypeReference;
                    if (j.Name == i.VariableName && defType == i.VariableType)
                    {
                        argPin = j;
                        Arguments.Remove(j);
                        // todo: 标记为丢失参数
                        Outputs.Remove(j);//非常危险的操作，这里有把握才能操作
                        break;
                    }
                }
                if (argPin == null)
                {
                    argPin = new PinOut();
                    argPin.Tag = i.VariableType;
                    argPin.Name = i.VariableName;
                }
                newArgs.Add(argPin);
            }

            foreach (var i in Arguments)
            {
                MethodGraph.RemoveLinkedOut(i);
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

                    retNode.Initialize(MethodGraph);
                    //if (retNode.ReturnValuePin != null && retNode.ReturnType != MethodGraph.Function.ReturnType)
                    //{
                    //    if (retNode.ReturnValuePin.HasLinker())
                    //    {
                    //        //FuncGraph.RemoveLinkedIn(retNode.ReturnValuePin);
                    //        retNode.HasError = true;
                    //    }
                    //    else
                    //    {
                    //        retNode.ReturnType = MethodGraph.Function.ReturnType;
                    //        retNode.Initialize(this.ParentGraph as UMacrossMethodGraph);
                    //    }
                    //}
                }
            }
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            for(int i=0; i<data.MethodDec.Arguments.Count; i++)
            {
                if(data.MethodDec.Arguments[i].OperationType == EMethodArgumentAttribute.Out)
                {
                    var assignOp = new UAssignOperatorStatement();
                    assignOp.To = new UVariableReferenceExpression() { VariableName = data.MethodDec.Arguments[i].VariableName };
                    assignOp.From = new UDefaultValueExpression() { Type = data.MethodDec.Arguments[i].VariableType };
                    data.CurrentStatements.Add(assignOp);
                }
            }

            var links = new List<UPinLinker>();
            data.NodeGraph.FindOutLinker(AfterExec, links);
            foreach (var i in links)
            {
                var nextNode = i.InNode;
                nextNode.BuildStatements(ref data);
                //funGraph.Function.Body.PushExpr(nextNode.GetExpr(funGraph, cGen, false));
            }

            if (data.MethodDec.ReturnValue != null)
            {
                data.CurrentStatements.Add(new UReturnStatement());
                //var retOp = new ReturnOp();
                //var newOp = new NewObjectOp();
                //newOp.Type = funGraph.Function.ReturnType;
                //retOp.ReturnExpr = newOp;
                //funGraph.Function.Body.PushExpr(retOp);
            }
        }

        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    var argType = (UTypeReference)(Arguments[i].Tag);
                    var typeDesc = argType.TypeDesc;
                    if (typeDesc != null)
                        return typeDesc;
                }
            }
            return null;
        }
    }
    public class MethodData : IO.ISerializer
    {
        public UMethodStartNode StartNode;
        [Rtti.Meta]
        public UMethodDeclaration MethodDec { get; set; }
        public Rtti.UClassMeta.MethodMeta Method;

        public static MethodData CreateFromMethod(UMacrossMethodGraph graph, System.Reflection.MethodInfo method)
        {
            MethodData methodData = new MethodData();
            methodData.MethodDec = new UMethodDeclaration()
            {
                IsOverride = true,
                MethodName = method.Name,
            };

            foreach (var param in method.GetParameters())
            {
                var argDec = new UMethodArgumentDeclaration()
                {
                    VariableType = new UTypeReference(param.ParameterType),
                    VariableName = param.Name,
                    OperationType = EMethodArgumentAttribute.Default,
                };
                if (param.IsOut)
                    argDec.OperationType = EMethodArgumentAttribute.Out;
                else if (param.IsIn)
                    argDec.OperationType = EMethodArgumentAttribute.In;
                else if (param.ParameterType.IsByRef)
                    argDec.OperationType = EMethodArgumentAttribute.Ref;
                methodData.MethodDec.Arguments.Add(argDec);
            }

            if(method.ReturnType != typeof(void))
            {
                methodData.MethodDec.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(method.ReturnType),
                    VariableName = method.Name + "_ReturnValue",
                };
            }

            methodData.StartNode = UMethodStartNode.NewStartNode(graph, methodData.MethodDec);
            return methodData;
        }
        public static MethodData CreateFromMethod(UMacrossMethodGraph graph, UMethodDeclaration method)
        {
            MethodData methodData = new MethodData();
            methodData.MethodDec = method;
            methodData.StartNode = UMethodStartNode.NewStartNode(graph, method);
            return methodData;
        }
        public override string ToString()
        {
            return (MethodDec.IsOverride ? "[O]" : "") + MethodDec.MethodName;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {
        }
    }
    public partial class UMacrossMethodGraph : UNodeGraph
    {
        public static UMacrossMethodGraph NewGraph(UMacrossEditor kls, UMethodDeclaration method = null)
        {
            var result = new UMacrossMethodGraph();
            result.MacrossEditor = kls;
            result.Initialize();
            //result.FunctionName = funName;
            //if (result.Function == null)
            //    return null;
            if (method != null)
            {
                var methodData = MethodData.CreateFromMethod(result, method);
                result.MethodDatas.Add(methodData);
                result.AddNode(methodData.StartNode);
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
        //[Obsolete]
        //public void BuildCodeExpr(ICodeGen cGen)
        //{
        //    Function.LocalVars.Clear();
        //    Function.Body.Lines.Clear();
        //    foreach(UNodeExpr i in this.Nodes)
        //    {
        //        i.HasError = false;
        //        i.CodeExcept = null;
        //    }
        //    StartNode.BuildExpr(this, cGen);
        //}
        public void BuildExpression(UClassDeclaration classDesc)
        {
            for(int i=0; i<Nodes.Count; i++)
            {
                (Nodes[i]).ResetErrors();
            }
            for(int i=0; i<MethodDatas.Count; i++)
            {
                MethodDatas[i].MethodDec.MethodBody.Sequence.Clear();
                BuildCodeStatementsData data = new BuildCodeStatementsData()
                {
                    ClassDec = classDesc,
                    MethodDec = MethodDatas[i].MethodDec,
                    CodeGen = MacrossEditor.CSCodeGen,
                    NodeGraph = this,
                    CurrentStatements = MethodDatas[i].MethodDec.MethodBody.Sequence,
                };
                MethodDatas[i].StartNode.BuildStatements(ref data);
            }
        }
        //[Rtti.Meta]
        //public string FunctionName
        //{
        //    get { return Function.GetFunctionDeclType(); }
        //}
        //[Rtti.Meta]
        //public Guid StartNodeId
        //{
        //    get { return StartNode.NodeId; }
        //    set
        //    {
        //        StartNode = this.FindNode(value) as UMethodStartNode;
        //        if (StartNode == null)
        //        {
        //            StartNode = UMethodStartNode.NewStartNode(this);
        //            AddNode(StartNode);
        //        }
        //    }
        //}
        //private UMethodStartNode StartNode;
        public UMacrossEditor MacrossEditor
        {
            get;
            private set;
        }
        public string Name
        {
            get => ToString();
        }
        public override string ToString()
        {
            if (MethodDatas.Count == 1)
                return MethodDatas[0].ToString();
            return GraphName;
        }
        public bool VisibleInClassGraphTables = false;
        //[Rtti.Meta]
        //public DefineFunction Function { get; set; }
        [Rtti.Meta]
        public List<MethodData> MethodDatas
        {
            get;
            set;
        } = new List<MethodData>();
        private uint _mCurSerialId = 0; 
        protected uint GenSerialId()
        {
            return _mCurSerialId++;
        }
        static void GetNodeNameAndMenuStr(in string menuString, UMacrossMethodGraph graph, ref string nodeName, ref string menuName)
        {
            menuName = menuString;
            nodeName = menuName;
            var idx = menuString.IndexOf('@');
            if (idx >= 0)
            {
                var idxEnd = menuString.IndexOf('@', idx + 1);
                var subStr = menuString.Substring(idx + 1, idxEnd - idx - 1);
                subStr = subStr.Replace("serial", graph.GenSerialId().ToString());
                menuName = menuString.Remove(idx, idxEnd - idx + 1);
                nodeName = menuName.Insert(idx, subStr);
            }
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";
            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(ContextMenuAttribute), true);
                    if (atts.Length > 0)
                    {
                        var parentMenu = CanvasMenus;
                        var att = atts[0] as ContextMenuAttribute;
                        if (!att.HasKeyString(UMacross.MacrossEditorKeyword))
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
                                    (UMenuItem item, object sender) =>
                                    {
                                        var node = Rtti.UTypeDescManager.CreateInstance(typeDesc) as UNodeBase;
                                        if (nodeName != null)
                                            node.Name = nodeName;
                                        node.UserData = MacrossEditor;
                                        node.Position = PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);
                                    });
                            }
                        }
                    }
                }
            }
            var Datas = CanvasMenus.AddMenuItem("Data", null, null);
            {
                Datas.AddMenuItem("TypeConverter", null,
                    (UMenuItem item, object sender) =>
                    {
                        var type = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeStr(typeof(object)));
                        var node = TypeConverterVar.NewTypeConverterVar(type, type);
                        node.Name = $"lAnyVar_{GenSerialId()}";
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }
            var flowControls = CanvasMenus.AddMenuItem("FlowControl", null, null);
            {
                flowControls.AddMenuItem("Return", null,
                    (UMenuItem item, object sender) =>
                    {
                        var node = ReturnNode.NewReturnNode(this);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }

            for (int i = 0; i < MacrossEditor.DefClass.SupperClassNames.Count; i++)
            {
                var classMeta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(MacrossEditor.DefClass.SupperClassNames[i]);
                if (classMeta == null)
                    continue;

                UpdateMenuWithClassMeta(classMeta);
            }

            var selfNode = CanvasMenus.AddMenuItem("Self", null, null);
            {
                var selfMeta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(MacrossEditor.DefClass.GetFullName());
                if (selfMeta != null)
                {
                    UpdateMenuWithClassMeta(selfMeta);
                }
            }
            //var selfNode = CanvasMenus.AddMenuItem("Self", null, null);
            //{
            //    for(int i=0; i<MacrossEditor.DefClass.Methods.Count; i++)
            //    {
            //        var methodDef = MacrossEditor.DefClass.Methods[i];
            //        if (methodDef.IsOverride)
            //            continue;

            //        selfNode.AddMenuItem(methodDef.MethodName, null,
            //            (UMenuItem item, object sender) =>
            //            {
            //                var node = MethodNode.NewMethodNode(methodDef);
            //                node.UserData = MacrossEditor;
            //                node.Position = PopMenuPosition;
            //                SetDefaultActionForNode(node);
            //                this.AddNode(node);
            //            });
            //    }
            //}
        }

        private void UpdateMenuWithClassMeta(Rtti.UClassMeta classMeta)
        {
            for (int proIdx = 0; proIdx < classMeta.CurrentVersion.Propertys.Count; proIdx++)
            {
                var pro = classMeta.CurrentVersion.Propertys[proIdx];
                string[] menuPath = null;
                string filterStr = pro.PropertyName;
                var proInfo = pro.PropInfo;
                if (proInfo != null)
                {
                    var atts = proInfo.GetCustomAttributes(typeof(ContextMenuAttribute), false);
                    if (atts.Length > 0)
                    {
                        var att = atts[0] as ContextMenuAttribute;
                        menuPath = att.MenuPaths;
                        filterStr = att.FilterStrings;
                    }
                }
                if (menuPath == null)
                {
                    menuPath = new string[] { "Self", pro.PropertyName };
                }
                var parentMenu = CanvasMenus;
                for (var menuIdx = 0; menuIdx < menuPath.Length; menuIdx++)
                {
                    var menuStr = menuPath[menuIdx];
                    string nodeName = null;
                    GetNodeNameAndMenuStr(menuStr, this, ref nodeName, ref menuStr);
                    if (menuIdx < menuPath.Length - 1)
                        parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                    else
                    {
                        parentMenu.AddMenuItem(menuStr, filterStr, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = ClassPropertyVar.NewClassProperty(pro, true);
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                        parentMenu.AddMenuItem(menuStr, filterStr, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = ClassPropertyVar.NewClassProperty(pro, false);
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                    }
                }
            }
            for (int fieldIdx = 0; fieldIdx < classMeta.Fields.Count; fieldIdx++)
            {
                var field = classMeta.Fields[fieldIdx];
                string[] menuPath = null;
                var fieldInfo = field.Field;
                string filterStr = fieldInfo.Name;
                var atts = fieldInfo.GetCustomAttributes(typeof(ContextMenuAttribute), false);
                if (atts.Length > 0)
                {
                    var att = atts[0] as ContextMenuAttribute;
                    menuPath = att.MenuPaths;
                    filterStr = att.FilterStrings;
                }
                if (menuPath == null)
                    menuPath = new string[] { "Self", fieldInfo.Name };
                var parentMenu = CanvasMenus;
                for (var menuIdx = 0; menuIdx < menuPath.Length; menuIdx++)
                {
                    var menuStr = menuPath[menuIdx];
                    string nodeName = null;
                    GetNodeNameAndMenuStr(menuStr, this, ref nodeName, ref menuStr);
                    if (menuIdx < menuPath.Length - 1)
                        parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                    else
                    {
                        parentMenu.AddMenuItem(menuStr, filterStr, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = ClassFieldVar.NewClassMemberVar(field, true);
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                        parentMenu.AddMenuItem(menuStr, filterStr, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = ClassFieldVar.NewClassMemberVar(field, false);
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                    }
                }
            }
            for (int methodIdx = 0; methodIdx < classMeta.Methods.Count; methodIdx++)
            {
                var method = classMeta.Methods[methodIdx];
                string[] menuPath = null;
                string filterStr = method.MethodName;
                var atts = method.GetCustomAttributes(typeof(ContextMenuAttribute), false);
                if (atts.Length > 0)
                {
                    var att = atts[0] as ContextMenuAttribute;
                    menuPath = att.MenuPaths;
                    filterStr = att.FilterStrings;
                }
                if (menuPath == null)
                    menuPath = new string[] { "Self", method.MethodName };
                var parentMenu = CanvasMenus;
                for (var menuIdx = 0; menuIdx < menuPath.Length; menuIdx++)
                {
                    var menuStr = menuPath[menuIdx];
                    string nodeName = null;
                    GetNodeNameAndMenuStr(menuStr, this, ref nodeName, ref menuStr);
                    if (menuIdx < menuPath.Length - 1)
                        parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                    else
                    {
                        parentMenu.AddMenuItem(menuStr, filterStr, null,
                            (UMenuItem item, object sender) =>
                            {
                                var node = MethodNode.NewMethodNode(method);
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                            });
                    }
                }
            }
        }

        public override void SetDefaultActionForNode(UNodeBase node)
        {
            node.OnLinkedToAction = NodeOnLinkedTo;
            node.OnLinkedFromAction = NodeOnLinkedFrom;
            node.OnLButtonClickedAction = NodeOnLButtonClicked;
            node.OnPreReadAction = NodeOnPreRead;
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
                node.UserData = MacrossEditor;
                node.Position = PopMenuPosition;
                SetDefaultActionForNode(node);
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

            //if (Function.IsFunctionDefineChanged)
            //{
            //    StartNode.UpdateMethodDefine();
            //    Function.IsFunctionDefineChanged = false;
            //}
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
            var nodeExpr = linking.StartPin.HostNode;
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
                        KlassSelector.KlsMeta = Rtti.UClassMetaManager.Instance.GetMeta(type.TypeString);

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
            base.OnBeforeDrawMenu(styles);
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
                            var oNode = oPin.HostNode;
                            var type = oNode.GetOutPinType(oPin);
                            if (type != null)
                            {
                                var srcType = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeStr(type));
                                if (srcType != null)
                                {
                                    var node = TypeConverterVar.NewTypeConverterVar(srcType, KlassSelector.mSltSubClass);
                                    if (node != null)
                                    {
                                        node.UserData = MacrossEditor;
                                        node.Position = this.PopMenuPosition;
                                        SetDefaultActionForNode(node);
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
                        var node = ClassPropertyVar.NewClassProperty(KlassSelector.mSltMember, true);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
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
                        var node = ClassFieldVar.NewClassMemberVar(KlassSelector.mSltField, true);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
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
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            if (KlassSelector.mSltMethod.IsStatic == false)
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
        private void NodeOnLinkedTo(UNodeBase node, PinOut oPin, UNodeBase InNode, PinIn iPin)
        {
            var funcGraph = ParentGraph as UMacrossMethodGraph;
            if (funcGraph == null || oPin.Link == null || iPin.Link == null)
            {
                return;
            }

            if (oPin.Link.CanLinks.Contains("Exec"))// || oPin.Link.CanLinks.Contains("Bool"))
            {
                funcGraph.RemoveLinkedOutExcept(oPin, InNode, iPin.Name);
                //funcGraph.AddLink(this, oPin.Name, InNode, iPin.Name, false);
            }
        }
        private void NodeOnLinkedFrom(UNodeBase node, PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as UMacrossMethodGraph;
            if (funcGraph == null || oPin.Link == null || iPin.Link == null)
            {
                return;
            }
            if (iPin.Link.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
                //funcGraph.AddLink(OutNode, oPin.Name, this, iPin.Name, false);
            }
        }
        private void NodeOnLButtonClicked(UNodeBase node, NodePin clickedPin)
        {
            var editor = node.UserData as UMacrossEditor;
            if (node.HasError)
            {
                if (editor != null)
                {
                    editor.PGMember.Target = node.CodeExcept;
                }
                return;
            }
            if (node.GetPropertyEditObject() == null)
            {
                if (editor != null)
                    editor.PGMember.Target = null;
                return;
            }

            if (editor != null)
                editor.PGMember.Target = node.GetPropertyEditObject();
        }
        private void NodeOnPreRead(UNodeBase node, object tagObject, object hostObject, bool fromXml)
        {
            var klsGraph = tagObject as UMacrossEditor;
            if (klsGraph == null)
                return;

            node.UserData = klsGraph;
        }
    }
}

