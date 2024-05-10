using EngineNS.Animation.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public class TtAnimationChunk : IO.BaseSerializer
    {
        [Rtti.Meta]
        public RName RescouceName { get; set; }
        [Rtti.Meta]
        public Dictionary<Guid,Curve.ICurve> AnimCurvesList { get; set; } = new Dictionary<Guid, Curve.ICurve>();
        [Rtti.Meta]
        public Dictionary<String, TtAnimatedObjectDescription> AnimatedObjectDescs { get; set; } = new Dictionary<String, TtAnimatedObjectDescription>();
    }

    public class TtAnimationChunkManager
    {
        Dictionary<RName, TtAnimationChunk> Chunks = new Dictionary<RName, TtAnimationChunk>();

        //for new don't see animationChunk as the asset
        public TtAnimationChunk GetAnimationChunk(RName name)
        {
            TtAnimationChunk result;
            if (Chunks.TryGetValue(name, out result))
                return result;
            return null;
        }
        public void Remove(RName name)
        {
            Chunks.Remove(name);
        }
    }
}

namespace EngineNS.Animation
{
    public partial class TtAnimationModule
    {
        public Animation.Asset.TtAnimationChunkManager AnimationChunkManager { get; } = new Animation.Asset.TtAnimationChunkManager();
    }

}