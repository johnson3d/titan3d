using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Data
{
    public partial class UAnimationData : IO.BaseSerializer
    {
        [Rtti.Meta]
        public Base.AnimHierarchy AnimatedHierarchy { get; set; }
        [Rtti.Meta]
        public List<Curve.ICurve> AnimCurvesList { get; set; } = new List<Curve.ICurve>();
    }

    public class UAnimationDataManagerModule : EngineNS.UModule<EngineNS.UEngine>
    {
        Dictionary<string, UAnimationData> AnimationDataDic = new Dictionary<string, UAnimationData>();
        public async Task<bool> LoadAnimationData(string filepath)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            AnimationDataDic = null;
            return false;
        }
    }
}

namespace EngineNS
{

    partial class UEngine
    {
        public Animation.Data.UAnimationDataManagerModule AnimationDataManagerModule { get; } = new Animation.Data.UAnimationDataManagerModule();
    }

}