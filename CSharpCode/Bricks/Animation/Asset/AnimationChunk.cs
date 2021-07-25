using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset
{
    public class UAnimationChunk : IO.BaseSerializer
    {
        [Rtti.Meta]
        public RName RescouceName { get; set; }
        [Rtti.Meta]
        public Base.AnimHierarchy AnimatedHierarchy { get; set; }
        [Rtti.Meta]
        public Dictionary<Guid,Curve.ICurve> AnimCurvesList { get; set; } = new Dictionary<Guid, Curve.ICurve>();
    }

    public class UAnimationBlockManagerModule : EngineNS.UModule<EngineNS.UEngine>
    {
        Dictionary<RName, UAnimationChunk> Chunks = new Dictionary<RName, UAnimationChunk>();
        public async Task<bool> LoadAnimationData(string filepath)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            
            return false;
        }
    }
}

namespace EngineNS
{

    partial class UEngine
    {
        public Animation.Asset.UAnimationBlockManagerModule AnimationDataManagerModule { get; } = new Animation.Asset.UAnimationBlockManagerModule();
    }

}