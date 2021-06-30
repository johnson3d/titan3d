#define COMPACT_OLD
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Network.RPC
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct FPkgHeader
    {
        public void ToDefault()
        {
            PackageSize = 0;
            PKGFlags = 0;
        }

        public ushort PackageSize;
        public byte PKGFlags;
        

        public static int SizeOf()
        {
            unsafe
            {
                return sizeof(FPkgHeader);
            }
        }

        public void SetWeakPkg(bool b)
        {
            if (b)
            {
                PKGFlags |= (byte)EPkgTypes.WeakPkg;//弱包，可以抛弃
            }
            else
            {
                unchecked
                {
                    PKGFlags &= (byte)(~EPkgTypes.WeakPkg);
                }
            }
        }
        public bool IsWeakPkg()
        {
            if ((PKGFlags & (byte)EPkgTypes.WeakPkg) > 0)
                return true;
            else
                return false;
        }
        public void SetHasReturn(bool b)
        {
            if (b)
            {
                PKGFlags |= (byte)EPkgTypes.IsReturn;
            }
            else
            {
                unchecked
                {
                    PKGFlags &= (byte)(~EPkgTypes.IsReturn);
                }
            }
        }
        public bool IsHasReturn()
        {
            if ((PKGFlags & (byte)EPkgTypes.IsReturn) > 0)
                return true;
            else
                return false;
        }
    }
}
