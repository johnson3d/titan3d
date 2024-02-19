using EngineNS.Graphics.Pipeline.Common;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtRenderGraphPin
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
        public TtRenderGraphNode HostNode { get; set; }
        public T GetNakedHostNode<T>() where T : TtRenderGraphNode
        {
            if (HostNode is UFindNode)
            {
                return (HostNode as UFindNode).GetReferNode() as T;
            }
            else
            {
                return HostNode as T;
            }
        }

        public UAttachmentDesc Attachement = new UAttachmentDesc();
        public UAttachBuffer ImportedBuffer = null;
        public TtRenderGraphLinker FindInLinker()
        {
            return HostNode.RenderGraph.FindInLinker(this);
        }
        public static TtRenderGraphPin CreateInput(string name)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.Input;
            result.IsAutoResize = false;
            result.Attachement.Format = EPixelFormat.PXF_UNKNOWN;
            return result;
        }
        public static TtRenderGraphPin CreateOutput(string name, bool isAutoSize, EPixelFormat defaultFormat)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.Output;
            result.IsAutoResize = isAutoSize;
            result.Attachement.Format = defaultFormat;
            return result;
        }
        public static TtRenderGraphPin CreateInputOutput(string name)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.InputOutput;
            result.IsAutoResize = false;
            result.Attachement.Format = EPixelFormat.PXF_UNKNOWN;
            return result;
        }
        public static TtRenderGraphPin CreateInputOutput(string name, bool isAutoSize, EPixelFormat defaultFormat)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.InputOutput;
            result.IsAutoResize = isAutoSize;
            result.Attachement.Format = defaultFormat;
            return result;
        }
    }
    public class TtRenderGraphLinker
    {
        public TtRenderGraphPin OutPin;
        public TtRenderGraphPin InPin;
        public bool IsSame(TtRenderGraphLinker rh)
        {
            if (OutPin != rh.OutPin ||
                InPin != rh.InPin)
            {
                return false;
            }
            return true;
        }
    }
    public class TtRenderGraphNode : IO.BaseSerializer, IDisposable
    {
        ~TtRenderGraphNode()
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
        public virtual Color GetTileColor()
        {
            return Color.FromRgb(255, 0, 255);
        }
        public UDrawBuffers BasePass = new UDrawBuffers();
        public TtRenderGraph RenderGraph { get; internal set; }
        protected List<TtRenderGraphPin> InputGraphPins { get; } = new List<TtRenderGraphPin>();
        protected List<TtRenderGraphPin> OutputGraphPins { get; } = new List<TtRenderGraphPin>();
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
        public bool AddInput(TtRenderGraphPin pin, NxRHI.EBufferType needTypes)
        {
            if (pin.PinType != TtRenderGraphPin.EPinType.Input)
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
        public bool AddOutput(TtRenderGraphPin pin, NxRHI.EBufferType provideTypes)
        {
            if (pin.PinType != TtRenderGraphPin.EPinType.Output)
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
        public bool AddInputOutput(TtRenderGraphPin pin, NxRHI.EBufferType needTypes)
        {
            if (pin.PinType != TtRenderGraphPin.EPinType.InputOutput)
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
        public virtual TtRenderGraphPin GetInput(int index)
        {
            return InputGraphPins[index];
        }
        public virtual TtRenderGraphPin GetOutput(int index)
        {
            return OutputGraphPins[index];
        }
        public TtRenderGraphPin FindInput(string name)
        {
            foreach (var i in InputGraphPins)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public TtRenderGraphPin FindOutput(string name)
        {
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public UAttachBuffer ImportAttachment(TtRenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.ImportAttachment(pin);
        }
        public UAttachBuffer FindAttachBuffer(TtRenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
        }
        public UAttachBuffer GetAttachBuffer(TtRenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.GetAttachement(pin.Attachement.AttachmentName, pin.Attachement);
        }
        public bool MoveAttachment(TtRenderGraphPin pinFrom, TtRenderGraphPin pinTo)
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
        public virtual void OnLinkIn(TtRenderGraphLinker linker)
        {
            if ((linker.InPin.Attachement.BufferViewTypes & linker.OutPin.Attachement.BufferViewTypes) != linker.OutPin.Attachement.BufferViewTypes)
            {
                return;
            }
            //linker.InPin.Attachement.Format = linker.OutPin.Attachement.Format;
        }
        public virtual void OnLinkOut(TtRenderGraphLinker linker)
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

        public void TryReleaseBufers(List<TtRenderGraphLinker> linkers)
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

        public void UpdateNodeDistance(TtRenderGraph graph, TtRenderGraphNode root)
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

        public virtual Shader.UGraphicsShadingEnv GetPassShading(Mesh.TtMesh.TtAtom atom = null)
        {
            return null;
        }
    }
}
