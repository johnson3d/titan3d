using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.HollowMaker
{
    [Rtti.MetaClass]
    public class PVSVoxel
    {
        [Rtti.MetaData]
        public BoundingBox Shape;

        public Graphics.Mesh.CGfxMeshCooker.EBoxFace FaceType = Graphics.Mesh.CGfxMeshCooker.EBoxFace.None;
        [Rtti.MetaData]
        public List<int> LinkedVoxels = new List<int>();

        [Rtti.MetaData]
        public Support.BitSet Bits;
        public int mTempBitsIndex = -1;
        [Rtti.MetaData]
        public UInt32 BitsHash;

        [Rtti.MetaData]
        public int X;
        [Rtti.MetaData]
        public int Y;
        [Rtti.MetaData]
        public int Z;

        public static void WhenMetaDataBeginLoad()
        {
            
        }

        private bool IsCluster(PVSVoxel vx)
        {
            if (vx.BitsHash != BitsHash)
                return false;

            if (vx.Bits.IsSame(Bits) == false)
                return false;

            return true;
        }
    }
    public class CombineVoxel
    {
        public CombineVoxel(PVSVoxel vx)
        {
            OriFlagNumber = vx.Bits.FlagNumber;
            CombineBits = new Support.BitSet();
            CombineBits.Init(vx.Bits.BitCount, vx.Bits.Data);
        }
        public int CmbIndex = -1;
        private int OriFlagNumber = 0;
        public Support.BitSet CombineBits;
        public bool TryCombine(PVSVoxel vx, int tolerance)
        {
            int flagNumber = 0;
            var combine = Support.BitSet.BitOr(CombineBits, vx.Bits, ref flagNumber);
            if (flagNumber - OriFlagNumber < tolerance)
            {
                CombineBits = combine;
                return true;
            }

            return false;
        }

        public List<PVSVoxel> Voxels = new List<PVSVoxel>();
        public bool FindVoxel(PVSVoxel vx)
        {
            foreach(var i in Voxels)
            {
                if (i == vx)
                    return true;
            }
            return false;
        }        

        public Graphics.Mesh.CGfxMeshPrimitives BuildMesh()
        {
            var rc = CEngine.Instance.RenderContext;
            var meshs = new List<Graphics.Mesh.CGfxMeshPrimitives>();
            var trans = new List<Matrix>();
            foreach (var i in Voxels)
            {
                if ((i.FaceType & Graphics.Mesh.CGfxMeshCooker.EBoxFace.All) ==
                        Graphics.Mesh.CGfxMeshCooker.EBoxFace.All)
                    continue;
                var center = i.Shape.Minimum;
                var size = i.Shape.GetSize();
                var mesh = CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, 1);
                Graphics.Mesh.CGfxMeshCooker.MakeBox(rc, mesh,
                    center.X,
                    center.Y,
                    center.Z,
                    size.X,
                    size.Y,
                    size.Z,
                    ~i.FaceType);

                //return mesh;

                meshs.Add(mesh);
                trans.Add(Matrix.Identity);
            }
            var mergedMesh = CGeometryMesh.MergeGeomsAsMeshSimple(rc, meshs, trans);
            return mergedMesh;
        }
        public class BoxVolume
        {
            public int xMin = 0;
            public int yMin = 0;
            public int zMin = 0;
            public int xMax = 0;
            public int yMax = 0;
            public int zMax = 0;
            public List<PVSVoxel> Voxels = new List<PVSVoxel>();
            public CombineVoxel Combine;
            public BoxVolume(CombineVoxel cmb)
            {
                Combine = cmb;
            }
        }
        public void BuildBoxList(PVSBuilder builder, List<CombineVoxel.BoxVolume> bvs)
        {
            var vxFull = FullFill(builder);
            BoxVolume bv = null;
            do
            {
                bv = FindBest(vxFull);
                if (bv != null)
                {
                    bvs.Add(bv);
                }
            } while (bv != null);
        }
        private PVSVoxel[,,] FullFill(PVSBuilder builder)
        {
            var result = new PVSVoxel[builder.MaxX, builder.MaxY, builder.MaxZ];

            int xMin = int.MaxValue;
            int yMin = int.MaxValue;
            int zMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMax = int.MinValue;
            int zMax = int.MinValue;
            foreach (var i in Voxels)
            {
                if (i.X <= xMin)
                    xMin = i.X;
                if (i.Y <= yMin)
                    yMin = i.Y;
                if (i.Z <= zMin)
                    zMin = i.Z;
                if (i.X >= xMax)
                    xMax = i.X;
                if (i.Y >= yMax)
                    yMax = i.Y;
                if (i.Z >= zMax)
                    zMax = i.Z;
                result[i.X, i.Y, i.Z] = i;
            }
            return result;
        }
        private BoxVolume FindBest(PVSVoxel[,,] all)
        {
            int maxNum = 0;
            BoxVolume result = new BoxVolume(this);
            for (int x = 0; x < all.GetLength(0); x++)
            {
                for (int y = 0; y < all.GetLength(1); y++)
                {
                    for (int z = 0; z < all.GetLength(2); z++)
                    {
                        if (all[x, y, z] == null)
                            continue;

                        int xMin = 0;
                        int yMin = 0;
                        int zMin = 0;
                        int xMax = 0;
                        int yMax = 0;
                        int zMax = 0;
                        var num = GetExtendNum(all, all[x,y,z], ref xMin, ref yMin, ref zMin, ref xMax, ref yMax, ref zMax);
                        if (num > maxNum)
                        {
                            maxNum = num;
                            result.xMin = xMin;
                            result.yMin = yMin;
                            result.zMin = zMin;

                            result.xMax = xMax;
                            result.yMax = yMax;
                            result.zMax = zMax;
                        }
                    }
                }
            }
            if (maxNum == 0)
            {
                return null;
            }
            else
            {
                BuildBoxVolue(all, result);
            }
            
            return result;
        }
        private void BuildBoxVolue(PVSVoxel[,,] all, BoxVolume v)
        {
            for (int x = v.xMin; x <= v.xMax; x++)
            {
                for (int y = v.yMin; y <= v.yMax; y++)
                {
                    for (int z = v.zMin; z <= v.zMax; z++)
                    {
                        v.Voxels.Add(all[x, y, z]);
                        all[x, y, z] = null;
                    }
                }
            }
        }

        private int GetExtendNum(PVSVoxel[,,] all, PVSVoxel vx, 
            ref int xMin,
            ref int yMin,
            ref int zMin,
            ref int xMax,
            ref int yMax,
            ref int zMax)
        {
            xMin = vx.X;
            yMin = vx.Y;
            zMin = vx.Z;
            xMax = xMin;
            yMax = yMin;
            zMax = zMin;

            bool b_xMin = false;
            bool b_yMin = false;
            bool b_zMin = false;
            bool b_xMax = false;
            bool b_yMax = false;
            bool b_zMax = false;
            while (b_xMin == false ||
                b_xMax == false ||
                b_yMin == false ||
                b_yMax == false ||
                b_zMin == false ||
                b_zMax == false )
            {
                if (b_xMin==false)
                {
                    if (xMin == 0)
                    {
                        b_xMin = true;
                    }
                    else
                    {
                        xMin--;
                        bool canExt = true;
                        for (int y = yMin; y <= yMax; y++)
                        {
                            for (int z = zMin; z <= zMax; z++)
                            {
                                if (all[xMin, y, z] == null)
                                {
                                    canExt = false;
                                    break;
                                }
                            }
                        }
                        if (canExt == false)
                        {
                            xMin++;
                            b_xMin = true;
                        }
                    }
                }
                if (b_zMin==false)
                {
                    if(zMin==0)
                    {
                        b_zMin = true;
                    }
                    else
                    {
                        zMin--;
                        bool canExt = true;
                        for (int x = xMin; x <= xMax; x++)
                        {
                            for (int y = yMin; y <= yMax; y++)
                            {
                                if (all[x, y, zMin] == null)
                                {
                                    canExt = false;
                                    break;
                                }
                            }
                        }
                        if (canExt == false)
                        {
                            zMin++;
                            b_zMin = true;
                        }
                    }
                }
                if (b_xMax == false)
                {
                    if (xMax == all.GetLength(0)-1)
                    {
                        b_xMax = true;
                    }
                    else
                    {
                        xMax++;
                        bool canExt = true;
                        for (int y = yMin; y <= yMax; y++)
                        {
                            for (int z = zMin; z <= zMax; z++)
                            {
                                if (all[xMax, y, z] == null)
                                {
                                    canExt = false;
                                    break;
                                }
                            }
                        }
                        if (canExt == false)
                        {
                            xMax--;
                            b_xMax = true;
                        }
                    }
                }
                if (b_zMax == false)
                {
                    if(zMax == all.GetLength(2)-1)
                    {
                        b_zMax = true;
                    }
                    else
                    {
                        zMax++;
                        bool canExt = true;
                        for (int x = xMin; x <= xMax; x++)
                        {
                            for (int y = yMin; y <= yMax; y++)
                            {
                                if (all[x, y, zMax] == null)
                                {
                                    canExt = false;
                                    break;
                                }
                            }
                        }
                        if (canExt == false)
                        {
                            zMax--;
                            b_zMax = true;
                        }
                    }
                }
                if (b_yMin == false)
                {
                    if (yMin == 0)
                    {
                        b_yMin = true;
                    }
                    else
                    {
                        yMin--;
                        bool canExt = true;
                        for (int x = xMin; x <= xMax; x++)
                        {
                            for (int z = zMin; z <= zMax; z++)
                            {
                                if (all[x, yMin, z] == null)
                                {
                                    canExt = false;
                                    break;
                                }
                            }
                        }
                        if (canExt == false)
                        {
                            yMin++;
                            b_yMin = true;
                        }
                    }
                }
                if (b_yMax == false)
                {
                    if(yMax == all.GetLength(1)-1)
                    {
                        b_yMax = true;
                    }
                    else
                    {
                        yMax++;
                        bool canExt = true;
                        for (int x = xMin; x <= xMax; x++)
                        {
                            for (int z = zMin; z <= zMax; z++)
                            {
                                if (all[x, yMax, z] == null)
                                {
                                    canExt = false;
                                    break;
                                }
                            }
                        }
                        if (canExt == false)
                        {
                            yMax--;
                            b_yMax = true;
                        }
                    }
                }
            }

            return (xMax - xMin + 1) + (yMax - yMin + 1) + (zMax - zMin + 1);
        }
    }
    public class PVSBuilder
    {
        public PVSVoxel[] Voxels;

        public int MaxX;
        public int MaxY;
        public int MaxZ;
        private PVSVoxel[,,] FullVoxel;
        public Vector3 BVSize;
        public Matrix WorldMatrix;
        private List<Support.BitSet> mBitSets = new List<Support.BitSet>();
        private class BitExtData
        {
            public UInt32 Hash;
            public int MergeNumber;
        }
        private List<BitExtData> mBitSetExts = new List<BitExtData>();
        public List<CombineVoxel> UnitedCluster = new List<CombineVoxel>();
        private List<Support.BitSet> mCombinedBitSets = null;
        public void BuildUnitedCluster(int tolerance)
        {
            List<PVSVoxel> procVoxels = new List<PVSVoxel>();
            procVoxels.AddRange(Voxels);

            while(procVoxels.Count>0)
            {
                var cur = procVoxels[0];
                procVoxels.RemoveAt(0);
                CombineVoxel cmb = new CombineVoxel(cur);
                UnitedCluster.Add(cmb);
                ProcNeighbour(cmb, cur, procVoxels, tolerance);
            }

            mCombinedBitSets = new List<Support.BitSet>(UnitedCluster.Count);
            int totalVis = 0;
            int maxVis = 0;
            for (int i = 0; i < UnitedCluster.Count; i++)
            {
                UnitedCluster[i].CmbIndex = mCombinedBitSets.Count;
                mCombinedBitSets.Add(UnitedCluster[i].CombineBits);

                var num = UnitedCluster[i].CombineBits.FlagNumber;
                totalVis += num;
                if (num > maxVis)
                    maxVis = num;
            }
            totalVis /= UnitedCluster.Count;

            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "PVSBuilder", $"平均每个Volume可见对象个数：{totalVis},最大可见：{maxVis}");
        }
        public void BuildBoxVolume(List<CombineVoxel.BoxVolume> bvs)
        {
            bvs.Clear();
            for (int i = 0; i < UnitedCluster.Count; i++)
            {
                UnitedCluster[i].BuildBoxList(this, bvs);
            }
        }
        public GamePlay.SceneGraph.GPvsSet CreatePVSSet(List<CombineVoxel.BoxVolume> bvs)
        {
            var retSet = new GamePlay.SceneGraph.GPvsSet();
            retSet.WorldMatrix = this.WorldMatrix;
            retSet.WorldMatrixInv = Matrix.Invert(ref this.WorldMatrix);
            retSet.PvsData = mCombinedBitSets;
            retSet.PvsCells = new List<GamePlay.SceneGraph.GPvsCell>(bvs.Count);

            var offset = BVSize / 2;
            for (int i=0; i<bvs.Count; i++)
            {
                var cellSize = bvs[i].Voxels[0].Shape.GetSize();
                var cell = new GamePlay.SceneGraph.GPvsCell();
                cell.HostSet = retSet;

                cell.BoundVolume.Minimum.X = cellSize.X * (float)bvs[i].xMin;
                cell.BoundVolume.Minimum.Y = cellSize.Y * (float)bvs[i].yMin;
                cell.BoundVolume.Minimum.Z = cellSize.Z * (float)bvs[i].zMin;
                cell.BoundVolume.Minimum -= offset;

                cell.BoundVolume.Maximum.X = cellSize.X * (float)(bvs[i].xMax + 1);
                cell.BoundVolume.Maximum.Y = cellSize.Y * (float)(bvs[i].yMax + 1);
                cell.BoundVolume.Maximum.Z = cellSize.Z * (float)(bvs[i].zMax + 1);
                cell.BoundVolume.Maximum -= offset;

                //cell.PvsIndex = FindBitSetIndex(bvs[i].Voxels[0].Bits);
                cell.PvsIndex = bvs[i].Combine.CmbIndex;
                System.Diagnostics.Debug.Assert(cell.PvsIndex >= 0);

                retSet.PvsCells.Add(cell);
            }
            return retSet;
        }
        public void SaveXnd(IO.XndNode node, List<CombineVoxel.BoxVolume> bvs)
        {
            var set = new GamePlay.SceneGraph.GPvsSet();
            set.WorldMatrix = this.WorldMatrix;
            set.WorldMatrixInv = Matrix.Invert(ref this.WorldMatrix);
            set.PvsData = mBitSets;
            set.PvsCells = new List<GamePlay.SceneGraph.GPvsCell>(bvs.Count);
            for (int i = 0; i < bvs.Count; i++)
            {
                var cellSize = bvs[i].Voxels[0].Shape.GetSize();
                var cell = new GamePlay.SceneGraph.GPvsCell();

                cell.BoundVolume.Minimum.X = cellSize.X * (float)bvs[i].xMin;
                cell.BoundVolume.Minimum.Y = cellSize.Y * (float)bvs[i].yMin;
                cell.BoundVolume.Minimum.Z = cellSize.Z * (float)bvs[i].zMin;

                cell.BoundVolume.Maximum.X = cellSize.X * (float)bvs[i].xMax;
                cell.BoundVolume.Maximum.Y = cellSize.Y * (float)bvs[i].yMax;
                cell.BoundVolume.Maximum.Z = cellSize.Z * (float)bvs[i].zMax;

                cell.PvsIndex = FindBitSetIndex(bvs[i].Voxels[0].Bits);
                System.Diagnostics.Debug.Assert(cell.PvsIndex >= 0);

                set.PvsCells.Add(cell);
            }
            var att = node.AddAttrib("BoxVolumes");
            att.BeginWrite();
            att.WriteMetaObject(set);
            att.EndWrite();
        }
        public static GamePlay.SceneGraph.GPvsSet LoadXnd(IO.XndNode node)
        {
            var att = node.FindAttrib("BoxVolumes");
            att.BeginRead();
            var retVal = new GamePlay.SceneGraph.GPvsSet();
            att.ReadMetaObject(retVal);
            att.EndRead();

            return retVal;
        }
        private static Agent.GeoBox.BoxFace GetOppositeFace(Agent.GeoBox.BoxFace face)
        {
            switch(face)
            {
                case Agent.GeoBox.BoxFace.Front:
                    return Agent.GeoBox.BoxFace.Back;
                case Agent.GeoBox.BoxFace.Back:
                    return Agent.GeoBox.BoxFace.Front;
                case Agent.GeoBox.BoxFace.Left:
                    return Agent.GeoBox.BoxFace.Right;
                case Agent.GeoBox.BoxFace.Right:
                    return Agent.GeoBox.BoxFace.Left;
                case Agent.GeoBox.BoxFace.Top:
                    return Agent.GeoBox.BoxFace.Bottom;
                case Agent.GeoBox.BoxFace.Bottom:
                    return Agent.GeoBox.BoxFace.Top;
                default:
                    return Agent.GeoBox.BoxFace.Number;
            }
        }
        private void ProcNeighbour(CombineVoxel cmb, PVSVoxel vx, List<PVSVoxel> procVoxels, int tolerance)
        {
            cmb.Voxels.Add(vx);
            for (Agent.GeoBox.BoxFace i = Agent.GeoBox.BoxFace.StartIndex; i < Agent.GeoBox.BoxFace.Number; i++)
            {
                int index = vx.LinkedVoxels[(int)i];
                if (index < 0)
                    continue;
                if (cmb.TryCombine(Voxels[index], tolerance))
                {
                    vx.FaceType |= (Graphics.Mesh.CGfxMeshCooker.EBoxFace)(1 << (int)i);
                    var oppositeFace = (int)GetOppositeFace(i);
                    Voxels[index].FaceType |= (Graphics.Mesh.CGfxMeshCooker.EBoxFace)(1 << oppositeFace);
                    if (false == Remove(procVoxels, Voxels[index]))
                    {
                        //System.Diagnostics.Debug.Assert(cmb.FindVoxel(Voxels[index]));
                    }
                    else
                    {
                        ProcNeighbour(cmb, Voxels[index], procVoxels, tolerance);
                    }
                }
            }
        }

        private bool Remove(List<PVSVoxel> lst, PVSVoxel vx)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i] == vx)
                {
                    lst.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        
        private bool TryMerge(Support.BitSet bs1, Support.BitSet bs2)
        {
            if (bs1.BitCount != bs2.BitCount)
                return false;

            UInt32 actorNum = bs1.BitCount / 2;
            for (UInt32 i = 0; i < actorNum; i++)
            {
                int b1 = bs1.IsBit(i * 2) ? 1 : 0;
                int b2 = bs1.IsBit(i * 2 + 1) ? 1 : 0;
                int v1 = b1 | (b2 << 1);

                b1 = bs2.IsBit(i * 2) ? 1 : 0;
                b2 = bs2.IsBit(i * 2 + 1) ? 1 : 0;
                int v2 = b1 | (b2 << 1);

                if (v1 == 0 && v2 != 0)
                {
                    return false;
                }
                if (v2 == 0 && v1 != 0)
                {
                    return false;
                }
                if (v1 < v2)
                {
                    bs1.SetBit(i * 2, b1 == 0 ? false : true);
                    bs1.SetBit(i * 2 + 1, b2 == 0 ? false : true);
                }
            }

            return true;
        }
        private int FindBitSet(Support.BitSet bs, UInt32 hash)
        {
            for(int i = 0; i<mBitSets.Count; i++)
            {
                if (hash != mBitSetExts[i].Hash)
                    continue;
                if (mBitSets[i].IsSame(bs))
                    return i;
                //if (TryMerge(i, bs))
                //    return i;
            }
            mBitSets.Add(bs);
            var ext = new BitExtData();
            ext.Hash = hash;
            mBitSetExts.Add(ext);
            return mBitSets.Count - 1;
        }
        private int FindBitSetIndex(Support.BitSet bs)
        {
            for (int i = 0; i < mBitSets.Count; i++)
            {
                if (mBitSets[i] == bs)
                    return i;
            }
            return -1;
        }
        
        public void LoadPVSVoxels(string absFolder)
        {
            using (var xnd = EngineNS.IO.XndHolder.SyncLoadXND(absFolder + "/vis.data"))
            {
                var attr = xnd.Node.FindAttrib("voxelData");
                attr.BeginRead();
                attr.Read(out BVSize);
                attr.Read(out WorldMatrix);
                int count = 0;
                attr.Read(out count);

                Voxels = new PVSVoxel[count];
                MaxX = -1;
                MaxY = -1;
                MaxZ = -1;

                for (int i = 0; i < count; i++)
                {
                    var voxel = attr.ReadMetaObject(null) as PVSVoxel;
                    voxel.mTempBitsIndex = FindBitSet(voxel.Bits, voxel.BitsHash);
                    voxel.Bits = mBitSets[voxel.mTempBitsIndex];
                    Voxels[i] = voxel;

                    if (voxel.X > MaxX)
                        MaxX = voxel.X;
                    if (voxel.Y > MaxY)
                        MaxY = voxel.Y;
                    if (voxel.Z > MaxZ)
                        MaxZ = voxel.Z;
                }
                attr.EndRead();

                MaxX++;
                MaxY++;
                MaxZ++;
                FullVoxel = new PVSVoxel[MaxX, MaxY, MaxZ];
                foreach (var i in Voxels)
                {
                    FullVoxel[i.X, i.Y, i.Z] = i;
                }
            }
        }

        private void MergeBitsets(int tolerance = 10)
        {
            int maxMerge = 0;
            int mergeIndex = -1;
            for (int i = 0; i < mBitSets.Count; i++)
            {
                mBitSetExts[i].MergeNumber = 0;
                for (int j = 0; j < mBitSets.Count; j++)
                {
                    if (i == j)
                        continue;
                    int flagNumber = 0;
                    Support.BitSet.BitOr(mBitSets[i], mBitSets[j], ref flagNumber);
                    if(flagNumber <= tolerance)
                        mBitSetExts[i].MergeNumber++;
                }
                if(maxMerge < mBitSetExts[i].MergeNumber)
                {
                    mergeIndex = i;
                    maxMerge = mBitSetExts[i].MergeNumber;
                }
            }
            if(maxMerge>=0)
            {
                //Support.BitSet mergedBitSet = null;
                //mergedBitSet = 
                for (int j = 0; j < mBitSets.Count; j++)
                {
                    if (maxMerge == j)
                        continue;
                    int flagNumber = 0;
                    Support.BitSet.BitOr(mBitSets[maxMerge], mBitSets[j], ref flagNumber);
                    if (flagNumber <= tolerance)
                    {
                        //mBitSetExts[i].MergeNumber++;
                    }
                }
            }
        }
    }
}
