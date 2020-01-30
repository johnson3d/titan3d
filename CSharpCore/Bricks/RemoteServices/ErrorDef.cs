using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    [IO.Serializer.EnumSizeAttribute(typeof(IO.Serializer.SByteEnum))]
    public enum RPCError : sbyte
    {
        Unknown = -127,
        Timeout = -126,
        OK = 1,
        Reconnected = 2,//是断线重连，需要客户端重新请求数据
    }
}
