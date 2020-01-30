using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    public partial class ExpandNode : CodeGenerateSystem.Base.IDebugableNode
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
            ChangeParentLogicLinkLine(change, MethodLink_Pre);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, MethodLink_Pre);
        }
        public bool CanBreak()
        {
            return true;
        }

        #endregion

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                UpdateFilter(mFilterString);

                OnPropertyChanged("FilterString");
            }
        }
        void UpdateFilter(string filterStr)
        {
            if(string.IsNullOrEmpty(filterStr))
            {
                foreach (var child in StackPanel_Members.Children)
                {
                    var cb = child as CheckBox;
                    cb.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                var lowerFilter = filterStr.ToLower();
                foreach (var child in StackPanel_Members.Children)
                {
                    var cb = child as CheckBox;
                    if (cb.Content.ToString().ToLower().Contains(lowerFilter))
                        cb.Visibility = System.Windows.Visibility.Visible;
                    else
                        cb.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        partial void InitConstruction()
        {
            this.InitializeComponent();

            mCtrlMethodPin_Pre = MethodLink_Pre;
            mCtrlMethodPin_Next = MethodLink_Next;
            mCtrlTarget = TargetPin;
            mCtrlTargetOut = TargetOutPin;
            mParamPanel = StackPanel_Values;
            mChildNodeContainer = mParamPanel;

            // todo:去掉没有的成员
            InitMembers();
        }

        void InitMembers()
        {
            StackPanel_Members.Children.Clear();

            var param = CSParam as ExpandNodeConstructParam;
            if (param.ExpandType == null)
                return;
            foreach (var pro in param.ExpandType.GetProperties())
            {
                var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts == null || atts.Length == 0)
                    continue;
                
                var mm = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                var isReadOnly = mm.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly) || !pro.CanWrite;
                InitCheckWithMember(pro.Name, pro.PropertyType, isReadOnly);
            }
            foreach(var field in param.ExpandType.GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts == null || atts.Length == 0)
                    continue;

                var mm = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                var isReadOnly = mm.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly);
                InitCheckWithMember(field.Name, field.FieldType, isReadOnly);
            }
        }

        bool mCheckDelegateOperationEnable = true;
        void InitCheckWithMember(string memberName, Type memberType, bool isReadOnly)
        {
            var param = CSParam as ExpandNodeConstructParam;
            var cb = new CheckBox();
            cb.Content = memberName;
            cb.Foreground = TryFindResource("TextForeground") as Brush;
            if (param.ActiveMembers.Contains(memberName))
                cb.IsChecked = true;
            else
                cb.IsChecked = false;
            cb.Checked += (object sender, System.Windows.RoutedEventArgs e) =>
            {
                if (!mCheckDelegateOperationEnable)
                    return;

                var redoAction = new Action<object>((obj) =>
                {
                    mCheckDelegateOperationEnable = false;

                    param.ActiveMembers.Add(memberName);
                    var childNodeParam = new ExpandNodeChild.ExpandNodeChildConstructionParams()
                    {
                        CSType = param.CSType,
                        HostNodesContainer = param.HostNodesContainer,
                        ParamName = memberName,
                        ParamType = memberType,
                        IsReadOnly = isReadOnly,
                    };
                    var paramCtrl = new ExpandNodeChild(childNodeParam);
                    AddChildNode(paramCtrl, mParamPanel);
                    cb.Tag = paramCtrl;
                    cb.IsChecked = true;
                    CreateTemplateClass_Show();
                    mCheckDelegateOperationEnable = true;
                });
                redoAction.Invoke(null);
                var undoAction = new Action<object>((obj) =>
                {
                    mCheckDelegateOperationEnable = false;
                    param.ActiveMembers.Remove(memberName);
                    var node = cb.Tag as ExpandNodeChild;
                    RemoveChildNode(node);
                    cb.Tag = null;
                    cb.IsChecked = false;
                    CreateTemplateClass_Show();
                    mCheckDelegateOperationEnable = true;
                });
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null, undoAction, "Add Member");
                IsDirty = true;
            };
            cb.Unchecked += (object sender, System.Windows.RoutedEventArgs e) =>
            {
                if (!mCheckDelegateOperationEnable)
                    return;

                var node = cb.Tag as ExpandNodeChild;
                if (node == null)
                    return;
                var nodeIdx = mParamPanel.Children.IndexOf(node);

                var undoRedoDatas = new List<UndoRedoData>();
                foreach(var lPin in node.GetLinkPinInfos())
                {
                    for(int i=0; i<lPin.GetLinkInfosCount(); i++)
                    {
                        var lInfo = lPin.GetLinkInfo(i);
                        var data = new UndoRedoData();
                        data.StartObj = lInfo.m_linkFromObjectInfo;
                        data.EndObj = lInfo.m_linkToObjectInfo;
                        undoRedoDatas.Add(data);
                    }
                }
                var redoAction = new Action<object>((obj) =>
                {
                    mCheckDelegateOperationEnable = false;
                    RemoveChildNode(node);
                    cb.Tag = null;
                    cb.IsChecked = false;
                    param.ActiveMembers.Remove(memberName);
                    CreateTemplateClass_Show();
                    mCheckDelegateOperationEnable = true;
                });
                redoAction.Invoke(null);
                var undoAction = new Action<object>((obj) =>
                {
                    mCheckDelegateOperationEnable = false;
                    param.ActiveMembers.Insert(nodeIdx, memberName);
                    InsertChildNode(nodeIdx, node, mParamPanel);
                    cb.Tag = node;
                    cb.IsChecked = true;
                    foreach(var data in undoRedoDatas)
                    {
                        var linkInfo = new CodeGenerateSystem.Base.LinkInfo(ParentDrawCanvas, data.StartObj, data.EndObj);
                    }
                    CreateTemplateClass_Show();
                    mCheckDelegateOperationEnable = true;
                });
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null, undoAction, "Remove Member");
                IsDirty = true;
            };
            StackPanel_Members.Children.Add(cb);
        }

        protected override void CollectionErrorMsg()
        {
            var param = CSParam as ExpandNodeConstructParam;
            if (param.ExpandType == null)
            {
                HasError = true;
                ErrorDescription = "找不到对应类型";
            }
            if(!mCtrlTarget.HasLink)
            {
                HasError = true;
                ErrorDescription = "Target没有链接";
            }
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance_Show;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ExpandNode;
            foreach(var child in mChildNodes)
            {
                var copyedChild = child.Duplicate(param);
                var copyedChildParam = copyedChild.CSParam as ExpandNodeChild.ExpandNodeChildConstructionParams;
                foreach(var cb in copyedNode.StackPanel_Members.Children)
                {
                    var cbTemp = cb as CheckBox;
                    if(((string)cbTemp.Content) == copyedChildParam.ParamName)
                    {
                        cbTemp.Tag = copyedChild;
                    }
                }
                copyedNode.AddChildNode(copyedChild, copyedNode.mParamPanel);
            }
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance_All, copyedNode.mTemplateClassInstance_All);
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance_Show, copyedNode.mTemplateClassInstance_Show);
            return copyedNode;
        }
    }
}
