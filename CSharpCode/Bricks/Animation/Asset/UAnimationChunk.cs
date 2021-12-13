using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta]
    public class UAnimationChunk : IO.BaseSerializer
    {
        [Rtti.Meta]
        public RName RescouceName { get; set; }
        [Rtti.Meta]
        public Base.UAnimHierarchy AnimatedHierarchy { get; set; }
        [Rtti.Meta]
        public Dictionary<Guid,Curve.ICurve> AnimCurvesList { get; set; } = new Dictionary<Guid, Curve.ICurve>();
    }

    public class UAnimationChunkManager
    {
        Dictionary<RName, UAnimationChunk> Chunks = new Dictionary<RName, UAnimationChunk>();

        //for new don't see animationChunk as the asset
        public UAnimationChunk GetAnimationChunk(RName name)
        {
            UAnimationChunk result;
            if (Chunks.TryGetValue(name, out result))
                return result;
            return null;
        }
    }
}

namespace EngineNS.Animation
{
    public partial class UAnimationModule
    {
        public Animation.Asset.UAnimationChunkManager AnimationChunkManager { get; } = new Animation.Asset.UAnimationChunkManager();
    }

}