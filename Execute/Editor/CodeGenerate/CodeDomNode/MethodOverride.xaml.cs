using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CodeGenerateSystem.Base;

namespace CodeDomNode
{
    public partial class MethodOverride : CodeGenerateSystem.Base.IDebugableNode
    {
        bool mBreaked = false;
        public bool Breaked
        {
            get { return mBreaked; }
            set
            {
                if (mBreaked == value)
                    return;
                mBreaked = value;
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    BreakedPinShow = mBreaked;
                    ChangeParentLogicLinkLine(mBreaked);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }

        public void ChangeParentLogicLinkLine(bool change)
        {
        }
        public override void Tick(long elapsedMillisecond)
        {
        }
        public bool CanBreak()
        {
            return true;
        }

        public bool IsEvent
        {
            get { return (bool)GetValue(IsEventProperty); }
            set { SetValue(IsEventProperty, value); }
        }
        public static readonly DependencyProperty IsEventProperty = DependencyProperty.Register("IsEvent", typeof(bool), typeof(MethodOverride), new UIPropertyMetadata(false, OnIsEventPropertyChanged));
        private static void OnIsEventPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as MethodOverride;
            var newValue = (bool)e.NewValue;
            if (newValue)
                ctrl.IconImg = ctrl.TryFindResource("Icon_Event") as ImageSource;
            else
                ctrl.IconImg = ctrl.TryFindResource("Icon_OverrideFunction") as ImageSource;
        }

        public ImageSource IconImg
        {
            get { return (ImageSource)GetValue(IconImgProperty); }
            set { SetValue(IconImgProperty, value); }
        }
        public static readonly DependencyProperty IconImgProperty = DependencyProperty.Register("IconImg", typeof(ImageSource), typeof(MethodOverride), new UIPropertyMetadata(null));

        partial void InitConstruction()
        {
            this.InitializeComponent();

            mCtrlMethodPin_Next = MethodPin_Next;
            mParamsPanel = StackPanel_Params;

            var param = CSParam as MethodOverrideConstructParam;
            if(param.MethodInfo.IsEvent())
            {
                BlendBrush = TryFindResource("Node_Event") as Brush;
            }
            else
            {
                BlendBrush = TryFindResource("Node_Function") as Brush;
            }
        }

        public string GetMethodKeyName()
        {
            return NodeName;
        }

        protected override void CollectionErrorMsg()
        {
            var noUse = CollectionErrorMsg_Async();
        }
        protected async Task CollectionErrorMsg_Async()
        {
            var param = CSParam as MethodOverrideConstructParam;

            if (param.MethodInfo.ParentClassType == null)
            {
                HasError = true;
                ErrorDescription = "找不到该函数所属类型";
                return;
            }
            if(param.MethodInfo.IsFromMacross)
            {
                var customFunctions = new List<Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData>();

                var resInfo = await Macross.ResourceInfos.MacrossResourceInfo.GetBaseMacrossResourceInfo(param.MethodInfo.ParentClassType);
                if (resInfo == null)
                {
                    HasError = true;
                    ErrorDescription = "找不到继承类";
                    return;
                }

                switch (param.CSType)
                {
                    case EngineNS.ECSType.Client:
                        customFunctions = resInfo.CustomFunctions_Client;
                        break;
                    case EngineNS.ECSType.Server:
                        customFunctions = resInfo.CustomFunctions_Server;
                        break;
                }

                Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData customFunc = null;
                foreach(var func in customFunctions)
                {
                    if(func.Id == param.MethodInfo.FuncId)
                    {
                        customFunc = func;
                    }
                }
                if(customFunc == null)
                {
                    HasError = true;
                    ErrorDescription = "找不到继承的函数";
                    return;
                }

                if(param.MethodInfo.Params.Count != (customFunc.MethodInfo.InParams.Count + customFunc.MethodInfo.OutParams.Count))
                {
                    HasError = true;
                    ErrorDescription = "继承函数参数已改变";
                    return;
                }
                List<CustomMethodInfo.FunctionParam> funcParams = new List<CustomMethodInfo.FunctionParam>();
                funcParams.AddRange(customFunc.MethodInfo.InParams);
                funcParams.AddRange(customFunc.MethodInfo.OutParams);
                for(int i=0; i<param.MethodInfo.Params.Count; i++)
                {
                    var param1 = param.MethodInfo.Params[i];
                    var param2 = funcParams[i];

                    if(param1.ParameterType != param2.ParamType.GetActualType())
                    {
                        HasError = true;
                        ErrorDescription = "继承函数参数已改变";
                        return;
                    }
                }
            }
            else
            {
                Type[] paramTypes = new Type[param.MethodInfo.Params.Count];
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    switch (param.MethodInfo.Params[i].FieldDirection)
                    {
                        case System.CodeDom.FieldDirection.In:
                            if (param.MethodInfo.Params[i].IsParamsArray)
                                throw new InvalidOperationException("未实现");
                            else
                                paramTypes[i] = param.MethodInfo.Params[i].ParameterType;
                            break;
                        case System.CodeDom.FieldDirection.Out:
                        case System.CodeDom.FieldDirection.Ref:
                            if (param.MethodInfo.Params[i].IsParamsArray)
                                throw new InvalidOperationException("未实现");
                            else
                                paramTypes[i] = param.MethodInfo.Params[i].ParameterType.MakeByRefType();
                            break;
                    }
                    if (paramTypes[i] == null)
                    {
                        HasError = true;
                        ErrorDescription = $"找不到参数{param.MethodInfo.Params[i].ParamName}的类型";
                        return;
                    }
                }
                var method = param.MethodInfo.ParentClassType.GetMethod(param.MethodInfo.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null);
                if (method == null)
                {
                    HasError = true;
                    ErrorDescription = $"在类型{EngineNS.Rtti.RttiHelper.GetAppTypeString(param.MethodInfo.ParentClassType)}找不到符合当前参数的函数{param.MethodInfo.MethodName}";
                    return;
                }
            }

            foreach (var child in mChildNodes)
            {
                if (!child.CheckError())
                {
                    HasError = true;
                    ErrorDescription = child.ErrorDescription;
                    break;
                }
            }
        }

        public override bool CanDuplicate()
        {
            return false;
        }
    }
}
