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

    public class FSparseDistanceFieldMip
    {
        public Vector3i IndirectionDimensions = Vector3i.Zero;
        public int NumDistanceFieldBricks = 0;
        public Vector3 VolumeToVirtualUVScale = Vector3.Zero;
        public Vector3 VolumeToVirtualUVAdd = Vector3.Zero;
        public Vector2 DistanceFieldToVolumeScaleBias = Vector2.Zero;
        public uint BulkOffset = 0;
        public uint BulkSize = 0;

        public List<uint> IndirectionTable = new List<uint>();
        public List<Byte> DistanceFieldBrickData = new List<byte>();
    };

    public class UDistanceFieldVolumeData
    {
        public UDistanceFieldVolumeData()
        {
            bMostlyTwoSided = false;
            Id = NextId.Value;
            NextId++;
            Mips = new List<FSparseDistanceFieldMip>();
        }

        /** Local space bounding box of the distance field volume. */
        public BoundingBox LocalSpaceMeshBounds;

        /** Whether most of the triangles in the mesh used a two-sided material. */
        public bool bMostlyTwoSided;

        public int Id;
        static Thread.TtAtomic_Int NextId = new Thread.TtAtomic_Int();

        // For stats
        public string AssetName;

        public List<FSparseDistanceFieldMip> Mips;

        public void LoadXnd(IO.TtXndNode node)
        {

        }
        public void SaveXnd(IO.TtXndNode node)
        {

        }

    };

}
