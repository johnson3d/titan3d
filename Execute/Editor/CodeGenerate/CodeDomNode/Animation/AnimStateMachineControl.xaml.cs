using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode.Animation
{
    public partial class AnimStateMachineControl
    {
        public AnimStateMachineControl()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }

        public void EndPreviewLine(LinkPinControl objInfo, NodesContainer nodesContainer)
        {
            if (nodesContainer.ContainerDrawCanvas == null)
                return;

            if (objInfo == null)
            {
                if (nodesContainer.StartLinkObj != null && nodesContainer.PreviewLinkCurve.Visibility == System.Windows.Visibility.Visible)
                {
                    nodesContainer.IsOpenContextMenu = true;
                }
            }
            else if (nodesContainer.StartLinkObj != null && objInfo != null)
            {
                nodesContainer.PreviewLinkCurve.Visibility = System.Windows.Visibility.Hidden;
                nodesContainer.enPreviewBezierType = CodeGenerateSystem.Base.enBezierType.None;

                if (nodesContainer.StartLinkObj.LinkOpType == objInfo.LinkOpType && objInfo.LinkOpType != enLinkOpType.Both) // 只有start和end能连 或者其中之一 和 Both
                    return;
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(nodesContainer.StartLinkObj, objInfo))
                {
                    objInfo.MouseAssistVisible = System.Windows.Visibility.Hidden;

                    var container = new NodesContainer.LinkInfoContainer();
                    if (nodesContainer.StartLinkObj.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = nodesContainer.StartLinkObj;
                        container.End = objInfo;
                    }
                    else if (objInfo.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = objInfo;
                        container.End = nodesContainer.StartLinkObj;
                    }
                    else
                    {
                        container.Start = nodesContainer.StartLinkObj;
                        container.End = objInfo;
                    }

                    if (nodesContainer.StartLinkObj.LinkCurveType == enLinkCurveType.Line)
                    {
                        NodesContainer.TransitionStaeBaseNodeForUndoRedo transCtrl = new NodesContainer.TransitionStaeBaseNodeForUndoRedo();
                        var redoAction = new Action<object>((obj) =>
                        {
                            var linkInfo = new AnimStateLinkInfo(nodesContainer.ContainerDrawCanvas, container.Start, container.End);
                            transCtrl.TransitionStateNode = linkInfo.AddTransition();
                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(nodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                                    (obj) =>
                                                    {
                                                        for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                        {
                                                            var info = container.End.GetLinkInfo(i);
                                                            if (info.m_linkFromObjectInfo == container.Start)
                                                            {
                                                                var transitionInfo = info as AnimStateLinkInfo;
                                                                transitionInfo.RemoveTransition(transCtrl.TransitionStateNode);
                                                                break;
                                                            }
                                                        }
                                                    }, "Create Link");
                    }
                    else
                    {
                        var redoAction = new Action<Object>((obj) =>
                        {
                            var linkInfo = new CodeGenerateSystem.Base.LinkInfo(nodesContainer.ContainerDrawCanvas, container.Start, container.End);

                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(nodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                                    (obj) =>
                                                    {
                                                        for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                        {
                                                            var info = container.End.GetLinkInfo(i);
                                                            if (info.m_linkFromObjectInfo == container.Start)
                                                            {
                                                                info.Clear();
                                                                break;
                                                            }
                                                        }
                                                    }, "Create Link");
                    }

                    IsDirty = true;
                }
                else
                    nodesContainer.OnLinkFailure(nodesContainer.StartLinkObj, objInfo);
            }
        }
    }
}
