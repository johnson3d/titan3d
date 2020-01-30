using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace EngineNS.Support
{
    public class BitSet
    {
        UInt32 mBitCount;
        public UInt32 BitCount
        {
            get { return mBitCount; }
        }
        System.Byte[] mData;

        public System.Byte[] Data
        {
            get { return mData; }
        }

        public BitSet()
        {

        }
        public BitSet(UInt32 bitCount)
        {
            Init(bitCount);
        }
        ~BitSet()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            if(mData != null)
                mData = null;
            mBitCount = 0;
        }

        public void Init(UInt32 bitCount)
        {
            Cleanup();
            var buffSize = bitCount/8 + 1;
            mData = new byte[buffSize];
            mBitCount = bitCount;
        }
        public bool Init(UInt32 bitCount, System.Byte[] data)
        {
            Cleanup();
            var buffSize = bitCount / 8 + 1;
            if (data.Length < buffSize)
                return false;
            mData = data;
            mBitCount = bitCount;
            return true;
        }
        public int FlagNumber
        {
            get
            {
                int num = 0;
                for(int i=0; i<mBitCount; i++)
                {
                    if (IsBit(i))
                        num++;
                }
                return num;
            }
        }
        public static BitSet BitOr(BitSet lh, BitSet rh, ref int flagNum)
        {
            if (lh.BitCount != rh.BitCount)
                return null;
            BitSet tar = new BitSet();
            tar.Init(lh.BitCount);
            flagNum = 0;
            for (int i = 0; i < lh.mBitCount; i++)
            {
                if (lh.IsBit(i) || rh.IsBit(i))
                {
                    tar.SetBit(i, true);
                    flagNum++;
                }
            }
            return tar;
        }
        public void SetBit(int iBit, bool value)
        {
            if (iBit < 0)
                return;
            SetBit((UInt32)iBit, value);
        }
        public void SetBit(UInt32 iBit, bool value)
        {
            if (iBit < 0 || iBit >= mBitCount)
            {
                //Log.FileLog.WriteLine("Error! SetBit {0}/{1}", iBit, mBitCount);
                System.Diagnostics.Trace.WriteLine(string.Format("Error! SetBit {0}/{1}", iBit, mBitCount));

                System.Diagnostics.Trace.WriteLine(new System.Diagnostics.StackFrame());
                return; 
            }

            var master = iBit/8;
            int slave = (int)iBit%8;
            if(value)
                mData[master] = (byte)(mData[master] | (1<<slave));
            else
                mData[master] = (byte)(mData[master] & (~(1<<slave)));
        }
        public bool IsBit(int iBit)
        {
            if (iBit < 0)
                return false;
            return IsBit((UInt32)iBit);
        }
        public bool IsBit(UInt32 iBit)
        {
            if (iBit < 0 || iBit >= mBitCount)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "", "Error! IsBit {0}/{1}", iBit, mBitCount);
                return true;
            }

            UInt32 master = iBit/8;
            int slave = (int)(iBit%8);

            return ((mData[master] & (1<<slave)) != 0) ? true : false;
        }
        public string ToBase64String()
        {
            return System.Convert.ToBase64String(mData);
        }

        public bool FromBase64String(string str, UInt32 bitCount)
        {
            var buffSize = bitCount / 8 + 1;
            byte[] temp = System.Convert.FromBase64String(str);
            if (buffSize < temp.Length)
                return false;

            mData = new byte[buffSize];
            temp.CopyTo(mData, 0);
            mBitCount = bitCount;
            return true;
        }

        public void FromBinary(byte[] bitData, UInt32 bitCount)
        {
            if (bitData.Length * 8 < bitCount)
            {
                Init(bitCount);
                bitData.CopyTo(mData, 0);
            }
            else
            {
                mData = bitData;                
            }
            mBitCount = bitCount;
        }

        public void Resize(UInt32 newBitCount)
        {
            var tempData = mData;
            var buffSize = newBitCount / 8 + 1;
            var minCount = System.Math.Min(mData.Length, buffSize);
            mData = new byte[buffSize];
            Array.Copy(tempData, mData, minCount);
            mBitCount = newBitCount;
        }
        // 查找第一个值是bitValue的bit
        public UInt32 FindFirstBit(bool bitValue)
        {
            if(bitValue)
            {
                for(UInt32 i=0; i<Data.Length; i++)
                {
                    if(Data[i] != 0)
                    {
                        for(int idx = 0; idx < 8; idx++)
                        {
                            if ((mData[i] & (1<<idx)) != 0)
                                return (UInt32)(i * 8 + idx);
                        }
                    }
                }
            }
            else
            {
                for(UInt32 i=0; i<Data.Length; i++)
                {
                    if(Data[i] != 0xFF)
                    {
                        for(int idx = 0; idx < 8; idx++)
                        {
                            if ((mData[i] & (1<<idx)) == 0)
                                return (UInt32)(i * 8 + idx);
                        }
                    }
                }
            }

            return UInt32.MaxValue;
        }
        public bool IsSame(BitSet bs)
        {
            if (bs.BitCount != BitCount)
                return false;

            UInt32 byteNum = BitCount / 8;
            for(UInt32 i=0; i< byteNum; i++)
            {
                if (Data[i] != bs.Data[i])
                    return false;
            }
            UInt32 remain = BitCount % 8;
            for (int i = 0; i < remain; i++)
            {
                var v1 = Data[byteNum] & (1 << i);
                var v2 = bs.Data[byteNum] & (1 << i);
                if (v1 != v2)
                    return false;
            }

            return true;
        }
    }

    public class BitSetSerializer : IO.Serializer.FieldSerializer
    {
        public static System.Type SerializeType
        {
            get;
        } = typeof(BitSet);
        public override object ReadValue(IO.Serializer.IReader pkg)
        {
            BitSet v;
            pkg.Read(out v);
            return v;
        }
        public override void WriteValue(object obj, IO.Serializer.IWriter pkg)
        {
            var v = obj as BitSet;
            pkg.Write(v);
        }
        public override void ReadValueList(System.Collections.IList obj, IO.Serializer.IReader pkg)
        {
            var lst = (obj);
            UInt16 count;
            unsafe
            {
                pkg.ReadPtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    BitSet v;
                    pkg.Read(out v);
                    lst.Add(v);
                }
            }
        }
        public override void WriteValueList(System.Collections.IList obj, IO.Serializer.IWriter pkg)
        {
            var lst = obj;
            unsafe
            {
                UInt16 count = 0;
                if (lst != null)
                    count = (UInt16)lst.Count;
                pkg.WritePtr(&count, sizeof(UInt16));
                for (UInt16 i = 0; i < count; i++)
                {
                    var v = lst[i] as BitSet;
                    pkg.Write(v);
                }
            }
        }
        public override string ObjectToString(Rtti.MemberDesc p, object o)
        {
            var v = o as BitSet;
            return v.ToBase64String() + ':' + v.BitCount;
        }
        public override object ObjectFromString(Rtti.MemberDesc p, string str)
        {
            var v = new BitSet();
            var segs = str.Split(':');
            v.FromBase64String(segs[0], System.Convert.ToUInt32(segs[1]));
            return v;
        }
    }

    public class CBitSet
    {
        private struct NativePointer : INativePointer
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
        NativePointer CoreObject;
        public CBitSet()
        {
            CoreObject = SDK_vBitset_New();
        }
        ~CBitSet()
        {
            if (CoreObject.GetPointer() != IntPtr.Zero)
            {
                SDK_vBitset_Delete(CoreObject);
                CoreObject.SetPointer(IntPtr.Zero);
            }
        }
        public void CopyFrom(CBitSet src)
        {
            SDK_vBitset_Copy(CoreObject, src.CoreObject);
        }
        public void LeftShift(UInt32 num)
        {
            SDK_vBitset_LeftShift(CoreObject, num);
        }
        public void RightShift(UInt32 num)
        {
            SDK_vBitset_RightShift(CoreObject, num);
        }
        public void And(CBitSet right)
        {
            SDK_vBitset_And(CoreObject, right.CoreObject);
        }
        public void Or(CBitSet right)
        {
            SDK_vBitset_Or(CoreObject, right.CoreObject);
        }
        public void ExclusiveOr(CBitSet right)
        {
            SDK_vBitset_ExclusiveOr(CoreObject, right.CoreObject);
        }
        public void Not()
        {
            SDK_vBitset_Not(CoreObject);
        }
        public void Save(IO.XndAttrib pAttr)
        {
            SDK_vBitset_Save(CoreObject, pAttr.CoreObject);
        }
        public void Load(IO.XndAttrib pAttr)
        {
            SDK_vBitset_Load(CoreObject, pAttr.CoreObject);
        }
        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static NativePointer SDK_vBitset_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_Delete(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_Copy(NativePointer self, NativePointer src);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_LeftShift(NativePointer self, UInt32 num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_RightShift(NativePointer self, UInt32 num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_And(NativePointer self, NativePointer right);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_Or(NativePointer self, NativePointer right);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_ExclusiveOr(NativePointer self, NativePointer right);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_Not(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_vBitset_Save(NativePointer self, IO.XndAttrib.NativePointer pAttr);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static vBOOL SDK_vBitset_Load(NativePointer self, IO.XndAttrib.NativePointer pAttr);
        #endregion
    }
}
