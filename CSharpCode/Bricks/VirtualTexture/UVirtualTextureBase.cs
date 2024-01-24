using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VirtualTexture
{
    public class TtTextureSlot
    {
        public uint ArrayIndex;
        public uint GetGpuDesc()
        {
            return ArrayIndex;
        }
    }
    public class TtTextureSlotAllocator : IDisposable
    {
        public NxRHI.FTextureDesc TexDesc = new NxRHI.FTextureDesc();
        public NxRHI.UTexture TextureArray = null;
        public NxRHI.USrView TextureArraySRV = null;
        public Stack<TtTextureSlot> FreeSlots = new Stack<TtTextureSlot>();
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref TextureArray);
        }
        public bool Initialize(EPixelFormat format, uint texSize = 512, uint mipLevel = 1, uint arrayNum = 256)
        {
            TexDesc.SetDefault();
            TexDesc.Format = format;
            TexDesc.ArraySize = arrayNum;
            TexDesc.Width = texSize;
            TexDesc.Height = texSize;
            TexDesc.MipLevels = mipLevel;
            var srcDesc = new NxRHI.FSrvDesc();
            srcDesc.SetTexture2DArray();
            srcDesc.Format = format;
            srcDesc.Texture2DArray.MipLevels = mipLevel;
            srcDesc.Texture2DArray.ArraySize = arrayNum;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            TextureArray = rc.CreateTexture(in TexDesc);
            TextureArraySRV = rc.CreateSRV(TextureArray, srcDesc);

            int rvtNum = MaxActive;
            for (uint i = 0; i < rvtNum; i++)
            {
                var ttTextureSlot = new TtTextureSlot();
                ttTextureSlot.ArrayIndex = i % arrayNum;

                FreeSlots.Push(ttTextureSlot);
            }
            return true;
        }
        public int MaxActive
        {
            get
            {
                return (int)TexDesc.ArraySize;
            }
        }

        public TtTextureSlot Alloc()
        {
            lock (FreeSlots)
            {
                var result = FreeSlots.Peek();
                FreeSlots.Pop();
                return result;
            }
        }
        public void Free(TtTextureSlot slot)
        {
            System.Diagnostics.Debug.Assert(slot != null);
            lock (FreeSlots)
            {
                FreeSlots.Push(slot);
            }
        }
    }
    public class TtRVT
    {
        public TtRVT()
        {            
        }
        public int UniqueTexID;
        public TtTextureSlot Slot = null;
        public NxRHI.USrView Texture;
        public RName TextureName;
        public TtVirtualTextureBase Owner;
    }

    public class TtVirtualTextureBase : IDisposable
    {
        public Graphics.Pipeline.TtCpu2GpuBuffer<uint> TextureSlotBuffer = new Graphics.Pipeline.TtCpu2GpuBuffer<uint>();
        public TtTextureSlotAllocator TextureSlotAllocator = new TtTextureSlotAllocator();
        public List<TtRVT> Rvts = new List<TtRVT>();
        private List<int> PrevActiveTexIDs = new List<int>();
        private List<int> ActiveTexIDs = new List<int>();
        private List<int> AddTexIDs = new List<int>();
        private List<int> RemoveTexIDs = new List<int>();
        NxRHI.FTextureDesc TexDesc = new NxRHI.FTextureDesc();
        public void ActiveRVT(NxRHI.USrView tex)
        {
            var rvt = RegRVT(tex);
            var index = ActiveTexIDs.BinarySearch(rvt.UniqueTexID);
            if (index >= 0)
            {
                return;
            }
            ActiveTexIDs.Insert(~index, rvt.UniqueTexID);
        }
        ~TtVirtualTextureBase()
        {
            Dispose();
        }
        public void Dispose()
        {
            TextureSlotAllocator.Dispose();
            Rvts.Clear();
            PrevActiveTexIDs.Clear();
            ActiveTexIDs.Clear();
            AddTexIDs.Clear();
            RemoveTexIDs.Clear();
        }
        public bool Initialize(EPixelFormat format, uint texSize = 512, uint mipLevel = 1, uint arrayNum = 256)
        {
            Dispose();
            TextureSlotAllocator.Initialize(format, texSize, mipLevel, arrayNum);
            TextureSlotBuffer.Initialize(NxRHI.EBufferType.BFT_SRV);
            TextureSlotBuffer.SetSize(ushort.MaxValue);
            
            return true;
        }
        public int MaxActive
        {
            get
            {
                return TextureSlotAllocator.MaxActive;
            }
        }
        public TtRVT RegRVT(NxRHI.USrView tex)
        {
            lock (Rvts)
            {
                if (tex.Rvt != null)
                {
                    if (tex.Rvt.Owner == this)
                    {
                        return tex.Rvt;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }

                foreach (var i in Rvts)
                {
                    if (i.TextureName == tex.AssetName)
                    {
                        i.Texture = tex;
                        return i;
                    }
                }

                TtRVT result = new TtRVT();
                result.Texture = tex;
                result.TextureName = tex.AssetName;
                result.UniqueTexID = Rvts.Count;
                result.Slot = null;
                result.Owner = this;
                tex.Rvt = result;
                Rvts.Add(result);
                return result;
            }
        }
        private bool ProcessChanged()
        {
            AddTexIDs.Clear();
            foreach (var i in ActiveTexIDs)
            {
                if (PrevActiveTexIDs.BinarySearch(i) < 0)
                {
                    AddTexIDs.Add(i);
                }
            }
            RemoveTexIDs.Clear();
            foreach (var i in PrevActiveTexIDs)
            {
                if (ActiveTexIDs.BinarySearch(i) < 0)
                {
                    RemoveTexIDs.Add(i);
                }
            }
            CoreSDK.Swap(ref PrevActiveTexIDs, ref ActiveTexIDs);
            ActiveTexIDs.Clear();
            return (AddTexIDs.Count + RemoveTexIDs.Count) > 0;
        }
        public void TickSync(NxRHI.UCommandList cmd)
        {
            if (ProcessChanged() == false)
                return;
            
            foreach (var i in RemoveTexIDs)
            {
                TextureSlotAllocator.Free(Rvts[i].Slot);
            }
            RemoveTexIDs.Clear();
            using (new NxRHI.TtCmdListScope(cmd))
            {
                foreach (var i in AddTexIDs)
                {
                    Rvts[i].Slot = TextureSlotAllocator.Alloc();
                    UploadRVT(cmd, Rvts[i]);
                }
                AddTexIDs.Clear();
                cmd.FlushDraws();
            }
            for (int i = 0; i < Rvts.Count; i++)
            {
                var slot = Rvts[i].Slot;
                if (slot == null)
                    TextureSlotBuffer.UpdateData(i, uint.MaxValue);
                else
                    TextureSlotBuffer.UpdateData(i, slot.GetGpuDesc());
            }
            TextureSlotBuffer.Flush2GPU(cmd);
            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(cmd, NxRHI.EQueueType.QU_Transfer);
        }
        public void UploadRVT(NxRHI.UCommandList cmd, TtRVT rvt)
        {
            //Copy rvt to
            //TextureArrays[rvt.SlotDesc.TextureIndex]
            for (uint mip = 0; mip < TextureSlotAllocator.TexDesc.MipLevels; mip++)
            {
                var cpDraw = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
                cpDraw.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                cpDraw.BindTextureSrc(rvt.Texture.GetTexture());
                cpDraw.BindTextureDest(TextureSlotAllocator.TextureArray);
                ref var fp = ref cpDraw.FootPrint;
                cpDraw.DestSubResource = rvt.Slot.ArrayIndex * TextureSlotAllocator.TexDesc.MipLevels + mip;
                cpDraw.SrcSubResource = 0;
                cmd.PushGpuDraw(cpDraw);
            }
        }
    }
}

namespace EngineNS.NxRHI
{
    partial class USrView
    {
        public Bricks.VirtualTexture.TtRVT Rvt = null;
    }
}

