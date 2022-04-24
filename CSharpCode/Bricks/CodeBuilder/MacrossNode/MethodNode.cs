using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class MethodNode : INodeExpr, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public PinOut Result = null;
        public PinIn Self = null;
        public List<PinIn> Arguments = new List<PinIn>();
        public List<PinOut> OutArguments = new List<PinOut>();
        public Rtti.UClassMeta.MethodMeta Method
        {
            get
            {
                var segs = mMethodMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if (kls != null)
                {
                    return kls.GetMethod(segs[1]);
                }
                return null;
            }
        }
        public string mMethodMeta;
        [Rtti.Meta(Order = 0)]
        public string MethodMeta
        {
            get
            {
                return mMethodMeta;
                //var kls = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(Method.Method.DeclaringType);
                //return $"{kls}#{Method.GetMethodDeclareString()}";
            }
            set
            {
                mMethodMeta = value;
                var segs = value.Split('#');
                if (segs.Length != 2)
                    return;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if (kls != null)
                {
                    var mtd = kls.GetMethod(segs[1]);
                    if (mtd != null)
                        Initialize(mtd);
                }
            }
        }
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public Dictionary<string, string> DefaultArguments { get; } = new Dictionary<string, string>();
        }
        [Rtti.Meta(Order = 1)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                foreach(var i in Arguments)
                {
                    if (i.EditValue == null)
                        continue;

                    tmp.DefaultArguments[i.Name] = i.EditValue.Value.ToString();
                }
                return tmp;
            }
            set
            {
                foreach (var i in value.DefaultArguments)
                {
                    for (int j = 0; j < Arguments.Count; j++)
                    {
                        if (Arguments[j].EditValue == null)
                            continue;
                        if(Arguments[j].Name == i.Key)
                        {
                            Arguments[j].EditValue.Value = Support.TConvert.ToObject(Method.GetParameter(j).ParamInfo.ParameterType, i.Value);
                            OnValueChanged(Arguments[j].EditValue);
                        }
                    }
                }
            }
        }
        public static MethodNode NewMethodNode(Rtti.UClassMeta.MethodMeta m)
        {
            var result = new MethodNode();
            result.Initialize(m);
            return result;
        }
        public MethodNode()
        {
            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);

            Icon = MacrossStyles.Instance.FunctionIcon;
            TitleColor = MacrossStyles.Instance.FunctionTitleColor;
            BackColor = MacrossStyles.Instance.FunctionBGColor;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            if (ev.ValueType.FullName == "System.Type")
            {
                var pin = ev.Tag as PinIn;
                if (pin == null)
                    return;
                var arg = Method.FindParameter(pin.Name);
                if (arg.Meta != null && arg.Meta.FilterType != null)
                {
                    if ((arg.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        Result.Tag = ev.Value;
                    }
                }
            }
        }
        private void Initialize(Rtti.UClassMeta.MethodMeta m)
        {
            //Method = m;
            if (mMethodMeta == null)
            {
                mMethodMeta = Rtti.UTypeDesc.TypeStr(m.Method.DeclaringType) + "#" + m.GetMethodDeclareString();
            }
            Name = Method.Method.Name;

            if (Method.Method.IsStatic == false)
            {
                Self = new PinIn();
                Self.Link = MacrossStyles.Instance.NewInOutPinDesc();
                Self.Link.CanLinks.Add("Value");
                Self.Name = "Self";
                AddPinIn(Self);
            }

            if (Method.Method.ReturnType != typeof(void))
            {
                Result = new PinOut();
                Result.Link = MacrossStyles.Instance.NewInOutPinDesc();
                Result.Link.CanLinks.Add("Value");
                Result.Name = "Result";
                AddPinOut(Result);
            }

            Arguments.Clear();
            OutArguments.Clear();
            foreach (var i in Method.Parameters)
            {
                var pin = new PinIn();
                pin.Link = MacrossStyles.Instance.NewInOutPinDesc();
                pin.Link.CanLinks.Add("Value");
                pin.Name = i.ParamInfo.Name;

                var ev = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, i.ParamInfo.ParameterType, pin);
                if (ev != null)
                {
                    ev.ControlWidth = 80;
                    pin.EditValue = ev;
                    if (i.Meta != null && i.ParamInfo.ParameterType == typeof(System.Type))
                    {
                        var typeEV = ev as EGui.Controls.NodeGraph.TypeSelectorEValue;
                        if (typeEV != null)
                        {
                            typeEV.Selector.BaseType = Rtti.UTypeDesc.TypeOf(i.Meta.FilterType);
                            if (typeEV.Selector.SelectedType == null)
                                typeEV.Selector.SelectedType = Rtti.UTypeDesc.TypeOf(i.Meta.FilterType);
                            if (ev.Value == null)
                            {
                                ev.Value = i.Meta.FilterType;
                                OnValueChanged(typeEV);
                            }
                        }
                    }
                }

                Arguments.Add(pin);
                AddPinIn(pin);
                if (i.ParamInfo.IsOut)
                {
                    var pinOut = new PinOut();
                    pinOut.Link = MacrossStyles.Instance.NewInOutPinDesc();
                    pin.Link.CanLinks.Add("Value");
                    pinOut.Name = i.ParamInfo.Name;
                    OutArguments.Add(pinOut);
                    AddPinOut(pinOut);
                }
            }
            for (int i = 0; i < Method.Parameters.Length; i++)
            {
                var param = Method.Parameters[i];
                if (param.Meta != null && param.Meta.FilterType != null && param.Meta.ConvertOutArguments != 0)
                {
                    if (Result != null && (param.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        if (Result.Tag != null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{Method.Method.Name} ParamMeta Error");
                        }
                        Result.Tag = param.Meta.FilterType;
                    }
                    for (int j = 0; j < 30; j++)
                    {

                    }
                }
            }
        }
        public override void OnMouseStayPin(NodePin pin)
        {
            if (pin == Self)
            {
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.Method.DeclaringType.FullName}"); 
                return;
            }
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as System.Type;
                    if (cvtType != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{cvtType.FullName}");
                        return;
                    }
                }
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.Method.ReturnType.FullName}");
                return;
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    var inPin = pin as PinIn;
                    var paramMeta = GetInPinParamMeta(inPin);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParamInfo.ParameterType.FullName}");
                    }
                    return;
                }
            }
            for (int i = 0; i < OutArguments.Count; i++)
            {
                if (pin == OutArguments[i])
                {
                    var paramMeta = Method.FindParameter(pin.Name);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParamInfo.ParameterType.FullName}");
                    }
                    return;
                }
            }
        }
        public override IExpression GetExpr(UMacrossFunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            ConvertTypeOp cvtExpr = null;
            DefineVar retVar = null;

            var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{this.NodeId.GetHashCode().ToString().Replace("-", "_")}";
            if (Method.Method.ReturnType != typeof(void))
            {
                if (bTakeResult)
                {
                    return new OpUseVar(mth_ret_temp_name, false);
                }
                retVar = new DefineVar();
                retVar.IsLocalVar = true;
                retVar.DefType = Method.Method.ReturnType.FullName;
                retVar.VarName = mth_ret_temp_name;
                retVar.InitValue = cGen.GetDefaultValue(Method.Method.ReturnType);

                if (Result != null && Result.Tag != null && ((Result.Tag as System.Type) != Method.Method.ReturnType))
                {
                    var cvtTargetType = (Result.Tag as System.Type);
                    retVar.DefType = cvtTargetType.FullName;
                    cvtExpr = new ConvertTypeOp();
                    cvtExpr.TargetType = retVar.DefType;
                }
            }
            if (bTakeResult)
            {
                throw new GraphException(this, Self, "Use return value with void function");
            }

            var callExpr = GetExpr_Impl(funGraph, cGen) as CallOp;
            
            if (retVar != null)
            {
                funGraph.Function.AddLocalVar(retVar);
                callExpr.FunReturnLocalVar = retVar.VarName;
            }
            if (cvtExpr != null)
            {
                callExpr.ConvertType = cvtExpr;
            }

            callExpr.NextExpr = this.GetNextExpr(funGraph, cGen);
            return callExpr;
        }
        private IExpression GetExpr_Impl(UMacrossFunctionGraph funGraph, ICodeGen cGen)
        {
            CallOp CallExpr = new CallOp();
            var links = new List<UPinLinker>();
            if (Self != null)
            {
                CallExpr.IsStatic = false;
                funGraph.FindInLinker(Self, links);
                if (links.Count == 0)
                {
                    CallExpr.Host = new NewObjectOp(){ Type = Method.Method.DeclaringType.FullName };
                    CallExpr.Name = Method.Method.Name;
                }
                else if (links.Count == 1)
                {
                    var selfNode = links[0].OutNode as INodeExpr;
                    var selfExpr = selfNode.GetExpr(funGraph, cGen, true) as OpExpress;
                    CallExpr.Host = selfExpr;
                    CallExpr.Name = Method.Method.Name;
                }
                else
                {
                    throw new GraphException(this, Self, "Please Self pin");
                }
            }
            else
            {
                //这里要处理Static名字获取
                //CallExpr.Host = selfExpr;
                CallExpr.IsStatic = true;
                CallExpr.Host = new HardCodeOp() { Code = Method.Method.DeclaringType.FullName };
                CallExpr.Name = Method.Method.Name;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                links.Clear();
                links = new List<UPinLinker>();
                funGraph.FindInLinker(Arguments[i], links);
                OpExpress argExpr = null;
                if (links.Count == 1)
                {
                    var argNode = links[0].OutNode as INodeExpr;
                    argExpr = argNode.GetExpr(funGraph, cGen, true) as OpExpress;
                }
                else if (links.Count == 0)
                {
                    var newOp = new NewObjectOp();
                    argExpr = newOp;
                    var paramType = Method.GetParameter(i).ParamInfo.ParameterType;
                    newOp.Type = paramType.FullName;

                    if (Arguments[i].EditValue != null)
                    {
                        if (Arguments[i].EditValue.Value is System.Type)
                            newOp.InitValue = ((System.Type)Arguments[i].EditValue.Value).FullName;
                        else
                            newOp.InitValue = Arguments[i].EditValue.Value?.ToString();
                    }
                    else if (paramType.IsValueType == false)
                    {
                        newOp.InitValue = "null";
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    throw new GraphException(this, Self, $"Arg error:{Arguments[i].Name}");
                }
                CallExpr.Arguments.Add(argExpr);
            }
            
            return CallExpr;
        }
        public Rtti.UClassMeta.MethodMeta.ParamMeta GetInPinParamMeta(PinIn pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    return Method.GetParameter(i);
                }
            }
            return null;
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as System.Type;
                    if (cvtType != null)
                        return cvtType;
                }
                return Method.Method.ReturnType;
            }
            foreach(var i in OutArguments)
            {
                if (pin == i)
                {
                    foreach(var j in Method.Parameters)
                    {
                        if (j.ParamInfo.Name == i.Name && j.ParamInfo.IsOut)
                            return j.ParamInfo.ParameterType.GetElementType();
                    }
                }
            }
            return null;
        }

        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as INodeExpr;
            if (nodeExpr == null)
                return true;

            if (iPin == Self)
            {
                var testType = nodeExpr.GetOutPinType(oPin);
                return ICodeGen.CanConvert(testType, Method.Method.DeclaringType);
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i])
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    return ICodeGen.CanConvert(testType, Method.GetParameter(i).ParamInfo.ParameterType);
                }
            }
            return true;
        }
    }
    public class MethodSelector
    {
        public class MethodSelectorStyle
        {
            public static MethodSelectorStyle Instance = new MethodSelectorStyle();
            public uint NameSpaceColor = 0xFFFFFFFF;
            public uint ClassColor = 0xFFFF80FF;
            public uint MemberColor = 0xFF80FF00;
            public uint FieldColor = 0xFF806F40;
            public uint MethodColor = 0xFFFF4080;
            public uint SubClassColor = 0xFF5340FF;
        }
        public MethodSelectorStyle Styles = MethodSelectorStyle.Instance;
        private string mFilterText;
        public Rtti.UClassMeta.MethodMeta mSltMethod;
        public Rtti.UClassMeta.FieldMeta mSltField;
        public Rtti.UMetaVersion.MetaField mSltMember;
        public unsafe void OnDraw(Vector2 pos)
        {
            var pivot = new Vector2(0, 0);
            var size = new Vector2(300, 500);
            ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_None, in pivot);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopup("MethodSelector", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                OnDrawTree();

                ImGuiAPI.EndPopup();
            }
        }
        public unsafe void OnDrawTree()
        {
            ImGuiAPI.InputText("##in", ref mFilterText);
            ImGuiAPI.Separator();

            DrawNSTree(Rtti.UClassMetaManager.Instance.TreeManager.RootNS);
        }
        public unsafe void DrawNSTree(Rtti.NameSpace ns)
        {
            bool bTestFilter = string.IsNullOrEmpty(mFilterText) == false;
            if (bTestFilter && ns.IsContain(mFilterText) == false)
            {
                return;
            }
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.NameSpaceColor);
            var bShow = ImGuiAPI.TreeNode(ns.Name);
            ImGuiAPI.PopStyleColor(1);
            if (bShow)
            {
                foreach(var i in ns.ChildrenNS)
                {
                    DrawNSTree(i);
                }
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
                foreach (var i in ns.Types)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.ClassColor);
                    bShow = ImGuiAPI.TreeNode(i.ClassType.Name);
                    ImGuiAPI.PopStyleColor(1);
                    if (bShow)
                    {
                        foreach(var j in i.CurrentVersion.Fields)
                        {
                            if (bTestFilter && j.FieldName.Contains(mFilterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MemberColor);
                            ImGuiAPI.TreeNodeEx(j.FieldName, flags);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                mSltMember = j;
                            }
                            ImGuiAPI.PopStyleColor(1);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                        }                        
                        foreach (var j in i.Fields)
                        {
                            if (bTestFilter && j.Field.Name.Contains(mFilterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.FieldColor);
                            ImGuiAPI.TreeNodeEx(j.Field.Name, flags);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                mSltField = j;
                            }
                            ImGuiAPI.PopStyleColor(1);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                        }
                        foreach (var j in i.Methods)
                        {
                            if (bTestFilter && j.Method.Name.Contains(mFilterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MethodColor);
                            ImGuiAPI.TreeNodeEx(j.Method.Name, flags);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                mSltMethod = j;
                            }
                            ImGuiAPI.PopStyleColor(1);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                        }
                        ImGuiAPI.TreePop();
                    }   
                }
                ImGuiAPI.TreePop();
            }
        }
    }

    public class MacrossSelector
    {
        public Rtti.UClassMeta KlsMeta;
        public Rtti.UClassMeta.MethodMeta mSltMethod;
        public Rtti.UClassMeta.FieldMeta mSltField;
        public Rtti.UMetaVersion.MetaField mSltMember;
        public Rtti.UClassMeta mSltSubClass;
        public unsafe void OnDraw(Vector2 pos)
        {
            var Styles = MethodSelector.MethodSelectorStyle.Instance;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            //列出DownCast List
            if (ImGuiAPI.TreeNode("Cast"))
            {
                if (KlsMeta != null)
                {
                    var kls = KlsMeta.SubClasses;
                    foreach (var j in kls)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.SubClassColor);
                        ImGuiAPI.TreeNodeEx(j.ClassType.Name, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltSubClass = j;
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ClassType.FullName);
                        ImGuiAPI.PopStyleColor(1);
                    }
                }
                ImGuiAPI.TreePop();
            }

            //列出所有属性
            if (ImGuiAPI.TreeNode("Properties"))
            {
                if (KlsMeta != null)
                {
                    foreach (var j in KlsMeta.CurrentVersion.Fields)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MemberColor);
                        ImGuiAPI.TreeNodeEx(j.FieldName, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltMember = j;
                        }
                        ImGuiAPI.PopStyleColor(1);
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                    }
                }
                ImGuiAPI.TreePop();
            }

            //所有field
            if (ImGuiAPI.TreeNode("Fields"))
            {
                if (KlsMeta != null)
                {
                    foreach (var j in KlsMeta.Fields)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.FieldColor);
                        ImGuiAPI.TreeNodeEx(j.Field.Name, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltField = j;
                        }
                        ImGuiAPI.PopStyleColor(1);
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                    }
                }
                ImGuiAPI.TreePop();
            }            

            //method
            if (ImGuiAPI.TreeNode("Methods"))
            {
                if (KlsMeta != null)
                {
                    foreach (var j in KlsMeta.Methods)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MethodColor);
                        ImGuiAPI.TreeNodeEx(j.Method.Name, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltMethod = j;
                        }
                        ImGuiAPI.PopStyleColor(1);
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                    }
                }
                ImGuiAPI.TreePop();
            }
        }
    }
}