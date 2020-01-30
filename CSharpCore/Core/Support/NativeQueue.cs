using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Support
{
    internal class NativeQueueImpl : AuxCoreObject<NativeQueueImpl.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public NativeQueueImpl(UInt32 stride)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("CsQueue");
            SDK_CsQueue_SetStride(CoreObject, stride);
        }

        public UInt32 Stride
        {
            get
            {
                return SDK_CsQueue_GetStride(CoreObject);
            }
        }

        public int Count
        {
            get
            {
                return (int)SDK_CsQueue_GetCount(CoreObject);
            }
        }
        public unsafe void Enqueue(byte* ptr)
        {
            SDK_CsQueue_Enqueue(CoreObject, ptr);
        }
        public void Dequeue()
        {
            SDK_CsQueue_Dequeue(CoreObject);
        }
        public void Clear()
        {
            SDK_CsQueue_Clear(CoreObject);
        }
        public unsafe bool Peek(byte* p)
        {
            return SDK_CsQueue_Peek(CoreObject, p);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsQueue_GetStride(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsQueue_SetStride(NativePointer self, UInt32 stride);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsQueue_GetCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_CsQueue_Enqueue(NativePointer self, byte* p);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsQueue_Dequeue(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsQueue_Clear(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_CsQueue_Peek(NativePointer self, byte* p);
        #endregion
    }

    public class NativeQueue<T> where T : unmanaged
    {
        private NativeQueueImpl mImpl;
        private static int mStructSize;
        static NativeQueue()
        {
            unsafe
            {
                mStructSize = sizeof(T);
            }
        }
        public NativeQueue()
        {
            mImpl = new NativeQueueImpl((UInt32)mStructSize);
        }
        public int Count
        {
            get { return mImpl.Count; }
        }
        public int Stride
        {
            get { return mStructSize; }
        }
        public void Clear()
        {
            lock (this)
            {
                mImpl.Clear();
            }
        }
        public void Enqueue(T obj)
        {
            lock (this)
            {
                unsafe
                {
                    mImpl.Enqueue((byte*)&obj);
                }
            }
        }
        public T Peek()
        {
            lock (this)
            {
                T result = new T();
                unsafe
                {
                    mImpl.Peek((byte*)&result);
                }
                return result;
            }
        }
        public T Dequeue()
        {
            lock (this)
            {
                T result = new T();
                unsafe
                {
                    mImpl.Peek((byte*)&result);
                    mImpl.Dequeue();
                }
                return result;
            }
        }
    }
}
