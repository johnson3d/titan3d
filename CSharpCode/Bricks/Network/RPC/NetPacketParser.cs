using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EngineNS.Bricks.Network.RPC
{
    public class PacketBuilder : IDisposable
    {
        public UNetPackageManager NetPackageManager;
        MemStreamWriter Writer;
        uint PacketSize = 0;
        public PacketBuilder()
        {
            Writer = MemStreamWriter.CreateInstance(1024 * 2);
        }
        ~PacketBuilder()
        {
            Dispose();
        }
        public void Dispose()
        {
            Writer.Dispose();
        }
        private unsafe byte* GetPointer()
        {
            return (byte*)Writer.GetDataPointer();
        }

        private unsafe byte* AppendData(byte* pSrc, UInt32 count, ref UInt32 length)
        {
            if (count == 0)
                return pSrc;

            if (length < count)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "RPC", "夭寿啦！");
                length = 0;
                return pSrc;
            }

            unsafe
            {
                Writer.Write(pSrc, count);
                length -= count;
                
                return pSrc + count;
            }
        }
        const UInt32 PacketHeadSize = sizeof(ushort);
        public unsafe bool ParsePackage(byte* pData, UInt32 length, INetConnect conn)
        {
            lock (this)
            {
                while (true)
                {
                    if (length == 0)
                        return true;
                    unsafe
                    {
                        //包头还没有读出来，不知道长度
                        if (PacketSize == 0)
                        {
                            if ((uint)Writer.Tell() + length < PacketHeadSize)
                            {//加上新读取的都还没有完成包头
                                AppendData(pData, length, ref length);
                                return true;
                            }
                            else
                            {
                                var remainHead = PacketHeadSize - (uint)Writer.Tell();
                                pData = AppendData(pData, remainHead, ref length);
                                PacketSize = ((FPkgHeader*)GetPointer())->PackageSize;
                            }
                        }

                        if (PacketSize == 0 || PacketSize >= UInt16.MaxValue)
                        {//被修改或者错误的包
                            return false;
                        }

                        if ((uint)Writer.Tell() + length < PacketSize)
                        {//还不够
                            AppendData(pData, length, ref length);
                            return true;
                        }
                        else
                        {
                            pData = AppendData(pData, PacketSize - (uint)Writer.Tell(), ref length);

                            NetPackageManager.PushPackage(GetPointer(), PacketSize, conn);
                            Writer.Seek(0);
                            PacketSize = 0;
                        }
                    }
                }
            }
        }
    }
}
