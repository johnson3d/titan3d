using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace EngineNS.Graphics.Pipeline.GI
{
    public class TtBVH
    {
        public static uint EncodeMorton3D(Vector3 pos)
        {
            if(Vector3.Less(pos, Vector3.Zero).Any() || Vector3.Great(pos, Vector3.One).Any())
            {
                throw new Exception("pos must be in [0,1)");
            }
            // 归一化到 [0,1) 后量化为 10-bit 整数（范围 0~1023）
            uint x = (uint)(pos.X * 1024.0f) & 0x3FF; // 取低10位
            uint y = (uint)(pos.Y * 1024.0f) & 0x3FF;
            uint z = (uint)(pos.Z * 1024.0f) & 0x3FF;

            uint code = 0;
            for (int i = 0; i < 10; ++i)
            {
                var r_move = (9 - i) * 3;
                var l_move = (9 - i);
                // 从最高位（第9位）到最低位（第0位）依次提取
                code |= ((x >> l_move) & 1) << (r_move + 2);
                code |= ((y >> l_move) & 1) << (r_move + 1);
                code |= ((z >> l_move) & 1) << (r_move + 0);
            }
            return code;
        }
        public class TtBVHPrimitive
        {
            public Vector3 Center;
            public uint MortenCode;
            public BoundingBox AABB;
            //public Vector4ui Payload;
        }
        public struct FBVHNode
        {
            public BoundingBox AABB;
            //public BoundingBox AABB1;
            public uint LeftChild;//右节点是左节点+1
            //public int ChildCount;//完全二叉树除了叶子节点外，其他节点的左子树和右子树个数相同

            public uint RightChild
            {
                get => LeftChild + 1;
            }
            public bool IsLeaf
            {
                get => (LeftChild & (1 << 31)) != 0;
            }
            public uint LeafPrimStartIndex
            {
                get => (LeftChild & (~(1 << 31)));
            }
            public void SetLeafPrimStartIndex(uint leaf)
            {
                LeftChild = leaf;
                LeftChild |= (1u << 31);
            }
        }
        public BoundingBox AABB;
        public List<TtBVHPrimitive> Primitives;
        public FBVHNode[] HbvNodes = null;
        public uint LeafPrimiviveCount = 1;//actually, value 1 is best 
        public uint PrimitiveGroup = 0;

        public unsafe bool Initialize(BoundingBox aabb, List<TtBVHPrimitive> prims, uint leafPrimiviveCount = 1)
        {
            AABB = aabb;
            Primitives = prims;
            LeafPrimiviveCount = leafPrimiviveCount;

            foreach (var i in prims)
            {
                if (AABB.Contains(i.Center) != ContainmentType.Contains)
                    return false;
                var local = (i.Center - AABB.Minimum) / AABB.GetSize();
                i.MortenCode = EncodeMorton3D(local);
            }

            Primitives.Sort((x, y) =>
            {
                return x.MortenCode.CompareTo(y.MortenCode);
            });

            return true;
        }
        public void BuildBVH()
        {
            uint level = 1;
            uint leafTreeNode = 1;
            uint totalTreeNode = 1;
            PrimitiveGroup = ((uint)Primitives.Count + (LeafPrimiviveCount - 1)) / LeafPrimiviveCount;
            while (leafTreeNode < PrimitiveGroup)
            {
                leafTreeNode *= 2;
                totalTreeNode += leafTreeNode;
                level++;
            }
            HbvNodes = new FBVHNode[totalTreeNode];
            uint leafStart = totalTreeNode - leafTreeNode;
            //目前是从左到右给Leaf，这样导致右边的子树AABB分布得不均匀，应该让leaf均匀分布对于bvh结构更友好
            for (uint i = 0; i < HbvNodes.Length; i++)
            {
                HbvNodes[i].AABB.InitEmptyBox();
                if (i >= leafStart)
                {
                    var index = i - leafStart;
                    if (index >= PrimitiveGroup)
                    {
                        HbvNodes[i].SetLeafPrimStartIndex(0xffffffff);
                    }
                    else
                    {
                        HbvNodes[i].SetLeafPrimStartIndex((i - leafStart)* LeafPrimiviveCount);
                    }
                }
                else
                {
                    var left = i * 2 + 1;
                    HbvNodes[i].LeftChild = left;
                }
            }
            for (uint i = leafStart; i < HbvNodes.Length; i++)
            {
                if (i - leafStart >= PrimitiveGroup)
                    break;
                
                for (uint j = 0; j < LeafPrimiviveCount; j++)
                {
                    var index = HbvNodes[i].LeafPrimStartIndex + j;
                    if (index > Primitives.Count)
                        continue;
                    HbvNodes[i].AABB = BoundingBox.Merge(HbvNodes[i].AABB, Primitives[(int)(index)].AABB);
                }

                uint cur = i;
                uint parent = (cur - 1) / 2;
                while (parent > 0)
                {
                    HbvNodes[parent].AABB = BoundingBox.Merge(HbvNodes[parent].AABB, HbvNodes[cur].AABB);

                    cur = parent;
                    parent = (cur - 1) / 2;
                }
            }
            HbvNodes[0].AABB = AABB;

            //UpdateAABB(ref HbvNodes[0]);
        }
        //private void UpdateAABB(ref FHBVNode node)
        //{
        //    node.AABB1.InitEmptyBox();
        //    if (node.IsLeaf)
        //    {
        //        for (uint j = 0; j < LeafPrimiviveCount; j++)
        //        {
        //            var index = (int)(node.LeafPrimStartIndex + j);
        //            if (index >= Primitives.Count)
        //                break;
        //            node.AABB1 = BoundingBox.Merge(node.AABB1, Primitives[index].AABB);
        //        }   
        //    }
        //    else
        //    {
        //        UpdateAABB(ref HbvNodes[node.LeftChild]);
        //        UpdateAABB(ref HbvNodes[node.RightChild]);
        //        node.AABB1 = BoundingBox.Merge(HbvNodes[node.LeftChild].AABB1, HbvNodes[node.RightChild].AABB1);
        //    }
        //    if (node.IsLeaf == false && node.AABB1.IsEmpty())
        //    {
        //        int xxx = 0;
        //    }
        //}
        public TtBVHPrimitive Hit(in Vector3 pos)
        {
            Queue<FBVHNode> q = new Queue<FBVHNode>();
            q.Enqueue(HbvNodes[0]);//queue root
            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (cur.AABB.Contains(in pos) == ContainmentType.Disjoint)
                    continue;
                if (cur.IsLeaf)
                {
                    var primIndex = cur.LeafPrimStartIndex;
                    if (primIndex >= PrimitiveGroup)
                        continue;
                    for (uint j = 0; j < LeafPrimiviveCount; j++)
                    {
                        var index = (int)(primIndex + j);
                        if (index >= Primitives.Count)
                            break;
                        if (Primitives[index].AABB.Contains(in pos) == ContainmentType.Contains)
                            return Primitives[index];
                    }
                }
                else
                {
                    q.Enqueue(HbvNodes[cur.LeftChild]);
                    q.Enqueue(HbvNodes[cur.RightChild]);
                }
            }
            return null;
        }

        public struct FRayDesc
        {
            public Vector3 Origin;
            public Vector3 Direction;
            public float TMin;
            public float TMax;
            public int HitPrimitiveIndex;
            public int NodeHitCount;
            public int PrimitiveHitCount;
        }
        public virtual bool PrimitiveIntersect(ref FRayDesc ray, in Ray curRay, int primIndex)
        {
            float dist = 0;
            for (uint i = 0; i < LeafPrimiviveCount; i++)
            {
                var index = (int)(primIndex + i);
                if (index >= Primitives.Count)
                    break;
                ray.PrimitiveHitCount++;
                if (Ray.Intersects(in curRay, in Primitives[index].AABB, out dist))
                {
                    //Call AnyHit
                    if (ray.TMax >= dist)
                    {
                        ray.TMax = dist;
                        ray.HitPrimitiveIndex = index;
                    }
                    return true;
                }
            }
                
            return false;
        }
        public int Intersects(ref FRayDesc ray)
        {
            //ray.HitPrimitiveIndex = -1;
            float dist = 0;
            var pos = ray.Origin + ray.Direction * ray.TMin;
            Ray curRay = new Ray(in pos, in ray.Direction);
            Queue<FBVHNode> q = new Queue<FBVHNode>();
            q.Enqueue(HbvNodes[0]);//queue root
            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                ray.NodeHitCount++;
                if (Ray.Intersects(in curRay, in cur.AABB, out dist) == false)
                {
                    continue;
                }
                if (cur.IsLeaf)
                {
                    var primIndex = (int)cur.LeafPrimStartIndex;
                    if (primIndex >= Primitives.Count)
                        continue;
                    PrimitiveIntersect(ref ray, in curRay, primIndex);
                }
                else
                {
                    if (HbvNodes[cur.LeftChild].AABB.IsEmpty() == false)
                        q.Enqueue(HbvNodes[cur.LeftChild]);
                    if (HbvNodes[cur.RightChild].AABB.IsEmpty() == false)
                        q.Enqueue(HbvNodes[cur.RightChild]);
                }
            }

            if (ray.HitPrimitiveIndex >= 0)
            {
                //call ClosestHit
                return ray.HitPrimitiveIndex;
            }
            return -1;
        }
    }
    public class TtBLAS
    {
    }

    [UnitTest.TtTest]
    public class TtGI_Tester
    {
        public unsafe void UnitTestEntrance()
        {
            var aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1024, 1024, 1024));
            var size = aabb.GetSize();
            var prims = new List<TtBVH.TtBVHPrimitive>();
            for (int i = 0; i < 1000; i++)
            {
                var center = MathHelper.RandomDirection(false);
                center = center * size;
                var prim = new TtBVH.TtBVHPrimitive();
                prim.Center = center;
                prim.AABB = new BoundingBox(center - new Vector3(0.5f, 0.5f, 0.5f), center + new Vector3(0.5f, 0.5f, 0.5f));
                prims.Add(prim);
            }
            var hbv = new TtBVH();
            hbv.Initialize(aabb, prims, 1);
            hbv.BuildBVH();

            var hbv10 = new TtBVH();
            hbv10.Initialize(aabb, prims, 10);
            hbv10.BuildBVH();

            var t = hbv.Hit(prims[0].Center);
            if (t == prims[0])
            {

            }
            var ray = new TtBVH.FRayDesc();
            for (int i = 0; i < 10000; i++)
            {
                ray.Direction = MathHelper.RandomDirection();
                ray.Origin = MathHelper.RandomDirection() * 500;//hbv.Primitives[i].Center;//
                ray.TMin = 0.001f;
                ray.TMax = 10000.0f;
                ray.HitPrimitiveIndex = -1;
                ray.NodeHitCount = 0;
                ray.PrimitiveHitCount = 0;

                var index = hbv.Intersects(ref ray);
                if (index >= 0)
                {
                    var p = hbv.Primitives[index];
                    var hitPos = ray.Origin + ray.Direction * ray.TMax;
                    if (p.AABB.Contains(hitPos) != ContainmentType.Disjoint)
                    {

                    }
                    var ray1 = new TtBVH.FRayDesc();
                    ray1.Direction = ray.Direction;
                    ray1.Origin = ray.Origin;
                    ray1.TMin = 0.001f;
                    ray1.TMax = 10000.0f;
                    ray1.HitPrimitiveIndex = -1;
                    ray1.NodeHitCount = 0;
                    ray1.PrimitiveHitCount = 0;
                    var index10 = hbv10.Intersects(ref ray1);
                    if (index10 == index)
                    {

                    }
                }
            }
        }
    }
}
