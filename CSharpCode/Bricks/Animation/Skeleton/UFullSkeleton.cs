using EngineNS.Animation.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public interface ILimb
    {

    }
    public class UFullSkeleton : ILimb
    {
        List<ILimb> Children { get; set; } = new List<ILimb>();
    }
}
