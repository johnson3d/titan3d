using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeGenerateSystem.Base;

namespace CodeGenerateSystem.Base
{
    public class ClickableLinkInfo : LinkInfo
    {
        public ClickableLinkInfo(Canvas drawCanvas, LinkPinControl startObj, LinkPinControl endObj) : base(drawCanvas, startObj, endObj)
        {
            if (m_LinkPath != null)
            {
                m_LinkPath.MouseEnter += LinkPath_MouseEnter;
                m_LinkPath.MouseDown += LinkPath_MouseDown;
                m_LinkPath.MouseLeave += LinkPath_MouseLeave;
                m_LinkPath.StrokeThickness = 2;
                m_LinkPath.Stroke = Brushes.GhostWhite;
                Canvas.SetZIndex(m_LinkPath, 5);
            }
            timer.Elapsed += Timer_Elapsed;
        }
        private void LinkPath_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo == this)
                return;
            m_LinkPath.StrokeThickness = 4;
            Canvas.SetZIndex(m_LinkPath, 7);
            m_LinkPath.Stroke = Brushes.Yellow;
        }
        public void SelectLinkInfo()
        {
            if (m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo != this && m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo != null)
            {
                m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.StrokeThickness = 2;
                m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.Stroke = Brushes.GhostWhite;
                Canvas.SetZIndex(m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath, 5);
            }
            m_linkFromObjectInfo.HostNodesContainer.SetSelectedLinkInfo(this);
            m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.StrokeThickness = 3;
            m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath.Stroke = Brushes.LightSkyBlue;
            Canvas.SetZIndex(m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo.LinkPath, 6);
        }
        System.Timers.Timer timer = new System.Timers.Timer(200);
        bool hasSingleClick = false;
        bool singleClickTimeOut = false;
        private void LinkPath_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (hasSingleClick && !singleClickTimeOut)
            {
                timer.Stop();
                hasSingleClick = false;
                m_linkFromObjectInfo.HostNodesContainer.LinkInfoDoubleClick(this);
                return;
            }
            hasSingleClick = true;
            singleClickTimeOut = false;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            singleClickTimeOut = true;
            timer.Stop();
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
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
                hasSingleClick = false;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);

        }

        private void LinkPath_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (m_linkFromObjectInfo.HostNodesContainer.SelectedLinkInfo == this)
                return;
            m_LinkPath.StrokeThickness = 2;
            m_LinkPath.Stroke = Brushes.GhostWhite;
            Canvas.SetZIndex(m_LinkPath, 5);
        }


    }
}
