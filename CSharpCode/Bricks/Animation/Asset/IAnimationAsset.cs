using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset
{
    public interface IAnimationAsset : IO.IAsset
    {
        public float Duration { get; }
    }
    public interface IAnimationCompositeAsset : IAnimationAsset
    {

    }
}
