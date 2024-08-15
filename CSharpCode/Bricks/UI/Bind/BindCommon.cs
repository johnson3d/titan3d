using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.WorldSimulator;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Editor;
using EngineNS.UI.Template;
using EngineNS.UI.Trigger;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;

namespace EngineNS.UI.Bind
{
    // test only /////////////////////////////////////////////
    public partial class BindA : TtBindableObject
    {
        Guid mId = Guid.NewGuid(); 
        public bool IsSource = false;
        int mBindValueA = 1;
        [BindProperty(IsAutoGen = false)]
        public int BindValueA
        {
            get => mBindValueA;
            set
            {
                OnValueChange(value, mBindValueA);  
                mBindValueA = value;
            }
        }
        static TtBindableProperty BindValueAProperty = TtEngine.Instance.UIBindManager.Register<int, BindA>("BindValueA", -1);

        [BindProperty(IsAutoGen = false, IsCallSetProperty = false)]
        public int BindValueA2
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
        static TtBindableProperty BindValueA2Property = TtEngine.Instance.UIBindManager.Register<int, BindA>("BindValueA2", -6, 
            (bObj, bp, val)=>
            {
                
            });

        public static TtBindableProperty AttachedProperty_Int = TtEngine.Instance.UIBindManager.RegisterAttached<int, BindA>("AttPro_Int",0);
        public static int GetAttPro_Int(IBindableObject target)
        {
            return target.GetValue<int>(AttachedProperty_Int);
        }
        public static void SetAttPro_Int(IBindableObject target, int value)
        {
            target.SetValue<int>(value, AttachedProperty_Int);
        }
    }
    public partial class BindB //: TtBindableObject
    {
        Guid mId = Guid.NewGuid();
        public bool IsSource = false;
        int mBindValueB = 0;
        [BindProperty]
        [Category("CategoryTest")]
        public int BindValueB
        {
            get => mBindValueB;
            set
            {
                OnValueChange(value, mBindValueB);
                mBindValueB = value;
            }
        }

        public BindC InnerObject = new BindC();
    }
    public partial class BindC
    {
        int mBindValueC = 5;
        [BindProperty(DefaultValue = 6)]
        public int BindValueC
        {
            get => mBindValueC;
            set
            {
                OnValueChange(value, mBindValueC);
                mBindValueC = value;
            }
        }

        double mBindValueC2 = 0.1;
        [BindProperty]
        public double BindValueC2
        {
            get => mBindValueC2;
            set
            {
                OnValueChange(value, mBindValueC2);
                mBindValueC2 = value;
            }
        }
    }
    class Int2DoubleConvert : TtBindTypeConvertBase
    {

        public override bool CanConvertTo<TTag, TSrc>()
        {
            if ((typeof(TTag) == typeof(double)) && (typeof(TSrc) == typeof(int)))
                return true;
            return false;
        }
        public override bool CanConvertFrom<TTag, TSrc>()
        {
            if ((typeof(TTag) == typeof(double)) && (typeof(TSrc) == typeof(int)))
                return true;
            return false;
        }

        public override TTag ConvertTo<TTag, TSrc>(TtBindingExpressionBase bindingExp, TSrc value)
        {
            dynamic valueInt = value;
            dynamic valueDouble = (double)valueInt;
            return valueDouble;
        }
        public double ConvertTo<TTag, TSrc>(TtBindingExpressionBase bindingExp, int value)
        {
            return (double)value;
        }
        public override TSrc ConvertFrom<TTag, TSrc>(TtBindingExpressionBase bindingExp, TTag value)
        {
            dynamic valueDouble = value;
            dynamic valueInt = (int)valueDouble;
            return valueInt;
        }
        public int ConvertFrom(TtBindingExpressionBase bindingExp, double value)
        {
            return (int)value;
        }
    }
    public static class BindTestClass
    {
        public static void BindTest()
        {
            var bindA1 = new BindA();
            var bindA2 = new BindA();
            //bindA2.IsSource = true;
            //TtBindingOperations.SetBinding<int, int>(bindA1, "BindValueA", bindA2, "BindValueA", EBindingMode.OneWay);
            TtBindingOperations.SetBinding<int, int>(bindA1, "BindValueA2", bindA2, "BindValueA", EBindingMode.OneWay);
            bindA2.BindValueA = 10;

            var bindB1 = new BindB();
            var bindB2 = new BindB();
            var bindC1 = new BindC();
            bindB2.IsSource = true;
            TtBindingOperations.SetBinding<int, int>(bindA1, "BindValueA", bindB2, "BindValueB", EBindingMode.TwoWay);
            //TtBindingOperations.SetBinding<BindC, int, BindB, int>(bindC1, "BindValueC", bindB2, "InnerObject.BindValueC", EBindingMode.TwoWay);
            //bindB2.InnerObject.BindValueC = 100;
            //bindC1.BindValueC = 666;
            bindB2.BindValueB = 11;
            bindA1.BindValueA = 22;

            BindA.SetAttPro_Int(bindB1, 6);
            var attVal = BindA.GetAttPro_Int(bindB1);
            //TtBindingOperations.SetBinding<int, int>(bindB1, "BindValueB", bindB2, "BindValueB", EBindingMode.OneWay);
            //TtBindingOperations.SetBinding<int, int>(bindC1, "BindValueC", bindB2, "BindValueB", EBindingMode.OneWay);
            //var binding = new TtBinding();
            //binding.Convert = new Int2DoubleConvert();
            //binding.Mode = EBindingMode.TwoWay;
            //TtBindingOperations.SetBinding<double, int>(bindC1, "BindValueC2", bindB2, "BindValueB", binding);
            //bindB2.BindValueB = 20;
            //bindC1.BindValueC2 = 30;
        }
    }
    // generated code ////////////////////////////////////////
    //public partial class BindB
    //{
    //    static TtBindableProperty BindValueBProperty = TtEngine.Instance.UIBindManager.Register<int, BindB>("BindValueB", -10);
    //    public TtBindingBase CreateBinding(string propertyName)
    //    {
    //        switch(propertyName)
    //        {
    //            case "BindValueB":
    //                return new BindB_BindingImp_BindValueB();
    //        }
    //        return null;
    //    }
    //}
    //public class BindB_BindingImp_BindValueB : TtBinding<int, BindB>
    //{
    //    public override TtBindingExpressionBase CreateBindingExpression(IBindableObject targetObject, TtBindableProperty targetProperty)
    //    {
    //        var retVal = new BindB_BindingExprImp_BindValueB(this, null);
    //        retVal.TargetObject = targetObject;
    //        retVal.TargetProperty = targetProperty;
    //        return retVal;
    //    }
    //}
    //public class BindB_BindingExprImp_BindValueB : TtBindingExpression<int, BindB>
    //{

