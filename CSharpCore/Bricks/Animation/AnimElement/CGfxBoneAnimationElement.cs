using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Bricks.Animation.Binding;
using EngineNS.Bricks.Animation.Curve;
using EngineNS.IO;

namespace EngineNS.Bricks.Animation.AnimElement
{
    public class CGfxBoneAnimationElement : CGfxAnimationElement
    {
        public CGfxBoneAnimationElement()
        {
            ElementType = AnimationElementType.Bone;
        }
        public CGfxBoneAnimationElement(NativePointer nativePointer) : base(nativePointer)
        {
            ElementType = AnimationElementType.Skeleton;
        }
        public override void Evaluate(float curveT, Binding.AnimationElementBinding bindingElement)
        {
            var binding = bindingElement as BoneAnimationBinding;
            if (binding == null)
                return;
            var boneCurve = Curve as CGfxBoneCurve;
            CurveResult curveResult = new CurveResult();
            curveResult.BoneSRTResult = binding.Bone.Transform;
            curveResult = boneCurve.Evaluate(curveT, ref curveResult);
            binding.MotionData = boneCurve.EvaluateMotionState(curveT);
            binding.Value = curveResult;

        }
        public override CGfxICurve Curve
        {
            get
            {
                if (mCurve == null)
                {
                    var nativeCurve = SDK_GfxAnimationElement_GetCurve(CoreObject);
                    mCurve = new CGfxBoneCurve(nativeCurve);
                }
                return mCurve;
            }
            set => base.Curve = value;
        }
        public override void SyncNative()
        {
            //base.SyncNative();
        }
        #region SaveLoad
        public override async Task<bool> Load(CRenderContext rc, XndNode node)
        {
            await CEngine.Instance.EventPoster.Post(async () =>
            {
                SyncLoad(rc, node);
                await Thread.AsyncDummyClass.DummyFunc();
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
        public override bool SyncLoad(CRenderContext rc, XndNode node)
        {
            var att = node.FindAttrib("AnimationElement");
            if (att != null)
            {
                att.BeginRead();
                att.ReadMetaObject(this);
                att.EndRead();
            }
            Curve = new Curve.CGfxBoneCurve();
            Curve.SyncLoad(rc, node);
            return true;
        }

        public override void Save(XndNode node)
        {
            base.Save(node);
        }
        #endregion
    }
}
