using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EngineNS.Bricks.RemoteServices
{
    public interface IPackageReceiver
    {
        ApplicationData AppData
        {
            get;
        }
        void OnReceivePackage(NetCore.NetConnection sender, NetCore.PkgReader pkg);
        NetCore.NetConnection GetServerConnect(NetCore.ERouteTarget target);
    }

    public class ApplicationData
    {
        static ApplicationData smInstance;
        public static ApplicationData Instance
        {
            get { return smInstance; }
        }
        public ApplicationData()
        {
            smInstance = this;
        }
        public object Tag;
        public System.Reflection.Assembly[] AppAssemblies;
        public void RegAssmblies()
        {
            var serverWindowsAssembly = Rtti.RttiHelper.GetAssemblyFromDllFileName(ECSType.Server, "ServerCommon.dll");
            Rtti.RttiHelper.RegisterAnalyseAssembly("cscommon", serverWindowsAssembly);

            var serverAssembly = Rtti.RttiHelper.GetAssemblyFromDllFileName(ECSType.Server, "Server.dll");
            Rtti.RttiHelper.RegisterAnalyseAssembly("game", serverAssembly);
        }
    }
}
