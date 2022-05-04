using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    public interface ISuperPixelOperatorBase
    {
        Rtti.UTypeDesc ElementType { get; }
        Rtti.UTypeDesc BufferType { get; }
        unsafe void SetAsMaxValue(void* tar);
        unsafe void SetAsMinValue(void* tar);
        unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
    }

    public interface ISuperPixelOperator<T> : ISuperPixelOperatorBase where T : unmanaged
    {
        T MaxValue { get; }
        T MinValue { get; }
        int Compare(in T left, in T right);
        T Add(in T left, in T right);
    }
    public struct FFloatOperator : ISuperPixelOperator<float>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<float>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<float,FFloatOperator>>.TypeDesc;
            }
        }
        public float MaxValue { get => float.MaxValue; }
        public float MinValue { get => float.MinValue; }
        public int Compare(in float left, in float right)
        {
            return left.CompareTo(right);
        }
        public float Add(in float left, in float right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(float*)tar) = float.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(float*)tar) = float.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(float*)tar) = (*(float*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) * rValue;
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) / rValue;
        }
    }
    public struct FFloat3Operator : ISuperPixelOperator<Vector3>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Vector3>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Vector3, FFloat3Operator>>.TypeDesc;
            }
        }
        public Vector3 MaxValue { get => Vector3.MaxValue; }
        public Vector3 MinValue { get => Vector3.MinValue; }
        public int Compare(in Vector3 left, in Vector3 right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public Vector3 Add(in Vector3 left, in Vector3 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Vector3*)tar) = Vector3.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Vector3*)tar) = Vector3.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Vector3*)tar) = (*(Vector3*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.One;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) * rValue;
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.One;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) / rValue;
        }
    }
    public class UBufferConponent
    {
        public UBufferCreator BufferCreator { get; private set; }
        public virtual ISuperPixelOperatorBase PixelOperator { get => null; }
        public int LifeCount { get; set; } = 0;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }
        public int Pitch { get; private set; }
        public int Slice { get; private set; }
        public int ElementSize { get; private set; }
        public BigStackBuffer SuperPixels;
        protected UBufferConponent()
        {

        }
        ~UBufferConponent()
        {
            SuperPixels.Dispose();
        }
        #region CppMemBuffer
        public unsafe static UBufferConponent CreateInstance(in UBufferCreator creator)
        {
            var result = Rtti.UTypeDescManager.CreateInstance(creator.BufferType) as UBufferConponent;
            result.BufferCreator = creator.Clone();
            var elemSize = System.Runtime.InteropServices.Marshal.SizeOf(result.PixelOperator.ElementType.SystemType); 
            result.CreateBuffer(elemSize, creator.XSize, creator.YSize, creator.ZSize, IntPtr.Zero.ToPointer());
            return result;
        }
        public unsafe void CreateBuffer(int elementSize, int xSize, int ySize, int zSize, void* initValue)
        {
            Width = xSize;
            Height = ySize;
            Depth = zSize;

            ElementSize = elementSize;
            SuperPixels.Dispose();
            Pitch = xSize * ElementSize;
            Slice = Pitch * ySize;
            SuperPixels = BigStackBuffer.CreateInstance(Slice * zSize);
            if (initValue != IntPtr.Zero.ToPointer())
            {
                var pBuffer = (byte*)SuperPixels.GetBuffer();
                for (int i = 0; i < xSize * ySize * zSize; i++)
                {
                    CoreSDK.MemoryCopy(&pBuffer[i * ElementSize], initValue, (uint)ElementSize);
                }
            }
        }
        public unsafe byte* GetSuperPixelAddress(int x, int y, int z)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return null;
            var pBuffer = (byte*)SuperPixels.GetBuffer();
            //return &pBuffer[(z * (Width * Depth) + Width * y + x) * ElementSize];
            return &pBuffer[Slice * z + y * Pitch + x * ElementSize];
        }
        public unsafe void SetSuperPixelAddress(int x, int y, int z, void* valueAddress)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return;
            var pBuffer = (byte*)SuperPixels.GetBuffer();
            CoreSDK.MemoryCopy(&pBuffer[Slice * z + y * Pitch + x * ElementSize], valueAddress, (uint)ElementSize);
        }
        public unsafe byte* GetSliceAddress(int index)
        {
            return GetSuperPixelAddress(0, 0, index);
        }
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
        #endregion

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
                fixed (T* pAddr = &value)
                {
                    SetSuperPixelAddress(x, 0, 0, (byte*)pAddr);
                }
            }
        }

        public void GetRangeUnsafe<T, TOperator>(out T minValue, out T maxValue) 
            where T : unmanaged
            where TOperator : ISuperPixelOperator<T>, new()
        {
            var tOp = new TOperator();
            minValue = tOp.MaxValue;
            maxValue = tOp.MinValue;
            for (int i = 0; i < Depth; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    for (int k = 0; k < Width; k++)
                    {
                        ref T pixl = ref GetPixel<T>(k, j, i);
                        if (tOp.Compare(pixl, maxValue) > 0)
                            maxValue = pixl;
                        if (tOp.Compare(pixl, minValue) < 0)
                            minValue = pixl;
                    }
                }
            }
        }

        public virtual RHI.CShaderResourceView CreateAsHeightMapTexture2D(float minHeight, float maxHeight, EPixelFormat format = EPixelFormat.PXF_R32_FLOAT, bool bNomalized = false)
        {
            return null;
        }
        //public void GetMaxAbs(out float maxValue)
        //{
        //    maxValue = float.MinValue;
        //    for (int i = 0; i < Pixels.Length; i++)
        //    {
        //        var f = Pixels[i];
        //        if (Math.Abs(f) > maxValue)
        //            maxValue = f;
        //    }
        //}
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
                CreateBuffer(sizeof(T), xSize, ySize, zSize, pAddr);
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
        public void GetRange(out T minValue, out T maxValue)
        {
            minValue = mOperator.MaxValue;
            maxValue = mOperator.MinValue;
            for (int i = 0; i < Depth; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    for (int k = 0; k < Width; k++)
                    {
                        ref T pixl = ref GetSuperPixel(k, j, i);
                        if (mOperator.Compare(pixl, maxValue) > 0)
                            maxValue = pixl;
                        if (mOperator.Compare(pixl, minValue) < 0)
                            minValue = pixl;
                    }
                }
            }
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
                            var t = ((float)image.Data[Width * i * 4 + j * 4 + 1]) / 256.0f;
                            SetSuperPixel(k, j, i, (*(T*)&t));
                        }
                    }
                }   
            }
        }
        public override unsafe RHI.CShaderResourceView CreateAsHeightMapTexture2D(float minHeight, float maxHeight, EPixelFormat format = EPixelFormat.PXF_R32_FLOAT, bool bNomalized = false)
        {
            RHI.CTexture2D texture;
            if (minHeight >= 0 && minHeight <= 1 && maxHeight >= 0 && maxHeight <= 1)
            {
                minHeight = 0;
                maxHeight = 1;
            }
            float hRange = maxHeight - minHeight;
            unsafe
            {
                var desc = new ITexture2DDesc();
                desc.SetDefault();
                desc.Width = (uint)Width;
                desc.Height = (uint)Height;
                desc.MipLevels = 1;
                desc.Format = format;
                ImageInitData initData = new ImageInitData();
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
                                initData.SysMemPitch = desc.Width * sizeof(float);
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R16_FLOAT:
                        {//高度信息有2的11次方级别精度高度

                            var Count = Width * Height;
                            var tarPixels = new Half[Count];
                            var pSlice = (float*)this.GetSliceAddress(0);
                            for (int i = 0; i < Count; i++)
                            {
                                //tarPixels[i] = HalfHelper.SingleToHalf(Pixels[i]);
                                if (bNomalized)
                                {
                                    tarPixels[i] = HalfHelper.SingleToHalf((pSlice[i] - minHeight) / hRange);
                                }
                                else
                                {
                                    tarPixels[i] = HalfHelper.SingleToHalf(pSlice[i] - minHeight);
                                }
                            }
                            fixed (Half* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(Half));
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    case EPixelFormat.PXF_R8G8_UNORM:
                        {//如果希望表达更高的精度，而不是浪费在half上的指数位，可以RG8UNorm格式，让高度信息有65535级别
                            var Count = Width * Height;
                            var tarPixels = new UInt8_2[Width * Height];
                            var pSlice = (float*)this.GetSliceAddress(0);
                            float range = maxHeight - minHeight;
                            for (int i = 0; i < Count; i++)
                            {
                                float alt = pSlice[i] - minHeight;
                                float rate = alt / range;
                                ushort value = (ushort)(rate * (float)ushort.MaxValue);
                                tarPixels[i].x = (byte)(value & 0xFF);
                                tarPixels[i].y = (byte)((value >> 8) & 0xFF);
                            }
                            fixed (UInt8_2* p = &tarPixels[0])
                            {
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(UInt8_2));
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
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
                                initData.SysMemPitch = (uint)(desc.Width * sizeof(Byte));
                                initData.pSysMem = p;
                                texture = UEngine.Instance.GfxDevice.RenderContext.CreateTexture2D(in desc);
                            }
                        }
                        break;
                    default:
                        return null;
                }


                var rsvDesc = new IShaderResourceViewDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = ESrvType.ST_Texture2D;
                rsvDesc.mGpuBuffer = texture.mCoreObject;
                rsvDesc.Format = format;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                var result = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in rsvDesc);
                return result;
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
                if (linker != null)
                {
                    if (CachedBuffers.TryGetValue(linker.OutPin, out buffer))
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
