using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.AnimNode
{
    public interface IAnimation
    {
        RName Name { get; }
        uint KeyFrames { get;}
        float SampleRate { get;}
        float Duration { get;  }
        float PlayRate { get; }
    }
    public interface IFireNotify
    {
        void AddNofity(Notify.CGfxNotify notify);
        void FiringNofities();
    }
}
