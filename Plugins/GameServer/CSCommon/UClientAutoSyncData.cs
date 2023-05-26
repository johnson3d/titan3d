#define TitanEngine_AutoGen_AutoSync
using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.AutoSync;

namespace EngineNS.Plugins.CSCommon
{
    [UAutoSync()]
    public partial class UClientAutoSyncData : IAutoSyncObject
    {
		public bool IsDirty { get; set; } = false;
        #region Property
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
        #endregion
    }
}

#if TitanEngine_AutoGen_AutoSync
#region TitanEngine_AutoGen_AutoSync
namespace EngineNS.Plugins.CSCommon
{
	partial class UClientAutoSyncData
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
		EngineNS.Support.UBitset mFlags = new EngineNS.Support.UBitset(2);
		public ref EngineNS.Support.UBitset Flags { get => ref mFlags; }
		public bool IsGhostSyncObject { get; set; }
	}
}
#endregion//TitanEngine_AutoGen_AutoSync
#endif//TitanEngine_AutoGen_AutoSync