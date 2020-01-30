using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EngineNS.Bricks.RemoteServices.Net
{
    public class RecvData
    {
        internal static RecvData CreateData(PkgPoolManager mgr, byte[] src, int len)
        {
            var pkg = mgr.QueryRecvData();

            pkg.Init(mgr, src, len);
            return pkg;
        }
        private void Init(PkgPoolManager mgr, byte[] src,int len)
        {
            if (len >= UInt16.MaxValue)
            {
                PkgData = new byte[len];
            }
            else
            {
                PkgMgr = mgr;
                PkgData = mgr.AllocPkg((UInt16)len);
            }
            Length = len;
            RecvTime = Support.Time.GetTickCount();
            unsafe
            {
                //Buffer.BlockCopy(src, 0, PkgData, 0, (int)len);
                fixed (byte* ps = &src[0])
                fixed (byte* dest = &PkgData[0])
                {
                    CoreSDK.SDK_Memory_Copy(dest, ps, (UInt32)len);
                }
            }
        }
        public void Dispose()
        {
            if (PkgMgr != null)
            {
                PkgMgr.FreePkg(PkgData);
                PkgData = null;
                Length = 0;
                RecvTime = 0;
                PkgMgr.ReleaseRecvData(this);
                PkgMgr = null;
            }
        }
        public PkgPoolManager PkgMgr;
        public byte[] PkgData;
        public int Length;
        public Int64 RecvTime;
    }

    public class NetPacketParser
    {
        static public int PREFIX_SIZE = 2;

        static public int HandlePrefix(SocketAsyncEventArgs saea, AsyncUserToken dataToken, int remainingBytesToProcess)
        {
            if (saea.AcceptSocket.Connected == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "RPC", "HandlePrefix saea.AcceptSocket.Connected == false");
                return 0;
            }

            if (remainingBytesToProcess >= PREFIX_SIZE - dataToken.prefixBytesDone)
            {
                for (int i = 0; i < PREFIX_SIZE - dataToken.prefixBytesDone; i++)
                {
                    dataToken.prefixBytes[dataToken.prefixBytesDone + i] = saea.Buffer[dataToken.DataOffset + i];
                }
                remainingBytesToProcess = remainingBytesToProcess - PREFIX_SIZE + dataToken.prefixBytesDone;
                dataToken.bufferSkip += PREFIX_SIZE - dataToken.prefixBytesDone;
                dataToken.prefixBytesDone = PREFIX_SIZE;
                dataToken.messageLength = BitConverter.ToUInt16(dataToken.prefixBytes, 0);
            }
            else
            {
                for (int i = 0; i < remainingBytesToProcess; i++)
                {
                    dataToken.prefixBytes[dataToken.prefixBytesDone + i] = saea.Buffer[dataToken.DataOffset + i];
                }
                dataToken.prefixBytesDone += remainingBytesToProcess;
                remainingBytesToProcess = 0;
            }

            return remainingBytesToProcess;
        }

        static public int HandleMessage(SocketAsyncEventArgs saea, AsyncUserToken dataToken, int remainingBytesToProcess)
        {
            if (saea.AcceptSocket.Connected == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "RPC", "HandleMessage saea.AcceptSocket.Connected == false");
                return 0;
            }
            if (dataToken.messageBytesDone == 0)
            {
                if (dataToken.messageLength > 65535)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "RPC", "HandleMessage dataToken.messageLength = {0}", dataToken.messageLength);
                    return 0;
                }
                dataToken.messageBytes = new byte[dataToken.messageLength];
            }

            var nonCopiedBytes = 0;
            if (remainingBytesToProcess + dataToken.messageBytesDone >= dataToken.messageLength)
            {
                var copyedBytes = dataToken.RemainByte;
                nonCopiedBytes = remainingBytesToProcess - copyedBytes;
                Buffer.BlockCopy(saea.Buffer, dataToken.DataOffset, dataToken.messageBytes, dataToken.messageBytesDone, copyedBytes);
                dataToken.messageBytesDone = dataToken.messageLength;
                dataToken.bufferSkip += copyedBytes;
            }
            else
            {
                Buffer.BlockCopy(saea.Buffer, dataToken.DataOffset, dataToken.messageBytes, dataToken.messageBytesDone, remainingBytesToProcess);
                dataToken.messageBytesDone += remainingBytesToProcess;
            }

            return nonCopiedBytes;
        }
    }

    public class PacketBuilder
    {
        byte[] mDataBuffer = new byte[UInt16.MaxValue+RPCHeader.SizeOf()];
        public byte[] DataBuffer
        {
            get { return mDataBuffer; }
        }

        UInt32 PacketSize = 0;
        UInt32 CurPos = 0;

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
                fixed (byte* pTar = &DataBuffer[CurPos])
                {
                    for(int i=0;i<count;i++)
                    {
                        pTar[i] = pSrc[i];
                    }
                }
                CurPos += (uint)count;
                length -= count;
                
                return pSrc + count;
            }
        }

        public delegate void FOnPacketOK(RecvData rcv);
        const UInt32 PacketHeadSize = sizeof(ushort);
        public unsafe bool ParsePackage(byte* pData, UInt32 length, FOnPacketOK onPacket, PkgPoolManager parameter)
        {
            lock (this)
            {
                //int whileCount = 0;
                while (true)
                {
                    //++whileCount;
                    //if (whileCount >= 2)
                    //{
                    //    Log.FileLog.WriteLine($"ParsePackage whileCount = {whileCount}");
                    //}

                    if (length == 0)
                        return true;
                    unsafe
                    {
                        //包头还没有读出来，不知道长度
                        if (PacketSize == 0)
                        {
                            if (CurPos + length < PacketHeadSize)
                            {//加上新读取的都还没有完成包头
                                AppendData(pData, length, ref length);
                                return true;
                            }
                            else
                            {
                                var remainHead = PacketHeadSize - CurPos;
                                pData = AppendData(pData, remainHead, ref length);
                                fixed (byte* pBuffer = &mDataBuffer[0])
                                {
                                    PacketSize = ((RPCHeader*)pBuffer)->PackageSize;
                                }
                            }
                        }

                        if (PacketSize == 0 || PacketSize >= UInt16.MaxValue)
                        {//被修改或者错误的包
                            return false;
                        }

                        if (CurPos + length < PacketSize)
                        {//还不够
                            AppendData(pData, length, ref length);
                            return true;
                        }
                        else
                        {
                            //if (PacketSize == 0)
                            //    return false;
                            pData = AppendData(pData, PacketSize - CurPos, ref length);
                            //int pkgLength = (int)PacketSize;

                            #region CreateFullPacket
                            var pkg = RecvData.CreateData(parameter, mDataBuffer, (int)PacketSize);
                            CurPos = 0;
                            PacketSize = 0;
                            #endregion

                            onPacket(pkg);
                            //return ParsePackage(pData, length, onPacket, parameter);
                        }
                    }
                }
            }
        }

        public void ResetPacket()
        {
            CurPos = 0;
            PacketSize = 0;
        }
    }

    public class PkgPoolManager
    {
        public class PkgPool
        {
            public int Size;
            public List<byte[]> mPools = new List<byte[]>();
            public byte[] AllocPkg()
            {
                lock (this)
                {
                    if (mPools.Count == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            mPools.Add(new byte[Size]);
                        }
                    }
                    var result = mPools[mPools.Count - 1];
                    mPools.RemoveAt(mPools.Count - 1);
                    return result;
                }
            }
            public bool FreePkg(byte[] pkg)
            {
                lock (this)
                {
                    if (pkg.Length != Size)
                        return false;
                    mPools.Add(pkg);
                    return true;
                }
            }
        }

        PkgPool[] mPkgPools;
        int mNumber;
        int mStride;
        public PkgPoolManager(int stride = 64)
        {
            InitPools(stride);
        }
        public void InitPools(int stride)
        {
            mNumber = 65536 / stride;
            mPkgPools = new PkgPool[mNumber];
            mStride = stride;
            for (int i = 0; i < mNumber; i++)
            {
                var pool = new PkgPool();
                pool.Size = RPCHeader.SizeOf() + mStride * (i+1);
                mPkgPools[i] = pool;
            }
        }
        public byte[] AllocPkg(UInt16 size)
        {
            int index = (int)size / mStride;
            return mPkgPools[index].AllocPkg();
        }
        public bool FreePkg(byte[] pkg)
        {
            int size = pkg.Length - RPCHeader.SizeOf();
            if (size < 0)
                return false;
            int index = size / mStride - 1;
            if (index >= mNumber)
                return false;
            return mPkgPools[index].FreePkg(pkg);
        }

        List<RecvData> mRecvDataPool = new List<RecvData>();
        public RecvData QueryRecvData()
        {
            lock(this)
            {
                if (mRecvDataPool.Count == 0)
                {
                    for(int i=0;i<10;i++)
                    {
                        mRecvDataPool.Add(new RecvData());
                    }
                }
                var result = mRecvDataPool[mRecvDataPool.Count - 1];
                mRecvDataPool.RemoveAt(mRecvDataPool.Count - 1);
                return result;
            }
        }
        public void ReleaseRecvData(RecvData rcv)
        {
            lock(this)
            {
                mRecvDataPool.Add(rcv);
            }
        }
    }
}
