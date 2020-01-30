using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// LinkInControl.xaml 的交互逻辑
    /// </summary>
    public partial class LinkInControl : Base.LinkPinControl
    {
        public LinkInControl()
        {
            InitializeComponent();
            UpdateImageShow();
        }

        protected override void SelectedOperation(bool selected)
        {
            var storyboard = this.FindResource("Storyboard_Selected") as Storyboard;
            if (selected)
                storyboard?.Begin();
            else
                storyboard?.Stop();
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tempInfos = new List<Base.LinkInfo>(LinkInfos);
            var redoAction = new Action<object>((obj) =>
            {
                Clear();
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                            (obj) =>
                                            {
                                                foreach (var li in tempInfos)
                                                {
                                                    LinkInfos.Add(new Base.LinkInfo(HostNodesContainer.GetDrawCanvas(), li.m_linkFromObjectInfo, li.m_linkToObjectInfo));
                                                }
                                            }, "Remove Link");
            HostNodeControl.IsDirty = true;
        }

        private void userControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShow();
        }

        public override double GetPinWidth()
        {
            return Img.Width;
        }
        public override double GetPinHeight()
        {
            return Img.Height;
        }
    }
}
