using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeGenerateSystem.Base;

namespace CodeDomNode.Particle
{
    public class ModuleLinkInfo : LinkInfo
    {
        public ModuleLinkInfo(Canvas drawCanvas, LinkPinControl startObj, LinkPinControl endObj) : base(drawCanvas, startObj, endObj)
        {
            m_LinkPath.MouseEnter += LinkPath_MouseEnter;
            m_LinkPath.MouseDown += LinkPath_MouseDown;
            m_LinkPath.MouseLeave += LinkPath_MouseLeave;

        }

        private void LinkPath_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo == this)
                return;
            m_LinkPath.StrokeThickness = 4;
            Canvas.SetZIndex(m_LinkPath, 7);
            m_LinkPath.Stroke = Brushes.Yellow;
        }

        private void LinkPath_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo != this && m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo != null)
            {
                m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.StrokeThickness = 2;
                m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.Stroke = Brushes.GhostWhite;
                Canvas.SetZIndex(m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath, 5);
            }
            m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo = this;
            m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.StrokeThickness = 3;
            m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.Stroke = Brushes.LightSkyBlue;
            Canvas.SetZIndex(m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath, 6);
            m_LinkPath.Focus();
            e.Handled = true;

        }

        private void LinkPath_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo == this)
                return;
            m_LinkPath.StrokeThickness = 2;
            m_LinkPath.Stroke = Brushes.GhostWhite;
            Canvas.SetZIndex(m_LinkPath, 5);
        }

        public static bool CheckLinkObject(LinkPinControl startObj, LinkPinControl endObj)
        {
            var basenode = endObj.HostNodeControl as StructNodeControl;
            if (basenode != null && basenode.CtrlValueLinkHandleDown.LinkInfos.Count > 0)
            {
                var infos = basenode.CtrlValueLinkHandleDown.LinkInfos;
                for (int i = 0; i < infos.Count; i++)
                {
                    if (infos[i].m_linkToObjectInfo.HostNodeControl == startObj.HostNodeControl)
                        return false;

                    if (CheckLinkObject(startObj, infos[i].m_linkToObjectInfo))
                        return false;
                }
            }

            return true;
        }

        public static bool CanLinkWith2(LinkPinControl startObj, LinkPinControl endObj)
        {
            if (startObj == endObj)
                return false;
            //if (endObj.LinkInfos.Count > 0)
            //    return false;
            if (startObj.HostNodeControl == endObj.HostNodeControl)
            {
                return false;
            }
            else
            {
                CheckLinkObject(startObj, endObj);
            }
            

            return true;
        }
    }
}
