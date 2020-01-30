using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode.Particle
{
    public interface IParticleSaveData
    {
        bool IsLoadLink();
        void SetLoadLink(bool value);
        
        void SetXndNode(XndNode  data);
        XndNode GetXndNode();

        void SetXndAttribName(string name);
        string GetXndAttribName();

        System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad();

    }

}
