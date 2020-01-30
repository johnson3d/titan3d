using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for BezierControl.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("运算/曲线(bezier)", "根据输入获取输出")]
    public partial class BezierControl
    {
        //double mYMax = 100;
        //public double YMax
        //{
        //    get { return mYMax; }
        //    set
        //    {
        //        mYMax = value;
        //        IsDirty = true;
        //        OnPropertyChanged("YMax");
        //    }
        //}

        //double mYMin = 0;
        //public double YMin
        //{
        //    get { return mYMin; }
        //    set
        //    {
        //        mYMin = value;
        //        IsDirty = true;
        //        OnPropertyChanged("YMin");
        //    }
        //}

        //double mXMax = 100;
        //public double XMax
        //{
        //    get { return mXMax; }
        //    set
        //    {
        //        mXMax = value;
        //        IsDirty = true;
        //        OnPropertyChanged("XMax");
        //    }
        //}

        //double mXMin = 0;
        //public double XMin
        //{
        //    get { return mXMin; }
        //    set
        //    {
        //        mXMin = value;
        //        IsDirty = true;
        //        OnPropertyChanged("XMin");
        //    }
        //}

        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlValueInputHandle = ValueInputHandle;
            mCtrlValueOutputHandle = ValueOutputHandle;
            mCtrlValueYMaxInputHandle = ValueYMaxInputHandle;
            mCtrlValueYMinInputHandle = ValueYMinInputHandle;
            mCtrlValueXMinInputHandle = ValueXMinInputHandle;
            mCtrlValueXMaxInputHandle = ValueXMaxInputHandle;
            mCtrlValueXLoopHandle = ValueXLoopHandle;
            OnCreateBezierPoint = _OnCreateBezierPoint;
            
            //LineXBezierCtrl.BezierWidth = mBezierWidth;
            //LineXBezierCtrl.BezierHeight = mBezierHeight;
            LineXBezierCtrl.OnDirtyChanged = OnLineXBezierControlDirtyChanged;

            BindingOperations.SetBinding(CheckBox_IsXLoop, CheckBox.IsCheckedProperty, new Binding("XLoop") { Source = mTemplateClassInstance, Mode=BindingMode.TwoWay });
            var clsType = mTemplateClassInstance.GetType();
            var xMaxPro = clsType.GetProperty("XMax");
            xMaxPro.SetValue(mTemplateClassInstance, 1.0);
            var xMinPro = clsType.GetProperty("XMin");
            xMinPro.SetValue(mTemplateClassInstance, 0.0);
            var yMaxPro = clsType.GetProperty("YMax");
            yMaxPro.SetValue(mTemplateClassInstance, 1.0);
            var yMinPro = clsType.GetProperty("YMin");
            yMinPro.SetValue(mTemplateClassInstance, 0.0);

            BindingOperations.SetBinding(XMaxDefaultValue, TextBlock.TextProperty, new Binding("XMax") { Source = mTemplateClassInstance });
            BindingOperations.SetBinding(XMinDefaultValue, TextBlock.TextProperty, new Binding("XMin") { Source = mTemplateClassInstance });
            BindingOperations.SetBinding(YMaxDefaultValue, TextBlock.TextProperty, new Binding("YMax") { Source = mTemplateClassInstance });
            BindingOperations.SetBinding(YMinDefaultValue, TextBlock.TextProperty, new Binding("YMin") { Source = mTemplateClassInstance });
        }
        EngineNS.BezierPointBase _OnCreateBezierPoint()
        {
            return new EditorCommon.Controls.LineXBezierControl.BezierPoint(LineXBezierCtrl);
        }
        partial void SetBezierPointsToCtrl()
        {
            LineXBezierCtrl.BezierWidth = mBezierWidth;
            LineXBezierCtrl.BezierHeight = mBezierHeight;
            LineXBezierCtrl.BezierPoints.Clear();
            LineXBezierCtrl.BezierPoints.AddRange(mBezierPoints);
            LineXBezierCtrl.UpdateShow();
        }
        partial void GetBezierPointsFromCtrl()
        {
            mBezierPoints = new List<EngineNS.BezierPointBase>(LineXBezierCtrl.BezierPoints);
            mBezierWidth = LineXBezierCtrl.BezierWidth;
            mBezierHeight = LineXBezierCtrl.BezierHeight;
        }

        private void OnLineXBezierControlDirtyChanged(bool bDirty)
        {
            if(bDirty)
                IsDirty = true;
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        private void TextBox_InputValue_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                var bindingExp = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
                bindingExp.UpdateSource();
            }
        }

        private void TextBox_InputValue_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            var bindingExp = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
            bindingExp.UpdateSource();
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as BezierControl;

            GetBezierPointsFromCtrl();
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            copyedNode.mBezierPoints = new List<EngineNS.BezierPointBase>(mBezierPoints);
            copyedNode.mBezierWidth = mBezierWidth;
            copyedNode.mBezierHeight = mBezierHeight;
            copyedNode.SetBezierPointsToCtrl();

            return copyedNode;
        }

        #region 代码生成

        protected override void CollectionErrorMsg()
        {
            if(ValueOutputHandle.HasLink)
            {
                if(!ValueInputHandle.HasLink)
                {
                    HasError = true;
                    ErrorDescription += "未设置X输入";
                }
            }
        }

        #endregion

    }
}
