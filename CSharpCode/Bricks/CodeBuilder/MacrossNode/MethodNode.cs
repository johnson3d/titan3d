using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class MethodNode : UNodeBase, UEditableValue.IValueEditNotify, IBeforeExecNode, IAfterExecNode, IBreakableNode, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public PinOut Result = null;
        public PinIn Self = null;
        public class PinData : UEditableValue.IValueEditNotify
        {
            public PinIn PinIn;
            public PinOut PinOut;
            public EMethodArgumentAttribute OpType;
            public bool IsCustomPin = false;

            public UMacrossMethodGraph DelegateGraph;
            public List<PinData> SubPins;

            public EGui.UIProxy.MenuItemProxy.MenuState AddPinMenuState;
            public EGui.UIProxy.MenuItemProxy.MenuState DeletePinMenuState;

            MethodNode mHost;

            public PinData(MethodNode host)
            {
                mHost = host;
                AddPinMenuState.Reset();
                DeletePinMenuState.Reset();
            }

            public void OnValueChanged(UEditableValue ev)
            {
                if (ev.ValueType.FullName == "System.Type")
                {
                    var pin = ev.Tag as PinIn;
                    if (pin == null)
                        return;

                    var type = (Rtti.TtTypeDesc)ev.Value;
                    pin.Tag = type;

                    if (DelegateGraph != null)
                    {
                        for(int i=0; i< DelegateGraph.MethodDatas[0].MethodDec.Arguments.Count; i++)
                        {
                            var arg = DelegateGraph.MethodDatas[0].MethodDec.Arguments[i];
                            if(arg.VariableName == pin.Name)
                            {
                                arg.VariableType = new TtTypeReference(type);
                                break;
                            }
                        }
                        DelegateGraph.MethodDatas[0].StartNode.UpdateMethodDefine(DelegateGraph.MethodDatas[0].MethodDec);
                    }
                }
            }
        }
        public List<PinData> Arguments = new List<PinData>();

        private Rtti.TtClassMeta.TtMethodMeta Method
        {
            get
            {
                var segs = mMethodMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                var kls = Rtti.TtClassMetaManager.Instance.GetMeta(segs[0]);
                if (kls != null)
                {
                    return kls.GetMethod(segs[1]);
                }
                return null;
            }
        }
        private Rtti.TtClassMeta HostClass
        {
            get
            {
                var segs = mMethodMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                return Rtti.TtClassMetaManager.Instance.GetMeta(segs[0]);
            }
        }
        public TtMethodDeclaration MethodDesc;
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
                if (string.IsNullOrEmpty(value))
                    return;
                if(value[0] == '@')
                {
                    var mtd = GetMethodMeta(value.TrimStart('@'));
                    if (mtd != null)
                        Initialize(mtd);
                }
                else
                {
                    var mtd = GetMethodMeta(value);
                    if (mtd != null)
                        Initialize(mtd);
                }
            }
        }
        Rtti.TtClassMeta.TtMethodMeta GetMethodMeta(string metaStr)
        {
            var segs = metaStr.Split('#');
            if (segs.Length != 2)
                return null;
            var kls = Rtti.TtClassMetaManager.Instance.GetMeta(segs[0]);
            if (kls != null)
            {
                var mtd = kls.GetMethod(segs[1]);
                return mtd;
            }
            return null;
        }
        public class DelegateArgumentSaveData : IO.ISerializer
        {
            [Rtti.Meta]
            public string ArgumentName { get; set; }

            public class ExtPinData : IO.ISerializer
            {
                [Rtti.Meta]
                public string PinName { get; set; }
                [Rtti.Meta]
                public Rtti.TtTypeDesc Type { get; set; }

                public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
                public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml) { }
            }
            [Rtti.Meta]
            public List<ExtPinData> ExtPinDatas { get; set; } = new List<ExtPinData>();

            public void OnPreRead(object tagObject, object hostObject, bool fromXml) { }

            public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml) { }
        }

        [Rtti.Meta]
        public bool SelfMethod { get; set; } = false;
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public Dictionary<string, string> DefaultArguments { get; } = new Dictionary<string, string>();
            [Rtti.Meta]
            public List<DelegateArgumentSaveData> DelegateArgumentSaveDatas { get; set; } = new List<DelegateArgumentSaveData>();
        }

        [Rtti.Meta(Order = 1)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                foreach(var i in Arguments)
                {
                    if (i.PinIn == null)
                        continue;
                    var pin = i.PinIn;

                    var pinType = pin.Tag as Rtti.TtTypeDesc;
                    if(pinType.IsDelegate)
                    {
                        var saveData = new DelegateArgumentSaveData();
                        saveData.ArgumentName = i.PinIn.Name;
                        tmp.DelegateArgumentSaveDatas.Add(saveData);
                        for(int subPinIdx = 0; subPinIdx < i.SubPins.Count; subPinIdx++)
                        {
                            var subPin = i.SubPins[subPinIdx];
                            if(subPin.IsCustomPin)
                            {
                                saveData.ExtPinDatas.Add(new DelegateArgumentSaveData.ExtPinData()
                                {
                                    PinName = subPin.PinIn.Name,
                                    Type = subPin.PinIn.Tag as Rtti.TtTypeDesc,
                                });
                            }
                        }
                    }

                    if (pin.EditValue != null)
                        tmp.DefaultArguments[pin.Name] = pin.EditValue.Value?.ToString();// GetValueString();
                }
                return tmp;
            }
            set
            {
                foreach (var i in value.DefaultArguments)
                {
                    for (int j = 0; j < Arguments.Count; j++)
                    {
                        if(Arguments[j].PinIn != null)
                        {
                            var pin = Arguments[j].PinIn;
                            if (pin.EditValue == null)
                                continue;
                            if (pin.Name == i.Key)
                            {
                                var pType = Method.GetParameter(j).ParameterType;
                                if (pin.EditValue.Value != null)
                                {
                                    pType = Rtti.TtTypeDesc.TypeOf(pin.EditValue.Value.GetType());
                                }
                                pin.EditValue.Value = Support.TConvert.ToObject(pType, i.Value);
                                OnValueChanged(pin.EditValue);
                            }
                        }
                    }
                }

                foreach(var saveData in value.DelegateArgumentSaveDatas)
                {
                    for(int i=0; i<Arguments.Count; i++)
                    {
                        if (Arguments[i].PinIn == null)
                            continue;

                        if(Arguments[i].PinIn.Name == saveData.ArgumentName)
                        {
                            for(int extIdx = 0; extIdx < saveData.ExtPinDatas.Count; extIdx++)
                            {
                                var extPinData = saveData.ExtPinDatas[extIdx];
                                AddDelegateExtPin(Arguments[i], extPinData.PinName, extPinData.Type, -1);
                            }
                            break;
                        }
                    }
                }

                OnPositionChanged();
            }
        }
        public static MethodNode NewMethodNode(Rtti.TtClassMeta.TtMethodMeta m)
        {
            var result = new MethodNode();
            result.Initialize(m);
            return result;
        }
        public static MethodNode NewMethodNode(TtMethodDeclaration methodDef)
        {
            var result = new MethodNode();
            result.Initialize(methodDef);
            return result;
        }
        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut AfterExec { get; set; } = new PinOut();
        public MethodNode()
        {
            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);

            Icon = MacrossStyles.Instance.FunctionIcon;
            TitleColor = MacrossStyles.Instance.FunctionTitleColor;
            BackColor = MacrossStyles.Instance.FunctionBGColor;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            if (ev.ValueType.FullName == "System.Type")
            {
                var pin = ev.Tag as PinIn;
                if (pin == null)
                    return;
                var arg = Method.FindParameter(pin.Name);
                if (arg.Meta != null && ((arg.Meta.FilterType != null) || (arg.Meta.TypeList != null)))
                {
                    if ((arg.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        Result.Tag = (Rtti.TtTypeDesc)ev.Value;// Rtti.TtTypeDesc.TypeOf((Type)ev.Value);
                    }
                }
            }
        }
        [Rtti.Meta]
        public float InputControlWidth { get; set; } = 60.0f;

        public string BreakerName 
        {
            get
            {
                string methodName = "";
                if (Method != null)
                    methodName = Method.MethodName;
                else if (MethodDesc != null)
                    methodName = MethodDesc.MethodName;

                return "breaker_" + methodName + "_" + (uint)NodeId.GetHashCode();
            }
        }
        EBreakerState mBreakerState = EBreakerState.Hidden;
        public EBreakerState BreakerState
        { 
            get => mBreakerState; 
            set
            {
                mBreakerState = value;
                Macross.UMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }


        void AddSubPinFromDelegate(System.Reflection.ParameterInfo info, string hostPinName, List<PinData> subPins)
        {
            var pinData = new PinData(this);
            if (info.IsOut)
                pinData.OpType = EMethodArgumentAttribute.Out;
            else if (info.IsIn)
                pinData.OpType = EMethodArgumentAttribute.In;
            else if (info.ParameterType.IsByRef)
                pinData.OpType = EMethodArgumentAttribute.Ref;

            if(typeof(Delegate).IsAssignableFrom(info.ParameterType))
            {
                var pin = new PinIn();
                pin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
                pin.ShowIcon = false;
                pin.Name = info.Name;
                pin.Tag = Rtti.TtTypeDesc.TypeOf(info.ParameterType);
                pin.GroupName = "Delegate_" + hostPinName + "_" + info.Name;
                AddPinIn(pin);
                pinData.PinIn = pin;
            }
            else
            {
                if(!info.IsOut)
                {
                    var pin = new PinIn();
                    pin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                    pin.LinkDesc.CanLinks.Add("Value");
                    pin.Name = info.Name;
                    pin.Tag = Rtti.TtTypeDesc.TypeOf(info.ParameterType);
                    AddPinIn(pin);
                    pinData.PinIn = pin;
                }
                if (info.IsOut || info.ParameterType.IsByRef)
                {
                    var pinOut = new PinOut();
                    pinOut.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                    pinOut.MultiLinks = true;
                    pinOut.LinkDesc.CanLinks.Add("Value");
                    pinOut.Name = info.Name;
                    pinOut.Tag = Rtti.TtTypeDesc.TypeOf(info.ParameterType);
                    AddPinOut(pinOut);
                    pinData.PinOut = pinOut;
                }
            }

            subPins.Add(pinData);
        }

        public void SetDelegateGraph(UMacrossEditor editor)
        {
            for(int i=0; i<Arguments.Count; i++)
            {
                if (Arguments[i].PinIn == null)
                    continue;
                var argType = Arguments[i].PinIn.Tag as Rtti.TtTypeDesc;
                if(argType.IsDelegate)
                {
                    var methodName = GetDelegateParamMethodName(Arguments[i].PinIn.Name);
                    for(int methodIdx = 0; methodIdx < editor.Methods.Count; methodIdx++)
                    {
                        var methodGraph = editor.Methods[methodIdx];
                        if (methodGraph.MethodDatas.Count == 0)
                            continue;
                        if(methodGraph.MethodDatas[0].MethodDec.MethodName == methodName)
                        {
                            Arguments[i].DelegateGraph = methodGraph;
                            break;
                        }
                    }
                }
            }
        }
        public override void OnRemoveNode()
        {
            var graph = ParentGraph as UMacrossMethodGraph;
            var editor = graph.MacrossEditor;
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (Arguments[i].PinIn == null)
                    continue;
                var argType = Arguments[i].PinIn.Tag as Rtti.TtTypeDesc;
                if (argType.IsDelegate)
                {
                    editor.RemoveMethod(Arguments[i].DelegateGraph);
                }
            }
        }

        public void SetSelfMethod()
        {
            Self.LinkDesc.SetColor(MacrossStyles.Instance.SelfLinkColor);
        }

        private void Initialize(Rtti.TtClassMeta.TtMethodMeta m)
        {
            //Method = m;
            if (string.IsNullOrEmpty(mMethodMeta))
            {
                mMethodMeta = Rtti.TtTypeDesc.TypeStr(m.DeclaringType) + "#" + m.GetMethodDeclareString(false);
            }
            var method = Method;
            Name = method.MethodName;

            if (method.IsStatic == false)
            {
                Self = new PinIn();
                Self.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                Self.LinkDesc.CanLinks.Add("Value");
                Self.Name = "Self";
                Self.LinkDesc.SetColor(MacrossStyles.Instance.HostClassLinkColor);
                AddPinIn(Self);
            }

            if (!method.ReturnType.IsEqual(typeof(void)))
            {
                Result = new PinOut();
                Result.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                Result.MultiLinks = true;
                Result.LinkDesc.CanLinks.Add("Value");
                Result.Name = "Result";
                AddPinOut(Result);

                Result.Tag = method.ReturnType;
            }

            Arguments.Clear();
            foreach (var i in method.Parameters)
            {
                var pinData = new PinData(this);
                pinData.OpType = EMethodArgumentAttribute.Default;
                if (i.IsOut)
                    pinData.OpType = EMethodArgumentAttribute.Out;
                else if (i.IsIn)
                    pinData.OpType = EMethodArgumentAttribute.In;
                else if (i.IsRef)
                    pinData.OpType = EMethodArgumentAttribute.Ref;

                if(i.IsDelegate)
                {
                    var pin = new PinIn();
                    pin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                    pin.LinkDesc.CanLinks.Add("Dummy");
                    pin.ShowIcon = false;
                    pin.Name = i.Name;
                    pin.Tag = i.ParameterType;
                    pin.GroupName = "Delegate_" + i.Name;
                    AddPinIn(pin);
                    pinData.PinIn = pin;

                    var delegateMethod = i.ParameterType.SystemType.GetMethod("Invoke");
                    var delegateMethodParams = delegateMethod.GetParameters();
                    if(delegateMethodParams.Length > 0)
                    {
                        if (pinData.SubPins == null)
                            pinData.SubPins = new List<PinData>(delegateMethodParams.Length);
                        foreach (var param in delegateMethodParams)
                        {
                            AddSubPinFromDelegate(param, i.Name, pinData.SubPins);
                        }
                    }
                }
                else if(i.IsParamArray)
                {
                    // todo
                }
                else
                {
                    if (!i.IsOut)
                    {
                        var pin = new PinIn();
                        pin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                        pin.LinkDesc.CanLinks.Add("Value");
                        pin.Name = i.Name;
                        pin.Tag = i.ParameterType;
                        var ev = UEditableValue.CreateEditableValue(this, i.ParameterType, pin, i.DefaultValue);
                        if (ev != null)
                        {
                            //ev.ControlWidth = InputControlWidth;
                            pin.EditValue = ev;
                            if (i.Meta != null && i.ParameterType.IsEqual(typeof(System.Type)))
                            {
                                var typeEV = ev as UTypeSelectorEValue;
                                if (typeEV != null)
                                {
                                    if (i.Meta.TypeList != null)
                                    {
                                        var typeList = new Rtti.TtTypeDesc[i.Meta.TypeList.Length];
                                        for (int j = 0; j < i.Meta.TypeList.Length; j++)
                                        {
                                            typeList[j] = Rtti.TtTypeDesc.TypeOf(i.Meta.TypeList[j]);
                                        }
                                        typeEV.Selector.TypeList = typeList;
                                        if (typeEV.Selector.SelectedType == null)
                                            typeEV.Selector.SelectedType = typeList[0];
                                    }
                                    else if (i.Meta.FilterType != null)
                                    {
                                        typeEV.Selector.BaseType = Rtti.TtTypeDesc.TypeOf(i.Meta.FilterType);
                                        if (typeEV.Selector.SelectedType == null)
                                            typeEV.Selector.SelectedType = typeEV.Selector.BaseType;
                                    }
                                    if (ev.Value == null)
                                    {
                                        ev.Value = typeEV.Selector.BaseType;// Rtti.TtTypeDesc.TypeOf(i.Meta.FilterType);
                                        OnValueChanged(typeEV);
                                    }
                                }
                            }
                            else if (i.ParameterType.IsEqual(typeof(RName)))
                            {
                                var rnameEV = ev as URNameEValue;
                                if (rnameEV != null)
                                {
                                    var attrs = i.GetCustomAttributes(typeof(RName.PGRNameAttribute), false);
                                    if (attrs.Length > 0)
                                    {
                                        var rnAttr = attrs[0] as RName.PGRNameAttribute;
                                        rnameEV.FilterExts = rnAttr.FilterExts;
                                    }
                                }
                            }
                        }
                        AddPinIn(pin);
                        pinData.PinIn = pin;
                    }
                    if (i.IsOut || i.IsRef)
                    {
                        var pinOut = new PinOut();
                        pinOut.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                        pinOut.MultiLinks = true;
                        pinOut.LinkDesc.CanLinks.Add("Value");
                        pinOut.Name = i.Name;
                        pinOut.Tag = i.ParameterType;
                        AddPinOut(pinOut);
                        pinData.PinOut = pinOut;
                    }
                }
                Arguments.Add(pinData);

                if (i.Meta != null && i.Meta.FilterType != null && i.Meta.ConvertOutArguments != 0)
                {
                    if (Result != null && (i.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        if (Result.Tag != null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Warning, $"{method.MethodName} ParamMeta Error");
                        }

                        Result.Tag = Rtti.TtTypeDesc.TypeOf(i.Meta.FilterType);
                    }
                }
            }

            var argFilterArray = Enum.GetValues(typeof(Rtti.MetaParameterAttribute.EArgumentFilter));
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (method.Parameters[i].Meta == null)
                    continue;

                int argIdx = 0;
                foreach (Rtti.MetaParameterAttribute.EArgumentFilter argFilter in argFilterArray)
                {
                    if (argIdx >= Arguments.Count)
                        break;

                    if ((method.Parameters[i].Meta.ConvertOutArguments & argFilter) != 0)
                    {
                        if (argFilter != Rtti.MetaParameterAttribute.EArgumentFilter.R)
                        {
                            var type = Rtti.TtTypeDesc.TypeOf(method.Parameters[i].Meta.FilterType);
                            if(Arguments[argIdx].PinOut != null)
                                Arguments[argIdx].PinOut.Tag = type;
                            if(Arguments[argIdx].PinIn != null)
                                Arguments[argIdx].PinIn.Tag = type;
                        }
                    }

                    argIdx++;
                }
            }
        }
        public static string GetMethodMeta(TtMethodDeclaration m)
        {
            var methodMeta = "@";
            if(m.HostClass != null)
            {
                methodMeta += ((m.HostClass.Namespace != null) ? (m.HostClass.Namespace.Namespace + ".") : "") +
                               m.HostClass.ClassName;
            }
            methodMeta += "#" + m.GetKeyword();
            return methodMeta;
        }
        private void Initialize(TtMethodDeclaration m)
        {
            if (string.IsNullOrEmpty(mMethodMeta))
            {
                mMethodMeta = GetMethodMeta(m);
            }
            Name = m.MethodName;
            MethodDesc = m;

            if(m.ReturnValue != null)
            {
                Result = new PinOut();
                Result.MultiLinks = true;
                Result.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                Result.LinkDesc.CanLinks.Add("Value");
                Result.Name = "Result";
                AddPinOut(Result);
            }

            Arguments.Clear();
            foreach(var i in m.Arguments)
            {
                var pinData = new PinData(this);
                pinData.OpType = i.OperationType;

                if(i.OperationType != EMethodArgumentAttribute.Out)
                {
                    var pin = new PinIn();
                    pin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                    pin.LinkDesc.CanLinks.Add("Value");
                    pin.Name = i.VariableName;
                    pin.Tag = i.VariableType.TypeDesc;
                    var ev = UEditableValue.CreateEditableValue(this, i.VariableType.TypeDesc, pin);
                    if (ev != null)
                    {
                        ev.ControlWidth = 80;
                        pin.EditValue = ev;
                        if (i.Meta != null && i.VariableType.IsEqual(typeof(System.Type)))
                        {
                            var typeEV = ev as UTypeSelectorEValue;
                            if (typeEV != null)
                            {
                                typeEV.Selector.BaseType = Rtti.TtTypeDesc.TypeOf(i.Meta.FilterType);
                                if (typeEV.Selector.SelectedType == null)
                                    typeEV.Selector.SelectedType = Rtti.TtTypeDesc.TypeOf(i.Meta.FilterType);
                                if (ev.Value == null)
                                {
                                    ev.Value = i.Meta.FilterType;
                                    OnValueChanged(typeEV);
                                }
                            }
                        }
                    }
                    AddPinIn(pin);
                    pinData.PinIn = pin;
                }
                if(i.OperationType == EMethodArgumentAttribute.Out || i.OperationType == EMethodArgumentAttribute.Ref)
                {
                    var pinOut = new PinOut();
                    pinOut.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                    pinOut.MultiLinks = true;
                    pinOut.LinkDesc.CanLinks.Add("Value");
                    pinOut.Name = i.VariableName;
                    pinOut.Tag = i.VariableType.TypeDesc;
                    AddPinOut(pinOut);
                    pinData.PinOut = pinOut;
                }
                Arguments.Add(pinData);

                if (i.Meta != null && i.Meta.FilterType != null && i.Meta.ConvertOutArguments != 0)
                {
                    if (Result != null && (i.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        if (Result.Tag != null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtMacrossCategory>(Profiler.ELogTag.Warning, $"{m.MethodName} ParamMeta Error");
                        }
                        Result.Tag = i.Meta.FilterType;
                    }
                }
            }
        }
        public override void OnMouseStayPin(NodePin pin, UNodeGraph graph)
        {
            if (pin == Self)
            {
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.DeclaringType.FullName}"); 
                return;
            }
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    string valueString = GetRuntimeValueString(GetReturnValueName());
                    string typeString = "";
                    var cvtType = Result.Tag as Rtti.TtTypeDesc;
                    if (cvtType != null)
                    {
                        if(Method.IsAsync())
                        {
                            if (cvtType.GetGenericArguments().Length == 1)
                                cvtType = Rtti.TtTypeDesc.TypeOf(cvtType.GetGenericArguments()[0]);
                            typeString = cvtType.FullName;
                        }
                        else
                            typeString = cvtType.FullName;
                    }
                    EGui.Controls.CtrlUtility.DrawHelper($"{valueString}({typeString})");
                }
                return;
            }
            //for (int i = 0; i < Arguments.Count; i++)
            //{
            //    if (pin == Arguments[i].PinIn)
            //    {
            //        var inPin = pin as PinIn;
            //        var paramMeta = GetInPinParamMeta(inPin);
            //        if (paramMeta != null)
            //        {
            //            EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
            //        }
            //        return;
            //    }
            //    else if(pin == Arguments[i].PinOut)
            //    {
            //        var paramMeta = Method.FindParameter(pin.Name);
            //        if (paramMeta != null)
            //        {
            //            EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
            //        }
            //        return;
            //    }
            //}
            var type = pin.Tag as Rtti.TtTypeDesc;
            if(type != null)
            {
                if(type.IsDelegate)
                    EGui.Controls.CtrlUtility.DrawHelper($"Double click to open graph");
                else
                {
                    var paramName = GetParamValueName(pin.Name);
                    string helperString = GetRuntimeValueString(paramName);                    
                    EGui.Controls.CtrlUtility.DrawHelper($"{helperString}({type.FullName})");
                }
            }
        }
        public override object GetPropertyEditObject()
        {
            return base.GetPropertyEditObject();
        }
        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;
        public void GetProperties(ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            for(int i=0; i<Arguments.Count; i++)
            {
                var pinIn = Arguments[i].PinIn;
                if(pinIn != null && !((Rtti.TtTypeDesc)pinIn.Tag).IsDelegate)
                {
                    var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                    proDesc.Name = pinIn.Name;
                    proDesc.DisplayName = pinIn.Name;
                    proDesc.PropertyType = (Rtti.TtTypeDesc)pinIn.Tag;
                    proDesc.CustomValueEditor = pinIn.EditValue;
                    collection.Add(proDesc);
                }
            }
        }
        public object GetPropertyValue(string propertyName)
        {
            for(int i=0; i<Arguments.Count; i++)
            {
                var pinIn = Arguments[i].PinIn;
                if (pinIn == null)
                    continue;
                if (pinIn.Name == propertyName && pinIn.EditValue != null)
                {
                    return pinIn.EditValue.Value;
                }
            }
            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            for(int i=0; i<Arguments.Count; i++)
            {
                var pinIn = Arguments[i].PinIn;
                if (pinIn == null)
                    continue;
                if(pinIn.Name == propertyName && pinIn.EditValue != null)
                {
                    pinIn.EditValue.Value = value;
                    OnValueChanged(pinIn.EditValue);
                }
            }
        }
        public override void OnDoubleClickedPin(NodePin hitPin)
        {
            var hitType = hitPin.Tag as Rtti.TtTypeDesc;
            if(hitType.IsDelegate)
            {
                var method = Method;
                var macrossGraph = ParentGraph as UMacrossMethodGraph;
                UMacrossMethodGraph showGraph = null;
                for(int i=0; i<Arguments.Count; i++)
                {
                    if(hitPin == Arguments[i].PinIn)
                    {
                        //for(int graphIdx=0; graphIdx < macrossGraph.MacrossEditor.Methods.Count; graphIdx++)
                        //{
                        //    var graph = macrossGraph.MacrossEditor.Methods[graphIdx];
                        //    var checkMethodName = GetDelegateParamMethodName(hitPin.Name);
                        //    for (var dataIdx = 0; dataIdx < graph.MethodDatas.Count; dataIdx++)
                        //    {
                        //        var data = graph.MethodDatas[dataIdx];
                        //        if(data.GetMethodName() == checkMethodName)
                        //        {
                        //            showGraph = graph;
                        //            break;
                        //        }
                        //    }
                        //    if (showGraph != null)
                        //        break;
                        //}
                        showGraph = Arguments[i].DelegateGraph;
                        if(showGraph == null)
                        {
                            var invokeMethod = method.Parameters[i].ParameterType.SystemType.GetMethod("Invoke");
                            var f = TtMethodDeclaration.GetMethodDeclaration(invokeMethod);
                            f.MethodName = GetDelegateParamMethodName(hitPin.Name);
                            f.IsOverride = false;
                            for(int subPinIdx = 0; subPinIdx < Arguments[i].SubPins.Count; subPinIdx++)
                            {
                                if (!Arguments[i].SubPins[subPinIdx].IsCustomPin)
                                    continue;

                                var name = Arguments[i].SubPins[subPinIdx].PinIn.Name;
                                var type = Arguments[i].SubPins[subPinIdx].PinIn.Tag as Rtti.TtTypeDesc;

                                f.Arguments.Add(new TtMethodArgumentDeclaration()
                                {
                                    VariableName = name,
                                    VariableType = new TtTypeReference(type),
                                });
                            }
                            showGraph = UMacrossMethodGraph.NewGraph(macrossGraph.MacrossEditor, f);
                            showGraph.MethodDatas[0].IsDelegate = true;
                            macrossGraph.MacrossEditor.Methods.Add(showGraph);
                            macrossGraph.MacrossEditor.DefClass.Methods.Add(f);

                            Arguments[i].DelegateGraph = showGraph;
                        }
                        break;
                    }
                }
                if(showGraph != null)
                {
                    showGraph.ParentGraph = this.ParentGraph;
                    showGraph.VisibleInClassGraphTables = true;
                    ((UMacrossMethodGraph)ParentGraph).GraphRenderer.SetGraph(showGraph);
                }
            }
        }
        void AddDelegateExtPin(PinData pinData, string pinName, Rtti.TtTypeDesc pinType, int idx)
        {
            var subPin = new PinIn();
            subPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            subPin.LinkDesc.CanLinks.Add("Value");
            subPin.Name = pinName;
            subPin.Tag = pinType;
            var ev = UEditableValue.CreateEditableValue(pinData, Rtti.TtTypeDesc.TypeOf(typeof(Type)), subPin) as UTypeSelectorEValue;
            if (ev != null)
            {
                ev.ControlWidth = InputControlWidth;
                subPin.EditValue = ev;
                var typeList = new List<Rtti.TtTypeDesc>()
                {
                    Rtti.TtTypeDesc.TypeOf(typeof(SByte)),
                    Rtti.TtTypeDesc.TypeOf(typeof(Byte)),
                    Rtti.TtTypeDesc.TypeOf(typeof(Int16)),
                    Rtti.TtTypeDesc.TypeOf(typeof(UInt16)),
                    Rtti.TtTypeDesc.TypeOf(typeof(Int32)),
                    Rtti.TtTypeDesc.TypeOf(typeof(UInt32)),
                    Rtti.TtTypeDesc.TypeOf(typeof(Int64)),
                    Rtti.TtTypeDesc.TypeOf(typeof(UInt64)),
                    Rtti.TtTypeDesc.TypeOf(typeof(float)),
                    Rtti.TtTypeDesc.TypeOf(typeof(double)),
                    Rtti.TtTypeDesc.TypeOf(typeof(string)),
                };
                foreach(var ser in Rtti.TtTypeDescManager.Instance.Services.Values)
                {
                    typeList.AddRange(ser.Types.Values);
                }
                ev.Selector.TypeList = typeList.ToArray();
                ev.Selector.SelectedType = pinType;
            }
            var subPinData = new PinData(this)
            {
                PinIn = subPin,
                IsCustomPin = true,
            };
            //if (idx >= 0 && idx < pinData.SubPins.Count)
            //{
            int i = 0;
            for (i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i] == pinData.PinIn)
                    break;
            }
            if (idx < 0)
            {
                i += pinData.SubPins.Count + 1;
                InsertPinIn(i, subPin);
                pinData.SubPins.Add(subPinData);
            }
            else
            {
                //foreach (var ssp in pinData.SubPins)
                //{
                //    if (!ssp.IsCustomPin)
                //        i++;
                //}
                InsertPinIn(i + idx + 1, subPin);
                if (idx < pinData.SubPins.Count)
                    pinData.SubPins.Insert(idx, subPinData);
                else
                    pinData.SubPins.Add(subPinData);
            }
            //}
            //else
            //{
            //    AddPinIn(subPin);
            //    pinData.SubPins.Add(subPinData);
            //}
        }
        public override void OnShowPinMenu(NodePin pin)
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            var menuData = new Support.TtAnyPointer();
            bool processed = false;
            for(int i=0; i<Arguments.Count; i++)
            {
                if (Arguments[i].SubPins == null)
                    continue;
                var pinData = Arguments[i];
                Action addPinAction = () =>
                        {
                            string valueKeyName = "Value";
                            int idx = 0, nameIdx = 1;
                            foreach (var ssp in pinData.SubPins)
                            {
                                if (!ssp.IsCustomPin)
                                {
                                    idx++;
                                    continue;
                                }

                                var curIdx = System.Convert.ToInt32(ssp.PinIn.Name.Remove(0, valueKeyName.Length));
                                if (curIdx != nameIdx)
                                    break;
                                idx++;
                                nameIdx++;
                            }
                            var pinName = valueKeyName + nameIdx;
                            var pinType = Rtti.TtTypeDesc.TypeOf(typeof(int));
                            AddDelegateExtPin(pinData, pinName, pinType, idx);

                            if (pinData.DelegateGraph != null)
                            {
                                var methodData = pinData.DelegateGraph.MethodDatas[0];
                                var newMethodArg = new TtMethodArgumentDeclaration()
                                {
                                    VariableType = new TtTypeReference(typeof(int)),
                                    VariableName = pinName,
                                };
                                if (idx >= 0 && idx < methodData.MethodDec.Arguments.Count)
                                    methodData.MethodDec.Arguments.Insert(idx, newMethodArg);
                                else
                                    methodData.MethodDec.Arguments.Add(newMethodArg);
                                methodData.StartNode.UpdateMethodDefine(methodData.MethodDec);
                            }

                            OnPositionChanged();
                        };

                if(pinData.PinIn == pin)
                {
                    if (EGui.UIProxy.MenuItemProxy.MenuItem("AddPin", null, false, null, in drawList, in menuData, ref pinData.AddPinMenuState))
                        addPinAction();
                    processed = true;
                }
                for (int subIdx = 0; subIdx < Arguments[i].SubPins.Count; subIdx++)
                {
                    var subPin = pinData.SubPins[subIdx];
                    if(subPin.PinIn == pin)
                    {
                        if (EGui.UIProxy.MenuItemProxy.MenuItem("AddPin", null, false, null, in drawList, in menuData, ref subPin.AddPinMenuState))
                            addPinAction();
                        if (subPin.IsCustomPin)
                        {
                            if (EGui.UIProxy.MenuItemProxy.MenuItem("DeletePin", null, false, null, in drawList, in menuData, ref subPin.DeletePinMenuState))
                            {
                                RemovePinIn(subPin.PinIn);
                                pinData.SubPins.RemoveAt(subIdx);
                                if(pinData.DelegateGraph != null)
                                {
                                    var methodData = pinData.DelegateGraph.MethodDatas[0];
                                    methodData.MethodDec.Arguments.RemoveAt(subIdx);
                                    methodData.StartNode.UpdateMethodDefine(methodData.MethodDec);
                                }
                                OnPositionChanged();
                            }
                        }
                        processed = true;
                        break;
                    }
                }
                if (processed)
                    break;
            }
        }
        string GetReturnValueName()
        {
            return $"tmp_r_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }
        string GetParamValueName(string paramName)
        {
            return $"v_{paramName}_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }
        string GetDelegateParamMethodName(string paramName)
        {
            return $"dm_{paramName}_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }

        void GenArgumentCodes(int argIdx, ref BuildCodeStatementsData data, out TtExpressionBase exp, 
            TtMethodInvokeStatement invokeStatement = null,
            List<TtStatementBase> beforeStatements = null, 
            List<TtStatementBase> afterStatements = null)
        {
            var pinData = Arguments[argIdx];
            var pinType = ((pinData.PinIn != null) ? pinData.PinIn.Tag : pinData.PinOut.Tag) as Rtti.TtTypeDesc;
            if (pinType.IsPointer && invokeStatement != null)
                invokeStatement.IsUnsafe = true;
            
            if (pinType.IsDelegate)
            {
                var inPin = pinData.PinIn;
                var paramName = GetParamValueName(inPin.Name);
                var delegateMethod = pinType.SystemType.GetMethod("Invoke");
                var lambdaExp = new TtLambdaExpression();
                exp = lambdaExp;
                if (delegateMethod.ReturnType != typeof(void) && delegateMethod.ReturnType != typeof(System.Threading.Tasks.Task))
                {
                    if (delegateMethod.ReturnType.IsSubclassOf(typeof(System.Threading.Tasks.Task)) ||
                        delegateMethod.ReturnType.IsSubclassOf(typeof(Thread.Async.TtTask)))
                    {
                        lambdaExp.ReturnType = new TtTypeReference(delegateMethod.ReturnType.GetGenericArguments()[0]);
                        lambdaExp.IsAsync = true;
                    }
                    else
                        lambdaExp.ReturnType = new TtTypeReference(delegateMethod.ReturnType);
                }
                else if ((delegateMethod.ReturnType == typeof(System.Threading.Tasks.Task)) ||
                         (delegateMethod.ReturnType == typeof(Thread.Async.TtTask)))
                        lambdaExp.IsAsync = true;

                var delegateMethodParams = delegateMethod.GetParameters();
                foreach(var param in delegateMethodParams)
                {
                    var lambdaArg = new TtMethodInvokeArgumentExpression();
                    lambdaArg.Expression = new TtVariableReferenceExpression("___" + param.Name);
                    if (param.IsIn)
                        lambdaArg.OperationType = EMethodArgumentAttribute.In;
                    else if (param.IsOut)
                        lambdaArg.OperationType = EMethodArgumentAttribute.Out;
                    else if (param.ParameterType.IsByRef)
                        lambdaArg.OperationType = EMethodArgumentAttribute.Ref;
                    else
                        lambdaArg.OperationType = EMethodArgumentAttribute.Default;
                    lambdaExp.LambdaArguments.Add(lambdaArg);
                }

                if(pinData.DelegateGraph != null)
                {
                    var methodInvokeExp = new TtMethodInvokeStatement();
                    lambdaExp.MethodInvoke = methodInvokeExp;
                    methodInvokeExp.MethodName = GetDelegateParamMethodName(inPin.Name);
                    methodInvokeExp.IsAsync = lambdaExp.IsAsync;
                    for(int i=0; i< pinData.SubPins.Count; i++)
                    {
                        var argExp = new TtMethodInvokeArgumentExpression()
                        {
                            OperationType = pinData.SubPins[i].OpType,
                        };
                        var pinName = pinData.SubPins[i].PinIn.Name;
                        if (i < delegateMethodParams.Length)
                        {
                            pinName = "___" + pinName;
                            argExp.Expression = new TtVariableReferenceExpression(pinName);
                        }
                        else
                        {
                            if (data.NodeGraph.PinHasLinker(pinData.SubPins[i].PinIn))
                                argExp.Expression = data.NodeGraph.GetOppositePinExpression(pinData.SubPins[i].PinIn, ref data);
                            else
                                argExp.Expression = new TtDefaultValueExpression(pinData.SubPins[i].PinIn.Tag as Rtti.TtTypeDesc);
                        }
                        methodInvokeExp.Arguments.Add(argExp);
                    }
                }
            }
            else
            {
                switch (pinData.OpType)
                {
                    case EMethodArgumentAttribute.Out:
                        {
                            var outPin = pinData.PinOut;
                            var paramName = GetParamValueName(outPin.Name);
                            exp = new TtVariableReferenceExpression(paramName);
                            if (beforeStatements != null)
                            {
                                var typeRef = new TtTypeReference(pinType);
                                var varDec = new TtVariableDeclaration()
                                {
                                    VariableType = typeRef,
                                    VariableName = paramName,
                                    InitValue = new TtDefaultValueExpression(typeRef.TypeDesc),
                                };
                                beforeStatements.Add(varDec);
                                beforeStatements.Add(new TtDebuggerSetWatchVariable()
                                {
                                    VariableType = typeRef,
                                    VariableName = paramName,
                                    VariableValue = exp,
                                });
                            }
                        }
                        break;
                    case EMethodArgumentAttribute.Ref:
                    case EMethodArgumentAttribute.In:
                        {
                            var inPin = pinData.PinIn;
                            var paramName = GetParamValueName(inPin.Name);
                            if (data.NodeGraph.PinHasLinker(inPin))
                            {
                                var srcType = data.NodeGraph.GetOppositePinType(inPin);
                                var tagType = GetPinType(inPin);
                                var texp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);

                                if (texp is TtCastExpression)
                                {
                                    var castExp = texp as TtCastExpression;
                                    while (castExp != null)
                                    {
                                        texp = castExp.Expression;
                                        srcType = castExp.SourceType.TypeDesc;
                                        castExp = texp as TtCastExpression;
                                    }
                                }

                                TtExpressionBase fexp = texp;
                                if (srcType != tagType && srcType != null && tagType != null &&
                                    pinData.OpType != EMethodArgumentAttribute.Ref)
                                {
                                    fexp = new TtCastExpression()
                                    {
                                        SourceType = new TtTypeReference(srcType),
                                        TargetType = new TtTypeReference(tagType),
                                        Expression = texp,
                                    };
                                }
                                if (texp is TtVariableReferenceExpression)
                                {
                                    var refExpr = ((TtVariableReferenceExpression)texp);
                                    bool IsRefVar = false;
                                    if (refExpr.PropertyDeclClass != null)
                                    {
                                        var prop = refExpr.PropertyDeclClass.GetProperty(refExpr.VariableName);
                                        if (prop != null)
                                        {
                                            var attrs = prop.GetCustomAttributes(typeof(Rtti.MetaAttribute), false);
                                            if (attrs.Length==1)
                                            {
                                                var meta = attrs[0] as Rtti.MetaAttribute;
                                                if (meta.IsCanRefForMacross)
                                                {
                                                    IsRefVar = true;
                                                }
                                            }
                                        }
                                    }
                                    if (refExpr.IsProperty && IsRefVar == false)
                                    {
                                        exp = new TtVariableReferenceExpression(paramName);
                                        var typeRef = new TtTypeReference(pinType);
                                        if (beforeStatements != null)
                                        {
                                            var varDec = new TtVariableDeclaration()
                                            {
                                                VariableType = typeRef,
                                                VariableName = paramName,
                                                InitValue = fexp,
                                            };
                                            beforeStatements.Add(varDec);
                                            beforeStatements.Add(new TtDebuggerSetWatchVariable()
                                            {
                                                VariableType = typeRef,
                                                VariableName = paramName,
                                                VariableValue = exp,
                                            });
                                        }
                                        if (afterStatements != null && pinData.OpType == EMethodArgumentAttribute.Ref)
                                        {
                                            var assign = new TtAssignOperatorStatement()
                                            {
                                                From = exp,
                                                To = fexp,
                                            };
                                            afterStatements.Add(assign);
                                            afterStatements.Add(new TtDebuggerSetWatchVariable()
                                            {
                                                VariableType = typeRef,
                                                VariableName = paramName,
                                                VariableValue = exp,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        exp = fexp;
                                        if (beforeStatements != null)
                                        {
                                            var varName = ((TtVariableReferenceExpression)texp).VariableName;
                                            beforeStatements?.Add(new TtDebuggerSetWatchVariable()
                                            {
                                                VariableType = new TtTypeReference(pinType),
                                                VariableName = varName,
                                                VariableValue = exp,
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    exp = new TtVariableReferenceExpression(paramName);
                                    if (beforeStatements != null)
                                    {
                                        var typeRef = new TtTypeReference(pinType);
                                        var varDec = new TtVariableDeclaration()
                                        {
                                            VariableType = typeRef,
                                            VariableName = paramName,
                                            InitValue = fexp,
                                        };
                                        beforeStatements.Add(varDec);
                                        beforeStatements.Add(new TtDebuggerSetWatchVariable()
                                        {
                                            VariableType = typeRef,
                                            VariableName = paramName,
                                            VariableValue = exp,
                                        });
                                    }
                                }
                            }
                            else
                            {
                                exp = new TtVariableReferenceExpression(paramName);
                                if (beforeStatements != null)
                                {
                                    var typeRef = new TtTypeReference(pinType);
                                    var varDec = new TtVariableDeclaration()
                                    {
                                        VariableType = typeRef,
                                        VariableName = paramName,
                                        InitValue = GetNoneLinkedParameterExp(inPin, argIdx, ref data),
                                    };
                                    beforeStatements.Add(varDec);
                                    beforeStatements.Add(new TtDebuggerSetWatchVariable()
                                    {
                                        VariableType = typeRef,
                                        VariableName = paramName,
                                        VariableValue = exp,
                                    });
                                }
                            }
                        }
                        break;
                    default:
                        {
                            var inPin = pinData.PinIn;
                            if (data.NodeGraph.PinHasLinker(inPin))
                            {
                                var srcType = data.NodeGraph.GetOppositePinType(inPin);
                                var tagType = GetPinType(inPin);
                                exp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);
                                if (srcType != tagType && srcType != null && tagType != null)
                                {
                                    exp = new TtCastExpression()
                                    {
                                        SourceType = new TtTypeReference(srcType),
                                        TargetType = new TtTypeReference(tagType),
                                        Expression = exp,
                                    };
                                }
                            }
                            else
                                exp = GetNoneLinkedParameterExp(inPin, argIdx, ref data);

                            if(beforeStatements != null)
                            {
                                beforeStatements.Add(new TtDebuggerSetWatchVariable()
                                {
                                    VariableType = new TtTypeReference(pinType),
                                    VariableName = GetParamValueName(inPin.Name),
                                    VariableValue = exp,
                                });
                            }
                        }
                        break;
                }
            }
        }
        protected virtual TtExpressionBase GetNoneLinkedParameterExp(PinIn pin, int argIdx, ref BuildCodeStatementsData data)
        {
            //if (pin.EditValue.ValueType == Rtti.UTypeDescGetter<System.Type>.TypeDesc &&
            //    pin.EditValue.Value != null)
            //{
            //    var casExpr = new UCastExpression();
            //    casExpr.TargetType = new UTypeReference(pin.EditValue.Value.GetType());
            //    casExpr.Expression = new UPrimitiveExpression(pin.EditValue.ValueType, pin.EditValue.Value);
            //    return casExpr;
            //}
            //else
            {
                if (pin.EditValue == null)
                {
                    var type = pin.HostNode.GetInPinType(pin);
                    if (type.SystemType.IsValueType == false)
                        return new TtPrimitiveExpression(type, null);
                    else
                        return new TtDefaultValueExpression(type);// UPrimitiveExpression(type, pin.EditValue.Value);
                }
                else
                {
                    return new TtPrimitiveExpression(pin.EditValue.ValueType, pin.EditValue.Value);
                }
            }
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (MethodDesc != null)
                BuildStatementsWithMethodDesc(pin, ref data);
            else
                BuildStatementsWithMethodMeta(pin, ref data);
        }
        private void BuildStatementsWithMethodDesc(NodePin pin, ref BuildCodeStatementsData data)
        {
            var methodInvokeExp = new TtMethodInvokeStatement()
            {
                MethodName = MethodDesc.MethodName,
                Method = null,
            };
            if (MethodDesc.ReturnValue != null)
                methodInvokeExp.IsReturnRef = MethodDesc.ReturnValue.VariableType.IsRefType;
            if (Self != null)
            {
                if (data.NodeGraph.PinHasLinker(Self))
                    methodInvokeExp.Host = data.NodeGraph.GetOppositePinExpression(Self, ref data);
            }
            else
            {
                // method is static
                if (HostClass != null)
                    methodInvokeExp.Host = new TtClassReferenceExpression() { Class = HostClass.ClassType };
            }

            if (MethodDesc.ReturnValue != null)
            {
                var retValName = GetReturnValueName();
                methodInvokeExp.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = MethodDesc.ReturnValue.VariableType,
                    VariableName = retValName,
                    InitValue = new TtDefaultValueExpression(MethodDesc.ReturnValue.VariableType),
                };
                if (!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvokeExp.ReturnValue);
            }

            List<TtStatementBase> beforeSt = new List<TtStatementBase>();
            List<TtStatementBase> afterSt = new List<TtStatementBase>();
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = new TtMethodInvokeArgumentExpression()
                {
                    OperationType = Arguments[i].OpType,
                };
                TtExpressionBase exp;
                GenArgumentCodes(i, ref data, out exp, methodInvokeExp, beforeSt, afterSt);
                arg.Expression = exp;
                methodInvokeExp.Arguments.Add(arg);
            }
            data.CurrentStatements.AddRange(beforeSt);
            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(methodInvokeExp);
            data.CurrentStatements.AddRange(afterSt);

            if (MethodDesc.ReturnValue != null)
            {
                var retName = GetReturnValueName();
                data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
                {
                    VariableType = MethodDesc.ReturnValue.VariableType,
                    VariableName = retName,
                    VariableValue = new TtVariableReferenceExpression(retName),
                });
            }

            var nextNodePin = data.NodeGraph.GetOppositePin(AfterExec);
            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(nextNodePin, ref data);
        }
        private void BuildStatementsWithMethodMeta(NodePin pin, ref BuildCodeStatementsData data)
        {
            var method = Method;
            var methodInvokeExp = new TtMethodInvokeStatement()
            {
                MethodName = method.MethodName,
                Method = method,
            };
            if (method.ReturnType != null)
                methodInvokeExp.IsReturnRef = method.ReturnType.IsRefType;
            if (Self != null)
            {
                if(data.NodeGraph.PinHasLinker(Self))
                    methodInvokeExp.Host = data.NodeGraph.GetOppositePinExpression(Self, ref data);
            }
            else
            {
                // method is static
                if(HostClass != null)
                    methodInvokeExp.Host = new TtClassReferenceExpression() { Class = HostClass.ClassType };
            }

            if(method.HasReturnValue())
            {
                var retValName = GetReturnValueName();
                var type = Result.Tag as Rtti.TtTypeDesc;
                if(method.IsAsync())
                {
                    type = Rtti.TtTypeDesc.TypeOf(type.GetGenericArguments()[0]);
                    methodInvokeExp.IsAsync = true;
                }
                if (type.IsPointer)
                    methodInvokeExp.IsUnsafe = true;
                methodInvokeExp.ForceCastReturnType = type != method.ReturnType;
                methodInvokeExp.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(type),
                    VariableName = retValName,
                    InitValue = new TtDefaultValueExpression(type),
                };
                if(!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvokeExp.ReturnValue);
            }
            else if(method.IsAsync())
                methodInvokeExp.IsAsync = true;

            List<TtStatementBase> beforeSt = new List<TtStatementBase>();
            List<TtStatementBase> afterSt = new List<TtStatementBase>();
            for (int i=0; i<Arguments.Count; i++)
            {
                var arg = new TtMethodInvokeArgumentExpression()
                {
                    OperationType = Arguments[i].OpType,
                };
                TtExpressionBase exp;
                GenArgumentCodes(i, ref data, out exp, methodInvokeExp, beforeSt, afterSt);
                arg.Expression = exp;
                methodInvokeExp.Arguments.Add(arg);
            }
            data.CurrentStatements.AddRange(beforeSt);
            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(methodInvokeExp);
            data.CurrentStatements.AddRange(afterSt);

            if (method.HasReturnValue())
            {
                var retName = GetReturnValueName();
                data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
                {
                    VariableType = new TtTypeReference(Result.Tag as Rtti.TtTypeDesc),
                    VariableName = retName,
                    VariableValue = new TtVariableReferenceExpression(retName),
                });
            }

            var nextNodePin = data.NodeGraph.GetOppositePin(AfterExec);
            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(nextNodePin, ref data);
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == Result)
            {
                return new TtVariableReferenceExpression(GetReturnValueName());
            }
            else
            {
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (pin == Arguments[i].PinIn || 
                        pin == Arguments[i].PinOut)
                    {
                        TtExpressionBase retVal;
                        GenArgumentCodes(i, ref data, out retVal);
                        return retVal;
                    }
                }
            }
            return null;
        }
        public Rtti.TtClassMeta.TtMethodMeta.TtParamMeta GetInPinParamMeta(PinIn pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i].PinIn)
                {
                    return Method.GetParameter(i);
                }
            }
            return null;
        }
        public override Rtti.TtTypeDesc GetInPinType(PinIn pin)
        {
            return pin.Tag as Rtti.TtTypeDesc;
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == null)
                return null;
            var method = Method;
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as Rtti.TtTypeDesc;
                    if (cvtType != null)
                    {
                        if (method.IsAsync() && cvtType.GetGenericArguments().Length == 1)
                            return Rtti.TtTypeDesc.TypeOf(cvtType.GetGenericArguments()[0]);
                        else
                            return cvtType;
                    }
                }
                return method.ReturnType;
            }
            //foreach(var i in Arguments)
            //{
            //    if (pin == i.PinOut)
            //    {
            //        return i.PinOut.Tag as Rtti.TtTypeDesc;
            //        //foreach(var j in method.Parameters)
            //        //{
            //        //    if (j.Name == i.PinOut.Name && j.IsOut)
            //        //        return j.ParameterType;
            //        //}
            //    }
            //}
            return pin.Tag as Rtti.TtTypeDesc;
            //return null;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            if (nodeExpr == null)
                return true;

            if (iPin == Self)
            {
                var testType = nodeExpr.GetOutPinType(oPin);
                return TtCodeGeneratorBase.CanConvert(testType, Method.DeclaringType);
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i].PinIn)
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    switch(Arguments[i].OpType)
                    {
                        case EMethodArgumentAttribute.Default:
                        case EMethodArgumentAttribute.In:
                            return TtCodeGeneratorBase.CanConvert(testType, Method.GetParameter(i).ParameterType);
                        default:
                            return (testType == Method.GetParameter(i).ParameterType);
                    }
                }
            }
            return true;
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if(linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if(node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if(linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if(node != null)
                    node.UnLightDebuggerLine();
            }
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
        public Rtti.TtClassMeta.TtMethodMeta mSltMethod;
        public Rtti.TtClassMeta.TtFieldMeta mSltField;
        public Rtti.TtClassMeta.TtPropertyMeta mSltMember;
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
        public unsafe void OnDrawTree(string filterText = null)
        {
            //var buffer = BigStackBuffer.CreateInstance(256);
            //buffer.SetText(mFilterText);
            //ImGuiAPI.InputText("##in", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
            //mFilterText = buffer.AsText();
            //buffer.DestroyMe();
            ImGuiAPI.Separator();

            DrawNSTree(filterText, Rtti.TtClassMetaManager.Instance.TreeManager.RootNS);
        }
        public unsafe void DrawNSTree(string filterText, Rtti.TtNameSpace ns)
        {
            bool bTestFilter = string.IsNullOrEmpty(filterText) == false;
            if (bTestFilter && ns.IsContain(filterText) == false)
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
                    DrawNSTree(filterText, i);
                }
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
                foreach (var i in ns.Types)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.ClassColor);
                    bShow = ImGuiAPI.TreeNode(i.ClassType.Name);
                    ImGuiAPI.PopStyleColor(1);
                    if (bShow)
                    {
                        foreach(var j in i.Properties)
                        {
                            if (bTestFilter && j.PropertyName.Contains(filterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MemberColor);
                            ImGuiAPI.TreeNodeEx(j.PropertyName, flags);
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
                            if (bTestFilter && j.FieldName.Contains(filterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.FieldColor);
                            ImGuiAPI.TreeNodeEx(j.FieldName, flags);
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
                            if (bTestFilter && j.MethodName.Contains(filterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MethodColor);
                            ImGuiAPI.TreeNodeEx(j.MethodName, flags);
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
        public Rtti.TtClassMeta KlsMeta;
        public Rtti.TtClassMeta.TtMethodMeta mSltMethod;
        public Rtti.TtClassMeta.TtFieldMeta mSltField;
        public Rtti.TtClassMeta.TtPropertyMeta mSltMember;
        public Rtti.TtClassMeta mSltSubClass;
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
                    foreach (var j in KlsMeta.Properties)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MemberColor);
                        ImGuiAPI.TreeNodeEx(j.PropertyName, flags);
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
                        ImGuiAPI.TreeNodeEx(j.FieldName, flags);
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
                        ImGuiAPI.TreeNodeEx(j.MethodName, flags);
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