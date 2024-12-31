using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{   
    public struct TtName
    {//这是用来减少因为string拼接导致的gc的辅助类
        UInt32 Index;
        public TtName(TtName name)
        {
            Index = name.Index;
        }
        private TtName(UInt32 index)
        {
            Index = index;
        }
        public TtName(string name)
        {
            Index = GetNameIndex(name);
        }
        public override string ToString()
        {
            var bkt = (Index & 0xffff0000) >> 16;
            if (bkt >= NumOfBucket)
                return null;
            var pos = Index & 0x0000ffff;
            if (pos >= 0x0000ffff)
                return null;
            return NameTable[(int)bkt][(int)pos];
        }
        public static TtName FromString(string name)
        {
            return new TtName(GetNameIndex(name));
        }
        #region Manger
        const int NumOfBucket = 2048;
        static List<string>[] NameTable = new List<string>[NumOfBucket];
        private static UInt32 GetNameIndex(string name)
        {
            lock (NameTable)
            {
                var hash = (UInt32)name.GetHashCode();
                var bkt = hash % NumOfBucket;
                if (NameTable[bkt] == null)
                {
                    NameTable[bkt] = new List<string>();
                }
                var count = NameTable[bkt].Count;
                for (int i = 0; i < count; i++)
                {
                    if (NameTable[bkt][i] == name)
                        return (bkt << 16) | (UInt32)i;
                }
                if (count >= 0xffff)
                    return 0xffffffff;
                NameTable[bkt].Add(name);
                return (bkt << 16) | (UInt32)count;
            }
        }
        #endregion
    }

    public class TtNameTable
    {
        public static VNameString FontTexture = VNameString.FromString("FontTexture");
        public static VNameString Samp_FontTexture = VNameString.FromString("Samp_FontTexture");

        public static VNameString InstanceCulling = VNameString.FromString("InstanceCulling");
        public static VNameString InstanceMeshCulling = VNameString.FromString("InstanceMeshCulling");
        public static VNameString StaticMeshBatchCulling = VNameString.FromString("StaticMeshBatchCulling");
        public static VNameString TerrainMeshBatchCulling = VNameString.FromString("TerrainMeshBatchCulling");

        public static VNameString VSInstanceDataArray = VNameString.FromString("VSInstanceDataArray");
        public static VNameString UITexture = VNameString.FromString("UITexture");
        public static VNameString cbPerMaterial = VNameString.FromString("cbPerMaterial");
        public static VNameString cbGBufferDesc = VNameString.FromString("cbGBufferDesc");
    }
}
