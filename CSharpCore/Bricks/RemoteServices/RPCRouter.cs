using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public class IRouter
    {
        public virtual object GetHostObjectImpl(ref NetCore.RPCRouter.RouteData router)
        {
            return null;
        }
        public virtual string GetHostObjectName(ref NetCore.RPCRouter.RouteData router)
        {
            return "";
        }
    }

    #region Used Router
    public partial class DefaultRoute : IRouter
    {
        public static object HostObject;
        public override object GetHostObjectImpl(ref NetCore.RPCRouter.RouteData router)
        {
            return HostObject;
        }
    }

    public partial class ClientRoute : IRouter
    {

    }

    public partial class ClientRoleRoute : IRouter
    {

    }

    public partial class GateRoute : IRouter
    {

    }

    public partial class HallRoute : IRouter
    {

    }

    public partial class HPlayerRoute : IRouter
    {

    }

    public partial class RegRoute : IRouter
    {

    }

    public partial class LogRoute : IRouter
    {

    }

    public partial class PathRoute : IRouter
    {

    }

    public partial class KeepRoute : IRouter
    {

    }

    public partial class DataRoute : IRouter
    {

    }
    #endregion
}
