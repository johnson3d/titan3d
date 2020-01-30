using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode.Animation
{
    public partial class AnimStateLinkControl : LinkPinControl
    {
        bool mRecursionReached = false;
        public bool RecursionReached
        {
            get => mRecursionReached;
            set => mRecursionReached = value;
        }
        List<StateTransitionPair> mTransitionPairList = new List<StateTransitionPair>();
        List<Guid> GetTransitions(Guid from, Guid to)
        {
            foreach (var tran in mTransitionPairList)
            {
                if (tran.From == from && tran.To == to)
                {
                    return tran.Transitions;
                }
            }
            return null;
        }
        public void AddTransiton(Guid from, Guid to, Guid transtionNodeID)
        {
            if (transtionNodeID == Guid.Empty)
                return;
            var trans = GetTransitions(from, to);
            if (trans == null)
            {
                StateTransitionPair pair = new StateTransitionPair();
                pair.From = from;
                pair.To = to;
                pair.Transitions.Add(transtionNodeID);
                mTransitionPairList.Add(pair);
            }
            else
                trans.Add(transtionNodeID);
        }
        public void RemoveTransiton(Guid from, Guid to, Guid transtionNodeID)
        {
            var trans = GetTransitions(from, to);
            trans?.Remove(transtionNodeID);
        }

        protected override void SaveLinks(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("StateLinks");
            att.Version = 1;
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
                var list = GetTransitions(link.m_linkFromObjectInfo.GUID, link.m_linkToObjectInfo.GUID);
                if (list == null)
                {
                    att.Write(0);
                }
                else
                {
                    att.Write((int)(list.Count));
                    foreach (var loadedTransition in list)
                    {
                        att.Write(loadedTransition);
                    }
                }
            }
            att.EndWrite();
        }
        protected override void LoadLink(XndNode xndNode)
        {
            mIsLoadingLinks = true;
            mLinkPairs.Clear();
            var att = xndNode.FindAttrib("StateLinks");
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
                case 1:
                    {
                        int count = 0;
                        att.Read(out count);
                        for (int i = 0; i < count; i++)
                        {
                            var pair = new LinkPair();
                            att.Read(out pair.LoadedLinkFrom);
                            att.Read(out pair.LoadedLinkTo);
                            mLinkPairs.Add(pair);
                            int transitionCount = 0;
                            att.Read(out transitionCount);
                            {
                                if (transitionCount != 0)
                                {
                                    var stateTran = new StateTransitionPair();
                                    stateTran.From = pair.LoadedLinkFrom;
                                    stateTran.To = pair.LoadedLinkTo;
                                    for (int j = 0; j < transitionCount; j++)
                                    {
                                        Guid transition;
                                        att.Read(out transition);
                                        stateTran.Transitions.Add(transition);
                                    }
                                    mTransitionPairList.Add(stateTran);
                                }
                            }
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
                foreach (var one in mTransitionPairList)
                {
                    var startObj = container.GetLinkObjectWithGUID(ref one.From);
                    var endObj = container.GetLinkObjectWithGUID(ref one.To);
                    if (startObj != null && endObj != null)
                    {
                        var linkInfo = new AnimStateLinkInfo(MainDrawCanvas, startObj, endObj);
                        foreach (var transition in one.Transitions)
                        {
                            Guid id = transition;
                            var curve = linkInfo.LinkPath as CodeGenerateSystem.Base.ArrowLine;
                            curve.AddTransitionControl(container.GetNodeWithGUID(ref id));
                        }
                    }
                }
            }

        }
    }
}