    //    public BindB_BindingExprImp_BindValueB(BindB_BindingImp_BindValueB binding, TtBindingExpressionBase parent)
    //        : base(binding, parent)
    //    {

    //    }
    //    public override void UpdateSource()
    //    {
    //        if(mParentExp != null)
    //        {
    //            mParentExp.UpdateSource();
    //        }
    //        else
    //        {
    //            dynamic obj = TargetObject;
    //            obj.BindValueB = mValueStore;
    //        }
    //    }
    //}

    //public partial class BindC : IBindableObject
    //{
    //    Dictionary<string, List<TtBindingExpressionBase>> mBindExprDic = new Dictionary<string, List<TtBindingExpressionBase>>();

    //    public void OnValueChange<T>(T value, [CallerMemberName] string? propertyName = null)
    //    {

    //    }
    //}
    //////////////////////////////////////////////////////////


    // 用于自动代码生成
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BindPropertyAttribute : Attribute, IExternalPropertyData
    {
        public EBindingMode DefaultMode = EBindingMode.OneWay;
        public EUpdateSourceTrigger UpdateSourceTrigger = EUpdateSourceTrigger.PropertyChanged;
        public bool IsAutoGen = true;
        public bool IsCallSetProperty = true;
        public object DefaultValue;
        //public TtBindPropertyAttribute

        public BindPropertyAttribute()
        {
        }

        List<IBindableObject> mBindObjects = new List<IBindableObject>();
        List<IBindableObject> mBindingTargets = new List<IBindableObject>();
        List<TtUIElement.BindingDataBase> mBindingDatas = new List<TtUIElement.BindingDataBase>();
        string mBindPopFilterString = "";
        bool mBindFilterFocus = false;
        bool mBindedDataDirty = true;
        bool mBindingDatasIsSameOne = true;
        //TtUIElement mFirstElement = null;
        EBindingMode mSelectedMode = EBindingMode.Default;
        string[] mBindingModeNames;
        static Vector2 mCurrentBindBackgroundStart;
        static Vector2 mCurrentBindBackgroundEnd;
        void DeleteBind(string propertyName, Editor.EditorUIHost host)
        {
            for (int i = 0; i < mBindingTargets.Count; i++)
            {
                var bp = mBindingTargets[i].FindBindableProperty(propertyName);
                if (bp != null)
                    mBindingTargets[i].ClearBindExpression(bp);

                dynamic tag = mBindingTargets[i];
                TtUIElement.BindingDataBase bindData;
                if (tag.BindingDatas.TryGetValue(propertyName, out bindData))
                {
                    bindData.OnRemove(host.HostEditor);
                    tag.BindingDatas.Remove(propertyName);
                }
            }
            mBindedDataDirty = true;
        }

        public void OnDraw(in ExternalInfo info)
        {
            unsafe
            {
                Editor.EditorUIHost host = null;
                if(mBindedDataDirty)
                {
                    mBindingDatasIsSameOne = true;
                    mBindingModeNames = Enum.GetNames<EBindingMode>();
                    mBindObjects.Clear();
                    mBindingTargets.Clear();
                    mBindingDatas.Clear();
                    var enumerableInterfaace = info.Target.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                    if (enumerableInterfaace != null)
                    {
                        foreach (var objIns in (IEnumerable)info.Target)
                        {
                            if (objIns == null)
                                continue;

                            var bindObj = objIns as IBindableObject;
                            if (bindObj == null)
                                continue;

                            var bp = bindObj.FindBindableProperty(info.PropertyDescriptor.Name);
                            if (bp == null)
                                continue;

                            mBindingTargets.Add(bindObj);
                            //hasBinded = hasBinded || bindObj.HasBinded(bp);
                            mBindObjects.Add(bindObj);
                            //if (mFirstElement == null)
                            //{
                            //    var element = objIns as TtUIElement;
                            //    if (element != null && element.RootUIHost != null && element.RootUIHost.Children.Count > 0)
                            //    {
                            //        mFirstElement = element.RootUIHost.Children[0];
                            //    }
                            //}
                        }
                    }
                    else
                    {
                        var bindObj = info.Target as IBindableObject;
                        if (bindObj != null)
                        {
                            var bp = bindObj.FindBindableProperty(info.PropertyDescriptor.Name);
                            if (bp != null)
                            {
                                //hasBinded = hasBinded || bindObj.HasBinded(bp);
                                mBindObjects.Add(bindObj);
                            }
                            mBindingTargets.Add(bindObj);
                        }
                        //var element = info.Target as TtUIElement;
                        //if (element != null && element.RootUIHost != null && element.RootUIHost.Children.Count > 0)
                        //{
                        //    mFirstElement = element.RootUIHost.Children[0];
                        //}
                    }

                    // check if has binded value
                    for (int tagIdx = 0; tagIdx < mBindingTargets.Count; tagIdx++)
                    {
                        dynamic tag = mBindingTargets[tagIdx];
                        TtUIElement.BindingDataBase data;
                        if(tag.BindingDatas.TryGetValue(info.PropertyDescriptor.Name, out data))
                        {
                            if(mBindingDatas.Count > 0)
                            {
                                mBindingDatasIsSameOne = mBindingDatasIsSameOne && mBindingDatas[0].IsSame(data);
                            }
                            mBindingDatas.Add(data);
                        }
                    }
                    //mBindedDataDirty = false; // 这里有问题，点击绑定按钮后相应到host而不是本身控件
                }

                if (mBindObjects.Count == 0)
                    return;
                var uiEditor = info.HostEditor as TtUIEditor;
                if (uiEditor == null)
                    return;
                host = uiEditor.mUIHost;
                if (host == null)
                    return;
                if (host.Children.Count <= 0)
                    return;
                var firstElement = host.Children[0];
                if (firstElement == null)
                    return;

                var keyName = "#BindButton" + info.PropertyDescriptor.Name;
                var id = ImGuiAPI.GetID(keyName);

                var drawList = ImGuiAPI.GetWindowDrawList();
                var cursorPos = ImGuiAPI.GetCursorScreenPos();
                ImGuiStyle* style = ImGuiAPI.GetStyle();
                var size = new Vector2(10, 10);
                var start = new Vector2(cursorPos.X + style->FramePadding.X, cursorPos.Y + style->FramePadding.Y);
                var end = new Vector2(start.X + size.X, start.Y + size.Y);
                //drawList.AddRect(in start, in end, 0xff0000ff, 2.0f, ImDrawFlags_.ImDrawFlags_None, 2.0f);
                ImGuiAPI.ItemSize(in size, 0);
                if (!ImGuiAPI.ItemAdd(in start, in end, id, 0))
                    return;
                bool hovered = false, held = false;
                var pressed = ImGuiAPI.ButtonBehavior(in start, in end, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_Internal_PressedOnRelease);

                if(hovered)
                {
                    drawList.AddRectFilled(in start, in end, EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor, 2.0f, ImDrawFlags_.ImDrawFlags_None);
                }
                else if(mBindingDatas.Count > 0)
                {
                    drawList.AddRectFilled(in start, in end, 0xFF00FF00, 2.0f, ImDrawFlags_.ImDrawFlags_None);
                }
                else
                {
                    drawList.AddRectFilled(in start, in end, EGui.UIProxy.StyleConfig.Instance.ToolButtonTextColor_Hover, 2.0f, ImDrawFlags_.ImDrawFlags_None);
                }
                if (pressed)
                {
                    ImGuiAPI.OpenPopup("BindSourceSelectPopup" + keyName, ImGuiPopupFlags_.ImGuiPopupFlags_None);
                }
                if(ImGuiAPI.BeginPopup("BindSourceSelectPopup" + keyName, 
                    ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize |
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                    //ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar |
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoNav))
                {
                    var cmdList = ImGuiAPI.GetWindowDrawList();
                    var width = ImGuiAPI.GetColumnWidth(0);
                    EGui.UIProxy.SearchBarProxy.OnDraw(ref mBindFilterFocus, in cmdList, "search item", ref mBindPopFilterString, width);
                    mBindPopFilterString = mBindPopFilterString.ToLower();
                    if(mBindingDatas.Count > 0)
                    {
                        // has binded item
                        mCurrentBindBackgroundStart = ImGuiAPI.GetCursorScreenPos();
                        cmdList.AddRectFilled(in mCurrentBindBackgroundStart, in mCurrentBindBackgroundEnd, EGui.UIProxy.StyleConfig.Instance.PanelBackground, 0.0f, ImDrawFlags_.ImDrawFlags_None);
                        ImGuiAPI.Text("Current Binds:");
                        if(mBindingDatasIsSameOne)
                        {
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, 0xff00ff00);
                            mBindingDatas[0].DrawBindInfo(host, cmdList);
                            ImGuiAPI.PopStyleColor(1);
                        }
                        else
                        {
                            ImGuiAPI.SameLine(0, -1);
                            ImGuiAPI.Text(" multi bindings!");
                        }

                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, 0xff0000ff);
                        if(EGui.UIProxy.CustomButton.ToolButton("Delete Bind", Vector2.Zero,
                            0xff0000be,
                            0xff0000be,
                            0xff0000ff,
                            EGui.UIProxy.StyleConfig.Instance.PanelBackground,
                            EGui.UIProxy.StyleConfig.Instance.PanelBackground,
                            EGui.UIProxy.StyleConfig.Instance.PanelBackground))
                        {
                            DeleteBind(info.PropertyDescriptor.Name, host);
                        }
                        ImGuiAPI.PopStyleColor(1);
                        mCurrentBindBackgroundEnd = ImGuiAPI.GetCursorScreenPos() + new Vector2(width, 0);
                    }

                    var selectedModeStr = mSelectedMode.ToString();
                    if(EGui.UIProxy.ComboBox.BeginCombo("Mode", selectedModeStr, 200))
                    {
                        for(int i=0; i<mBindingModeNames.Length; i++)
                        {
                            var selected = (mBindingModeNames[i] == selectedModeStr);
                            if(ImGuiAPI.Selectable(mBindingModeNames[i], selected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                            {
                                Enum.TryParse<EBindingMode>(mBindingModeNames[i], out mSelectedMode);
                            }
                        }

                        EGui.UIProxy.ComboBox.EndCombo();
                    }
                    if (ImGuiAPI.TreeNodeEx("Create bind method", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf))
                    {
                        ImGuiAPI.TreePop();
                    }
                    if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        DeleteBind(info.PropertyDescriptor.Name, host);
                        for (int i=0; i<mBindingTargets.Count; i++)
                        {
                            dynamic bindObj = mBindingTargets[i];
                            if (bindObj == null)
                                continue;
                            var bindTarget = new TtUIElement.UIBindingData_Element()
                            {
                                PropertyName = info.PropertyDescriptor.Name,
                                PropertyType = info.PropertyDescriptor.PropertyType,
                                Id = bindObj.Id,
                            };
                            var curMode = mSelectedMode;
                            var bp = bindObj.FindBindableProperty(info.PropertyDescriptor.Name);
                            if (bp != null && (curMode == EBindingMode.Default))
                            {
                                curMode = bp.BindingMode;
                            }
                            // ------------------------------------------------------
                            void ConfigSetMethod(in ExternalInfo info)
                            {
                                //var setMethodName = bindObj.GetPropertyBindMethodName(info.PropertyDescriptor.Name, true);
                                var setMethodDesc = new UMethodDeclaration();
                                //setMethodDesc.MethodName = setMethodName;
                                //setMethodDesc.GetDisplayNameFunc = bindObj.GetMethodDisplayName;
                                setMethodDesc.Arguments.Add(new UMethodArgumentDeclaration()
                                {
                                    VariableName = "obj",
                                    VariableType = new UTypeReference(typeof(IBindableObject)),
                                });
                                setMethodDesc.Arguments.Add(new UMethodArgumentDeclaration()
                                {
                                    VariableName = "prop",
                                    VariableType = new UTypeReference(typeof(TtBindableProperty)),
                                });
                                setMethodDesc.Arguments.Add(new UMethodArgumentDeclaration()
                                {
                                    VariableName = "valueIn",
                                    VariableType = new UTypeReference(info.PropertyDescriptor.PropertyType),
                                    OperationType = EMethodArgumentAttribute.In,
                                });
                                bindObj.InitialMethodDeclaration(info.PropertyDescriptor.Name, setMethodDesc, true);
                                var setGraph = host.HostEditor.UIAsset.MacrossEditor.AddMethod(setMethodDesc);
                                //bindObj.SetPropertyBindMethod(info.PropertyDescriptor.Name, setMethodDesc, true);
                                host.HostEditor.UIAsset.MacrossEditor.OpenMethodGraph(setGraph);                                        
                            }
                            void ConfigGetMethod(in ExternalInfo info)
                            {
                                //var getMethodName = bindObj.GetPropertyBindMethodName(info.PropertyDescriptor.Name, false);
                                var getMethodDesc = new UMethodDeclaration();
                                //getMethodDesc.MethodName = getMethodName;
                                //getMethodDesc.GetDisplayNameFunc = bindObj.GetMethodDisplayName;
                                getMethodDesc.ReturnValue = new UVariableDeclaration()
                                {
                                    VariableName = "tempReturnValue",
                                    VariableType = new UTypeReference(info.PropertyDescriptor.PropertyType),
                                    InitValue = new UDefaultValueExpression(info.PropertyDescriptor.PropertyType),
                                };
                                getMethodDesc.Arguments.Add(new UMethodArgumentDeclaration()
                                {
                                    VariableName = "obj", 
                                    VariableType = new UTypeReference(typeof(IBindableObject)),
                                });
                                getMethodDesc.Arguments.Add(new UMethodArgumentDeclaration()
                                {
                                    VariableName = "prop",
                                    VariableType = new UTypeReference(typeof(TtBindableProperty)),
                                });
                                bindObj.InitialMethodDeclaration(info.PropertyDescriptor.Name, getMethodDesc, false);
                                var getGraph = host.HostEditor.UIAsset.MacrossEditor.AddMethod(getMethodDesc);
                                //bindObj.SetPropertyBindMethod(info.PropertyDescriptor.Name, getMethodDesc, false);
                                host.HostEditor.UIAsset.MacrossEditor.OpenMethodGraph(getGraph);
                            }
                            // -------------------------------------------------------
                            var bdMethod = new TtUIElement.BindingData_Method()
                            {
                                Target = bindTarget,
                                Mode = curMode,
                            };
                            bindObj.BindingDatas[info.PropertyDescriptor.Name] = bdMethod;
                            switch (curMode)
                            {
                                case EBindingMode.TwoWay:
                                    {
                                        bdMethod.SetMethodName = bindObj.GetPropertyBindMethodName(info.PropertyDescriptor.Name, true);
                                        bdMethod.GetMethodName = bindObj.GetPropertyBindMethodName(info.PropertyDescriptor.Name, false);

                                        ConfigSetMethod(info);
                                        ConfigGetMethod(info); 
                                    }
                                    break;
                                case EBindingMode.OneWay:
                                case EBindingMode.OneTime:
                                    {
                                        // update target property when source changed
                                        bdMethod.GetMethodName = bindObj.GetPropertyBindMethodName(info.PropertyDescriptor.Name, false);
                                        ConfigGetMethod(info);
                                    }
                                    break;
                                case EBindingMode.OneWayToSource:
                                    {
                                        // update source property when target changed
                                        bdMethod.SetMethodName = bindObj.GetPropertyBindMethodName(info.PropertyDescriptor.Name, true);
                                        ConfigSetMethod(info);
                                    }
                                    break;
                            }
                            host.HostEditor.DrawType = TtUIEditor.enDrawType.Macross;
                            mBindedDataDirty = true;
                        }
                        ImGuiAPI.CloseCurrentPopup();
                    }
                    if (ImGuiAPI.TreeNodeEx("Self", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth))
                    {
                        int idx = 0;
                        foreach(var pro in host.HostEditor.UIAsset.MacrossEditor.DefClass.Properties)
                        {
                            if (!pro.DisplayName.ToLower().Contains(mBindPopFilterString))
                                continue;
                            if (pro.VariableType.TypeDesc != info.PropertyDescriptor.PropertyType)
                                continue;
                            // todo: 判断是否本类
                            ImGuiAPI.TreeNodeEx(pro.DisplayName + "##" + idx++, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet);
                            if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                DeleteBind(info.PropertyDescriptor.Name, host);
                                for(int i=0; i<mBindingTargets.Count; i++)
                                {
                                    dynamic tElement = mBindingTargets[i];
                                    if(tElement != null)
                                    {
                                        var bindTarget = new TtUIElement.UIBindingData_Element()
                                        {
                                            PropertyName = info.PropertyDescriptor.Name,
                                            PropertyType = info.PropertyDescriptor.PropertyType,
                                            Id = tElement.Id,
                                        };
                                        var curMode = mSelectedMode;
                                        var tBp = tElement.FindBindableProperty(info.PropertyDescriptor.Name);
                                        if (tBp != null && (curMode == EBindingMode.Default))
                                        {
                                            curMode = tBp.BindingMode;
                                        }
                                        tElement.BindingDatas[info.PropertyDescriptor.Name] = new TtUIElement.BindingData_SelfProperty()
                                        {
                                            Target = bindTarget,
                                            PropertyName = pro.VariableName,
                                            Mode = curMode,
                                        };
                                        mBindedDataDirty = true;
                                    }
                                }
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.TreePop();
                        }

                        ImGuiAPI.TreePop();
                    }
                    if (ImGuiAPI.TreeNodeEx("UIControls", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth))
                    {
                        int idx = 0;
                        DrawUIElementBindableProperty(firstElement, in cmdList, host, ref idx, mBindPopFilterString, info);

                        ImGuiAPI.TreePop();
                    }

                    ImGuiAPI.EndPopup();
                }
            }
        }

