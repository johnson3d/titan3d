using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace MaterialEditor.Controls
{
    /// <summary>
    /// Comment.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("添加注释", "", CodeGenerateSystem.ShowInNodeList.enIconType.Comment)]
    [CodeGenerateSystem.CustomConstructionParams(typeof(CommentConstructParam))]
    public partial class Comment : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class CommentConstructParam : NodeControlConstructionBase
        {
            string mComment = "";
            [EngineNS.Rtti.MetaData]
            public string Comment
            {
                get => mComment;
                set
                {
                    mComment = value;
                    OnPropertyChanged("Comment");
                }
            }
            [EngineNS.Rtti.MetaData]
            public double Width { get; set; } = 300;
            [EngineNS.Rtti.MetaData]
            public double Height { get; set; } = 200;
            EngineNS.Color mColor = EngineNS.Color.White;
            [EngineNS.Rtti.MetaData]
            public EngineNS.Color Color
            {
                get => mColor;
                set
                {
                    mColor = value;
                    OnPropertyChanged("Color");
                }
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as CommentConstructParam;
                retVal.Width = Width;
                retVal.Height = Height;
                retVal.Comment = Comment;
                retVal.Color = Color;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as CommentConstructParam;
                if (param == null)
                    return false;
                if (Width != param.Width ||
                    Height != param.Height)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + Width.ToString() + Height.ToString()).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;

        public Comment(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            this.InitializeComponent();

            var param = CSParam as CommentConstructParam;
            this.Width = param.Width;
            this.Height = param.Height;

            CommentText.SetBinding(TextBlock.TextProperty, new Binding("Comment") { Source = param });
            ContentGrid.SetBinding(Grid.BackgroundProperty, new Binding("BlendBrush") { Source = this });

            this.BlendBrush = new SolidColorBrush(Color.FromArgb(param.Color.A, param.Color.R, param.Color.G, param.Color.B));

            int deepestZIndex = -10;
            if (HostNodesContainer != null && !HostNodesContainer.IsLoading)
            {
                bool has = false;
                double left = double.MaxValue, right = double.MinValue, top = double.MaxValue, bottom = double.MinValue;
                // 创建时包围所有选中的节点
                for (int i = 0; i < HostNodesContainer.CtrlNodeList.Count; i++)
                {
                    var node = HostNodesContainer.CtrlNodeList[i];
                    if (!node.Selected)
                        continue;
                    var loc = node.GetLocation();
                    var nRight = loc.X + node.GetWidth();
                    var nBottom = loc.Y + node.GetHeight();

                    var zIndex = Canvas.GetZIndex(node);
                    if (deepestZIndex > zIndex)
                        deepestZIndex = zIndex;

                    if (left > loc.X)
                        left = loc.X;
                    if (top > loc.Y)
                        top = loc.Y;
                    if (right < nRight)
                        right = nRight;
                    if (bottom < nBottom)
                        bottom = nBottom;

                    has = true;
                }

                if (has)
                {
                    mCreateX = left - 50;
                    mCreateY = top - 100;
                    this.Width = right + 50 - mCreateX;
                    this.Height = bottom + 50 - mCreateY;
                    param.Width = this.Width;
                    param.Height = this.Height;
                }
            }
            Canvas.SetZIndex(this, deepestZIndex - 1);

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
            cpInfo.PropertyName = "Comment";
            cpInfo.PropertyType = typeof(string);
            cpInfo.PropertyAttributes.Add(new DisplayNameAttribute("注释"));
            cpInfos.Add(cpInfo);
            cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
            cpInfo.PropertyName = "Color";
            cpInfo.PropertyType = typeof(EngineNS.Color);
            cpInfo.PropertyAttributes.Add(new DisplayNameAttribute("颜色"));
            cpInfo.PropertyAttributes.Add(new EngineNS.Editor.Editor_ColorPicker());
            cpInfos.Add(cpInfo);
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            var clsType = mTemplateClassInstance.GetType();
            var pro_comment = clsType.GetProperty("Comment");
            pro_comment.SetValue(mTemplateClassInstance, param.Comment);
            var pro_color = clsType.GetProperty("Color");
            pro_color.SetValue(mTemplateClassInstance, param.Color);
            mTemplateClassInstance.OnPropertyChangedAction = OnTemplateClassPropertyChanged;
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {

        }

        double mCreateX = double.NaN;
        double mCreateY = double.NaN;
        public override void ModifyCreatePosition(ref double x, ref double y)
        {
            if (!double.IsNaN(mCreateX))
                x = mCreateX;
            if (!double.IsNaN(mCreateY))
                y = mCreateY;
        }

        List<CodeGenerateSystem.Base.BaseNodeControl> mContainedNodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();
        protected override void StartDrag(UIElement dragObj, MouseButtonEventArgs e)
        {
            mContainedNodes.Clear();
            base.StartDrag(dragObj, e);

            // 计算所有在范围内的节点
            int deepestZIndex = -10;
            var lt = ContentGrid.TranslatePoint(new Point(0, 0), ParentDrawCanvas);
            var rb = ContentGrid.TranslatePoint(new Point(ContentGrid.ActualWidth, ContentGrid.ActualHeight), ParentDrawCanvas);
            var nodes = HostNodesContainer.CtrlNodeList;
            for (int i = 0; i < nodes.Count; i++)
            {
                var checkNode = nodes[i];
                var loc = checkNode.GetLocation();
                var nRight = loc.X + checkNode.GetWidth();
                var nBottom = loc.Y + checkNode.GetHeight();

                if (lt.X <= loc.X && rb.X >= nRight && lt.Y <= loc.Y && rb.Y >= nBottom)
                {
                    var zIndex = Canvas.GetZIndex(checkNode);
                    if (deepestZIndex > zIndex)
                        deepestZIndex = zIndex;
                    mContainedNodes.Add(checkNode);
                    checkNode.CalculateDeltaPt(e);
                }
            }
            Canvas.SetZIndex(this, deepestZIndex - 1);
        }
        protected override void DragMove(MouseEventArgs e)
        {
            base.DragMove(e);

            var pt = e.GetPosition(ParentDrawCanvas);
            for (int i = 0; i < mContainedNodes.Count; i++)
            {
                mContainedNodes[i].MoveWithPt(pt);
            }
        }

        protected override void OnSizeChanged(double width, double height)
        {
            var param = CSParam as CommentConstructParam;
            param.Width = width;
            param.Height = height;
        }

        void OnTemplateClassPropertyChanged(string propertyName, object newValue, object oldValue)
        {
            var param = CSParam as CommentConstructParam;
            switch (propertyName)
            {
                case "Comment":
                    param.Comment = (string)newValue;
                    //CommentText.Text = param.Comment;
                    break;
                case "Color":
                    param.Color = (EngineNS.Color)newValue;
                    this.BlendBrush = new SolidColorBrush(Color.FromArgb(param.Color.A, param.Color.R, param.Color.G, param.Color.B));
                    break;
            }
        }
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var consParam = CSParam as CommentConstructParam;
            var copyedNode = base.Duplicate(param) as Comment;
            copyedNode.Width = consParam.Width;
            copyedNode.Height = consParam.Height;
            var copyedConsParam = copyedNode.CSParam as CommentConstructParam;
            copyedConsParam.Width = copyedNode.Width;
            copyedConsParam.Height = copyedNode.Height;
            return copyedNode;
        }
    }
}
