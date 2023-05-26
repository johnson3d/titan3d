using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker.Command
{
    class UStartDS : UCookCommand
    {
        public override async System.Threading.Tasks.Task ExecuteCommand(string[] args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            var port = FindArgument(args, DS_Port);
            EngineNS.UEngine.Instance.DedicatedServer.StartServer("0.0.0.0", System.Convert.ToUInt16(port));
        }
    }
}
