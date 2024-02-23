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

            for (int i = MaxActive - 1; i >= 0; i--)
            {
                var ttTextureSlot = new TtTextureSlot();
                ttTextureSlot.ArrayIndex = (uint)i % arrayNum;

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
        public uint UniqueTexID;
        public TtTextureSlot Slot = null;
        public NxRHI.USrView Texture;
        public RName TextureName;
        public TtVirtualTextureBase Owner;
        public bool IsDirty = false;
        public void UpdateTexture(NxRHI.USrView texture)
        {
            Texture.Rvt = null;
            Texture = texture;
            IsDirty = true;
            Texture.Rvt = this;
        }
    }

    public class TtVirtualTextureBase : IDisposable
    {
        public Graphics.Pipeline.TtCpu2GpuBuffer<uint> TextureSlotBuffer = new Graphics.Pipeline.TtCpu2GpuBuffer<uint>();
        public TtTextureSlotAllocator TextureSlotAllocator = new TtTextureSlotAllocator();
        public List<TtRVT> Rvts = new List<TtRVT>();
        private List<uint> PrevActiveTexIDs = new List<uint>();
        private List<uint> ActiveTexIDs = new List<uint>();
        private List<uint> AddTexIDs = new List<uint>();
        private List<uint> RemoveTexIDs = new List<uint>();
        private List<uint> DirtyTexIDs = new List<uint>();
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
            DirtyTexIDs.Clear();
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
                result.UniqueTexID = (uint)Rvts.Count;
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
            DirtyTexIDs.Clear();
            foreach (var i in ActiveTexIDs)
            {
                if (PrevActiveTexIDs.BinarySearch(i) < 0)
                {
                    AddTexIDs.Add(i);
                }
                else if (Rvts[(int)i].IsDirty)
                {
                    DirtyTexIDs.Add(i);
                    Rvts[(int)i].IsDirty = false;
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
            return (AddTexIDs.Count + RemoveTexIDs.Count + DirtyTexIDs.Count) > 0;
        }
        public void TickSync(NxRHI.UCommandList cmd)
        {
            if (ProcessChanged() == false)
                return;
            
            foreach (var i in RemoveTexIDs)
            {
                TextureSlotAllocator.Free(Rvts[(int)i].Slot);
                Rvts[(int)i].Slot = null;
            }
            RemoveTexIDs.Clear();
            using (new NxRHI.TtCmdListScope(cmd))
            {
                foreach (var i in AddTexIDs)
                {
                    Rvts[(int)i].Slot = TextureSlotAllocator.Alloc();
                    UpLoadRVT(cmd, Rvts[(int)i]);
                }
                AddTexIDs.Clear();
                foreach (var i in DirtyTexIDs)
                {
                    UpLoadRVT(cmd, Rvts[(int)i]);
                }
                DirtyTexIDs.Clear();
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
        public void UpLoadRVT(NxRHI.UCommandList cmd, TtRVT rvt)
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
                cpDraw.SrcSubResource = mip;
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

