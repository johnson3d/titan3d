using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DistanceField
{
    public struct DistanceFieldConfig
    {
        public DistanceFieldConfig()
        { }

        // One voxel border around object for handling gradient
        public int MeshDistanceFieldObjectBorder = 1;
        public int UniqueDataBrickSize = 7;
        // Half voxel border around brick for trilinear filtering
        public int BrickSize = 8;
        // Trade off between SDF memory and number of steps required to find intersection
        public int BandSizeInVoxels = 4;

        public int MaxPerMeshResolution = 256;
        public int MaxIndirectionDimension = 1024;

        public float fDefaultVoxelDensity = 20.0f;

        public static int NumMips = 1; // TODO
        public uint InvalidBrickIndex = uint.MaxValue;
    };

    public class TtSparseSdfMip : IO.BaseSerializer
    {
        [Rtti.Meta]
        public Vector3i IndirectionDimensions = Vector3i.Zero;
        [Rtti.Meta]
        public int NumDistanceFieldBricks = 0;
        [Rtti.Meta]
        public Vector3 VolumeToVirtualUVScale = Vector3.Zero;
        [Rtti.Meta]
        public Vector3 VolumeToVirtualUVAdd = Vector3.Zero;
        [Rtti.Meta]
        public Vector2 DistanceFieldToVolumeScaleBias = Vector2.Zero;

        [Rtti.Meta]
        public List<uint> IndirectionTable = new List<uint>();
        [Rtti.Meta]
        public List<Byte> DistanceFieldBrickData = new List<byte>();
    };


    [Rtti.Meta]
    public class TtSdfAssetAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtSdfAsset.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.SdfAssetManager.GetSdfAsset(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "SDFVolumeData";
        }
    }

    [Rtti.Meta]
    public class TtSdfAsset : IO.BaseSerializer, IO.IAsset
    {
        public TtSdfAsset()
        {
            bMostlyTwoSided = false;
            Id = NextId.Value;
            NextId++;
            Mips = new List<TtSparseSdfMip>();
        }

        #region IO.IAsset
        public const string AssetExt = ".sdf";
        [Rtti.Meta]
        public RName AssetName { get; set; }

        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtSdfAssetAMeta();
            return result;

        }

        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void SaveAssetTo(RName name)
        {
            AssetName = name;
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("VolumeData", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
            UEngine.Instance.SourceControlModule.AddFile(name.Address);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        #endregion

        public static TtSdfAsset LoadXnd(TtSdfAssetManager manager, IO.TtXndNode node)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = node.TryGetAttribute("VolumeData");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }

                var sdfData = result as TtSdfAsset;
                if (sdfData != null)
                {
                    return sdfData;
                }
                return null;
            }
        }

        /** Local space bounding box of the distance field volume. */
        [Rtti.Meta]
        public BoundingBox LocalSpaceMeshBounds;

        /** Whether most of the triangles in the mesh used a two-sided material. */
        [Rtti.Meta]
        public bool bMostlyTwoSided;

        public int Id;
        static Thread.TtAtomic_Int NextId = new Thread.TtAtomic_Int();

        [Rtti.Meta]
        public List<TtSparseSdfMip> Mips;


    };


    public partial class TtSdfAssetManager
    {
        public Dictionary<RName, TtSdfAsset> sdfAssets { get; } = new Dictionary<RName, TtSdfAsset>();
        public async System.Threading.Tasks.Task<TtSdfAsset> GetSdfAsset(RName name)
        {
            TtSdfAsset result;
            if (sdfAssets.TryGetValue(name, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var sdfAsset = TtSdfAsset.LoadXnd(this, xnd.RootNode);
                        if (sdfAsset == null)
                            return null;

                        sdfAsset.AssetName = name;
                        return sdfAsset;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                sdfAssets[name] = result;
                return result;
            }

            return null;
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public DistanceField.TtSdfAssetManager SdfAssetManager { get; } = new DistanceField.TtSdfAssetManager();
    }
}