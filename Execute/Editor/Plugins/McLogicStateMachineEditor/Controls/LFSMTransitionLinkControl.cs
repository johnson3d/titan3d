using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace McLogicStateMachineEditor.Controls
{
    public partial class LFSMTransitionLinkControl : LinkPinControl
    {
        protected override void SaveLinks(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("TransitionLinks");
            att.Version = 0;
            att.BeginWrite();
            att.Write((int)(LinkInfos.Count));
            foreach (var link in LinkInfos)
            {
                // 创建新的GUID则输出以前的GUID
                if (newGuid)
                {
                    att.Write(link.m_linkFromObjectInfo.m_copyedGuid);
                    att.Write(link.m_linkToObjectInfo.m_copyedGuid);
                }
                else
                {
                    att.Write(link.m_linkFromObjectInfo.GUID);
                    att.Write(link.m_linkToObjectInfo.GUID);
                }
            }
            att.EndWrite();
        }
        protected override void LoadLink(XndNode xndNode)
        {
            mIsLoadingLinks = true;
            mLinkPairs.Clear();
            var att = xndNode.FindAttrib("TransitionLinks");
            att.BeginRead();
            switch (att.Version)
            {
                case 0:
                    {
                        int count = 0;
                        att.Read(out count);
                        for (int i = 0; i < count; i++)
                        {
                            var pair = new LinkPair();
                            att.Read(out pair.LoadedLinkFrom);
                            att.Read(out pair.LoadedLinkTo);
                            mLinkPairs.Add(pair);
                        }
                    }
                    break;
            }
            att.EndRead();

            mIsLoadingLinks = false;
        }

        public override void ConstructLinkInfo(NodesContainer container, List<Guid> includeIdList = null)
        {
            for (int i = 0; i < mLinkPairs.Count; ++i)
            {
                var from = mLinkPairs[i].LoadedLinkFrom;
                var to = mLinkPairs[i].LoadedLinkTo;
                if (from == Guid.Empty || to == Guid.Empty)
                    return;
                if (includeIdList != null && !includeIdList.Contains(from))
                    return;
                if (includeIdList != null && !includeIdList.Contains(to))
                    return;
                var startObj = container.GetLinkObjectWithGUID(ref from);
                var endObj = container.GetLinkObjectWithGUID(ref to);
                if (startObj != null && endObj != null)
                {
                    var linkInfo = new ClickableLinkInfo(MainDrawCanvas, startObj, endObj);
                }
            }
        }
    }
}
