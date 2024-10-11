using EngineNS.IO;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset.BlendSpace
{
    [Rtti.Meta]
    public class TtBlendSpace2DAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtBlendSpace2D.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "Blend Space";
        }
    }

    [TtBlendSpace2D.BlendSpaceCreate]
    [IO.AssetCreateMenu(MenuName = "Anim/BlendSpace")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtBlendSpace2D : TtBlendSpace
    {
        public TtBlendSpace2D() 
        {
            BlendAxises[0] = new TtBlendSpace_Axis("None");
            BlendAxises[1] = new TtBlendSpace_Axis("None");
            BlendAxises[2] = new TtBlendSpace_Axis("None");
        }
        ~TtBlendSpace2D()
        {

        }

        protected override void ExtractWeightedTrianglesByInput(Vector3 input, List<FWeightedTriangle> gridSamples)
        {
            var griddingIuput = GriddingInput(input);
            var gridIndex = new Vector3((int)Math.Truncate(griddingIuput.X), (int)Math.Truncate(griddingIuput.Y), 0);
            var remainder = griddingIuput - gridIndex;

            TtBlentSpace_Triangle EleLT = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y + 1);
            var LeftTop = new FWeightedTriangle();
            if (EleLT != null)
            {
                LeftTop.Triangle = EleLT;
                // now calculate weight - distance to each corner since input is already normalized within grid, we can just calculate distance 
                LeftTop.BlendWeight = (1.0f - remainder.X) * remainder.Y;
            }
            else
            {
                LeftTop.Triangle = new TtBlentSpace_Triangle();
                LeftTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftTop);

            TtBlentSpace_Triangle EleRT = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y + 1);
            var RightTop = new FWeightedTriangle();
            if (EleRT != null)
            {
                RightTop.Triangle = EleRT;
                RightTop.BlendWeight = remainder.X * remainder.Y;
            }
            else
            {
                RightTop.Triangle = new TtBlentSpace_Triangle();
                RightTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightTop);

            TtBlentSpace_Triangle EleLB = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y);
            var LeftBottom = new FWeightedTriangle();
            if (EleLB != null)
            {
                LeftBottom.Triangle = EleLB;
                LeftBottom.BlendWeight = (1.0f - remainder.X) * (1.0f - remainder.Y);
            }
            else
            {
                LeftBottom.Triangle = new TtBlentSpace_Triangle();
                LeftBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftBottom);

            TtBlentSpace_Triangle EleRB = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y);
            var RightBottom = new FWeightedTriangle();
            if (EleRB != null)
            {
                RightBottom.Triangle = EleRB;
                RightBottom.BlendWeight = remainder.X * (1.0f - remainder.Y);
            }
            else
            {
                RightBottom.Triangle = new TtBlentSpace_Triangle();
                RightBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightBottom);
        }
        TtBlentSpace_Triangle GetEditorElement(int XIndex, int YIndex)
        {
            int Index = XIndex * (BlendAxises[1].GridNum + 1) + YIndex;
            if (GridTriangles.Count > Index)
                return GridTriangles[Index];
            return null;
        }
        protected override void ReConstructTrangles()
        {
            var axisX = BlendAxises[0];
            var axisY = BlendAxises[1];
            TtDelaunayTriangleMaker maker = new TtDelaunayTriangleMaker();
            TtBlendSpaceGrid grid = new TtBlendSpaceGrid();
            grid.SetGridInfo(axisX, axisY);
            maker.SetGridBox(axisX, axisY);
            for (int i = 0; i < AnimPoints.Count; ++i)
            {
                maker.AddSample(AnimPoints[i].Value, i);
            }

            maker.Triangulate();

            var points = maker.GetSamplePointList();
            var triangles = maker.GetTriangleList();
            grid.GenerateGridElements(points, triangles);
            if (triangles.Count > 0)
            {
                var gridElements = grid.GetElements();
                FillTheGrid(maker.GetIndiceMapping().ToArray(), gridElements);
            }
        }
        #region IAnimationAsset
        public class BlendSpaceCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                PGAsset.Target = mAsset;

                mAsset = Rtti.TtTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;

            }
        }
        public const string AssetExt = ".blendspace2d";
        public override string TypeExt { get => AssetExt; }
        [Rtti.Meta]
        public override RName AssetName { get; set; }
        public override IAssetMeta CreateAMeta()
        {
            var result = new TtBlendSpace2DAMeta();
            return result;
        }

        public override IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public override void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }

        public override void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("BlendSpace", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }
            xnd.SaveXnd(name.Address);
        }
        public static TtBlendSpace2D LoadXnd(TtBlendSpace2DManager manager, IO.TtXndHolder holder)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = holder.RootNode.TryGetAttribute("BlendSpace");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }
                var blendSpace = result as TtBlendSpace2D;
                if (blendSpace != null)
                {
                    return blendSpace;
                }
                return null;
            }
        }

        #endregion
    }

    public class TtBlendSpace2DManager
    {
        Dictionary<RName, TtBlendSpace2D> BlendSpaces = new();

        //for new don't see animationChunk as the asset
        public async TtTask<TtBlendSpace2D> GetAnimation(RName name)
        {
            TtBlendSpace2D result;
            if (BlendSpaces.TryGetValue(name, out result))
                return result;

            //this is a demo for suspend&cancel operation
            Thread.Async.TtAsyncTaskToken token = null;// new Thread.Async.TtAsyncTaskToken();
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                if (state.TaskToken != null)
                {
                    if (state.TaskToken.TaskState == Thread.Async.EAsyncTaskState.Suspended)
                    {
                        state.TaskState = Thread.Async.EAsyncTaskState.Suspended;
                        return null;
                    }
                    else if (state.TaskToken.TaskState == Thread.Async.EAsyncTaskState.Canceled)
                    {
                        state.TaskState = Thread.Async.EAsyncTaskState.Canceled;
                        return null;
                    }
                }
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var clip = TtBlendSpace2D.LoadXnd(this, xnd);
                        clip.SaveAssetTo(name);
                        if (clip == null)
                            return null;

                        clip.AssetName = name;
                        return clip;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO).WithToken(token);

            foreach (var point in result.AnimPoints)
            {
                point.Animation = await TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(point.AnimationName);
            }

            if (result != null)
            {
                BlendSpaces[name] = result;
                return result;
            }
            return null;
        }
        public void Remove(RName name)
        {
            BlendSpaces.Remove(name);
        }
    }
}
