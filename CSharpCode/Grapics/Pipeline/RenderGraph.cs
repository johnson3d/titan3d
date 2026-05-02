using EngineNS.Graphics.Pipeline.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtRenderGraph : IDisposable
    {
        public virtual void Dispose()
        {
            RootNode = null;
            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    i.Clear();
                }
                NodeLayers = null;
            }

            AttachmentCache.ResetCache(true);
            foreach (var i in GraphNodes)
            {
                i.Value.Dispose();
            }
            GraphNodes.Clear();
        }
        public Common.TtEndingNode RootNode 
        { 
            get;
            set; 
        }
        public Dictionary<Guid, TtRenderGraphNode> GraphNodes { get; } = new Dictionary<Guid, TtRenderGraphNode>();
        public List<TtRenderGraphNode>[] NodeLayers;
        public List<TtRenderGraphLinker> Linkers { get; } = new List<TtRenderGraphLinker>();
        public TtAttachmentCache AttachmentCache { get; } = new TtAttachmentCache();
        public bool RegRenderNode(in Guid id, TtRenderGraphNode node)
        {
            TtRenderGraphNode fNode;
            if (GraphNodes.TryGetValue(id, out fNode))
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"Policy() node({node}) is repeated");
                return false;
            }
            node.RenderGraph = this;
            GraphNodes.Add(id, node);
            return true;
        }
        public void RegRenderNode2(string name, TtRenderGraphNode node)
        {
            node.Name = name;
            GraphNodes.Add(Guid.NewGuid(), node);
        }
        public TtRenderGraphNode FindNode(in Guid id)
        {
            TtRenderGraphNode fNode;
            if (GraphNodes.TryGetValue(id, out fNode))
            {
                return fNode;
            }
            return null;
        }
        public T FindNode<T>(string name = null, bool bInherit = false) where T : TtRenderGraphNode
        {
            foreach(var node in GraphNodes.Values)
            {
                if (bInherit)
                {
                    if (node.GetType() != typeof(T) && !node.GetType().IsSubclassOf(typeof(T)))
                        continue;
                }
                else
                {
                    if (node.GetType() != typeof(T))
                        continue;
                }
                if (node.Name == name || name == null)
                    return node as T;
            }
            return null;
        }
        public TtRenderGraphNode FindNodeIgnore(string name, Type ignoreType)
        {
            foreach (var node in GraphNodes.Values)
            {
                if (node.GetType() == ignoreType)
                    continue;
                if (node.Name == name || name == null)
                    return node;
            }
            return null;
        }
        public T FindFirstNode<T>(bool bInherit = false) where T : TtRenderGraphNode
        {
            foreach (var i in GraphNodes)
            {
                if (i.Value.GetType() == typeof(T) || (bInherit && i.Value.GetType().IsSubclassOf(typeof(T))))
                {
                    return i.Value as T;
                }
            }
            return null;
        }
        public bool AddLinker(TtRenderGraphPin outPin, TtRenderGraphPin inPin)
        {
            if (outPin.PinType == TtRenderGraphPin.EPinType.Input)
            {
                return false;
            }
            if (inPin.PinType == TtRenderGraphPin.EPinType.Output)
            {
                return false;
            }
            var linker = new TtRenderGraphLinker();
            linker.OutPin = outPin;
            linker.InPin = inPin;

            foreach (var i in Linkers)
            {
                if (i.IsSame(linker))
                    return true;
            }
            Linkers.Add(linker);
            inPin.HostNode.OnLinkIn(linker);
            outPin.HostNode.OnLinkOut(linker);
            return true;
        }
        public TtRenderGraphLinker FindInLinker(TtRenderGraphPin inPin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == inPin)
                    return i;
            }
            return null;
        }
        public List<TtRenderGraphLinker> FindOutLinkers(TtRenderGraphPin outPin)
        {
            var result = new List<TtRenderGraphLinker>();
            foreach (var i in Linkers)
            {
                if (i.OutPin == outPin)
                {
                    result.Add(i);
                }
            }
            return result;
        }
        public void FindOutLinkers(TtRenderGraphPin outPin, List<TtRenderGraphLinker> result)
        {
            result.Clear();
            foreach (var i in Linkers)
            {
                if (i.OutPin == outPin)
                {
                    result.Add(i);
                }
            }
        }
        private void SetUsedNode(TtRenderGraphNode node)
        {
            if (node.IsUsed)
                return;
            node.IsUsed = true;
            for (int i = 0; i < node.NumOfInput; i++)
            {
                var lnk = this.FindInLinker(node.GetInput(i));
                if (lnk == null)
                    continue;
                SetUsedNode(lnk.OutPin.HostNode);
            }
        }
        public void BuildGraph(ref bool hasInputError)
        {
            if (RootNode == null)
                return;

            List<TtEndingNode> endingNodes = new List<TtEndingNode>();
            foreach (var i in GraphNodes)
            {
                i.Value.InitNodePins();
                i.Value.IsUsed = false;

                if (i.Value is TtEndingNode enode && enode.IsMainRoot == false)
                {
                    endingNodes.Add(enode);
                }
            }

            SetUsedNode(RootNode);
            foreach (var i in endingNodes)
            {
                SetUsedNode(i);
            }

            OnBuildGraph();

            if (RootNode != null)
            {
                UpdateNodeTree(RootNode, ref hasInputError);
                foreach (var i in endingNodes)
                {
                    UpdateNodeTree(i, ref hasInputError);
                }
                int maxDistance = 0;
                foreach (var i in GraphNodes)
                {
                    var dist = GetMaxLeafDistance(i.Value);
                    if (dist >= maxDistance)
                    {
                        maxDistance = dist;
                    }
                }
                maxDistance += 1;
                NodeLayers = new List<TtRenderGraphNode>[maxDistance];
                for (int i = 0; i < maxDistance; i++)
                {
                    NodeLayers[i] = new List<TtRenderGraphNode>();
                }
                foreach (var i in GraphNodes)
                {
                    i.Value.TempRootDistance = 0;
                    NodeLayers[i.Value.MaxLeafDistance].Add(i.Value);
                    var debugger = i.Value as Common.TtDebuggerNode;
                    if (debugger != null)
                    {
                        if (debugger.SrcPinIn.FindInLinker() != null)
                        {
                            debugger.IsUsed = true;
                            UpdateNodeTree(debugger, ref hasInputError, false);
                        }
                    }
                }
            }

            foreach (var i in GraphNodes)
            {
                //for (int j = 0; j < i.Value.NumOfInput; j++)
                //{
                //    var pin = i.Value.GetInput(j);
                //    var linkers = FindOutLinkers(pin);
                //    foreach (var k in linkers)
                //    {
                //        //pin.Attachement.Format =
                //    }
                //}
            }
        }
        private void UpdateNodeTree(TtRenderGraphNode node, ref bool hasInputError, bool bRecursive = true)
        {
            for (int i = 0; i < node.NumOfInput; i++)
            {
                var pin = node.GetInput(i);
                var linker = this.FindInLinker(pin);
                if (linker != null)
                {
                    //if (linker.OutPin.PinType != TtRenderGraphPin.EPinType.Output)
                    //{                        
                    //}
                    if (bRecursive)
                    {
                        UpdateNodeTree(linker.OutPin.HostNode, ref hasInputError);
                    }
                    linker.InPin.Attachement.AttachmentName = linker.OutPin.Attachement.AttachmentName;
                    linker.InPin.Attachement.Format = linker.OutPin.Attachement.Format;
                    linker.InPin.Attachement.Width = linker.OutPin.Attachement.Width;
                    linker.InPin.Attachement.Height = linker.OutPin.Attachement.Height;
                    linker.InPin.IsAutoResize = linker.OutPin.IsAutoResize;
                    linker.InPin.LifeMode = linker.OutPin.LifeMode;
                }
                else
                {
                    if (pin.IsAllowInputNull == false)
                    {
                        hasInputError = true;
                        Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"Input({pin.HostNode.Name}.{pin.Name}) linked null");
                    }
                }
            }
        }
        public int GetMaxLeafDistance(TtRenderGraphNode root)
        {
            root.mMaxLeafDistance = 0;
            foreach (var i in GraphNodes)
            {
                i.Value.TempRootDistance = 0;
            }
            root.UpdateNodeDistance(this, root);
            return root.MaxLeafDistance;
        }
        protected virtual void OnBuildGraph()
        {

        }
        public void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            foreach(var i in NodeLayers)
            {
                foreach (var j in i)
                {
                    if (j.IsUsed == false || j.Enable == false)
                        continue;
                    j.FrameBuild(policy);
                }
            }
            //foreach (var i in GraphNodes)
            //{
            //    if (i.Value.IsUsed == false)
            //        continue;
            //    i.Value.FrameBuild(policy);
            //}
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeBeginTick;
        private static Profiler.TimeScope ScopeBeginTick
        {
            get
            {
                if (mScopeBeginTick == null)
                    mScopeBeginTick = new Profiler.TimeScope(typeof(TtRenderGraph), nameof(BeginTick));
                return mScopeBeginTick;
            }
        }
        public virtual void BeginTick(GamePlay.TtWorld world)
        {
            using (new Profiler.TimeScopeHelper(ScopeBeginTick))
            {
                FrameBuild(this as TtRenderPolicy);

                if (NodeLayers != null)
                {
                    foreach (var i in NodeLayers)
                    {
                        foreach (var j in i)
                        {
                            if (j.IsUsed == false || j.Enable == false)
                                continue;
                            j.BeginTick(world, this as TtRenderPolicy, true);
                        }
                    }
                }
            }   
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeEndTick;
        private static Profiler.TimeScope ScopeEndTick
        {
            get
            {
                if (mScopeEndTick == null)
                    mScopeEndTick = new Profiler.TimeScope(typeof(TtRenderGraph), nameof(EndTick));
                return mScopeEndTick;
            }
        }
        public virtual void EndTick(GamePlay.TtWorld world)
        {
            using (new Profiler.TimeScopeHelper(ScopeEndTick))
            {
                if (NodeLayers != null)
                {
                    foreach (var i in NodeLayers)
                    {
                        foreach (var j in i)
                        {
                            if (j.IsUsed == false || j.Enable == false)
                                continue;
                            j.EndTick(world, this as TtRenderPolicy, true);
                        }
                    }
                }
            }   
        }
        private List<TtRenderGraphLinker> mTempTryReleaseLinkers = new List<TtRenderGraphLinker>();
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtRenderGraph), nameof(Tick));
                return mScopeTick;
            }
        }
        public virtual unsafe void Tick(GamePlay.TtWorld world, Action<TtRenderGraphNode, TtRenderGraphPin, TtAttachBuffer> onRemove)
        {
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new Profiler.TimeScopeHelper(ScopeTick))
            using (new NxRHI.TtCmdListScope(cmdlist, "RenderGraphFrame"))
            {
                if (NodeLayers != null)
                {
                    foreach (var i in NodeLayers)
                    {
                        foreach (var j in i)
                        {
                            if (j.IsUsed == false || j.Enable == false)
                                continue;

                            var scope = j.RDGTickScope;
                            using (new Profiler.TimeScopeHelper(scope))
                            {
                                j.BeforeTick((TtRenderPolicy)this);

                                j.Tick(world, (TtRenderPolicy)this, cmdlist, true);

                                j.TryReleaseBufers(mTempTryReleaseLinkers, onRemove);
                            }
                            //System.Diagnostics.Debug.Assert(j.RDGTickScope.mCoreObject.mParent.CppPointer == ScopeTick.mCoreObject.CppPointer);
                            //int NunOfRefZero = 0; 
                            //foreach (var ca in this.AttachmentCache.CachedAttachments)
                            //{
                            //    if(ca.Value.RefCount == 0 && ca.Value.LifeMode == UAttachBuffer.ELifeMode.Transient)
                            //    {
                            //        NunOfRefZero++;
                            //    }
                            //}
                            //if(NunOfRefZero!=0)
                            //{
                            //    int xxx = 0;
                            //}
                        }
                    }
                }
                //EndTick(world);

                mTempTryReleaseLinkers.Clear();
            }
            ((TtRenderPolicy)this).CommitCommandList(cmdlist, "RenderGraphFrame");
        }
        public virtual void TickSync()
        {
            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    foreach (var j in i)
                    {
                        if (j.IsUsed == false || j.Enable == false)
                            continue;
                        j.TickSync((TtRenderPolicy)this);
                    }
                }
            }
            AttachmentCache.ResetCache(true);
        }
        public virtual void OnResize(float x, float y)
        {
            foreach (var i in GraphNodes)
            {
                for (int j = 0; j < i.Value.NumOfInput; j++)
                {
                    var pin = i.Value.GetInput(j);
                    if (pin.IsAutoResize)
                    {
                        pin.Attachement.Width = (uint)x;
                        pin.Attachement.Height = (uint)y;
                    }
                }
                for (int j = 0; j < i.Value.NumOfOutput; j++)
                {
                    var pin = i.Value.GetOutput(j);
                    if (pin.IsAutoResize)
                    {
                        pin.Attachement.Width = (uint)x;
                        pin.Attachement.Height = (uint)y;
                    }
                }
                i.Value.OnResize((TtRenderPolicy)this, x, y);
            }
            AttachmentCache.ResetCache(true);
        }
        public virtual void BuildCache()
        {
            foreach (var i in GraphNodes)
            {
                for (int j = 0; j < i.Value.NumOfOutput; j++)
                {
                    var buffer = i.Value.GetOutput(j);

                    //AttachmentCache.GetAttachement(FHashText.Create($"{i.Key}_{buffer.Name}"), buffer.Attachement);
                }
            }
        }
    }
}
