using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CoreDefine
    {
        public const float Epsilon = 0.00001f;
    }

    public enum EColorSpace : UInt32
    {
        COLOR_SPACE_SRGB_NONLINEAR = 0,
        COLOR_SPACE_EXTENDED_SRGB_LINEAR,
    }
    public enum EPixelFormat : UInt32
    {
        PXF_UNKNOWN = 0,
        PXF_R16_FLOAT = 1,
        PXF_R16_UINT = 2,
        PXF_R16_SINT = 3,
        PXF_R16_UNORM = 4,
        PXF_R16_SNORM = 5,
        PXF_R32_UINT = 6,
        PXF_R32_SINT = 7,
        PXF_R32_FLOAT = 8,
        PXF_R8G8B8A8_SINT = 9,
        PXF_R8G8B8A8_UINT = 10,
        PXF_R8G8B8A8_UNORM = 11,
        PXF_R8G8B8A8_SNORM = 12,
        PXF_R16G16_UINT = 13,
        PXF_R16G16_SINT = 14,
        PXF_R16G16_FLOAT = 15,
        PXF_R16G16_UNORM = 16,
        PXF_R16G16_SNORM = 17,
        PXF_R16G16B16A16_UINT = 18,
        PXF_R16G16B16A16_SINT = 19,
        PXF_R16G16B16A16_FLOAT = 20,
        PXF_R16G16B16A16_UNORM = 21,
        PXF_R16G16B16A16_SNORM = 22,
        PXF_R32G32B32A32_UINT = 23,
        PXF_R32G32B32A32_SINT = 24,
        PXF_R32G32B32A32_FLOAT = 25,
        PXF_R32G32B32_UINT = 26,
        PXF_R32G32B32_SINT = 27,
        PXF_R32G32B32_FLOAT = 28,
        PXF_R32G32_UINT = 29,
        PXF_R32G32_SINT = 30,
        PXF_R32G32_FLOAT = 31,
        PXF_D24_UNORM_S8_UINT = 32,
        PXF_D32_FLOAT = 33,
        PXF_D32_FLOAT_S8X24_UINT = 34,
        PXF_D16_UNORM = 35,
        PXF_B8G8R8A8_UNORM = 36,
        PXF_R11G11B10_FLOAT = 37,
        PXF_R8G8_UNORM = 38,
        PXF_R8_UNORM = 39,
        PXF_R32_TYPELESS = 40,
        PXF_R32G32B32A32_TYPELESS = 41,
        PXF_R32G32B32_TYPELESS,
        PXF_R16G16B16A16_TYPELESS,
        PXF_R32G32_TYPELESS,
        PXF_R32G8X24_TYPELESS,
        PXF_R10G10B10A2_TYPELESS,
        PXF_R10G10B10A2_UNORM,
        PXF_R10G10B10A2_UINT,
        PXF_R8G8B8A8_TYPELESS,
        PXF_R8G8B8A8_UNORM_SRGB,
        PXF_R16G16_TYPELESS,
        PXF_R24G8_TYPELESS,
        PXF_R24_UNORM_X8_TYPELESS,
        PXF_X24_TYPELESS_G8_UINT,
        PXF_R8G8_TYPELESS,
        PXF_R8G8_UINT,
        PXF_R8G8_SNORM,
        PXF_R8G8_SINT,
        PXF_R16_TYPELESS,
        PXF_R8_TYPELESS,
        PXF_R8_UINT,
        PXF_R8_SNORM,
        PXF_R8_SINT,
        PXF_A8_UNORM,
        PXF_B8G8R8X8_UNORM,
        PXF_B8G8R8A8_TYPELESS,
        PXF_B8G8R8A8_UNORM_SRGB,
        PXF_B8G8R8X8_TYPELESS,
        PXF_B8G8R8X8_UNORM_SRGB,
        PXF_B5G6R5_UNORM,
        PXF_B4G4R4A4_UNORM,
    };


    public enum EBindFlags : UInt32
    {
        BF_VB = 0x1,
        BF_IB = 0x2,
        BF_CB = 0x4,
        BF_SHADER_RES = 0x8,
        BF_STREAM_OUTPUT = 0x10,
        BF_RENDER_TARGET = 0x20,
        BF_DEPTH_STENCIL = 0x40,
        BF_UNORDERED_ACCESS = 0x80
    };
    public enum ECSType
    {
        /// <summary>
        /// 服务器客户端共用
        /// </summary>
        Common,
        /// <summary>
        /// 服务器
        /// </summary>
        Server,
        /// <summary>
        /// 客户端
        /// </summary>
        Client,
        /// <summary>
        /// 所有，编辑器使用，正常游戏中类型使用common标识服务器客户端共用，不要使用All
        /// </summary>
        All,
    }

    public enum EStreamingState
    {
        SS_Unknown, //未知
        SS_WaitDownload,    //等待下载
        SS_Downloading, //正在下载
        SS_DLFailed,    //下载失败
        SS_Invalid, // 非法
        SS_Pending, // 即将读取
        SS_Streaming,   // 读取中
                        //SS_Streamed		,	// 已经读取，我们磁盘->内存，和构造没有分开，这个没有必要了
        SS_Valid,   // 可用
        SS_PendingKill, // 即将删除
        SS_Killing, // 销毁中
        SS_Killed           // 删除完毕
    }

    public class ConstCharPtrMarshaler : ICustomMarshaler
    {
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return Marshal.PtrToStringAnsi(pNativeData);
        }
        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            return IntPtr.Zero;
        }
        public void CleanUpNativeData(IntPtr pNativeData)
        {
        }
        public void CleanUpManagedData(object ManagedObj)
        {
        }
        public int GetNativeDataSize()
        {
            return IntPtr.Size;
        }
        static readonly ConstCharPtrMarshaler instance = new ConstCharPtrMarshaler();
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return instance;
        }
    }

    public class TextMarshaler : ICustomMarshaler
    {
        Encoding mEncoding;
        public TextMarshaler()
        {
            mEncoding = System.Text.Encoding.GetEncoding("gb2312");
        }
        public void CleanUpManagedData(object managedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeHGlobal(pNativeData);
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            if (object.ReferenceEquals(managedObj, null))
                return IntPtr.Zero;
            if (!(managedObj is string))
                throw new InvalidOperationException();

            byte[] asciibytes = mEncoding.GetBytes(managedObj as string);
            IntPtr ptr = Marshal.AllocHGlobal(asciibytes.Length + 1);
            Marshal.Copy(asciibytes, 0, ptr, asciibytes.Length);
            Marshal.WriteByte(ptr, asciibytes.Length, 0);
            return ptr;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;

            List<byte> bytes = new List<byte>();
            for (int offset = 0; ; offset++)
            {
                byte b = Marshal.ReadByte(pNativeData, offset);
                if (b == 0)
                    break;
                else
                    bytes.Add(b);
            }
            return mEncoding.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        private static TextMarshaler instance = new TextMarshaler();
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return instance;
        }
    }

    public struct vBOOL
    {
        public int Value;
        public static vBOOL FromBoolean(bool v)
        {
            vBOOL result;
            result.Value = v?1:0;
            return result;
        }
        public static implicit operator bool(vBOOL v)
        {
            return v.Value == 0 ? false : true;
        }
        public static bool operator ==(bool lh, vBOOL v)
        {
            return (v.Value == 0 ? false : true)==lh;
        }
        public static bool operator !=(bool lh, vBOOL v)
        {
            return (v.Value == 0 ? false : true) != lh;
        }
        public static bool operator ==(vBOOL lh, bool rh)
        {
            return (lh.Value == 0 ? false : true) == rh;
        }
        public static bool operator !=(vBOOL lh, bool rh)
        {
            return (lh.Value == 0 ? false : true) != rh;
        }
        public override bool Equals(object obj)
        {
            return ((vBOOL)obj).Value == Value;
        }
        public override int GetHashCode()
        {
            return Value;
        }
    }

    public partial struct CoreSDK
    {
        public const string ModuleNC1 = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC1, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_Memory_Copy(void* dest, void* src, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC1, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_Memory_Cmp(void* dest, void* src, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC1, CallingConvention = CallingConvention.Cdecl)]
        public extern static unsafe int SDK_Compress_RLE(byte* src, UInt32 in_length_by_count, byte* _out);
        [System.Runtime.InteropServices.DllImport(ModuleNC1, CallingConvention = CallingConvention.Cdecl)]
        public extern static unsafe int SDK_UnCompress_RLE(byte* src, UInt32 in_length_by_count, byte* _out);
        [System.Runtime.InteropServices.DllImport(ModuleNC1, CallingConvention = CallingConvention.Cdecl)]
        public extern static unsafe void SDK_ByteArray_Sub(byte* lh, byte* rh, byte* pOut, int length);
        [System.Runtime.InteropServices.DllImport(ModuleNC1, CallingConvention = CallingConvention.Cdecl)]
        public extern static unsafe void SDK_ByteArray_Add(byte* lh, byte* rh, byte* pOut, int length);
    }
}
