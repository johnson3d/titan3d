using EngineNS.DesignMacross;
using EngineNS.IO;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.DistanceField
{
    public struct DistanceFieldConfig
    {
        public DistanceFieldConfig()
        { }

        public int MeshDistanceFieldObjectBorder = 0;
        // One voxel border around object for handling gradient
        //public int MeshDistanceFieldObjectBorder = 1;
        public int UniqueDataBrickSize = 7;
        public int BrickSize = 7;
        // Half voxel border around brick for trilinear filtering
        //public int BrickSize = 8;
        // Trade off between SDF memory and number of steps required to find intersection
        public int BandSizeInVoxels = 1;

        // Max voxel resolution per mesh
        public int MaxPerMeshResolution = 64;
        public int MaxIndirectionDimension = 1024;

        // voxel count in 1 meter
        public float fDefaultVoxelDensity = 10.0f;

        public static int NumMips = 1; // TODO
        public uint InvalidBrickIndex = uint.MaxValue;
    };

    public class TtSparseSdfMip : IO.BaseSerializer
    {
        public TtSparseSdfMip()
        {
            IndirectionDimensions = Vector3i.Zero;
            NumDistanceFieldBricks = 0;
            VolumeToVirtualUVScale = Vector3.Zero;
            VolumeToVirtualUVAdd = Vector3.Zero;
            DistanceFieldToVolumeScaleBias = Vector2.Zero;
            IndirectionTable = new List<uint>();
            DistanceFieldBrickData = new List<byte>();
        }
        public Vector3i GetVoxelDimensions()
        {
            DistanceFieldConfig sdfConfig = new DistanceFieldConfig();
            return IndirectionDimensions * sdfConfig.BrickSize;
        }
        [Rtti.Meta]
        public Vector3i IndirectionDimensions { get; set; }
        [Rtti.Meta]
        public int NumDistanceFieldBricks { get; set; }
        [Rtti.Meta]
        public Vector3 VolumeToVirtualUVScale { get; set; }
        [Rtti.Meta]
        public Vector3 VolumeToVirtualUVAdd { get; set; }
        [Rtti.Meta]
        public Vector2 DistanceFieldToVolumeScaleBias { get; set; }

        public List<uint> IndirectionTable { get; set; }
        public List<Byte> DistanceFieldBrickData { get; set; }
    };


    [Rtti.Meta]
    public class TtSdfAssetAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtSdfAsset.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.SdfAssetManager.GetSdfAsset(GetAssetName());
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
        public string TypeExt { get => AssetExt; }
        [Rtti.Meta]
        public RName AssetName { get; set; }

        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtSdfAssetAMeta();
            return result;

        }

        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void SaveAssetTo(RName name)
        {
            AssetName = name;
            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("VolumeData", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                    foreach (var mip in Mips)
                    {
                        var tableData = mip.IndirectionTable.ToArray();
                        unsafe
                        {
                            fixed (uint* tableDataPtr = tableData)
                            {
                                ar.Write(mip.IndirectionTable.Count);
                                ar.WritePtr((void*)tableDataPtr, mip.IndirectionTable.Count * sizeof(uint));
                            }
                        }
                        //byte[] byteTableData = new byte[mip.IndirectionTable.Count * sizeof(uint)];
                        //Buffer.BlockCopy(tableData, 0, byteTableData, 0, mip.IndirectionTable.Count);
                        //ar.Write(byteTableData);

                        var byteBrickData = mip.DistanceFieldBrickData.ToArray();
                        ar.Write(mip.DistanceFieldBrickData.Count);
                        ar.Write(byteBrickData);
                    }
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
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
                        var sdfData = result as TtSdfAsset;
                        if (sdfData != null)
                        {
                            foreach (var mip in sdfData.Mips)
                            {
                                int BrickCount = 0;
                                ar.Read(out BrickCount);
                                uint[] tableData = new uint[BrickCount];
                                fixed(uint *tableDataPtr = tableData)
                                {
                                    ar.ReadPtr(tableDataPtr, BrickCount*sizeof(uint));
                                }
                                mip.IndirectionTable = new List<uint>(tableData);

                                int sdfCount = 0;
                                ar.Read(out sdfCount);
                                byte[] byteBrickData = new byte[sdfCount];
                                ar.Read(out byteBrickData);
                                mip.DistanceFieldBrickData = new List<Byte>(byteBrickData);
                            }

                            return sdfData;
                        }
                    }
                }
               return null;
            }
        }

        public int GetAllocatedSize()
        {
            int memorySize = 0;
            foreach (var mip in Mips)
            {
                memorySize += mip.IndirectionTable.Count * sizeof(uint);
                memorySize += mip.DistanceFieldBrickData.Count * sizeof(Byte);
            }
            return memorySize;
        }
        /** Local space bounding box of the distance field volume. */
        [Rtti.Meta]
        public BoundingBox LocalSpaceMeshBounds { get; set; }

        /** Whether most of the triangles in the mesh used a two-sided material. */
        [Rtti.Meta]
        public bool bMostlyTwoSided { get; set; }

        public int Id;
        static Thread.TtAtomic_Int NextId = new Thread.TtAtomic_Int();

        [Rtti.Meta]
        public List<TtSparseSdfMip> Mips { get; set; }


    };


    public partial class TtSdfAssetManager
    {
        public Dictionary<RName, TtSdfAsset> sdfAssets { get; } = new Dictionary<RName, TtSdfAsset>();
        public async System.Threading.Tasks.Task<TtSdfAsset> GetSdfAsset(RName name)
        {
            TtSdfAsset result;
            if (sdfAssets.TryGetValue(name, out result))
                return result;

            result = await TtEngine.Instance.EventPoster.Post((state) =>
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
    partial class TtEngine
    {
        public DistanceField.TtSdfAssetManager SdfAssetManager { get; } = new DistanceField.TtSdfAssetManager();
    }
}