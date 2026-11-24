using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public partial struct TtMemWriter
    {
        static bool msPkgSizeWarned = false;
        public void SurePkgHeader()
        {
            var size = Writer.Tell();
            // PackageSize 只有 16 位, 超限会被静默截断: 接收端按截断后的长度切包,
            // 剩下的字节留在流里, 后面所有包全部错位(表现为对端解析出巨大的长度后直接崩)
            if (size >= Bricks.Network.RPC.FPkgHeader.MaxPackageSize && msPkgSizeWarned == false)
            {
                msPkgSizeWarned = true;
                Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Error,
                    $"package size {size} exceeds FPkgHeader.MaxPackageSize({Bricks.Network.RPC.FPkgHeader.MaxPackageSize}), header will be truncated and the stream will be mis-parsed");
            }
            System.Diagnostics.Debug.Assert(size < Bricks.Network.RPC.FPkgHeader.MaxPackageSize);
            unsafe
            {
                ((Bricks.Network.RPC.FPkgHeader*)Writer.GetPointer())->PackageSize = (ushort)size;
            }
        }
    }
}
