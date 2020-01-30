using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    public sealed partial class PropertyNode : CodeGenerateSystem.Base.IDebugableNode
    {
        #region IDebugableNode

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
            ChangeParentLogicLinkLine(change, MethodInHandle);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, MethodInHandle);
        }
        public bool CanBreak()
        {
            var param = CSParam as PropertyNodeConstructionParams;
            return (param.PropertyInfo.Direction == PropertyInfoAssist.enDirection.Set);
        }

        #endregion

        public string ValuePinString
        {
            get { return (string)GetValue(ValuePinStringProperty); }
            set { SetValue(ValuePinStringProperty, value); }
        }
        public static readonly DependencyProperty ValuePinStringProperty = DependencyProperty.Register("ValuePinString", typeof(string), typeof(PropertyNode));
        public string ValueTypeString
        {
            get { return (string)GetValue(ValueTypeStringProperty); }
            set { SetValue(ValueTypeStringProperty, value); }
        }
        public static readonly DependencyProperty ValueTypeStringProperty = DependencyProperty.Register("ValueTypeString", typeof(string), typeof(PropertyNode));

        public Visibility TargetPinVisibility
        {
            get { return (Visibility)GetValue(TargetPinVisibilityProperty); }
            set { SetValue(TargetPinVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TargetPinVisibilityProperty = DependencyProperty.Register("TargetPinVisibility", typeof(Visibility), typeof(PropertyNode));

        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlMethodInHandle = MethodInHandle;
            mCtrlMethodOutHandle = MethodOutHandle;
            mCtrlInHandle_Target = InHandle_Target;
            mCtrlValueInHandle = ValueInHandle;
            mCtrlValueOutHandle = ValueOutHandle;

            var param = CSParam as PropertyNodeConstructionParams;
            BindingOperations.ClearBinding(this, PropertyNode.ValuePinStringProperty);
            BindingOperations.SetBinding(this, PropertyNode.ValuePinStringProperty, new Binding("PropertyName") { Source = param.PropertyInfo });
            BindingOperations.ClearBinding(this, PropertyNode.ValueTypeStringProperty);
            BindingOperations.SetBinding(this, PropertyNode.ValueTypeStringProperty, new Binding("PropertyType") { Source = param.PropertyInfo, Converter = new EditorCommon.Converter.Type2StringConverter_Name() });
            BindingOperations.ClearBinding(this, PropertyNode.NodeNameBinderProperty);
            BindingOperations.SetBinding(this, PropertyNode.NodeNameBinderProperty, new Binding("PropertyName") { Source = param.PropertyInfo });

            switch(param.PropertyInfo.HostType)
            {
                case MethodInfoAssist.enHostType.Static:
                case MethodInfoAssist.enHostType.Local:
                    TargetPinVisibility = Visibility.Collapsed;
                    break;
                case MethodInfoAssist.enHostType.This:
                case MethodInfoAssist.enHostType.Base:
                    TargetThisFlag.Visibility = Visibility.Visible;
                    break;
                default:
                    TargetThisFlag.Visibility = Visibility.Collapsed;
                    break;
            }
            switch(param.PropertyInfo.Direction)
            {
                case PropertyInfoAssist.enDirection.Set:
                    ValueOutHandle.NameString = "";
                    TB_ValueOutType.Visibility = Visibility.Collapsed;
                    Grid.SetRow(ValueOutPanel, 2);
                    break;
            }

            this.SetBinding(ToolTipProperty, new Binding("ClassInstanceName") { Source = this });
        }

        public static PropertyInfoAssist GetAssistFromPropertyInfo(PropertyInfo proInfo, Type parentClassType, PropertyInfoAssist.enDirection direction, string path, MethodInfoAssist.enHostType hostType)
        {
            var retValue = new PropertyInfoAssist();
            retValue.Path = path;
            if (proInfo == null)
                return retValue;
            if (direction == PropertyInfoAssist.enDirection.Set && !proInfo.CanWrite)
                return retValue;
            if (direction == PropertyInfoAssist.enDirection.Get && !proInfo.CanRead)
                return retValue;

            retValue.Direction = direction;
            retValue.PropertyName = proInfo.Name;
            retValue.PropertyType = proInfo.PropertyType;
            retValue.ParentClassType = parentClassType;
            retValue.HostType = hostType;
            return retValue;
        }

        public static PropertyInfoAssist GetAssistFromFieldInfo(FieldInfo fieldInfo, Type parentClassType, PropertyInfoAssist.enDirection direction, string path, MethodInfoAssist.enHostType hostType)
        {
            var retValue = new PropertyInfoAssist();
            retValue.Path = path;
            if (fieldInfo == null)
                return retValue;

            retValue.Direction = direction;
            retValue.PropertyName = fieldInfo.Name;
            retValue.PropertyType = fieldInfo.FieldType;
            retValue.ParentClassType = parentClassType;
            retValue.IsField = true;
            retValue.HostType = hostType;
            return retValue;
        }


        public static string GetParamPreInfo(PropertyInfoAssist.enDirection dir)
        {
            switch (dir)
            {
                case PropertyInfoAssist.enDirection.Set:
                    return "设置";
                case PropertyInfoAssist.enDirection.Get:
                    return "读取";
            }
            return "";
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys)
        {
            var param = CSParam as PropertyNodeConstructionParams;
            BindingOperations.ClearBinding(ProDefaultValue, TextBlock.TextProperty);
            ProDefaultValue.SetBinding(TextBlock.TextProperty, new Binding(param.PropertyInfo.PropertyName) { Source = mTemplateClassInstance });
        }

        partial void OnTargetLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            TargetThisFlag.Visibility = Visibility.Collapsed;
        }
        partial void OnTargetLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            var param = CSParam as PropertyNodeConstructionParams;
            switch (param.PropertyInfo.HostType)
            {
                case MethodInfoAssist.enHostType.Base:
                case MethodInfoAssist.enHostType.This:
                    TargetThisFlag.Visibility = Visibility.Visible;
                    break;
            }
        }
        partial void OnValueLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            ProDefaultValue.Visibility = Visibility.Collapsed;
        }
        partial void OnValueLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            ProDefaultValue.Visibility = Visibility.Visible;
        }

        partial void ChangeValueType_WPF(CodeDomNode.VariableType varType)
        {

        }

        //partial void InitializeLinkLine()
        //{
        //    if (ParentDrawCanvas == null)
        //        return;

        //    BindingOperations.ClearBinding(this.mParentLinkPath, Path.VisibilityProperty);
        //    BindingOperations.SetBinding(this.mParentLinkPath, Path.VisibilityProperty, new Binding("Visibility") { Source = this });
        //    mParentLinkPath.Stroke = Brushes.LightGray;
        //    mParentLinkPath.StrokeDashArray = new DoubleCollection(new double[] { 2, 4 });
        //    //m_ParentLinkPath.StrokeThickness = 3;
        //    mParentLinkPathFig.Segments.Add(mParentLinkBezierSeg);
        //    PathFigureCollection pfc = new PathFigureCollection();
        //    pfc.Add(mParentLinkPathFig);
        //    PathGeometry pg = new PathGeometry();
        //    pg.Figures = pfc;
        //    mParentLinkPath.Data = pg;
        //    ParentDrawCanvas.Children.Add(mParentLinkPath);
        //}

        //public override void UpdateLink()
        //{
        //    base.UpdateLink();

        //    if (mHostUsefulMemberData == null || mHostUsefulMemberData.LinkObject == null)
        //        return;

        //    mParentLinkPathFig.StartPoint = mHostUsefulMemberData.LinkObject.LinkElement.TranslatePoint(mHostUsefulMemberData.LinkObject.LinkElementOffset, ParentDrawCanvas);

        //    // 如果这个节点隐藏，就获取打包它的节点的坐标。
        //    mParentLinkBezierSeg.Point3 = GetPositionInContainer();

        //    double delta = Math.Max(Math.Abs(mParentLinkBezierSeg.Point3.X - mParentLinkPathFig.StartPoint.X) / 2, 25);
        //    delta = Math.Min(150, delta);

        //    switch (mHostUsefulMemberData.LinkObject.BezierType)
        //    {
        //        case CodeGenerateSystem.Base.enBezierType.Left:
        //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X - delta, mParentLinkPathFig.StartPoint.Y);
        //            break;
        //        case CodeGenerateSystem.Base.enBezierType.Right:
        //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X + delta, mParentLinkPathFig.StartPoint.Y);
        //            break;
        //        case CodeGenerateSystem.Base.enBezierType.Top:
        //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X, mParentLinkPathFig.StartPoint.Y - delta);
        //            break;
        //        case CodeGenerateSystem.Base.enBezierType.Bottom:
        //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X, mParentLinkPathFig.StartPoint.Y + delta);
        //            break;
        //    }

        //    mParentLinkBezierSeg.Point2 = new Point(mParentLinkBezierSeg.Point3.X, mParentLinkBezierSeg.Point3.Y - delta);

        //}

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as PropertyNode;
            copyedNode.AutoGenericIsNullCode = AutoGenericIsNullCode;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            return copyedNode;
        }
    }
}
