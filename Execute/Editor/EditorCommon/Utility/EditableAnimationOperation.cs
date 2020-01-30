using EditorCommon.Controls.Animation;
using EditorCommon.Controls.ResourceBrowser;
using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Utility
{
    public class EditableAnimationOperation
    {
        static Dictionary<RName, BlendSpace> mBlendSpaceDic = new Dictionary<RName, BlendSpace>();
        public static BlendSpace1D CreateBlendSpace1D(RName name)
        {
            if (mBlendSpaceDic.ContainsKey(name))
            {
                return mBlendSpaceDic[name] as BlendSpace1D;
            }
            else
            {
                var bs =BlendSpace1D.CreateSync(name);
                if (bs == null)
                    return null;
                mBlendSpaceDic.Add(name, bs);
                return bs;
            }
        }
        public static BlendSpace2D CreateBlendSpace(RName name)
        {
            if (mBlendSpaceDic.ContainsKey(name))
            {
                return mBlendSpaceDic[name] as BlendSpace2D;
            }
            else
            {
                var bs = BlendSpace2D.CreateSync(name);
                if (bs == null)
                    return null;
                mBlendSpaceDic.Add(name, bs);
                return bs;
            }
        }
        public static AdditiveBlendSpace1D CreateAdditiveBlendSpace1D(RName name)
        {
            if (mBlendSpaceDic.ContainsKey(name))
            {
                return mBlendSpaceDic[name] as AdditiveBlendSpace1D;
            }
            else
            {
                var bs = AdditiveBlendSpace1D.CreateSync(name);
                if (bs == null)
                    return null;
                mBlendSpaceDic.Add(name, bs);
                return bs;
            }
        }
        public static AdditiveBlendSpace2D CreateAdditiveBlendSpace2D(RName name)
        {
            if (mBlendSpaceDic.ContainsKey(name))
            {
                return mBlendSpaceDic[name] as AdditiveBlendSpace2D;
            }
            else
            {
                var bs = AdditiveBlendSpace2D.CreateSync(name);
                if (bs == null)
                    return null;
                mBlendSpaceDic.Add(name, bs);
                return bs;
            }
        }
    }
}
