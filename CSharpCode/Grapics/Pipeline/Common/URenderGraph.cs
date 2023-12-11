using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class URenderGraphPin
    {
        public UAttachBuffer.ELifeMode LifeMode { get; set; } = UAttachBuffer.ELifeMode.Transient;
        public enum EPinType
        {
            Input,
            Output,
            InputOutput,
        }
        public string Name { get; private set; }
        public EPinType PinType { get; private set; }
        public bool IsAutoResize { get; set; } = true;
        public bool IsAllowInputNull { get; set; } = false;
        public URenderGraphNode HostNode { get; set; }

        public UAttachmentDesc Attachement = new UAttachmentDesc();
        public UAttachBuffer ImportedBuffer = null;
        public URenderGraphLinker FindInLinker()
        {
            return HostNode.RenderGraph.FindInLinker(this);
        }
        public static URenderGraphPin CreateInput(string name)
        {
            var result = new URenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.Input;
            result.IsAutoResize = false;            
            result.Attachement.Format = EPixelFormat.PXF_UNKNOWN;
            return result;
        }
        public static URenderGraphPin CreateOutput(string name, bool isAutoSize, EPixelFormat defaultFormat)
        {
            var result = new URenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.Output;
            result.IsAutoResize = isAutoSize;
            result.Attachement.Format = defaultFormat;
            return result;
        }
        public static URenderGraphPin CreateInputOutput(string name)
        {
            var result = new URenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.InputOutput;
            result.IsAutoResize = false;
            result.Attachement.Format = EPixelFormat.PXF_UNKNOWN;
            return result;
        }
        public static URenderGraphPin CreateInputOutput(string name, bool isAutoSize, EPixelFormat defaultFormat)
        {
            var result = new URenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.InputOutput;
            result.IsAutoResize = isAutoSize;
            result.Attachement.Format = defaultFormat;
            return result;
        }
    }
    public class URenderGraphLinker
    {
        public URenderGraphPin OutPin;
        public URenderGraphPin InPin;
        public bool IsSame(URenderGraphLinker rh)
        {
            if (OutPin != rh.OutPin ||
                InPin != rh.InPin)
            {
                return false;
            }
            return true;
        }
    }
    public class URenderGraphNode : IO.BaseSerializer, IDisposable
    {
        ~URenderGraphNode()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            CoreSDK.DisposeObject(ref BasePass);
        }
        internal int mMaxLeafDistance = 0;
        public int MaxLeafDistance
        {
            get => mMaxLeafDistance;
        }
        public int TempRootDistance = 0;
        public bool IsUsed { get; set; } = true;
        public bool Enable { get; set; } = true;
        private string mName;
        public virtual string Name 
        {
            get => mName;
            set
            {
                mName = value;
                foreach (var i in OutputGraphPins)
                {
                    i.Attachement.AttachmentName = FHashText.Create($"{Name}->{i.Name}");
                }
            }
        }
        public UDrawBuffers BasePass = new UDrawBuffers();
        public URenderGraph RenderGraph { get; internal set; }
        protected List<URenderGraphPin> InputGraphPins { get; } = new List<URenderGraphPin>();
        protected List<URenderGraphPin> OutputGraphPins { get; } = new List<URenderGraphPin>();
        public virtual int NumOfInput
        {
            get
            {
                return InputGraphPins.Count;
            }
        }
        public virtual int NumOfOutput
        {
            get
            {
                return OutputGraphPins.Count;
            }
        }
        public bool AddInput(URenderGraphPin pin, NxRHI.EBufferType needTypes)
        {
            if (pin.PinType != URenderGraphPin.EPinType.Input)
                return false;
            foreach (var i in InputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            pin.HostNode = this;
            pin.Attachement.BufferViewTypes = needTypes;
            InputGraphPins.Add(pin);
            return true;
        }
        public bool AddOutput(URenderGraphPin pin, NxRHI.EBufferType provideTypes)
        {
            if (pin.PinType != URenderGraphPin.EPinType.Output)
                return false;
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            pin.Attachement.AttachmentName = FHashText.Create($"{Name}->{pin.Name}");
            pin.HostNode = this;
            pin.Attachement.BufferViewTypes = provideTypes;
            OutputGraphPins.Add(pin);
            return true;
        }
        public bool AddInputOutput(URenderGraphPin pin, NxRHI.EBufferType needTypes)
        {
            if (pin.PinType != URenderGraphPin.EPinType.InputOutput)
                return false;
            pin.HostNode = this;
            pin.Attachement.BufferViewTypes = needTypes;
            foreach (var i in InputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            InputGraphPins.Add(pin);
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            OutputGraphPins.Add(pin);
            return true;
        }
        public virtual URenderGraphPin GetInput(int index)
        {
            return InputGraphPins[index];
        }
        public virtual URenderGraphPin GetOutput(int index)
        {
            return OutputGraphPins[index];
        }
        public URenderGraphPin FindInput(string name)
        {
            foreach (var i in InputGraphPins)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public URenderGraphPin FindOutput(string name)
        {
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public UAttachBuffer ImportAttachment(Common.URenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.ImportAttachment(pin);
        }
        public UAttachBuffer FindAttachBuffer(URenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
        }
        public UAttachBuffer GetAttachBuffer(URenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.GetAttachement(pin.Attachement.AttachmentName, pin.Attachement);
        }
        public bool MoveAttachment(Common.URenderGraphPin pinFrom, Common.URenderGraphPin pinTo)
        {
            return RenderGraph.AttachmentCache.MoveAttachement(pinFrom.Attachement.AttachmentName, pinTo.Attachement.AttachmentName);
            //UAttachBuffer attachment;
            //if (RenderGraph.AttachmentCache.CachedAttachments.TryGetValue(pinFrom.Attachement.AttachmentName, out attachment))
            //{
            //    RenderGraph.AttachmentCache.CachedAttachments.Remove(pinFrom.Attachement.AttachmentName);
            //    RenderGraph.AttachmentCache.CachedAttachments.Add(pinTo.Attachement.AttachmentName, attachment);
            //    return true;
            //}
            //return false;
        }

        public virtual async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
        }
        public virtual void InitNodePins()
        {
            
        }
        public virtual void FrameBuild(URenderPolicy policy)
        {
        }
        public virtual void OnLinkIn(URenderGraphLinker linker)
        {
            if ((linker.InPin.Attachement.BufferViewTypes & linker.OutPin.Attachement.BufferViewTypes) != linker.OutPin.Attachement.BufferViewTypes)
            {
                return;
            }
            //linker.InPin.Attachement.Format = linker.OutPin.Attachement.Format;
        }
        public virtual void OnLinkOut(URenderGraphLinker linker)
        {

        }
        public virtual void OnResize(URenderPolicy policy, float x, float y)
        {

        }
        public virtual void BeginTickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {

        }
        public virtual void EndTickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {

        }
        public virtual void BeforeTickLogic(URenderPolicy policy)
        {
            
        }
        public virtual void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {

        }
        public virtual void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }

        public void TryReleaseBufers(List<URenderGraphLinker> linkers)
        {
            var j = this;
            for (int k = 0; k < j.NumOfOutput; k++)
            {
                var t = j.GetOutput(k);
                RenderGraph.FindOutLinkers(t, linkers);
                if (linkers.Count > 0)
                {
                    var buffer = RenderGraph.AttachmentCache.FindAttachement(t.Attachement.AttachmentName);
                    if (buffer != null && buffer.LifeMode == UAttachBuffer.ELifeMode.Transient)
                    {
                        buffer.AddRef(linkers.Count);
                    }
                }
                else
                {
                    var buffer = RenderGraph.AttachmentCache.FindAttachement(t.Attachement.AttachmentName);
                    if (buffer != null && buffer.LifeMode == UAttachBuffer.ELifeMode.Transient)
                    {
                        if (buffer.RefCount == 0)
                        {
                            RenderGraph.AttachmentCache.RemoveAttachement(t.Attachement.AttachmentName);
                        }
                    }
                }
            }

            for (int k = 0; k < j.NumOfInput; k++)
            {
                var t = j.GetInput(k);
                if (t.FindInLinker() == null)
                    continue;
                var buffer = RenderGraph.AttachmentCache.FindAttachement(t.Attachement.AttachmentName);
                if (buffer != null && buffer.LifeMode == UAttachBuffer.ELifeMode.Transient)
                {
                    var count = buffer.Release();
                    if (count <= 0)
                    {
                        this.RenderGraph.AttachmentCache.RemoveAttachement(t.Attachement.AttachmentName);
                    }
                }
            }
        }
        
        public void UpdateNodeDistance(URenderGraph graph, Common.URenderGraphNode root)
        {
            for (int i = 0; i < NumOfInput; i++)
            {
                var pin = this.GetInput(i);
                var linker = graph.FindInLinker(pin);
                if (linker != null)
                {
                    linker.OutPin.HostNode.TempRootDistance = TempRootDistance + 1;
                    if (root.mMaxLeafDistance < linker.OutPin.HostNode.TempRootDistance)
                    {
                        root.mMaxLeafDistance = linker.OutPin.HostNode.TempRootDistance;
                    }
                    linker.OutPin.HostNode.UpdateNodeDistance(graph, root);
                }
                //else
                //{
                //    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RenderGraph", $"Input({pin.HostNode.Name}.{pin.Name}) linked null");
                //}
            }
        }

        public virtual Shader.UGraphicsShadingEnv GetPassShading(Graphics.Pipeline.URenderPolicy.EShadingType type, Mesh.TtMesh.TtAtom atom)
        {
            return null;
        }
    }

    public class UAttachmentCache
    {
        public void ResetCache(bool bFree = true)
        {
            if (bFree)
            {
                foreach (var i in CachedAttachments)
                {
                    i.Value.FreeBuffer();
                }
            }
            
            CachedAttachments.Clear();
        }
        public Dictionary<FHashText, UAttachBuffer> CachedAttachments = new Dictionary<FHashText, UAttachBuffer>();
        public UAttachBuffer FindAttachement(Common.URenderGraphPin pin)
        {
            return FindAttachement(pin.Attachement.AttachmentName);
        }
        public UAttachBuffer FindAttachement(in FHashText name)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }
        public UAttachBuffer GetAttachement(in FHashText name, UAttachmentDesc desc)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                //System.Diagnostics.Debug.Assert(result.BufferDesc.IsMatchSize(in desc.BufferDesc));
                return result;
            }
            else
            {
                //result = new UAttachBuffer();
                //result.CreateBufferViews(in desc.BufferDesc);
                result = UEngine.Instance.GfxDevice.AttachBufferManager.Alloc(desc.BufferDesc);
                CachedAttachments.Add(name, result);
                return result;
            }
        }
        public UAttachBuffer ImportAttachment(Common.URenderGraphPin pin)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(pin.Attachement.AttachmentName, out result))
                return result;
            result = new UAttachBuffer();
            result.LifeMode = UAttachBuffer.ELifeMode.Imported;
            CachedAttachments.Add(pin.Attachement.AttachmentName, result);
            return result;
        }
        public bool MoveAttachement(in FHashText from, in FHashText to)
        {
            UAttachBuffer attachment;
            if (CachedAttachments.TryGetValue(from, out attachment))
            {
                CachedAttachments.Remove(from);
                CachedAttachments.Add(to, attachment);
                return true;
            }
            return false;
        }
        public void RemoveAttachement(in FHashText name)
        {
            UAttachBuffer result;
            if (CachedAttachments.TryGetValue(name, out result))
            {
                result.FreeBuffer();
                CachedAttachments.Remove(name);
            }
        }
    }

    public class URenderGraph : IDisposable
    {
        public virtual void Dispose()
        {
            RootNode = null;
            if(NodeLayers!=null)
            {
                foreach(var i in NodeLayers)
                {
                    i.Clear();
                }
                NodeLayers = null;
            }
            
            AttachmentCache.ResetCache(true);
            foreach(var i in GraphNodes)
            {
                i.Value.Dispose();
            }
            GraphNodes.Clear();
        }
        public Common.UCopy2SwapChainNode RootNode { get; set; }
        public Dictionary<string, Common.URenderGraphNode> GraphNodes { get; } = new Dictionary<string, Common.URenderGraphNode>();
        public List<Common.URenderGraphNode>[] NodeLayers;
        public List<URenderGraphLinker> Linkers { get; } = new List<URenderGraphLinker>();
        public UAttachmentCache AttachmentCache { get; } = new UAttachmentCache();
        public bool RegRenderNode(string name, Common.URenderGraphNode node)
        {
            Common.URenderGraphNode fNode;
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
        public Common.URenderGraphNode FindNode(string name)
        {
            Common.URenderGraphNode fNode;
            if (GraphNodes.TryGetValue(name, out fNode))
            {
                return fNode;
            }
            return null;
        }
        public T FindFirstNode<T>() where T : URenderGraphNode
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
        public bool AddLinker(URenderGraphPin outPin, URenderGraphPin inPin)
        {
            if (outPin.PinType == URenderGraphPin.EPinType.Input)
            {
                return false;
            }
            if (inPin.PinType == URenderGraphPin.EPinType.Output)
            {
                return false;
            }
            var linker = new URenderGraphLinker();
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
        public URenderGraphLinker FindInLinker(URenderGraphPin inPin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == inPin)
                    return i;
            }
            return null;
        }
        public List<URenderGraphLinker> FindOutLinkers(URenderGraphPin outPin)
        {
            var result = new List<URenderGraphLinker>();
            foreach (var i in Linkers)
            {
                if (i.OutPin == outPin)
                {
                    result.Add(i);
                }
            }
            return result;
        }
        public void FindOutLinkers(URenderGraphPin outPin, List<URenderGraphLinker> result)
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
        private void SetUsedNode(URenderGraphNode node)
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
                NodeLayers = new List<URenderGraphNode>[maxDistance];
                for (int i = 0; i < maxDistance; i++)
                {
                    NodeLayers[i] = new List<URenderGraphNode>();
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
        private void UpdateNodeTree(Common.URenderGraphNode node, ref bool hasInputError)
        {
            for (int i = 0; i < node.NumOfInput; i++)
            {
                var pin = node.GetInput(i);
                var linker = this.FindInLinker(pin);
                if (linker != null)
                {
                    //if (linker.OutPin.PinType != URenderGraphPin.EPinType.Output)
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
        public int GetMaxLeafDistance(Common.URenderGraphNode root)
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
        private List<URenderGraphLinker> mTempTryReleaseLinkers = new List<URenderGraphLinker>();
        public virtual void TickLogic(GamePlay.UWorld world)
        {
            //FrameBuild();

            //BeginTickLogic(world);
            
            if (NodeLayers != null)
            {
                foreach (var i in NodeLayers)
                {
                    foreach(var j in i)
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
