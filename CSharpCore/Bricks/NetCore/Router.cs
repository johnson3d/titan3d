using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NetCore
{
    public enum ERouteTarget : byte
    {
        Unknown,
        Self,
        Routed,
        Client,
        Hall,
        Keep,
        Data,
        Reg,
        Log,
        Path,
        Gate,

        ReturnFlag = 0x80,
        ReturnMask = 0x7F,
    }
    public enum RPCExecuteLimitLevel
    {
        Unknown = 0,
        Player = 100,
        Lord = 200,
        GM = 300,
        Developer = 400,
        God = 500,
        TheOne = 600,
    }
    public class RPCRouter
    {
        public struct RouteData
        {
            public UInt16 RouteSlot;
            public UInt16 Authority;
            public UInt16 MapInHall;
            public UInt16 PlayerInMap;
            public Guid AccountId;
            public Guid CurrentRoleId;

            public void Reset()
            {
                RouteSlot = UInt16.MaxValue;
                Authority = (UInt16)RPCExecuteLimitLevel.Player;
                MapInHall = UInt16.MaxValue;
                PlayerInMap = UInt16.MaxValue;
                AccountId = Guid.Empty;
            }
            public void Save(ref PkgWriter pkg, ERouteTarget target)
            {
                pkg.Write((byte)target);
                pkg.Write(RouteSlot);
                pkg.Write(Authority);
                switch (target)
                {
                    case ERouteTarget.Data:
                        pkg.Write(AccountId);
                        break;
                    case ERouteTarget.Hall:
                        pkg.Write(MapInHall);
                        pkg.Write(PlayerInMap);
                        break;
                }
            }
            public void Load(ref PkgReader pkg)
            {
                byte target;
                pkg.Read(out target);
                pkg.Read(out RouteSlot);
                pkg.Read(out Authority);
                switch ((ERouteTarget)target)
                {
                    case ERouteTarget.Data:
                        pkg.Read(out AccountId);
                        break;
                    case ERouteTarget.Hall:
                        pkg.Read(out MapInHall);
                        pkg.Read(out PlayerInMap);
                        break;
                }
            }
        }
        public RouteData RouteInfo;
        public NetCore.NetConnection C2GConnect;
        public NetCore.NetConnection ToGConnect;
        public NetCore.NetConnection ToHConnect;

        public RPCRouter()
        {
            RouteInfo.Reset();
        }
        public void Reset()
        {
            RouteInfo.Reset();
            C2GConnect = null;
            ToGConnect = null;
            ToHConnect = null;
        }
    }
}
