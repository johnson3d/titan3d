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
        public Common.UCopy2SwapChainNode RootNode { get; set; }
        public Dictionary<string, TtRenderGraphNode> GraphNodes { get; } = new Dictionary<string, TtRenderGraphNode>();
        public List<TtRenderGraphNode>[] NodeLayers;
        public List<TtRenderGraphLinker> Linkers { get; } = new List<TtRenderGraphLinker>();
        public TtAttachmentCache AttachmentCache { get; } = new TtAttachmentCache();
        public bool RegRenderNode(string name, TtRenderGraphNode node)
        {
            TtRenderGraphNode fNode;
            if (GraphNodes.TryGetValue(name, out fNode))
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Graphics", $"Policy() node({name}) is repeated");
                return false;
            }
            node.RenderGraph = this;
            node.Name = name;
            GraphNodes.Add(name, node);
            return true;
        }
        public TtRenderGraphNode FindNode(string name)
        {
            TtRenderGraphNode fNode;
            if (GraphNodes.TryGetValue(name, out fNode))
            {
                return fNode;
            }
            return null;
        }
        public T FindFirstNode<T>() where T : TtRenderGraphNode
        {
            foreach (var i in GraphNodes)
            {
                if (i.Value.GetType() == typeof(T))
                {
                    return (T)i.Value;
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
            foreach (var i in GraphNodes)
            {
                i.Value.InitNodePins();
                i.Value.IsUsed = false;
            }

            SetUsedNode(RootNode);

            OnBuildGraph();

            if (RootNode != null)
            {
                UpdateNodeTree(RootNode, ref hasInputError);
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
        private void UpdateNodeTree(TtRenderGraphNode node, ref bool hasInputError)
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
                    UpdateNodeTree(linker.OutPin.HostNode, ref hasInputError);
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
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RenderGraph", $"Input({pin.HostNode.Name}.{pin.Name}) linked null");
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
        public void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            foreach (var i in GraphNodes)
            {
                if (i.Value.IsUsed == false)
                    continue;
                i.Value.FrameBuild(policy);
            }
        }
        public virtual void BeginTickLogic(GamePlay.UWorld world)
        {
            FrameBuild(this as URenderPolicy);

            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    foreach (var j in i)
                    {
                        if (j.IsUsed == false)
                            continue;
                        if (j.Enable)
                            j.BeginTickLogic(world, this as URenderPolicy, true);
                    }
                }
            }
        }
        public virtual void EndTickLogic(GamePlay.UWorld world)
        {
            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    foreach (var j in i)
                    {
                        if (j.IsUsed == false)
                            continue;
                        if (j.Enable)
                            j.EndTickLogic(world, this as URenderPolicy, true);
                    }
                }
            }
        }
        private List<TtRenderGraphLinker> mTempTryReleaseLinkers = new List<TtRenderGraphLinker>();
        public virtual void TickLogic(GamePlay.UWorld world)
        {
            //FrameBuild();

            //BeginTickLogic(world);

            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    foreach (var j in i)
                    {
                        if (j.IsUsed == false)
                            continue;
                        if (j.Enable)
                        {
                            j.BeforeTickLogic((URenderPolicy)this);

                            j.TickLogic(world, (URenderPolicy)this, true);

                            j.TryReleaseBufers(mTempTryReleaseLinkers);

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
            }
            //EndTickLogic(world);

            mTempTryReleaseLinkers.Clear();
        }
        public virtual void TickSync()
        {
            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    foreach (var j in i)
                    {
                        if (j.Enable)
                            j.TickSync((URenderPolicy)this);
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
                i.Value.OnResize((URenderPolicy)this, x, y);
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
