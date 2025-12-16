using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public partial struct TtMemWriter
    {
        public void SurePkgHeader()
        {
            System.Diagnostics.Debug.Assert(Writer.Tell() < Bricks.Network.RPC.FPkgHeader.MaxPackageSize);
            unsafe
            {
                ((Bricks.Network.RPC.FPkgHeader*)Writer.GetPointer())->PackageSize = (ushort)Writer.Tell();
            }
        }
    }
}
