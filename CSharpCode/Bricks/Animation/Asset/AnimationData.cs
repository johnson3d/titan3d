using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Data
{
    public class UAnimationData : IAsyncXndSaveLoad
    {
        //Save
        public Base.AnimHierarchy AnimatedHierarchy { get; set; }
        public List<Curve.ICurve> AnimCurvesList { get; set; } = new List<Curve.ICurve>();
        ////
        ///
        public async Task<bool> LoadXnd(IO.CXndHolder xndHolder, XndNode parentNode)
        {
            await UEngine.Instance.EventPoster.Post(async () =>
            {
                using (var dataNode = parentNode.TryGetChildNode("AnimationData"))
                {
                    if (!dataNode.IsValidPointer)
                        return false;
                    uint curvesCount = dataNode.GetNumOfNode();
                    for (uint i = 0; i < curvesCount; ++i)
                    {
                        var curveNode = dataNode.GetNode(i);
                        using (var att = curveNode.TryGetAttribute("CurveType"))
                        {
                            if(!att.IsValidPointer)
                            {
                                return false;
                            }
                            string typeStr = "";
                            unsafe
                            {
                                VNameString typeName = *VNameString.CreateInstance();
                                att.BeginRead();
                                att.Read(ref typeName);
                                att.EndRead();
                                typeStr = typeName.GetString();
                            }
                            var curve = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(typeStr)) as Curve.ICurve;
                            await curve.LoadXnd(xndHolder, curveNode);
                        }
                    }
                }
                await Thread.AsyncDummyClass.DummyFunc();
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }

        public async Task<bool> SaveXnd(IO.CXndHolder xndHolder, XndNode parentNode)
        {
            await UEngine.Instance.EventPoster.Post(async () =>
            {
                using (var dataNode = xndHolder.NewNode("AnimationData", 1, 0))
                {
                    await AnimatedHierarchy.SaveXnd(xndHolder, dataNode);
                    for (int i = 0; i < AnimCurvesList.Count; ++i)
                    {
                        using (var curveNode = xndHolder.NewNode("Curve" + i.ToString(), 1, 0))
                        {
                            using (var att = xndHolder.NewAttribute("CurveType", 1, 0))
                            {

                                unsafe
                                {
                                    att.BeginWrite(10);
                                    VNameString typeName = *VNameString.CreateInstance(Rtti.UTypeDescManager.Instance.GetTypeStringFromType(AnimCurvesList[i].GetType()));
                                    att.Write(typeName);
                                    att.EndWrite();
                                    curveNode.AddAttribute(att.CppPointer);
                                }

                            }
                            await AnimCurvesList[i].SaveXnd(xndHolder, curveNode);
                        }
                    }
                    unsafe
                    {
                        parentNode.AddNode(dataNode);
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
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