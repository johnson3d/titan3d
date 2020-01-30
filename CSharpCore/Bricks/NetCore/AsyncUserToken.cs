
namespace EngineNS.Bricks.NetCore
{
    /// <summary>
    /// 提供给SocketAsyncEventArgs.UserToken使用
    /// </summary>
    public class AsyncUserToken
    {
        public byte[] messageBytes;
        public byte[] prefixBytes;
        public int messageBytesDone;
        public int prefixBytesDone;
        public int messageLength;
        public int bufferOffset;
        public int bufferSkip;
        public NetConnection tcpConn;
        public long SendTime;

        public bool IsPrefixReady
        {
            get { return prefixBytesDone == NetPacketParser.PREFIX_SIZE; }
        }

        public int DataOffset
        {
            get { return bufferOffset + bufferSkip; }
        }
        public int RemainByte
        {
            get { return messageLength - messageBytesDone; }
        }
        public bool IsMessageReady
        {
            get { return messageBytesDone == messageLength; }
        }
        public AsyncUserToken()
        {
            prefixBytes = new byte[NetPacketParser.PREFIX_SIZE];
        }

        public void Reset(bool skip)
        {
            this.messageBytes = null;
            for (int i = 0; i < NetPacketParser.PREFIX_SIZE; i++)
                prefixBytes[i] = 0;

            this.messageBytesDone = 0;
            this.prefixBytesDone = 0;
            this.messageLength = 0;
            if (skip)
                this.bufferSkip = 0;
        }
    }
}