        struct TourPropertiesData
        {
            public int Idx;
            public string Filter;
            public IBindableObject SourceObject;
            public List<IBindableObject> TargetObjects;
            public string TargetName;
            public Rtti.UTypeDesc TargetType;
            public EditorUIHost Host;
            public bool HasProperty;
        }
        void BindPropertyTourAction(string name, TtBindableProperty bp, ref TourPropertiesData data)
        {
            if (!name.ToLower().Contains(data.Filter))
                return;
            if (bp.PropertyType != data.TargetType)
                return;
            if (name == data.TargetName)
            {
                for (int i = 0; i < data.TargetObjects.Count; i++)
                {
                    if (data.TargetObjects[i] == data.SourceObject)
                        return;
                }
            }

            ImGuiAPI.TreeNodeEx(name + "##" + data.Idx, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet);
            if(ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                DeleteBind(data.TargetName, data.Host);
                TtUIElement.UIBindingData_Element bindSource = null;
                dynamic element = data.SourceObject;
                if (element != null)
                {
                    bindSource = new TtUIElement.UIBindingData_Element()
                    {
                        PropertyName = name,
                        PropertyType = bp.PropertyType,
                        Id = element.Id,
                    };
                }
                for (int i=0; i<data.TargetObjects.Count; i++)
                {
                    dynamic tElement = data.TargetObjects[i];
                    if(tElement != null)
                    {
                        var bindTarget = new TtUIElement.UIBindingData_Element()
                        {
                            PropertyName = data.TargetName,
                            PropertyType = data.TargetType,
                            Id = tElement.Id,
                        };
                        var curMode = mSelectedMode;
                        var tBp = tElement.FindBindableProperty(data.TargetName);
                        if(tBp != null && (curMode == EBindingMode.Default))
                        {
                            curMode = tBp.BindingMode;
                        }
                        tElement.BindingDatas[data.TargetName] = new TtUIElement.BindingData_Property()
                        {
                            Source = bindSource,
                            Target = bindTarget,
                            Mode = curMode,
                        };
                        mBindedDataDirty = true;
                    }
                }

                ImGuiAPI.CloseCurrentPopup();
            }
            ImGuiAPI.TreePop();
        }
        void HasProperty(string name, TtBindableProperty bp, ref TourPropertiesData data)
        {
            if (data.HasProperty)
                return;
            if (!name.ToLower().Contains(data.Filter))
                return;
            if (bp.PropertyType != data.TargetType)
                return;
            if(name == data.TargetName)
            {
                for(int i=0; i<data.TargetObjects.Count; i++)
                {
                    if (data.TargetObjects[i] == data.SourceObject)
                        return;
                }
            }
            data.HasProperty = true;
        }
        void DrawUIElementBindableProperty(TtUIElement element, in ImDrawList drawList, EditorUIHost host, ref int idx, string filter, in ExternalInfo info)
        {
            var container = element as TtContainer;
            var data = new TourPropertiesData()
            {
                Idx = idx,
                Filter = filter,
                TargetType = info.PropertyDescriptor.PropertyType,
                SourceObject = element,
                TargetObjects = mBindingTargets,
                TargetName = info.PropertyDescriptor.Name,
                Host = host,
                HasProperty = false,
            };
            element.TourBindProperties(ref data, HasProperty);
            if (!data.HasProperty && container == null)
                return;
            if(ImGuiAPI.TreeNodeEx(Editor.TtUIEditor.GetElementShowName(element) + "##" + idx, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth))
            {
                if (ImGuiAPI.TreeNodeEx("Properties##" + idx, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth))
                {
                    element.TourBindProperties(ref data, BindPropertyTourAction);
                    ImGuiAPI.TreePop();
                }
                if(container != null && container.Children.Count > 0)
                {
                    if(ImGuiAPI.TreeNodeEx("Children##" + idx, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth))
                    {
                        for(int i=0; i<container.Children.Count; i++)
                        {
                            DrawUIElementBindableProperty(container.Children[i], in drawList, host, ref idx, filter, info);
                        }
                        ImGuiAPI.TreePop();
                    }
                }

                ImGuiAPI.TreePop();
            }

            idx++;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AttachedPropertyAttribute : Attribute
    {
        public string Name;
        public string Category;
        public object DefaultValue;
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class BindPropertyDisplayNameAttribute : Attribute
    {
        public virtual string GetDisplayName(IBindableObject element)
        {
            return "";
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BindableObjectAttribute : Attribute
    {

    }
    public class TtBindableProperty
    {
        public string Name;
        public string Category;
        public UTypeDesc PropertyType;
        public UTypeDesc HostType;
        public PGCustomValueEditorAttribute CustomValueEditor;
        public BindPropertyDisplayNameAttribute DisplayNameAtt;

        [Flags]
        internal enum EFlags : UInt64
        {
            GlobalIndexMask = 0xFFFF,
            UpdateDefault = 1 << 16,
            UpdateOnPropertyChanged = 1 << 17,
            UpdateOnLostFocus = 1 << 18,
            UpdateExplicitly = 1 << 19,
            UpdateMask = UpdateDefault | UpdateOnPropertyChanged | UpdateOnLostFocus | UpdateExplicitly,

            ModeDefault = 1 << 20,
            ModeOneTime = 1 << 21,
            ModeOneWay = 1 << 22,
            ModeOneWayToSource = 1 << 23,
            ModeTwoWay = ModeOneWay | ModeOneWayToSource,
            ModeMask = ModeDefault | ModeOneTime | ModeTwoWay,

            IsReadonly          = 1 << 24,
            AutoGen             = 1 << 25,
            CallSetProperty     = 1 << 26,
            AttachedProperty  = 1 << 27,
        }
        EFlags mFlags;
        public UInt16 GlobalIndex
        {
            get => (UInt16)(mFlags & EFlags.GlobalIndexMask);
        }
        public bool IsReadonly
        {
            get => (mFlags & EFlags.IsReadonly) != 0;
            set
            {
                if (value)
                    mFlags |= EFlags.IsReadonly;
                else
                    mFlags &= ~EFlags.IsReadonly;
            }
        }
        public bool IsAutoGen
        {
            get => (mFlags & EFlags.AutoGen) != 0;
            internal set
            {
                if(value)
                    mFlags |= EFlags.AutoGen;
                else
                    mFlags &= ~EFlags.AutoGen;
            }
        }
        public bool IsCallSetProperty
        {
            get => (mFlags & EFlags.CallSetProperty) != 0;
            internal set
            {
                if (value)
                    mFlags |= EFlags.CallSetProperty;
                else
                    mFlags &= ~EFlags.CallSetProperty;
            }
        }
        public bool IsAttachedProperty
        {
            get => (mFlags & EFlags.AttachedProperty) != 0;
            internal set
            {
                if (value)
                    mFlags |= EFlags.AttachedProperty;
                else
                    mFlags &= ~EFlags.AttachedProperty;
            }
        }
        public EUpdateSourceTrigger UpdateSourceTrigger
        {
            get
            {
                switch(mFlags & EFlags.UpdateMask)
                {
                    case EFlags.UpdateDefault: return EUpdateSourceTrigger.Default;
                    case EFlags.UpdateOnPropertyChanged: return EUpdateSourceTrigger.PropertyChanged;
                    case EFlags.UpdateOnLostFocus: return EUpdateSourceTrigger.LostFocus;
                    case EFlags.UpdateExplicitly: return EUpdateSourceTrigger.Explicit;
                }
                return EUpdateSourceTrigger.Default;
            }
            set
            {
                EFlags flag = EFlags.UpdateDefault;
                switch(value)
                {
                    case EUpdateSourceTrigger.Default: 
                        flag = EFlags.UpdateDefault; 
                        break;
                    case EUpdateSourceTrigger.PropertyChanged:
                        flag = EFlags.UpdateOnPropertyChanged;
                        break;
                    case EUpdateSourceTrigger.LostFocus:
                        flag = EFlags.UpdateOnLostFocus;
                        break;
                    case EUpdateSourceTrigger.Explicit:
                        flag = EFlags.UpdateExplicitly;
                        break;
                }
                mFlags = (mFlags & ~EFlags.UpdateMask) | (flag & EFlags.UpdateMask);
            }
        }
        public EBindingMode BindingMode
        {
            get
            {
                switch(mFlags & EFlags.ModeMask)
                {
                    case EFlags.ModeDefault: return EBindingMode.Default;
                    case EFlags.ModeOneTime: return EBindingMode.OneTime;
                    case EFlags.ModeOneWay: return EBindingMode.OneWay;
                    case EFlags.ModeOneWayToSource: return EBindingMode.OneWayToSource;
                    case EFlags.ModeTwoWay: return EBindingMode.TwoWay;
                }
                return EBindingMode.Default;
            }
            set
            {
                EFlags flag = EFlags.ModeDefault;
                switch(value)
                {
                    case EBindingMode.Default:
                        flag = EFlags.ModeDefault;
                        break;
                    case EBindingMode.OneTime:
                        flag = EFlags.ModeOneTime;
                        break;
                    case EBindingMode.OneWay:
                        flag = EFlags.ModeOneWay;
                        break;
                    case EBindingMode.TwoWay:
                        flag = EFlags.ModeTwoWay;
                        break;
                    case EBindingMode.OneWayToSource:
                        flag = EFlags.ModeOneWayToSource;
                        break;
                }
                mFlags = (mFlags & ~EFlags.ModeMask) | (flag & EFlags.ModeMask);
            }
        }
        internal struct NameKey
        {
            private string mName;
            private UTypeDesc mHostType;
            private int mHashCode;

            public NameKey(string name, UTypeDesc hostType)
            {
                mName = name;
                mHostType = hostType;
                mHashCode = mName.GetHashCode() ^ mHostType.GetHashCode();
            }
            public override int GetHashCode()
            {
                return mHashCode;
            }
            public override bool Equals(object obj)
            {
                if ((obj != null) && (obj is NameKey))
                    return Equals((NameKey)obj);
                else
                    return false;
            }
            public bool Equals(NameKey key)
            {
                return (mName.Equals(key.mName)) && (mHostType.Equals(key.mHostType));
            }
        }

        protected TtBindableProperty()
        {
            mFlags = (EFlags)TtBindManager.GetBindablePropertyUniqueGlobalIndex();
        }
        public void CallOnValueChanged<T>(IBindableObject obj, TtBindableProperty property, in T value) 
        {
            var bp = this as TtBindableProperty<T>;
            if (bp != null)
                bp.OnValueChanged(obj, property, value);
        }

        public override int GetHashCode()
        {
            return GlobalIndex;
        }
        public override bool Equals(object obj)
        {
            var pro = obj as TtBindableProperty;
            if (pro == null)
                return false;
            return (this.GlobalIndex == pro.GlobalIndex);
        }
    }
    public class TtBindableProperty<TProperty> : TtBindableProperty
    {
        public TProperty DefaultValue;
        public Action<IBindableObject, TtBindableProperty, TProperty> OnValueChanged;
    }
    public interface IBindableObject
    {
#nullable enable
        public string Name { get; set; }
        public T GetValue<T>([CallerMemberName] string? propertyName = null);
        public T GetValue<T>(TtBindableProperty bp);
        public void SetValue<T>(in T value, [CallerMemberName] string? propertyName = null);
        public void SetValue<T>(in T value, TtBindableProperty bp);
        public void SetBindExpression(TtBindableProperty bp, TtBindingExpressionBase expr);
        public void OnValueChange<T>(in T value, in T oldValue, [CallerMemberName] string? propertyName = null);
        public TtBindingExpressionBase CreateBindingExpression<TProperty>(string propertyName, TtBindingBase binding, TtBindingExpressionBase parent);
        public void ClearBindExpression(TtBindableProperty bp);
        public void RemoveAttachedProperties(Type propertiesHostType);
        public void RemoveAttachedProperty(TtBindableProperty property);
        public void SetAttachedProperties(IBindableObject target);
        public void AddAttachedProperty<T>(TtBindableProperty property, IBindableObject bpHost, in T defaultValue);
        public TtBindableProperty FindBindableProperty(string propertyName);
        public bool HasBinded(EngineNS.UI.Bind.TtBindableProperty bp);

        public void SetTemplateValue(TtTemplateSimpleValue simpleValue);
        public bool IsMatchTriggerCondition<T>(TtTriggerConditionLogical<T> triggerCondition);
        public void SetFromTriggerSimpleValue<T>(TtTriggerSimpleValue<T> triggerSimpleValue);
        public void RestoreFromTriggerSimpleValue<T>(TtTriggerSimpleValue<T> triggerSimpleValue);
        public void SetExpressionValue(TtBindingExpressionBase exp, TtBindableProperty bp);
#nullable disable
    }
    public class TtBindableObject : IBindableObject, IPropertyCustomization
    {
        protected Dictionary<TtBindableProperty, TtBindablePropertyValueBase> mBindExprDic = new Dictionary<TtBindableProperty, TtBindablePropertyValueBase>();
        protected TtTriggerCollection mTriggers = new TtTriggerCollection();
        [Browsable(false)]
        public TtTriggerCollection Triggers => mTriggers;

        public virtual TtBindableProperty FindBindableProperty(string propertyName)
        {
            lock(mBindExprDic)
            {
                foreach(var key in mBindExprDic.Keys)
                {
                    if (key.Name == propertyName)
                        return key;
                }
            }
            return TtEngine.Instance.UIBindManager.FindBindableProperty(propertyName, UTypeDesc.TypeOf(GetType())); 
        }
        //public TtBindingExpressionBase SetBinding(TtBindableProperty bp, TtBindingBase binding)
        //{
        //    return TtBindingOperations.SetBinding(this, bp, binding);
        //}

        public virtual void SetBindExpression(TtBindableProperty bp, TtBindingExpressionBase expr)
        {
            TtBindablePropertyValueBase bpVal = null;
            lock(mBindExprDic)
            {
                if(!mBindExprDic.TryGetValue(bp, out bpVal))
                {
                    bpVal = new TtExpressionValues();
                    mBindExprDic[bp] = bpVal;
                }
            }
            ((TtExpressionValues)bpVal).Expressions.Add(expr);
        }
#nullable enable
        public virtual void SetValue<T>(in T value, [CallerMemberName] string? propertyName = null)
#nullable disable
        {
            var bp = TtEngine.Instance.UIBindManager.FindBindableProperty(propertyName, UTypeDesc.TypeOf(GetType()));
            SetValue<T>(value, bp);
        }
        public virtual void SetValue<T>(in T value, TtBindableProperty bp)
        {
            TtBindablePropertyValueBase bpVal = null;
            lock (mBindExprDic)
            {
                var result = mBindExprDic.TryGetValue(bp, out bpVal);
                //if (bp.IsAttachedProperty && !result)
                //{
                //    bpVal = new TtAttachedValue<IBindableObject, T>(this);
                //    mBindExprDic[bp] = bpVal;
                //}
            }
            if (bpVal == null)
                return;
            if(mTriggers.HasTrigger(bp))
            {
                var oldVal = bpVal.GetValue<T>(bp);
                bpVal.SetValue<T>(this, bp, in value);
                mTriggers.InvokeTriggers(this, bp, oldVal, value);
            }
            else
                bpVal.SetValue<T>(this, bp, in value);
        }
#nullable enable
        public virtual T GetValue<T>([CallerMemberName] string? propertyName = null)
#nullable disable
        {
            var bp = TtEngine.Instance.UIBindManager.FindBindableProperty(propertyName, UTypeDesc.TypeOf(GetType()));
            return GetValue<T>(bp);
        }
        public virtual T GetValue<T>(TtBindableProperty bp)
        {
            if (bp == null)
                return default(T);
            TtBindablePropertyValueBase bpVal = null;
            lock (mBindExprDic)
            {
                if (!mBindExprDic.TryGetValue(bp, out bpVal))
                    return default(T);
            }
            return bpVal.GetValue<T>(bp);            
        }
#nullable enable
        public virtual void OnValueChange<T>(in T value, in T oldValue, [CallerMemberName] string? propertyName = null)
#nullable disable
        {
            var bp = FindBindableProperty(propertyName);
            if (bp == null)
                return;
            TtBindablePropertyValueBase bpVal = null;
            lock (mBindExprDic)
            {
                mBindExprDic.TryGetValue(bp, out bpVal);
            }
            if (bpVal == null)
            {
                mTriggers.InvokeTriggers(this, bp, oldValue, value);
            }
            else
            {
                if (mTriggers.HasTrigger(bp))
                {
                    var oldVal = bpVal.GetValue<T>(bp);
                    bpVal.SetValue<T>(this, bp, in value);
                    mTriggers.InvokeTriggers(this, bp, oldVal, value);
                }
                else
                    bpVal.SetValue<T>(this, bp, in value);
            }
        }
        public virtual TtBindingExpressionBase CreateBindingExpression<TProperty>(string propertyName, TtBindingBase binding, TtBindingExpressionBase parent)
        {
            var retVal = new TtBindingExpression<TProperty>(binding, parent);
            retVal.TargetObject = this;
            retVal.TargetProperty = (TtBindableProperty<TProperty>)TtEngine.Instance.UIBindManager.FindBindableProperty(propertyName, UTypeDesc.TypeOf(GetType()));
            return retVal;
        }
        public virtual void ClearBindExpression(TtBindableProperty bp)
        {
            lock(mBindExprDic)
            {
                mBindExprDic.Remove(bp);
            }
        }
        public virtual void RemoveAttachedProperties(Type propertiesHostType)
        {
            System.Collections.Generic.HashSet<TtBindableProperty> removePros = new HashSet<TtBindableProperty>();
            foreach (var data in mBindExprDic)
            {
                if(data.Key.HostType.IsEqual(propertiesHostType))
                {
                    removePros.Add(data.Key);
                }
            }
            foreach(var key in removePros)
            {
                RemoveAttachedProperty(key);
            }
        }
        public virtual void RemoveAttachedProperty(TtBindableProperty property)
        {
            lock(mBindExprDic)
            {
                mBindExprDic.Remove(property);
            }
        }
        public virtual void SetAttachedProperties(IBindableObject target)
        {
        }
        public void AddAttachedProperty<T>(TtBindableProperty property, IBindableObject bpHost, in T defaultValue)
        {
        }

        [Browsable(false)]
        public bool IsPropertyVisibleDirty
        {
            get;
            set;
        } = false;
        public string Name { get; set; }

        public virtual void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            collection.InitValue(this, Rtti.UTypeDesc.TypeOf(this.GetType()), pros, parentIsValueType);

            // attached properties
            foreach(var bindData in mBindExprDic)
            {
                if(bindData.Value.Type == TtBindablePropertyValueBase.EType.AttachedValue)
                {
                    var proDesc = PropertyCollection.PropertyDescPool.QueryObjectSync();
                    proDesc.Name = bindData.Key.Name;
                    if(bindData.Key.DisplayNameAtt != null)
                        proDesc.DisplayName = bindData.Key.DisplayNameAtt.GetDisplayName(this);
                    proDesc.PropertyType = bindData.Key.PropertyType;
                    proDesc.Category = bindData.Key.Category;
                    proDesc.CustomValueEditor = bindData.Key.CustomValueEditor;
                    collection.Add(proDesc);
                }
            }
        }

        public virtual object GetPropertyValue(string propertyName)
        {
            var pro = this.GetType().GetProperty(propertyName);
            if (pro != null)
                return pro.GetValue(this);

            foreach(var bindData in mBindExprDic)
            {
                if(bindData.Key.Name == propertyName)
                {
                    return bindData.Value.GetValue<object>(bindData.Key);
                }
            }

            return null;
        }

        public virtual void SetPropertyValue(string propertyName, object value)
        {
            var pro = this.GetType().GetProperty(propertyName);
            if (pro != null)
                pro.SetValue(this, value);
            else
            {
                foreach(var bindData in mBindExprDic)
                {
                    if(bindData.Key.Name == propertyName)
                    {
                        bindData.Value.SetValue<object>(this, bindData.Key, in value);
                        break;
                    }
                }
            }
        }

        public bool HasBinded(TtBindableProperty bp)
        {
            lock(mBindExprDic)
            {
                return mBindExprDic.ContainsKey(bp);
            }
        }

        public void SetTemplateValue(TtTemplateSimpleValue simpleValue)
        {
            var objType = this.GetType();
            if ((simpleValue.Property.HostType.IsEqual(objType) || simpleValue.Property.HostType.IsParentClass(objType)) && !simpleValue.Property.IsAttachedProperty)
            {
                var prop = objType.GetProperty(simpleValue.Property.Name);
                if (prop == null)
                    return;
                else
                    prop.SetValue(this, simpleValue.Value);
            }
            else
            {
                this.SetValue(simpleValue.Value, simpleValue.Property);
            }
        }

        public bool IsMatchTriggerCondition<T>(TtTriggerConditionLogical<T> triggerCondition)
        {
            switch (triggerCondition.Op)
            {
                case TtTriggerConditionLogical<T>.ELogicalOperation.Equal:
                    return EqualityComparer<T>.Default.Equals(triggerCondition.Value.GetValue<T>(), this.GetValue<T>(triggerCondition.Property));
                case TtTriggerConditionLogical<T>.ELogicalOperation.NotEqual:
                    return !EqualityComparer<T>.Default.Equals(triggerCondition.Value.GetValue<T>(), this.GetValue<T>(triggerCondition.Property));
            }
            return false;
        }

        public void SetFromTriggerSimpleValue<T>(TtTriggerSimpleValue<T> triggerSimpleValue)
        {
            var objType = this.GetType();
            if (triggerSimpleValue.Property.IsAttachedProperty)
            {
                triggerSimpleValue.OldValueStore.SetValue(this.GetValue<T>(triggerSimpleValue.Property));
                this.SetValue(triggerSimpleValue.ValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }
            else if (triggerSimpleValue.Property.HostType.IsEqual(objType) || triggerSimpleValue.Property.HostType.IsParentClass(objType))
            {
                var prop = this.GetType().GetProperty(triggerSimpleValue.Property.Name);
                triggerSimpleValue.OldValueStore.SetValue((T)prop.GetValue(this));
                prop.SetValue(this, triggerSimpleValue.ValueStore.GetValue<T>());
            }
            else
            {
                triggerSimpleValue.OldValueStore.SetValue(this.GetValue<T>(triggerSimpleValue.Property));
                this.SetValue(triggerSimpleValue.ValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }
        }

        public void RestoreFromTriggerSimpleValue<T>(TtTriggerSimpleValue<T> triggerSimpleValue)
        {
            var objType = this.GetType();
            if (triggerSimpleValue.Property.IsAttachedProperty)
            {
                this.SetValue(triggerSimpleValue.OldValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }
            else if (triggerSimpleValue.Property.HostType.IsEqual(objType) || triggerSimpleValue.Property.HostType.IsParentClass(objType))
            {
                var prop = this.GetType().GetProperty(triggerSimpleValue.Property.Name);
                prop.SetValue(this, triggerSimpleValue.OldValueStore.GetValue<T>());
            }
            else
            {
                this.SetValue(triggerSimpleValue.OldValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }
        }

        public void SetExpressionValue(TtBindingExpressionBase exp, TtBindableProperty bp)
        {
            var value = this.GetPropertyValue(bp.Name);
            exp.SetValue(bp, value);
        }
    }
}
