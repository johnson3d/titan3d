using EngineNS.Animation.Curve;
using EngineNS.RHI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation
{
    namespace Curve
    {
        // curve can be a asset 
        public interface ICurve //: IO.IAsset
        {
            Task<bool> LoadXnd(IO.CXndHolder xndHolder, XndNode parentNode);
            Task<bool> SaveXnd(IO.CXndHolder xndHolder, XndNode parentNode);
        }

        public class Vector3Curve : ICurve
        {
            private TCurve<float, float> XCurve = null;
            private TCurve<float, float> YCurve = null;
            private TCurve<float, float> ZCurve = null;
            private TCurveCache<float> XCache;
            private TCurveCache<float> YCache;
            private TCurveCache<float> ZCache;
            public Vector3 Evaluate(float time)
            {
                Vector3 temp = Vector3.Zero;
                temp.X = XCurve.Evaluate(time, ref XCache);
                temp.Y = YCurve.Evaluate(time, ref YCache);
                temp.Z = ZCurve.Evaluate(time, ref ZCache);
                return temp;
            }
            #region interface ICurve
            public async Task<bool> LoadXnd(IO.CXndHolder xndHolder, XndNode parentNode)
            {
                await UEngine.Instance.EventPoster.Post(async () =>
                {
                    using (var curvesNode = parentNode.TryGetChildNode("Curves"))
                    {
                        if (!curvesNode.IsValidPointer)
                            return false;
                        using (var att = curvesNode.TryGetAttribute("XCurve"))
                        {
                            if (!att.IsValidPointer)
                            {
                                return false;
                            }
                            XCurve = new TCurve<float, float>();
                            XCurve.LoadXnd(att);
                        }
                        using (var att = curvesNode.TryGetAttribute("YCurve"))
                        {
                            if (att.IsValidPointer)
                            {
                                return false;
                            }
                            YCurve = new TCurve<float, float>();
                            YCurve.LoadXnd(att);
                        }
                        using (var att = curvesNode.TryGetAttribute("ZCurve"))
                        {
                            if (att.IsValidPointer)
                            {
                                return false;
                            }
                            ZCurve = new TCurve<float, float>();
                            ZCurve.LoadXnd(att);
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
                    unsafe
                    {
                        using (var node = xndHolder.NewNode("Curves", 1, 0))
                        {
                            using (var att = xndHolder.NewAttribute("XCurve", 1, 0))
                            {
                                XCurve.SaveXnd(att);
                                node.AddAttribute(att.CppPointer);
                            }
                            using (var att = xndHolder.NewAttribute("YCurve", 1, 0))
                            {
                                YCurve.SaveXnd(att);
                                node.AddAttribute(att.CppPointer);
                            }
                            using (var att = xndHolder.NewAttribute("ZCurve", 1, 0))
                            {
                                ZCurve.SaveXnd(att);
                                node.AddAttribute(att.CppPointer);
                            }
                            parentNode.AddNode(node);
                        }
                    }
                    await Thread.AsyncDummyClass.DummyFunc();
                }, Thread.Async.EAsyncTarget.AsyncIO);
                return true;
            }
            #endregion
        }
        public class FloatCurve : ICurve
        {
            private TCurve<float, float> Curve = null;
            private TCurveCache<float> Cache;
            public float Evaluate(float time)
            {
                return Curve.Evaluate(time, ref Cache);
            }
            #region interface ICurve
            public async Task<bool> LoadXnd(IO.CXndHolder xndHolder, XndNode parentNode)
            {
                await UEngine.Instance.EventPoster.Post(async () =>
                {
                    using (var att = parentNode.TryGetAttribute("Curve"))
                    {
                        if (!att.IsValidPointer)
                        {
                            return false;
                        }
                        Curve = new TCurve<float, float>();
                        Curve.LoadXnd(att);
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
                    unsafe
                    {
                        using (var att = xndHolder.NewAttribute("Curve", 1, 0))
                        {
                            Curve.SaveXnd(att);
                            parentNode.AddAttribute(att.CppPointer);
                        }
                    }
                    await Thread.AsyncDummyClass.DummyFunc();
                }, Thread.Async.EAsyncTarget.AsyncIO);
                return true;
            }
            #endregion
        }
    }
}
