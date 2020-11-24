using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public struct TName
    {
        private UInt16 mNameIndex;
        public TName(TName name)
        {
            mNameIndex = name.mNameIndex;
        }
        private TName(UInt16 index)
        {
            mNameIndex = index;
        }
        public TName(string name)
        {
            mNameIndex = GetNameIndex(name);
        }
        public override string ToString()
        {
            return NameTable[mNameIndex];
        }
        public static TName FromString(string name)
        {
            return new TName(GetNameIndex(name));
        }        
        #region Manger
        static UInt32 mCurIndex = 0;
        static string[] NameTable = new string[UInt16.MaxValue];
        private static UInt16 GetNameIndex(string name)
        {
            for (int i = 0; i < mCurIndex; i++)
            {
                if (NameTable[i] == name)
                {
                    return (UInt16)i;
                }
            }
            if (mCurIndex >= NameTable.Length)
            {
                throw new Exception("Number of TName overflow");
            }
            NameTable[mCurIndex] = name;
            return (UInt16)mCurIndex++;
        }
        #endregion
    }

    public struct TAssemblyName
    {
        private UInt16 mNameIndex;
        private TAssemblyName(UInt16 index)
        {
            mNameIndex = index;
        }
        private TAssemblyName(TAssemblyName name)
        {
            mNameIndex = name.mNameIndex;
        }
        public TAssemblyName(string name, System.Reflection.Assembly assembly = null)
        {
            if (assembly == null)
                assembly = System.Reflection.Assembly.GetCallingAssembly();
            var tab = GetNameTable(assembly);
            mNameIndex = tab.GetNameIndex(name);
        }
        public string ToString(System.Reflection.Assembly assembly = null)
        {
            if (assembly == null)
                assembly = System.Reflection.Assembly.GetCallingAssembly();
            var tab = GetNameTable(assembly);
            return tab.Names[mNameIndex];
        }
        public override string ToString()
        {
            return ToString(System.Reflection.Assembly.GetCallingAssembly());
        }
        public static TAssemblyName FromString(string name, System.Reflection.Assembly assembly = null)
        {
            if (assembly == null)
                assembly = System.Reflection.Assembly.GetCallingAssembly();
            var tab = GetNameTable(assembly);
            return new TAssemblyName(tab.GetNameIndex(name));
        }
        #region Manger
        class NameTable
        {
            public UInt32 mCurIndex;
            public string[] Names;
            public NameTable()
            {
                mCurIndex = 0;
                Names = new string[UInt16.MaxValue];
            }
            public UInt16 GetNameIndex(string name, System.Reflection.Assembly assembly = null)
            {
                for (int i = 0; i < mCurIndex; i++)
                {
                    if (Names[i] == name)
                    {
                        return (UInt16)i;
                    }
                }
                if (mCurIndex >= Names.Length)
                {
                    throw new Exception($"Number of TName overflow: assembly {assembly}");
                }
                Names[mCurIndex] = name;
                return (UInt16)mCurIndex++;
            }
        }
        static Dictionary<System.Reflection.Assembly, NameTable> AssmNameTalbes = new Dictionary<System.Reflection.Assembly, NameTable>();
        static NameTable GetNameTable(System.Reflection.Assembly assembly)
        {
            NameTable result;
            if(AssmNameTalbes.TryGetValue(assembly, out result) == false)
            {
                result = new NameTable();
                AssmNameTalbes.Add(assembly, result);
            }
            return result;
        }
        #endregion
    }
}
