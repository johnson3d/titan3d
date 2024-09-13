using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class UniHash32
    {
        public static uint DefaultHash(string str)
        {
            return JSHash(str);
        }
        public static uint XXHash(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return Standart.Hash.xxHash.xxHash32.ComputeHash(data, data.Length);
        }
        public static uint RSHash(string str)
        {
            uint b = 378551;
            uint a = 63689;
            uint hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = hash * a + str[i];
                a = a * b;
            }

            return hash;
        }
        public static uint JSHash(string str)
        {
            uint hash = 1315423911;

            for (int i = 0; i < str.Length; i++)
            {
                hash ^= ((hash << 5) + str[i] + (hash >> 2));
            }

            return hash;
        }
        public static uint ELFHash(string str)
        {
            uint hash = 0;
            uint x = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = (hash << 4) + str[i];

                if ((x = hash & 0xF0000000) != 0)
                {
                    hash ^= (x >> 24);
                }
                hash &= ~x;
            }
            return hash;
        }
        public static uint BKDRHash(string str)
        {
            uint seed = 131; // 31 131 1313 13131 131313 etc..   
            uint hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = (hash * seed) + str[i];
            }

            return hash;
        }
        /* End Of BKDR Hash Function */
        public static uint SDBMHash(string str)
        {
            uint hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = str[i] + (hash << 6) + (hash << 16) - hash;
            }

            return hash;
        }
        /* End Of SDBM Hash Function */
        public static uint DJBHash(string str)
        {
            uint hash = 5381;

            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) + hash) + str[i];
            }

            return hash;
        }
        /* End Of DJB Hash Function */
        public static uint DEKHash(string str)
        {
            int hash = str.Length;

            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) ^ (hash >> 27)) ^ str[i];
            }

            return (uint)hash;
        }
        /* End Of DEK Hash Function */
        public static uint BPHash(string str)
        {
            uint hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = hash << 7 ^ str[i];
            }

            return hash;
        }
        /* End Of BP Hash Function */
        public static uint FNVHash(string str)
        {
            uint fnv_prime = 0x811C9DC5;
            uint hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash *= fnv_prime;
                hash ^= str[i];
            }

            return hash;
        }
        /* End Of FNV Hash Function */
        public static uint APHash(string str)
        {
            uint hash = 0xAAAAAAAA;

            for (int i = 0; i < str.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    hash ^= ((hash << 7) ^ str[i] * (hash >> 3));
                }
                else
                {
                    hash ^= (~((hash << 11) + str[i] ^ (hash >> 5)));
                }
            }

            return hash;
        }
    }

    public class UniHash64
    {
        public static ulong XXHash(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return Standart.Hash.xxHash.xxHash64.ComputeHash(data, data.Length);
        }
    }
    //public class UniHash128
    //{
    //    public static System.UInt128 XXHash(string str)
    //    {
    //        byte[] data = Encoding.UTF8.GetBytes(str);
    //        return Standart.Hash.xxHash.xxHash128.ComputeHash(data, data.Length);
    //    }
    //}

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Hash64 : IComparable<Hash64>
    {
        [FieldOffset(0)]
        public unsafe fixed byte Data[8];
        [FieldOffset(0)]
        public UInt64 AllData;
        [FieldOffset(0)]
        public byte Data0;
        [FieldOffset(1)]
        public byte Data1;
        [FieldOffset(2)]
        public byte Data2;
        [FieldOffset(3)]
        public byte Data3;
        [FieldOffset(4)]
        public byte Data4;
        [FieldOffset(5)]
        public byte Data5;
        [FieldOffset(6)]
        public byte Data6;
        [FieldOffset(7)]
        public byte Data7;
        
        private static int CmpImpl(ref Hash64 lh, ref Hash64 rh)
        {
            if (lh.AllData > rh.AllData)
                return 1;
            else if (lh.AllData < rh.AllData)
                return 1;
            return 0;
        }
        public int CompareTo(Hash64 other)
        {
            return CmpImpl(ref this, ref other);
        }
        public static Hash64 TryParse(string str)
        {
            var segs = str.Split('_');
            if (segs.Length != 8)
                return Hash64.Empty;
            Hash64 result = new Hash64();
            result.Data0 = System.Convert.ToByte(segs[0], 16);
            result.Data1 = System.Convert.ToByte(segs[1], 16);
            result.Data2 = System.Convert.ToByte(segs[2], 16);
            result.Data3 = System.Convert.ToByte(segs[3], 16);
            result.Data4 = System.Convert.ToByte(segs[4], 16);
            result.Data5 = System.Convert.ToByte(segs[5], 16);
            result.Data6 = System.Convert.ToByte(segs[6], 16);
            result.Data7 = System.Convert.ToByte(segs[7], 16);
            return result;
        }
        public override string ToString()
        {
            return $"{Data0:X2}_{Data1:X2}_{Data2:X2}_{Data3:X2}_{Data4:X2}_{Data5:X2}_{Data6:X2}_{Data7:X2}";
        }
        public void PushTo(List<byte> data)
        {
            unsafe
            {
                fixed (byte* p = &Data0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        data.Add(p[i]);
                    }
                }
            }
        }
        public static void PushTo(List<byte> data, string str)
        {
            unsafe
            {
                var btArray = System.Text.Encoding.ASCII.GetBytes(str);
                for (int i = 0; i < btArray.Length; i++)
                {
                    data.Add(btArray[i]);
                }
            }
        }
        public static Hash64 FromString(string source)
        {
            Hash64 result = new Hash64();
            CalcHash64(ref result, source);
            return result;
        }
        public static unsafe Hash64 FromData(byte* pData, int size)
        {
            Hash64 result = new Hash64();
            CalcHash64(in result, pData, size);
            return result;
        }
        public static void CalcHash64(ref Hash64 hash, string source)
        {
            CalcHash64(ref hash, System.Text.Encoding.ASCII.GetBytes(source));
        }
        public unsafe static void CalcHash64(ref Hash64 hash, byte[] source)
        {
            if (source.Length == 0)
            {
                fixed (Hash64* p = &hash)
                {
                    SDK_HashHelper_CalcHash64(p, null, 0);
                }
            }
            else
            {
                fixed (Hash64* p = &hash)
                fixed (byte* pSource = &source[0])
                {
                    SDK_HashHelper_CalcHash64(p, pSource, source.Length);
                }
            }
        }
        public static unsafe void CalcHash64(in Hash64 hash, byte* source, int size)
        {
            fixed (Hash64* p = &hash)
            {
                SDK_HashHelper_CalcHash64(p, source, size);
            }
        }
        public static Hash64 Empty = new Hash64();
        public static bool operator ==(Hash64 hash1, Hash64 hash2)
        {
            return hash1.AllData == hash2.AllData;
        }
        public static bool operator !=(Hash64 hash1, Hash64 hash2)
        {
            return hash1.AllData != hash2.AllData;
        }
        public override int GetHashCode()
        {
            uint hash = 0xAAAAAAAA;

            unsafe
            {
                fixed (byte* ptr = &Data0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if ((i & 1) == 0)
                        {
                            hash ^= ((hash << 7) ^ ptr[i] * (hash >> 3));
                        }
                        else
                        {
                            hash ^= (~((hash << 11) + ptr[i] ^ (hash >> 5)));
                        }
                    }
                }
            }

            return (int)hash;
            //return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return this == (Hash64)obj;
        }

        public static unsafe void CalcHash64(Hash64* hash, byte* key, int len)
        {
            unsafe
            {
                SDK_HashHelper_CalcHash64(hash, key, len);
            }
        }

        public class EqualityComparer : IEqualityComparer<Hash64>
        {
            public bool Equals(Hash64 x, Hash64 y)
            {
                return x == y;
            }

            public int GetHashCode(Hash64 obj)
            {
                return obj.GetHashCode();
            }
        }
        #region SDK
        public const string ModuleNC = CoreSDK.CoreModule;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe void SDK_HashHelper_CalcHash64(Hash64* hash, byte* key, int len);
        #endregion
    }

    public interface IHash64
    {
        Hash64 GetHash64();
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public unsafe struct Hash160 : IComparable<Hash160>
    {
        [FieldOffset(0)]
        public fixed byte Data[20];
        [FieldOffset(0)]
        private uint mU0;
        [FieldOffset(4)]
        private uint mU1;
        [FieldOffset(8)]
        private uint mU2;
        [FieldOffset(12)]
        private uint mU3;
        [FieldOffset(16)]
        private uint mU4;
        public int CompareTo(Hash160 other)
        {
            fixed (byte* p0 = &Data[0])
            {
                return CoreSDK.MemoryCmp(p0, &other.Data[0], 20);
            }
        }
        public static bool operator ==(Hash160 hash1, Hash160 hash2)
        {
            return hash1.CompareTo(hash2) == 0;
        }
        public static bool operator !=(Hash160 hash1, Hash160 hash2)
        {
            return hash1.CompareTo(hash2) != 0;
        }
        public override bool Equals(object obj)
        {
            return this == (Hash160)obj;
        }
        public override int GetHashCode()
        {
            return (int)UniHash32.APHash(this.ToString());
        }
        public static Hash160 Emtpy = new Hash160();
        public static Hash160 CreateHash160(string src)
        {
            Hash160 result = new Hash160();
            var bytesSrc = Encoding.ASCII.GetBytes(src);
            var myRIPEMD160 = System.Security.Cryptography.RIPEMD160.Create();
            var hashCode = myRIPEMD160.ComputeHash(bytesSrc);
            fixed (byte* pSrc = &hashCode[0])
            {
                byte* pTar = &result.Data[0];
                CoreSDK.MemoryCopy(pTar, pSrc, 20);
            }
            return result;
        }
        public static Hash160 CreateHash160(byte[] bytesSrc)
        {
            Hash160 result = new Hash160();
            var myRIPEMD160 = System.Security.Cryptography.RIPEMD160.Create();
            var hashCode = myRIPEMD160.ComputeHash(bytesSrc);
            fixed (byte* pSrc = &hashCode[0])
            {
                byte* pTar = &result.Data[0];
                CoreSDK.MemoryCopy(pTar, pSrc, 20);
            }
            return result;
        }
        public static unsafe Hash160 CreateHash160(void* pAttr, uint length)
        {
            var bytes = new byte[length];
            fixed (byte* p = &bytes[0])
            {
                CoreSDK.MemoryCopy(p, pAttr, length);
            }
            return CreateHash160(bytes);
        }
        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < 20; i++)
            {
                if (i == 19)
                    result += String.Format("{0:X2}", Data[i]);
                else
                    result += String.Format("{0:X2}_", Data[i]);
            }
            return result;
        }
    }

    public struct FHashText : IComparable<FHashText>
    {
        public static readonly FHashText Empty = new FHashText();
        public Hash64 Hash;
        public string Text;
        public static FHashText Create(string text)
        {
            FHashText result;
            result.Text = text;
            result.Hash = Hash64.FromString(text);
            return result;
        }
        public int CompareTo(FHashText other)
        {
            return Hash.CompareTo(other.Hash);
        }
        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
        public static bool operator ==(in FHashText hash1, in FHashText hash2)
        {
            return hash1 == hash2;
        }
        public static bool operator !=(in FHashText hash1, in FHashText hash2)
        {
            return hash1 != hash2;
        }
        public override bool Equals(object obj)
        {
            var rh = (FHashText)obj;
            return this.Hash == rh.Hash;
        }
    }
}
