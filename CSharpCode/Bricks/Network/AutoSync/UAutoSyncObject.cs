#define TitanEngine_AutoGen_AutoSync
using EngineNS.IO;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Text;
using static NPOI.HSSF.Util.HSSFColor;

namespace EngineNS.Bricks.Network.AutoSync
{
    public class UAutoSyncAttribute : Attribute
    {
        public UInt16 Index;
    }
    public interface IAutoSyncObject
    {
        ref Support.TtBitset Flags { get; }
        bool IsDirty { get; set; }
        void AutoSyncWriteValue(IO.IWriter ar, int index);
        void AutoSyncReadValue(IO.IReader ar, int index, bool bSet = true);
    }
    public struct FSyncHelper
    {
        public static bool SetValue<T>(IAutoSyncObject host, uint index, ref T taget, in T v) where T : IComparable
        {
            if (taget.CompareTo(v) == 0)
                return false;
            lock (host.Flags)
            {
                host.Flags.SetBit(index);
                taget = v;
                host.IsDirty = true;
            }
            return true;
        }
        public static bool BuildModify(IAutoSyncObject host, IO.IWriter ar)
        {
            lock (host.Flags)
            {
                ar.Write(host.Flags);
                for (uint i = 0; i < host.Flags.BitCount; i++)
                {
                    if (host.Flags.IsSet(i))
                    {
                        host.Flags.UnsetBit(i);
                        host.AutoSyncWriteValue(ar, (int)i);
                    }
                }
            }
            host.IsDirty = false;
            return true;
        }
        [ThreadStatic]
        private static Support.TtBitset SyncFlags = null;
        public static bool SyncValues(IAutoSyncObject host, IO.IReader ar, bool bSet = true)
        {
            ar.Read(ref SyncFlags);
            for (uint i = 0; i < SyncFlags.BitCount; i++)
            {
                if (SyncFlags.IsSet(i))
                {
                    host.AutoSyncReadValue(ar, (int)i, bSet);
                }
            }
            return true;
        }
    }

    [UAutoSync()]
    public partial class IAutoSyncObject_Test0 : IAutoSyncObject
    {
        public bool IsDirty { get; set; }
        int mA;
        [UAutoSync(Index = 0)]
        public int A 
        {
            get => mA;
            set
            {
                var changed = FSyncHelper.SetValue(this, 0, ref mA, in value);
                if (changed)
                {
                    if (this.IsGhostSyncObject)
                    {
                        
                    }
                }
            }
        }
        float mB;
        [UAutoSync(Index = 1)]
        public float B
        {
            get => mB;
            set
            {
                FSyncHelper.SetValue(this, 1, ref mB, in value);
            }
        }

        public void RealObjectUpdate2Server()
        {
            using (var writer = IO.UMemWriter.CreateInstance())
            {
                var ar = new IO.AuxWriter<IO.UMemWriter>(writer);
                FSyncHelper.BuildModify(this, ar);
            }
        }
        public unsafe void GhostObjectUpdateByServer(IO.UMemWriter writer)
        {
            using (var reader = IO.UMemReader.CreateInstance((byte*)writer.Ptr, writer.GetPosition()))
            {
                var ar = new IO.AuxReader<IO.UMemReader>(reader, null);
                FSyncHelper.SyncValues(this, ar);
            }
        }
    }
}
#if TitanEngine_AutoGen_AutoSync
#region TitanEngine_AutoGen_AutoSync
namespace EngineNS.Bricks.Network.AutoSync
{
	partial class IAutoSyncObject_Test0
	{
		public void AutoSyncWriteValue(IO.IWriter ar, int index)
		{
			switch(index)
			{
				case 0:
				{
					ar.Write(A);
					break;
				}
				case 1:
				{
					ar.Write(B);
					break;
				}
			}
		}
		public void AutoSyncReadValue(IO.IReader ar, int index, bool bSet)
		{
			switch(index)
			{
				case 0:
				{
					int tmp;
					ar.Read(out tmp);
					if (bSet)
					{
						A = tmp;
					}
					break;
				}
				case 1:
				{
					float tmp;
					ar.Read(out tmp);
					if (bSet)
					{
						B = tmp;
					}
					break;
				}
			}
		}
		EngineNS.Support.TtBitset mFlags = new EngineNS.Support.TtBitset(2);
		public ref EngineNS.Support.TtBitset Flags { get => ref mFlags; }
		public bool IsGhostSyncObject { get; set; }
	}
}
#endregion//TitanEngine_AutoGen_AutoSync
#endif//TitanEngine_AutoGen_AutoSync