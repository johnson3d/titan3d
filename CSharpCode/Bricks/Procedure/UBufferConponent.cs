using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    public class BufferTypeOperatorAttribute : Attribute
    {
        public Type OperatorType;
        public BufferTypeOperatorAttribute(Type operatorType)
        {
            OperatorType = operatorType;
        }
    }

    public class UBufferCreator : IO.BaseSerializer
    {
        public static UBufferCreator CreateInstance<TBuffer>(int x = -1, int y = -1, int z = -1)
            where TBuffer : UBufferConponent
        {
            var result = new UBufferCreator();
            result.BufferType = Rtti.UTypeDesc.TypeOf<TBuffer>();
            result.XSize = x;
            result.YSize = y;
            result.ZSize = z;
            return result;
        }
        public static UBufferCreator CreateInstance(Rtti.UTypeDesc bufferType, int x = -1, int y = -1, int z = -1)
        {
            var result = new UBufferCreator();
            result.BufferType = bufferType;
            result.XSize = x;
            result.YSize = y;
            result.ZSize = z;
            return result;
        }
        public UBufferCreator Clone()
        {
            var result = new UBufferCreator();
            CopyTo(this, result);
            return result;
        }
        public static void CopyTo(UBufferCreator src, UBufferCreator tag)
        {
            if (src == null || tag == null)
                return;
            tag.BufferType = src.BufferType;
            tag.XSize = src.XSize;
            tag.YSize = src.YSize;
            tag.ZSize = src.ZSize;
        }
        public void SetSize(UBufferCreator creator)
        {
            XSize = creator.XSize;
            YSize = creator.YSize;
            ZSize = creator.ZSize;
        }
        public int GetElementTypeSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(ElementType);
        }
        Rtti.UTypeDesc mElementType;
        [EGui.Controls.PropertyGrid.PGTypeEditor(FilterMode = 0)]
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                if (mElementType == null)
                {
                    if (mBufferType == null)
                        return null;
                    var tmp = Rtti.UTypeDescManager.CreateInstance(mBufferType) as UBufferConponent;
                    if (tmp != null)
                        mElementType = tmp.PixelOperator.ElementType;
                }
                return mElementType;
            }
            private set
            {
                mElementType = value;
            }
        }
        Rtti.UTypeDesc mBufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<float, FFloatOperator>>();
        [Rtti.Meta]
        //[IO.UTypeDescSerializer()]
        [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(UBufferConponent), FilterMode = EGui.Controls.UTypeSelector.EFilterMode.IncludeObjectType)]
        public Rtti.UTypeDesc BufferType
        {
            get => mBufferType;
            set
            {
                mBufferType = value;
                var tmp = Rtti.UTypeDescManager.CreateInstance(value) as UBufferConponent;
                if (tmp != null)
                {
                    ElementType = tmp.PixelOperator.ElementType;
                }
                else
                {
                    mBufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<float, FFloatOperator>>();
                }
            }
        }
        [Rtti.Meta]
        public int XSize { get; set; } = 1;
        [Rtti.Meta]
        public int YSize { get; set; } = 1;
        [Rtti.Meta]
        public int ZSize { get; set; } = 1;

        public static Type GetBufferOperatorType(Type type)
        {
            var atts = type.GetCustomAttributes(typeof(BufferTypeOperatorAttribute), false);
            if(atts.Length != 0)
            {
                var att = atts[0] as BufferTypeOperatorAttribute;
                return att.OperatorType;
            }

            if (type == typeof(float))
                return typeof(FFloatOperator);
            else if (type == typeof(Vector2))
                return typeof(FFloat2Operator);
            else if (type == typeof(Vector3))
                return typeof(FFloat3Operator);
            else if (type == typeof(Vector4))
                return typeof(FFloat4Operator);
            else if (type == typeof(int))
                return typeof(FIntOperator);
            else if (type == typeof(Vector2i))
                return typeof(FInt2Operator);
            else if (type == typeof(Vector3i))
                return typeof(FInt3Operator);
            else if (type == typeof(DVector3))
                return typeof(FDouble3Operator);
            else if (type == typeof(sbyte))
                return typeof(FSByteOperator);
            else if (type == typeof(FTransform))
                return typeof(FTransformOperator);
            else if (type == typeof(FSquareSurface))
                return typeof(FSquareSurfaceOperator);
            else if (type == typeof(Quaternion))
                return typeof(FQuaternionOperator);

            return null;
        }
    }
    public partial class UBufferConponent
    {
        public UBufferCreator BufferCreator { get; private set; }
        public virtual ISuperPixelOperatorBase PixelOperator { get => null; }
        public int LifeCount { get; set; } = 0;
        public int Width 
        { 
            get
            {
                if (BufferCreator == null)
                    return 0;
                return BufferCreator.XSize;
            }
        }
        public int Height 
        {
            get
            {
                if (BufferCreator == null)
                    return 0;
                return BufferCreator.YSize;
            }
        }
        public int Depth 
        {
            get
            {
                if (BufferCreator == null)
                    return 0;
                return BufferCreator.ZSize;
            }
        }
        public int Pitch { get; private set; }
        public int Slice { get; private set; }
        public int ElementSize { get; private set; }
        public Vector3 UVWStep = Vector3.Zero;
        public Support.UBlobObject SuperPixels = new Support.UBlobObject();
        protected UBufferConponent()
        {

        }
        ~UBufferConponent()
        {
            SuperPixels.Dispose();
        }

        public unsafe Hash160 CalcPixelHash()
        {
            return Hash160.CreateHash160(SuperPixels.mCoreObject.GetData(), SuperPixels.mCoreObject.GetSize());
        }
        public void SaveToCache(string name, in Hash160 dataHash)
        {
            var xnd = new IO.TtXndHolder("PgcBuffer", 0, 0);
            unsafe
            {
                SaveXnd(xnd, xnd.RootNode.mCoreObject, in dataHash);
            }
            xnd.SaveXnd(name);
        }
        public bool LoadFromCache(string name, in Hash160 testHash)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name))
            {
                if (xnd == null)
                    return false;

                unsafe
                {
                    return LoadXnd(xnd.RootNode.mCoreObject, in testHash);
                }
            }
        }
        public unsafe void SaveXnd(IO.TtXndHolder xnd, XndNode node, in Hash160 dataHash)
        {
            using(var attr = xnd.NewAttribute("BufferCreator", 0, 0))
            {
                using (var ar = attr.GetWriter(30))
                {
                    ar.Write(dataHash);
                    ar.Write(ElementSize);
                    ar.Write(BufferCreator);
                }
                node.AddAttribute(attr);
            }

            using(var attr = xnd.NewAttribute("BufferData", 0, 0))
            {
                using (var ar = attr.GetWriter(SuperPixels.mCoreObject.GetSize()))
                {
                    int bfSize = ElementSize * BufferCreator.XSize * BufferCreator.YSize * BufferCreator.ZSize;
                    ar.WritePtr(SuperPixels.mCoreObject.GetData(), bfSize);
                }
                node.AddAttribute(attr);
            }
        }
        public unsafe bool LoadXnd(XndNode node, in Hash160 testHash)
        {
            var attr = node.TryGetAttribute("BufferCreator");
            if (attr.IsValidPointer)
            {
                Hash160 dataHash;
                int elemSize;
                IO.ISerializer creator;
                using (var ar = attr.GetReader(this))
                {   
                    ar.Read(out dataHash);
                    ar.Read(out elemSize);
                    ar.Read(out creator, this);
                }

                if (dataHash != testHash)
                    return false;

                var bfCreator = creator as UBufferCreator;
                if (bfCreator == null && BufferCreator.BufferType != bfCreator.BufferType)
                    return false;
                
                attr = node.TryGetAttribute("BufferData");
                if (attr.IsValidPointer)
                {
                    BufferCreator.XSize = bfCreator.XSize;
                    BufferCreator.YSize = bfCreator.YSize;
                    BufferCreator.ZSize = bfCreator.ZSize;

                    Pitch = bfCreator.XSize * ElementSize;
                    Slice = Pitch * bfCreator.YSize;

                    using (var ar = attr.GetReader(this))
                    {
                        int bfSize = elemSize * BufferCreator.XSize * BufferCreator.YSize * BufferCreator.ZSize;
                        SuperPixels.mCoreObject.ReSize((uint)bfSize);
                        ar.ReadPtr(SuperPixels.mCoreObject.GetData(), bfSize);
                    }

                    //var t1 = Support.Time.HighPrecision_GetTickCount();
                    //var contentHash = CalcPixelHash();
                    //var t2 = Support.Time.HighPrecision_GetTickCount();
                    return true;
                }
            }
            return false;
        }
        public virtual unsafe NxRHI.USrView CreateVector2Texture2D(Vector2 min, Vector2 max)
        {
            NxRHI.UTexture texture;
            if(min.X >= 0 && min.X <= 1 && max.X >= 0 && max.X <= 1)
            {
                min.X = 0;
                max.X = 1;
            }
            if(min.Y >= 0 && min.Y <= 1 && max.Y >= 0 && max.Y <= 1)
            {
                min.Y = 0;
                min.Y = 1;
            }
            var hRange = max - min;
            unsafe
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                var initData = new NxRHI.FMappedSubResource();
                initData.SetDefault();
                desc.InitData = &initData;
                {
                    var Count = Width * Height;
                    var tarPixels = new Byte4[Count];
                    var pSlice = (Vector2*)this.GetSliceAddress(0);
                    for (int i = 0; i < Count; i++)
                    {
                        var tmp = pSlice[i] - min;
                        tmp = tmp * 255.0f;
                        tmp = tmp / hRange;
                        tarPixels[i].X = (byte)tmp.X;
                        tarPixels[i].Y = (byte)tmp.Y;
                        tarPixels[i].Z = 0;
                        tarPixels[i].W = 255;
                    }
                    fixed (Byte4* p = &tarPixels[0])
                    {
                        initData.RowPitch = desc.Width * (uint)sizeof(Byte4);
                        initData.pData = p;
                        texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                    }
                }

                var rsvDesc = new NxRHI.FSrvDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(texture, in rsvDesc);
                return result;
            }
        }
        public virtual unsafe NxRHI.USrView CreateVector3Texture2D(Vector3 minHeight, Vector3 maxHeight)
        {
            NxRHI.UTexture texture;
            if (minHeight.X >= 0 && minHeight.X <= 1 && maxHeight.X >= 0 && maxHeight.X <= 1)
            {
                minHeight.X = 0;
                maxHeight.X = 1;
            }
            if (minHeight.Y >= 0 && minHeight.Y <= 1 && maxHeight.Y >= 0 && maxHeight.Y <= 1)
            {
                minHeight.Y = 0;
                maxHeight.Y = 1;
            }
            if (minHeight.Z >= 0 && minHeight.Z <= 1 && maxHeight.Z >= 0 && maxHeight.Z <= 1)
            {
                minHeight.Z = 0;
                maxHeight.Z = 1;
            }
            var hRange = maxHeight - minHeight;
            unsafe
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                var initData = new NxRHI.FMappedSubResource();
                initData.SetDefault();
                desc.InitData = &initData;
                {
                    var Count = Width * Height;
                    var tarPixels = new Byte4[Count];
                    var pSlice = (Vector3*)this.GetSliceAddress(0);
                    for (int i = 0; i < Count; i++)
                    {
                        var tmp = pSlice[i] - minHeight;
                        tmp = tmp * 255.0f;
                        tmp = tmp / hRange;
                        //tarPixels[i].X = (byte)(((pSlice[i].X - minHeight.X) / hRange.) * 255.0f);
                        //tarPixels[i].Y = (byte)(((pSlice[i].Y - minHeight) / hRange) * 255.0f);
                        //tarPixels[i].Z = (byte)(((pSlice[i].Z - minHeight) / hRange) * 255.0f);
                        tarPixels[i].X = (byte)tmp.X;
                        tarPixels[i].Y = (byte)tmp.Y;
                        tarPixels[i].Z = (byte)tmp.Z;
                        tarPixels[i].W = 255;
                    }
                    fixed (Byte4* p = &tarPixels[0])
                    {
                        initData.RowPitch = desc.Width * (uint)sizeof(Byte4);
                        initData.pData = p;
                        texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                    }
                }

                var rsvDesc = new NxRHI.FSrvDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(texture, in rsvDesc);
                return result;
            }
        }
        public virtual unsafe NxRHI.USrView CreateAsHeightMapTexture2D(out NxRHI.UTexture texture, float minHeight, float maxHeight, EPixelFormat format = EPixelFormat.PXF_R32_FLOAT, float finalScale = 1.0f, bool bNomalized = false)
        {
            texture = null;
            if (minHeight >= 0 && minHeight <= 1 && maxHeight >= 0 && maxHeight <= 1)
            {
                minHeight = 0;
                maxHeight = 1;
            }
            float hRange = maxHeight - minHeight;
            unsafe
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = format;
                var initData = new NxRHI.FMappedSubResource();
                initData.SetDefault();
                desc.InitData = &initData;
                switch (format)
                {
                    case EPixelFormat.PXF_R32_FLOAT:
                        {
                            var Count = Width * Height;
                            var tarPixels = new float[Count];
                            var pSlice = (float*)this.GetSliceAddress(0);
                            for (int i = 0; i < Count; i++)
                            {
                                tarPixels[i] = pSlice[i] - minHeight;
                                if (bNomalized)
                                {
                                    tarPixels[i] = tarPixels[i] / hRange;
                                }
                            }
                            fixed (float* p = &tarPixels[0])
                            {
                                initData.RowPitch = desc.Width * sizeof(float);
                                initData.pData = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R16_FLOAT:
                        {//高度信息有2的11次方级别精度高度
                            var Count = Width * Height;
                            var tarPixels = new Half[Count];
                            if (BufferCreator.ElementType == Rtti.UTypeDescGetter<float>.TypeDesc)
                            {
                                var pSlice = (float*)this.GetSliceAddress(0);
                                for (int i = 0; i < Count; i++)
                                {
                                    //tarPixels[i] = HalfHelper.SingleToHalf(Pixels[i]);
                                    if (bNomalized)
                                    {
                                        tarPixels[i] = HalfHelper.SingleToHalf((pSlice[i] - minHeight) * finalScale / hRange);
                                    }
                                    else
                                    {
                                        tarPixels[i] = HalfHelper.SingleToHalf((pSlice[i] - minHeight) * finalScale);
                                    }
                                }
                                
                            }
                            else if (BufferCreator.ElementType == Rtti.UTypeDescGetter<sbyte>.TypeDesc)
                            {
                                var pSlice = (sbyte*)this.GetSliceAddress(0);
                                for (int i = 0; i < Count; i++)
                                {
                                    //tarPixels[i] = HalfHelper.SingleToHalf(Pixels[i]);
                                    if (bNomalized)
                                    {
                                        tarPixels[i] = HalfHelper.SingleToHalf(((float)pSlice[i] - minHeight) * finalScale / hRange);
                                    }
                                    else
                                    {
                                        tarPixels[i] = HalfHelper.SingleToHalf(((float)pSlice[i] - minHeight) * finalScale);
                                    }
                                }

                            }
                            fixed (Half* p = &tarPixels[0])
                            {
                                initData.RowPitch = (uint)(desc.Width * sizeof(Half));
                                initData.pData = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R8G8_UNORM:
                        {//如果希望表达更高的精度，而不是浪费在half上的指数位，可以RG8UNorm格式，让高度信息有65535级别
                            var Count = Width * Height;
                            var tarPixels = new Byte2[Width * Height];
                            var pSlice = (float*)this.GetSliceAddress(0);
                            float range = maxHeight - minHeight;
                            for (int i = 0; i < Count; i++)
                            {
                                float alt = pSlice[i] - minHeight;
                                float rate = alt / range;
                                ushort value = (ushort)(rate * (float)ushort.MaxValue);
                                tarPixels[i].X = (byte)(value & 0xFF);
                                tarPixels[i].Y = (byte)((value >> 8) & 0xFF);
                            }
                            fixed (Byte2* p = &tarPixels[0])
                            {
                                initData.RowPitch = (uint)(desc.Width * sizeof(Byte2));
                                initData.pData = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R8_UINT:
                        {//如果希望表达更高的精度，而不是浪费在half上的指数位，可以RG8UNorm格式，让高度信息有65535级别
                            var Count = Width * Height;
                            var tarPixels = new Byte[Width * Height];
                            var pSlice = (float*)this.GetSliceAddress(0);
                            float range = maxHeight - minHeight;
                            for (int i = 0; i < Count; i++)
                            {
                                var alt = (Byte)pSlice[i];
                            }
                            fixed (Byte* p = &tarPixels[0])
                            {
                                initData.RowPitch = (uint)(desc.Width * sizeof(Byte));
                                initData.pData = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                            }
                        }
                        break;
                    default:
                        return null;
                }


                var rsvDesc = new NxRHI.FSrvDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                rsvDesc.Format = format;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(texture, in rsvDesc);
                return result;
            }
        }
        
        #region CppMemBuffer
        public unsafe static UBufferConponent CreateInstance(in UBufferCreator creator)
        {
            var result = Rtti.UTypeDescManager.CreateInstance(creator.BufferType) as UBufferConponent;
            result.BufferCreator = creator.Clone();
            result.CreateBuffer(creator.ElementType, creator.XSize, creator.YSize, creator.ZSize, IntPtr.Zero.ToPointer());
            return result;
        }
        public unsafe void CreateBuffer(Rtti.UTypeDesc type, int xSize, int ySize, int zSize, void* initValue)
        {
            int elementSize = System.Runtime.InteropServices.Marshal.SizeOf(type.SystemType);
            if (BufferCreator == null)
            {   
                BufferCreator = UBufferCreator.CreateInstance(type, xSize, ySize, zSize);
            }
            BufferCreator.XSize = xSize;
            BufferCreator.YSize = ySize;
            BufferCreator.ZSize = zSize;

            ElementSize = elementSize;
            Pitch = xSize * ElementSize;
            Slice = Pitch * ySize;
            //SuperPixels.Dispose();
            //SuperPixels = BigStackBuffer.CreateInstance(Slice * zSize);
            SuperPixels.mCoreObject.ReSize((uint)(Slice * zSize));
            if (initValue != IntPtr.Zero.ToPointer())
            {
                //var pBuffer = (byte*)SuperPixels.GetBuffer();
                var pBuffer = (byte*)SuperPixels.mCoreObject.GetData();
                for (int i = 0; i < xSize * ySize * zSize; i++)
                {
                    CoreSDK.MemoryCopy(&pBuffer[i * ElementSize], initValue, (uint)ElementSize);
                }
            }

            UVWStep.X = 1.0f / (float)xSize;
            UVWStep.Y = 1.0f / (float)ySize;
            UVWStep.Z = 1.0f / (float)zSize;
        }
        public unsafe byte* GetSuperPixelAddress(int x, int y, int z)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return null;
            //var pBuffer = (byte*)SuperPixels.GetBuffer();
            var pBuffer = (byte*)SuperPixels.mCoreObject.GetData();
            //return &pBuffer[(z * (Width * Depth) + Width * y + x) * ElementSize];
            return &pBuffer[Slice * z + y * Pitch + x * ElementSize];
        }
        public unsafe void SetSuperPixelAddress(int x, int y, int z, void* valueAddress)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return;
            //var pBuffer = (byte*)SuperPixels.GetBuffer();
            var pBuffer = (byte*)SuperPixels.mCoreObject.GetData();
            CoreSDK.MemoryCopy(&pBuffer[Slice * z + y * Pitch + x * ElementSize], valueAddress, (uint)ElementSize);
        }
        public unsafe byte* GetSliceAddress(int index)
        {
            return GetSuperPixelAddress(0, 0, index);
        }
        #endregion

        #region template
        [Rtti.Meta]
        public bool IsValidPixel(int x, int y = 0, int z = 0)
        {
            if (x < 0 || x >= Width ||
                y < 0 || y >= Height ||
                z < 0 || z >= Depth)
            {
                return false;
            }
            return true;
        }
        public enum EPixelAddressMode
        {
            Clamp,
            Wrap,
        }
        public unsafe void* GetSuperPixelAddress(in Vector3 uvw, EPixelAddressMode mode = EPixelAddressMode.Clamp)
        {
            int x = MathHelper.FloorToInt(uvw.X * (float)Width);
            int y = MathHelper.FloorToInt(uvw.Y * (float)Height);
            int z = MathHelper.FloorToInt(uvw.Z * (float)Depth);
            if (x >= Width)
                if (x >= Width)
            {
                switch (mode)
                {
                    case EPixelAddressMode.Clamp:
                        x = Width - 1;
                        break;
                    case EPixelAddressMode.Wrap:
                        x = x % Width;
                        break;
                }
            }
            if (y >= Height)
            {
                switch (mode)
                {
                    case EPixelAddressMode.Clamp:
                        y = Height - 1;
                        break;
                    case EPixelAddressMode.Wrap:
                        y = y % Height;
                        break;
                }
            }
            if (z >= Depth)
            {
                switch (mode)
                {
                    case EPixelAddressMode.Clamp:
                        z = Depth - 1;
                        break;
                    case EPixelAddressMode.Wrap:
                        z = z % Depth;
                        break;
                }
            }
            return GetSuperPixelAddress(x, y, z);
        }
        public ref T GetPixel<T>(in Vector3 uvw, EPixelAddressMode mode = EPixelAddressMode.Clamp) where T : unmanaged
        {
            unsafe
            {
                var pAddr = (T*)GetSuperPixelAddress(in uvw, mode);//GetSuperPixelAddress(in uvw, mode);
                return ref *pAddr;
            }
        }
        public void SetPixel<T>(in Vector3 uvw, in T value, EPixelAddressMode mode = EPixelAddressMode.Clamp) where T : unmanaged
        {
            int x = (int)(uvw.X * (float)Width);
            int y = (int)(uvw.Y * (float)Height);
            int z = (int)(uvw.Z * (float)Depth);
            if (x >= Width)
            {
                switch (mode)
                {
                    case EPixelAddressMode.Clamp:
                        x = Width - 1;
                        break;
                    case EPixelAddressMode.Wrap:
                        x = x % Width;
                        break;
                }
            }
            if (y >= Height)
            {
                switch (mode)
                {
                    case EPixelAddressMode.Clamp:
                        y = Height - 1;
                        break;
                    case EPixelAddressMode.Wrap:
                        y = y % Height;
                        break;
                }
            }
            if (z >= Depth)
            {
                switch (mode)
                {
                    case EPixelAddressMode.Clamp:
                        z = Depth - 1;
                        break;
                    case EPixelAddressMode.Wrap:
                        z = z % Depth;
                        break;
                }
            }
            unsafe
            {
                System.Diagnostics.Debug.Assert(sizeof(T) == ElementSize);
                fixed (T* pAddr = &value)
                {
                    SetSuperPixelAddress(x, y, z, (byte*)pAddr);
                }
            }
        }
        public ref T GetPixel<T>(int x, int y, int z) where T : unmanaged
        {
            unsafe
            {
                var pAddr = (T*)GetSuperPixelAddress(x, y, z);
                //if (pAddr == (T*)0)
                //{
                //    return new T();
                //}
                return ref *pAddr;
            }
        }
        public void SetPixel<T>(int x, int y, int z, in T value) where T : unmanaged
        {
            unsafe
            {
                System.Diagnostics.Debug.Assert(sizeof(T) == ElementSize);
                fixed (T* pAddr = &value)
                {
                    SetSuperPixelAddress(x, y, z, (byte*)pAddr);
                }
            }
        }
        public ref T GetPixel<T>(int x, int y) where T : unmanaged
        {
            unsafe
            {
                return ref *(T*)GetSuperPixelAddress(x, y, 0);
            }
        }
        public void SetPixel<T>(int x, int y, in T value) where T : unmanaged
        {
            unsafe
            {
                System.Diagnostics.Debug.Assert(sizeof(T) == ElementSize);
                fixed (T* pAddr = &value)
                {
                    SetSuperPixelAddress(x, y, 0, (byte*)pAddr);
                }
            }
        }
        public ref T GetPixel<T>(int x) where T : unmanaged
        {
            unsafe
            {
                return ref *(T*)GetSuperPixelAddress(x, 0, 0);
            }
        }
        public void SetPixel<T>(int x, in T value) where T : unmanaged
        {
            unsafe
            {
                System.Diagnostics.Debug.Assert(sizeof(T) == ElementSize);
                fixed (T* pAddr = &value)
                {
                    SetSuperPixelAddress(x, 0, 0, (byte*)pAddr);
                }
            }
        }
        public unsafe T* AddPixel<T>(in T value, int y = 0, int z = 0) where T : unmanaged
        {
            System.Diagnostics.Debug.Assert(sizeof(T) == ElementSize);
            this.BufferCreator.XSize++;
            return SuperPixels.PushValue(in value);
        }
        public unsafe void ResizePixels(int x = 0, int y = 1, int z = 1)
        {
            this.BufferCreator.XSize = x;
            this.BufferCreator.YSize = y;
            this.BufferCreator.ZSize = z;
            this.CreateBuffer(PixelOperator.ElementType, x, y, z, IntPtr.Zero.ToPointer());
        }
        public unsafe void GetRangeUnsafe<T, TOperator>(out T minValue, out T maxValue) 
            where T : unmanaged
            where TOperator : ISuperPixelOperator<T>, new()
        {
            var tOp = new TOperator();
            minValue = tOp.MaxValue;
            maxValue = tOp.MinValue;
            var srcType = Rtti.UTypeDescGetter<T>.TypeDesc;
            fixed(T* pMaxValue = &maxValue)
            fixed (T* pMinValue = &minValue)
            {
                for (int i = 0; i < Depth; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        for (int k = 0; k < Width; k++)
                        {
                            var curPixelAddr = GetSuperPixelAddress(k, j, i);
                            tOp.SetIfGreateThan(srcType, pMaxValue, srcType, curPixelAddr);
                            tOp.SetIfLessThan(srcType, pMinValue, srcType, curPixelAddr);
                        }
                    }
                }
            }   
        }
        #endregion

        #region Macross
        public delegate void FOnPerPixel(UBufferConponent result, int x, int y, int z);
        [Rtti.Meta]
        public void DispatchPixels(FOnPerPixel onPerPiexel, bool bMultThread = false)
        {
            //bMultThread = false;
            if (bMultThread == false)
            {
                for (int i = 0; i < Depth; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        for (int k = 0; k < Width; k++)
                        {
                            onPerPiexel(this, k, j, i);
                        }
                    }
                }
            }
            else
            {
                UEngine.Instance.EventPoster.ParrallelFor(Width * Height * Depth, static (index, arg1, arg2) =>
                {
                    var pThis = arg1 as UBufferConponent;
                    var onPerPiexel = arg2 as FOnPerPixel;
                    int pitch = pThis.Height * pThis.Width;
                    int z = index / pitch;
                    int y = (index % pitch) / pThis.Width;
                    int x = (index % pitch) % pThis.Width;

                    onPerPiexel(pThis, x, y, z);
                }, this, onPerPiexel);

                //var evt = new System.Threading.AutoResetEvent(false);
                //var smp = Thread.TtSemaphore.CreateSemaphore(Depth * Height * Width, evt);

                //if (Depth == 1 && Height == 1)
                //{
                //    for (int i = 0; i < Depth; i++)
                //    {
                //        for (int j = 0; j < Height; j++)
                //        {
                //            for (int k = 0; k < Width; k++)
                //            {
                //                int x = k;
                //                int y = j;
                //                int z = i;
                //                UEngine.Instance.EventPoster.RunOn((state) =>
                //                {
                //                    onPerPiexel(this, x, y, z);
                //                    smp.Release();
                //                    return true;
                //                }, Thread.Async.EAsyncTarget.TPools);
                //            }
                //        }
                //    }
                //}
                //else if (Depth == 1)
                //{
                //    for (int i = 0; i < Depth; i++)
                //    {
                //        for (int j = 0; j < Height; j++)
                //        {
                //            int y = j;
                //            int z = i;
                //            UEngine.Instance.EventPoster.RunOn((state) =>
                //            {
                //                for (int k = 0; k < Width; k++)
                //                {
                //                    onPerPiexel(this, k, y, z);
                //                    smp.Release();
                //                }
                //                return true;
                //            }, Thread.Async.EAsyncTarget.TPools);
                //        }
                //    }
                //}
                //else
                //{
                //    for (int i = 0; i < Depth; i++)
                //    {
                //        int z = i;
                //        UEngine.Instance.EventPoster.RunOn((state) =>
                //        {
                //            for (int j = 0; j < Height; j++)
                //            {
                //                for (int k = 0; k < Width; k++)
                //                {
                //                    onPerPiexel(this, k, j, i);
                //                    smp.Release();
                //                }
                //            }
                //            return true;
                //        }, Thread.Async.EAsyncTarget.TPools);
                //    }
                //}

                //smp.Wait(int.MaxValue);
                //smp.FreeSemaphore();
            }
        }
        [Rtti.Meta]
        public UBufferConponent Clone()
        {
            var result = UBufferConponent.CreateInstance(this.BufferCreator);
            CopyData(this, result);
            return result;
        }
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.ManualMarshal)]
        public static unsafe bool CopyData(UBufferConponent src, UBufferConponent dst)
        {
            if (src.Width != dst.Width ||
                src.Height != dst.Height)
                return false;
            for (int i = 0; i < src.Depth; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    for (int k = 0; k < src.Width; k++)
                    {
                        dst.SetSuperPixelAddress(k, j, i, src.GetSuperPixelAddress(k, j, i));
                    }
                }
            }
            return true;
        }
        public static unsafe bool macross_CopyData(string nodeName, UBufferConponent src, UBufferConponent dst)
        {
            using (var stackframe = EngineNS.Macross.UMacrossStackTracer.CurrentFrame)
            {
                if (stackframe != null)
                {
                    //stackframe.SetWatchVariable(nodeName + ":name", name);
                }
            }
            var _return_value = CopyData(src, dst);
            return _return_value;
        }
        [Rtti.Meta]
        public Vector3 GetUVW(int x, int y, int z)
        {
            Vector3 result;
            result.X = (float)x / (float)Width;
            result.Y = (float)y / (float)Height;
            result.Z = (float)z / (float)Depth;
            return result;
        }
        [Rtti.Meta]
        public Vector3 GetClampedUVW(int x, int y, int z)
        {
            Vector3 result;
            result.X = MathHelper.Clamp((float)x / (float)Width, 0, 1);
            result.Y = MathHelper.Clamp((float)y / (float)Height, 0, 1);
            result.Z = MathHelper.Clamp((float)z / (float)Depth, 0, 1);
            return result;
        }
        [Rtti.Meta(MethodGenericParameters = new System.Type[]
                {
                    typeof(float), typeof(Vector2), typeof(Vector3)
                })]
        public unsafe Span<T> GetSuperPixelSpan<T>(int x, int y, int z, int num = -1) where T : unmanaged
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return null;
            var total = BufferCreator.XSize* BufferCreator.YSize * BufferCreator.XSize* BufferCreator.XSize;
            var offset = z * BufferCreator.XSize * BufferCreator.YSize + y * BufferCreator.XSize + x;
            if (num < 0)
            {
                num = total;
            }
            if (offset + num >= total)
            {
                num = total - offset;
            }
            var pBuffer = (byte*)SuperPixels.mCoreObject.GetData();
            var ptr = &pBuffer[Slice * z + y * Pitch + x * ElementSize];
            Span<T> result = new Span<T>(ptr, num);
            return result;
        }
        [Rtti.Meta]
        protected unsafe void* GetSuperPixelAddress(int x, int y, int z,
                [Rtti.MetaParameter(TypeList = new System.Type[]
                {
                    typeof(float*), typeof(Vector2*), typeof(Vector3*)
                },
                ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type retType)
        {
            if (retType.IsValueType == false)
                return null;
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return null;
            var pBuffer = (byte*)SuperPixels.mCoreObject.GetData();
            var ptr = &pBuffer[Slice * z + y * Pitch + x * ElementSize];
            return ptr;
        }
        [Rtti.Meta]
        public unsafe object GetSuperPixelAddressEX(int x, int y, int z,
           [Rtti.MetaParameter(TypeList = new System.Type[]
                {
                    typeof(float), typeof(Vector2), typeof(Vector3), typeof(DVector3), 
                    typeof(Quaternion)
                },
            ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type retType)
        {
            if (retType.IsValueType == false)
                return null;
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return null;
            //var pBuffer = (byte*)SuperPixels.GetBuffer();
            var pBuffer = (byte*)SuperPixels.mCoreObject.GetData();
            //return &pBuffer[(z * (Width * Depth) + Width * y + x) * ElementSize];
            var ptr = &pBuffer[Slice * z + y * Pitch + x * ElementSize];
            if (retType == typeof(float))
            {
                return *(float*)ptr;
            }
            else if (retType == typeof(Vector2))
            {
                return *(Vector2*)ptr;
            }
            else if (retType == typeof(Vector3))
            {
                return *(Vector3*)ptr;
            }
            else if(retType == typeof(DVector3))
            {
                return *(DVector3*)ptr;
            }
            else if(retType == typeof(Quaternion))
            {
                return *(Quaternion*)ptr;
            }
            return null;
        }
        [Rtti.Meta]
        public float GetFloat1(int x, int y, int z)
        {
            return GetPixel<float>(x, y, z);
        }
        public Vector3 GetGradAndHeight(int x, int y, int z, in Vector2 uv)
        {
            System.Diagnostics.Debug.Assert(this.BufferCreator.ElementType.SystemType == typeof(float));

            float h1 = GetPixel<float>(x,y,z);
            float h2 = GetPixel<float>(x + 1, y, z);
            float h3 = GetPixel<float>(x, y + 1, z);
            float h4 = GetPixel<float>(x + 1, y + 1, z);

            var result = new Vector3();
            var u = uv.U;
            var v = uv.V;
            // gradient
            result.X = (h2 - h1) * (1 - v) + (h4 - h3) * v;
            result.Y = (h3 - h1) * (1 - u) + (h4 - h2) * u;
            // height
            result.Z = h1 * (1 - u) * (1 - v) + h2 * u * (1 - v) + h3 * (1 - u) * v + h4 * u * v;
            return result;
        }
        public float GetHeight(float x, float y)
        {
            int i = (int)x;
            int j = (int)y;
            float u = x - i;
            float v = y - j;

            float h1 = GetPixel<float>(i, j, 0);
            float h2 = GetPixel<float>(i + 1, j, 0);
            float h3 = GetPixel<float>(i, j + 1, 0);
            float h4 = GetPixel<float>(i + 1, j + 1, 0);

            return h1 * (1 - u) * (1 - v) + h2 * u * (1 - v) + h3 * (1 - u) * v + h4 * u * v;
        }
        public enum EBufferSamplerType
        {
            Point,
            Linear,
            Box,
        }
        [Rtti.Meta]
        public float Sampler2DFloat1(in Vector3 uvw, EBufferSamplerType type = EBufferSamplerType.Point, EPixelAddressMode address = EPixelAddressMode.Wrap)
        {
            return Sampler2DFloat1(uvw.X, uvw.Y, uvw.Z, type, address);
        }
        [Rtti.Meta]
        public float Sampler2DFloat1(float u, float v, float w = 0, EBufferSamplerType type = EBufferSamplerType.Point, EPixelAddressMode address = EPixelAddressMode.Wrap)
        {
            switch (type)
            {
                case EBufferSamplerType.Point:
                    {
                        var uvw = new Vector3(u, v, w);
                        return GetPixel<float>(in uvw, address);
                    }
                case EBufferSamplerType.Linear:
                    {
                        var rx = MathHelper.Mod(u, UVWStep.X);
                        var ry = MathHelper.Mod(v, UVWStep.Y);
                        var rz = MathHelper.Mod(w, UVWStep.Z);
                        var bx = u - rx;
                        var by = v - ry;
                        var bz = v - rz;

                        var v00 = GetPixel<float>(new Vector3(bx, by, bz), address);
                        var v01 = GetPixel<float>(new Vector3(bx + UVWStep.X, by, bz), address);
                        var v10 = GetPixel<float>(new Vector3(bx, by + UVWStep.Y, bz), address);
                        var v11 = GetPixel<float>(new Vector3(bx + UVWStep.X, by + UVWStep.Y, bz), address);

                        var lx = rx / UVWStep.X;
                        var f1 = MathHelper.Lerp(v00, v01, lx);
                        var f2 = MathHelper.Lerp(v10, v11, lx);
                        return MathHelper.Lerp(f1, f2, ry / UVWStep.Y);
                    }
                case EBufferSamplerType.Box:
                    {
                        var rx = MathHelper.Mod(u, UVWStep.X);
                        var ry = MathHelper.Mod(v, UVWStep.Y);
                        var rz = MathHelper.Mod(w, UVWStep.Z);
                        var bx = u - rx;
                        var by = v - ry;
                        var bz = v - rz;
                        var lx = rx / UVWStep.X;
                        var ly = ry / UVWStep.Y;
                        var lz = rz / UVWStep.Z;

                        var v000 = GetPixel<float>(new Vector3(bx, by, bz), address);
                        var v010 = GetPixel<float>(new Vector3(bx + UVWStep.X, by, bz), address);
                        var v100 = GetPixel<float>(new Vector3(bx, by + UVWStep.Y, bz), address);
                        var v110 = GetPixel<float>(new Vector3(bx + UVWStep.X, by + UVWStep.Y, bz), address);
                        var f10 = MathHelper.Lerp(v000, v010, lx);
                        var f20 = MathHelper.Lerp(v100, v110, lx);
                        var z0 = MathHelper.Lerp(f10, f20, ly);

                        var v001 = GetPixel<float>(new Vector3(bx, by, bz + UVWStep.Z), address);
                        var v011 = GetPixel<float>(new Vector3(bx + UVWStep.X, by, bz + UVWStep.Z), address);
                        var v101 = GetPixel<float>(new Vector3(bx, by + UVWStep.Y, bz + UVWStep.Z), address);
                        var v111 = GetPixel<float>(new Vector3(bx + UVWStep.X, by + UVWStep.Y, bz + UVWStep.Z), address);
                        var f11 = MathHelper.Lerp(v001, v011, lx);
                        var f21 = MathHelper.Lerp(v101, v111, lx);
                        var z1 = MathHelper.Lerp(f11, f21, ly);

                        return MathHelper.Lerp(z0, z1, lz);
                    }
            }
            return 0;
        }
        [Rtti.Meta]
        public void SetFloat1(int x, int y, int z, float v)
        {
            SetPixel<float>(x, y, z, v);
        }
        [Rtti.Meta]
        public Vector2 GetFloat2(int x, int y, int z)
        {
            return GetPixel<Vector2>(x, y, z);
        }
        [Rtti.Meta]
        public void SetFloat2(int x, int y, int z, in Vector2 v)
        {
            SetPixel<Vector2>(x, y, z, in v);
        }
        [Rtti.Meta]
        public Vector3 GetFloat3(int x, int y, int z)
        {
            return GetPixel<Vector3>(x, y, z);
        }
        [Rtti.Meta]
        public void SetFloat3(int x, int y, int z, in Vector3 v)
        {
            SetPixel<Vector3>(x, y, z, in v);
        }
        [Rtti.Meta]
        public DVector3 GetDouble3(int x, int y, int z)
        {
            return GetPixel<DVector3>(x, y, z);
        }
        [Rtti.Meta]
        public void SetDouble3(int x, int y, int z, in DVector3 v)
        {
            SetPixel<DVector3>(x, y, z, in v);
        }
        [Rtti.Meta]
        public int GetInt1(int x, int y, int z)
        {
            return GetPixel<int>(x, y, z);
        }
        [Rtti.Meta]
        public void SetInt1(int x, int y, int z, int v)
        {
            SetPixel<int>(x, y, z, v);
        }
        [Rtti.Meta]
        public Vector2i GetInt2(int x, int y, int z)
        {
            return GetPixel<Vector2i>(x, y, z);
        }
        [Rtti.Meta]
        public void SetInt2(int x, int y, int z, in Vector2i v)
        {
            SetPixel<Vector2i>(x, y, z, in v);
        }
        [Rtti.Meta]
        public Vector3i GetInt3(int x, int y, int z)
        {
            return GetPixel<Vector3i>(x, y, z);
        }
        [Rtti.Meta]
        public void SetInt3(int x, int y, int z, in Vector3i v)
        {
            SetPixel<Vector3i>(x, y, z, in v);
        }
        #endregion
    }
    public class USuperBuffer<T, TOperator> : UBufferConponent where T : unmanaged 
        where TOperator : ISuperPixelOperator<T>, new()
    {
        public readonly static TOperator mOperator = new TOperator();
        public override ISuperPixelOperatorBase PixelOperator { get => mOperator; }
        public unsafe void CreateBuffer(int xSize, int ySize, int zSize, in T initValue = default(T))
        {
            fixed (T* pAddr = &initValue)
            {
                CreateBuffer(Rtti.UTypeDescGetter<T>.TypeDesc, xSize, ySize, zSize, pAddr);
            }
        }
        public unsafe ref T GetSuperPixel(int x, int y, int z)
        {
            return ref *(T*)GetSuperPixelAddress(x, y, z);
        }
        public unsafe void SetSuperPixel(int x, int y, int z, in T value)
        {
            fixed (T* pAddr = &value)
            {
                SetSuperPixelAddress(x, y, z, (byte*)pAddr);
            }
        }
        public unsafe void GetRange(out T minValue, out T maxValue)
        {
            GetRangeUnsafe<T, TOperator>(out minValue, out maxValue);
        }
        public unsafe void LoadTexture(RName name, int xyz)
        {
            using (var stream = System.IO.File.OpenRead(name.Address))
            {
                var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                CreateBuffer(image.Height, image.Width, 1);
                for (int i = 0; i < Depth; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        for (int k = 0; k < Width; k++)
                        {
                            var t = ((float)image.Data[Width * i * 4 + j * 4 + 1]) / 255.0f;
                            SetSuperPixel(k, j, i, (*(T*)&t));
                        }
                    }
                }   
            }
        }
    }

    public class UPgcBufferCache
    {
        public Dictionary<NodePin, UBufferConponent> CachedBuffers { get; } = new Dictionary<NodePin, UBufferConponent>();
        public void ResetCache()
        {
            CachedBuffers.Clear();
        }
        public UBufferConponent FindBuffer(NodePin pin)
        {
            var node = pin.HostNode as UPgcNodeBase;
            UBufferConponent buffer;
            if (CachedBuffers.TryGetValue(pin, out buffer))
                return buffer;
            var oPin = pin as PinOut;
            if (oPin != null)
            {
                UPgcGraph graph = oPin.HostNode.ParentGraph as UPgcGraph;
                var creator = node.GetOutBufferCreator(oPin).Clone();
                if (creator.XSize == -1)
                {
                    creator.XSize = graph.DefaultCreator.XSize;
                }
                if (creator.YSize == -1)
                {
                    creator.YSize = graph.DefaultCreator.YSize;
                }
                if (creator.ZSize == -1)
                {
                    creator.ZSize = graph.DefaultCreator.ZSize;
                }
                buffer = UBufferConponent.CreateInstance(in creator);
                buffer.LifeCount = pin.HostNode.ParentGraph.GetNumOfOutLinker(oPin);
                CachedBuffers.Add(pin, buffer);
                return buffer;
            }
            else
            {
                var linker = pin.HostNode.ParentGraph.FindInLinkerSingle(pin as PinIn);
                while (linker != null)
                {
                    oPin = linker.OutPin;
                    if (oPin.RefInput == null)
                        break;
                    linker = pin.HostNode.ParentGraph.FindInLinkerSingle(linker.InPin);
                }
                if (oPin != null)
                {
                    if (CachedBuffers.TryGetValue(oPin, out buffer))
                        return buffer;
                }
            }
            return null;
        }
        public UBufferConponent RegBuffer(PinOut pin, UBufferConponent buffer)
        {
            UBufferConponent result = null;
            if (CachedBuffers.TryGetValue(pin, out result))
                return result;
            buffer.LifeCount = pin.HostNode.ParentGraph.GetNumOfOutLinker(pin);
            CachedBuffers[pin] = buffer;
            return buffer;
        }
    }
}
