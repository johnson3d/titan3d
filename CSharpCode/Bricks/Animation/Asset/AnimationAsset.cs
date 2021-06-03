using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation
{
    public interface IAsyncXndSaveLoad
    {
        Task<bool> LoadXnd(IO.CXndHolder xndHolder, XndNode parentNode);
        Task<bool> SaveXnd(IO.CXndHolder xndHolder, XndNode parentNode);
    }
    public interface ISyncXndSaveLoad
    {
        bool LoadXnd(IO.CXndHolder xndHolder, XndNode parentNode);
        bool SaveXnd(IO.CXndHolder xndHolder, XndNode parentNode);
    }
    public interface IAnimationAsset
    {

    }
}
