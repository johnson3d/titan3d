using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeGenerateSystem.Base;

namespace CodeDomNode.Animation
{
    public class AnimStateLinkInfo : LinkInfo
    {
        public AnimStateLinkInfo(Canvas drawCanvas, LinkPinControl startObj, LinkPinControl endObj) : base(drawCanvas, startObj, endObj)
        {

        }
        public override BaseNodeControl AddTransition()
        {
            if (m_linkFromObjectInfo.LinkOpType == enLinkOpType.Start)
                return null;
            var csParam = new AnimStateTransitionControlConstructionParams();
            csParam.CSType = m_linkFromObjectInfo.HostNodesContainer.CSType;
            csParam.NodeName = "StateTransition";
            csParam.HostNodesContainer = m_linkFromObjectInfo.HostNodesContainer;
            csParam.DrawCanvas = m_drawCanvas;
            var transCtrl = m_linkFromObjectInfo.HostNodesContainer.AddNodeControl(typeof(AnimStateTransitionControl), csParam, 0, 0);
            var lineCurve = m_LinkPath as ArrowLine;
            lineCurve.AddTransitionControl(transCtrl);
            var from = m_linkFromObjectInfo as AnimStateLinkControl;
            from?.AddTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transCtrl.Id);
            var to = m_linkToObjectInfo as AnimStateLinkControl;
            to?.AddTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transCtrl.Id);
            return transCtrl;
        }
        void AddTransitionToObject(BaseNodeControl transCtrl)
        {
            if (m_linkFromObjectInfo.LinkOpType == enLinkOpType.Start)
                return ;
            var lineCurve = m_LinkPath as ArrowLine;
            //lineCurve.AddTransitionControl(transCtrl);
            var from = m_linkFromObjectInfo as AnimStateLinkControl;
            from?.AddTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transCtrl.Id);
            var to = m_linkToObjectInfo as AnimStateLinkControl;
            to?.AddTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transCtrl.Id);
            m_linkFromObjectInfo.HostNodesContainer.GetDrawCanvas().Children.Add(transCtrl);
        }
        public override void RemoveTransition(BaseNodeControl transitionNode)
        {
            if (transitionNode == null)
                return;
            var lineCurve = m_LinkPath as ArrowLine;
            lineCurve.RemoveTransitionControl(transitionNode);
            var from = m_linkFromObjectInfo as AnimStateLinkControl;
            from?.RemoveTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transitionNode.Id);
            var to = m_linkToObjectInfo as AnimStateLinkControl;
            to?.RemoveTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transitionNode.Id);
            m_linkFromObjectInfo.HostNodesContainer.DeleteNode(transitionNode);

            if (lineCurve.TransitionControlList.Count == 0)
            {
                Clear();
            }
        }
        void RemoveTransitionFromObject(BaseNodeControl transitionNode)
        {
            if (transitionNode == null)
                return;
            var from = m_linkFromObjectInfo as AnimStateLinkControl;
            from?.RemoveTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transitionNode.Id);
            var to = m_linkToObjectInfo as AnimStateLinkControl;
            to?.RemoveTransiton(m_linkFromObjectInfo.GUID, m_linkToObjectInfo.GUID, transitionNode.Id);
            m_linkFromObjectInfo.HostNodesContainer.DeleteNode(transitionNode);
        }
            public void RemoveAllFromContainer()
        {
            var lineCurve = m_LinkPath as ArrowLine;
            foreach (var transition in lineCurve.TransitionControlList)
            {
                RemoveTransitionFromObject(transition);
                //m_linkFromObjectInfo.HostNodesContainer.DeleteNode(transition);
            }
        }
        public void AddAllToContainer()
        {
            var lineCurve = m_LinkPath as ArrowLine;
            foreach (var transition in lineCurve.TransitionControlList)
            {
                AddTransitionToObject(transition);
                //m_linkFromObjectInfo.HostNodesContainer.DeleteNode(transition);
            }
        }

    }
}
