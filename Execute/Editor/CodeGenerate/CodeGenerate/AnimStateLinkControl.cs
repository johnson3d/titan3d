using CodeGenerateSystem.Base;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerateSystem.Controls
{
    public partial class AnimStateLinkControl
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
                            att.Read(out mLoadedLinkFrom);
                            att.Read(out mLoadedLinkTo);
                        }
                    }
                    break;
                case 1:
                    {
                        int count = 0;
                        att.Read(out count);
                        for (int i = 0; i < count; i++)
                        {
                            att.Read(out mLoadedLinkFrom);
                            att.Read(out mLoadedLinkTo);
                            int transitionCount = 0;
                            att.Read(out transitionCount);
                            {
                                if (transitionCount != 0)
                                {
                                    var stateTran = new StateTransitionPair();
                                    stateTran.From = mLoadedLinkFrom;
                                    stateTran.To = mLoadedLinkTo;
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
            if (mLoadedLinkFrom == Guid.Empty || mLoadedLinkTo == Guid.Empty)
                return;
            if (includeIdList != null && !includeIdList.Contains(mLoadedLinkFrom))
                return;
            if (includeIdList != null && !includeIdList.Contains(mLoadedLinkTo))
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
                        var curve = linkInfo.LinkPath as CodeGenerateSystem.Base.AnimStateTransitionCurve;
                        curve.AddTransitionControl(container.GetNodeWithGUID(ref id));
                    }
                }
            }

        }
    }
}
