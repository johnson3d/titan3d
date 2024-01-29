using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{   
    public struct TName
    {//这是用来减少因为string拼接导致的gc的辅助类
        private UInt32 mNameIndex;
        public TName(TName name)
        {
            mNameIndex = name.mNameIndex;
        }
        private TName(UInt32 index)
        {
            mNameIndex = index;
        }
        public TName(string name)
        {
            mNameIndex = GetNameIndex(name);
        }
        public override string ToString()
        {
            return NameTable[(int)mNameIndex];
        }
        public static TName FromString(string name)
        {
            return new TName(GetNameIndex(name));
        }
        public static TName FromString2(string name1, string name2)
        {
            return new TName(GetNameIndex2(name1, name2));
        }
        #region Manger
        static List<string> NameTable = new List<string>();
        private static UInt32 GetNameIndex(string name)
        {
            lock (NameTable)
            {
                for (int i = 0; i < NameTable.Count; i++)
                {
                    if (NameTable[i] == name)
                    {
                        return (UInt16)i;
                    }
                }
                NameTable.Add(name);
                return (UInt32)NameTable.Count - 1;
            }
        }
        private static UInt32 GetNameIndex2(string name1, string name2)
        {
            lock (NameTable)
            {
                for (int i = 0; i < NameTable.Count; i++)
                {
                    var name = NameTable[i];
                    if (name.Length == name1.Length + name2.Length)
                    {
                        if (name.StartsWith(name1) && name.EndsWith(name2))
                            return (UInt32)i;
                    }
                }
                NameTable.Add(name1 + name2);
                return (UInt32)NameTable.Count - 1;
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
