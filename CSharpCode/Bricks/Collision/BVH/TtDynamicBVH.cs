using System;
using System.Collections.Generic;

// =============================================================================
// TtDynamicBVH<T>
//
// A C# port of UE's FDynamicBVH / Box2D b2DynamicTree-style dynamic AABB tree.
// Features:
//   - Binary AABB tree with internal nodes storing the union of child AABBs.
//   - Object proxies live only in leaf nodes; user data of type T is stored per leaf.
//   - Incremental Insert / Remove / Update (MoveProxy) suitable for moving objects.
//   - "Fat" AABBs: leaf AABBs are inflated by Margin and shifted along the
//     displacement vector to amortize re-insertions during movement.
//   - Sibling selection driven by Surface Area Heuristic (SAH).
//   - Per-insertion bottom-up balancing via single rotations (b2/btDbvt style).
//   - Free-list node pool to recycle nodes and avoid GC churn.
//   - Queries: AABB overlap, point overlap, ray cast (slab test),
//     self-overlap pairs, and a generic depth-first traversal.
//
// Naming follows engine convention: Tt prefix, EngineNS namespace.
// Thread-safety: NOT thread safe. Wrap externally if accessed from multiple threads.
// =============================================================================

namespace EngineNS.Bricks.Collision.BVH
{
    /// <summary>
    /// A dynamic bounding volume hierarchy (AABB tree) suitable for broad-phase
    /// collision detection, frustum culling, ray casting and spatial queries on
    /// scenes whose objects are added/removed/moved frequently.
    /// </summary>
    /// <typeparam name="T">User payload stored on each leaf proxy.</typeparam>
    public class TtDynamicBVH<T>
    {
        // Sentinel for "no node".
        public const int NullNode = -1;

        // ------------------------------------------------------------------
        // Internal node layout. Stored in a contiguous array, free slots are
        // chained via the Parent field as a singly linked free list.
        // ------------------------------------------------------------------
        private struct Node
        {
            public Aabb Box;          // For leaves: fat AABB. For internals: union of children.
            public T UserData;        // Valid only when IsLeaf().

            public int Parent;        // When in use: parent index, NullNode for root.
                                      // When free:   index of next free node (free list).
            public int Child1;        // First child (NullNode for leaves).
            public int Child2;        // Second child (NullNode for leaves).
            public int Height;        // 0 for leaves, -1 for free slots, >=1 for internals.
            public int MoveStamp;     // Last frame/version this leaf was updated (optional book-keeping).

            public bool IsLeaf
            {
                get { return Child1 == NullNode; }
            }
            public bool IsFree
            {
                get { return Height == -1; }
            }
        }

        private Node[] mNodes;
        private int mNodeCount;       // Number of allocated (in-use) nodes.
        private int mNodeCapacity;    // Length of mNodes array.
        private int mFreeList;        // Head of the free list (NullNode if empty).

        private int mRoot = NullNode;
        private int mProxyCount;      // Number of leaf proxies currently in the tree.

        // Fat-AABB inflation applied to every leaf box on insert. Larger values
        // make MoveProxy cheaper (re-insertion happens less often) at the cost
        // of looser queries. Mirrors b2_aabbExtension / FDynamicBVH::Margin.
        private float mMargin;

        // Movement multiplier: when MoveProxy detects displacement, the fat box
        // is also shifted by Multiplier * displacement to "predict" future motion.
        private float mDisplacementMultiplier;

        /// <summary>
        /// Total number of leaf proxies currently in the tree.
        /// </summary>
        public int ProxyCount { get { return mProxyCount; } }

        /// <summary>
        /// Internal node count (leaves + internal nodes), useful for diagnostics.
        /// </summary>
        public int NodeCount { get { return mNodeCount; } }

        /// <summary>
        /// Root node index. Returns <see cref="NullNode"/> when the tree is empty.
        /// </summary>
        public int RootIndex { get { return mRoot; } }

        /// <summary>
        /// Construct an empty dynamic BVH.
        /// </summary>
        /// <param name="initialCapacity">Initial node pool capacity. Grows on demand.</param>
        /// <param name="margin">Per-axis fat-AABB inflation applied to leaves on insert (>= 0).</param>
        /// <param name="displacementMultiplier">Multiplier applied to predicted displacement during MoveProxy (>= 0).</param>
        public TtDynamicBVH(int initialCapacity = 16, float margin = 0.1f, float displacementMultiplier = 2.0f)
        {
            if (initialCapacity < 4) 
                initialCapacity = 4;
            mNodeCapacity = initialCapacity;
            mNodes = new Node[mNodeCapacity];
            mNodeCount = 0;
            mFreeList = 0;
            mMargin = margin < 0 ? 0 : margin;
            mDisplacementMultiplier = displacementMultiplier < 0 ? 0 : displacementMultiplier;

            BuildFreeList(0);
        }

        // Initialize the free list starting at startIndex.
        private void BuildFreeList(int startIndex)
        {
            for (int i = startIndex; i < mNodeCapacity - 1; i++)
            {
                mNodes[i].Parent = i + 1;
                mNodes[i].Height = -1;
            }
            mNodes[mNodeCapacity - 1].Parent = NullNode;
            mNodes[mNodeCapacity - 1].Height = -1;
            mFreeList = startIndex;
        }

        // --------------------------------------------------------------
        // Node pool primitives
        // --------------------------------------------------------------
        private int AllocateNode()
        {
            if (mFreeList == NullNode)
            {
                // Grow.
                int oldCap = mNodeCapacity;
                mNodeCapacity = oldCap * 2;
                Array.Resize(ref mNodes, mNodeCapacity);
                BuildFreeList(oldCap);
            }

            int nodeId = mFreeList;
            mFreeList = mNodes[nodeId].Parent;
            mNodes[nodeId].Parent = NullNode;
            mNodes[nodeId].Child1 = NullNode;
            mNodes[nodeId].Child2 = NullNode;
            mNodes[nodeId].Height = 0;
            mNodes[nodeId].UserData = default(T);
            mNodes[nodeId].MoveStamp = 0;
            mNodeCount++;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            // Push back onto free list.
            mNodes[nodeId].Parent = mFreeList;
            mNodes[nodeId].Height = -1;
            mNodes[nodeId].UserData = default(T);
            mFreeList = nodeId;
            mNodeCount--;
        }

        // --------------------------------------------------------------
        // Aabb helpers (we deliberately do NOT use Aabb.IsIntersect which
        // has a known typo in this codebase, see Math/Aabb.cs).
        // --------------------------------------------------------------
        private static Aabb Combine(in Aabb a, in Aabb b)
        {
            DVector3 minA = a.Center - a.Extent.AsDVector();
            DVector3 maxA = a.Center + a.Extent.AsDVector();
            DVector3 minB = b.Center - b.Extent.AsDVector();
            DVector3 maxB = b.Center + b.Extent.AsDVector();

            DVector3 lo = new DVector3(
                Math.Min(minA.X, minB.X),
                Math.Min(minA.Y, minB.Y),
                Math.Min(minA.Z, minB.Z));
            DVector3 hi = new DVector3(
                Math.Max(maxA.X, maxB.X),
                Math.Max(maxA.Y, maxB.Y),
                Math.Max(maxA.Z, maxB.Z));
            return new Aabb(in lo, in hi);
        }

        // Surface area of the (full-size) AABB. Used as the SAH cost metric.
        private static double SurfaceArea(in Aabb box)
        {
            double dx = box.Extent.X * 2.0;
            double dy = box.Extent.Y * 2.0;
            double dz = box.Extent.Z * 2.0;
            return 2.0 * (dx * dy + dy * dz + dz * dx);
        }

        private static bool Overlaps(in Aabb a, in Aabb b)
        {
            if (Math.Abs(a.Center.X - b.Center.X) > (double)a.Extent.X + b.Extent.X) return false;
            if (Math.Abs(a.Center.Y - b.Center.Y) > (double)a.Extent.Y + b.Extent.Y) return false;
            if (Math.Abs(a.Center.Z - b.Center.Z) > (double)a.Extent.Z + b.Extent.Z) return false;
            return true;
        }

        private static bool Contains(in Aabb outer, in Aabb inner)
        {
            // Returns true if outer fully contains inner.
            DVector3 oMin = outer.Center - outer.Extent.AsDVector();
            DVector3 oMax = outer.Center + outer.Extent.AsDVector();
            DVector3 iMin = inner.Center - inner.Extent.AsDVector();
            DVector3 iMax = inner.Center + inner.Extent.AsDVector();
            return oMin.X <= iMin.X && oMin.Y <= iMin.Y && oMin.Z <= iMin.Z
                && oMax.X >= iMax.X && oMax.Y >= iMax.Y && oMax.Z >= iMax.Z;
        }

        private Aabb FattenBox(in Aabb tight)
        {
            // Inflate by mMargin on each axis (extent grows by margin).
            var ext = tight.Extent;
            ext.X += mMargin;
            ext.Y += mMargin;
            ext.Z += mMargin;
            return new Aabb(tight.Center, ext);
        }

        // ==================================================================
        // Public API: Insert / Remove / Update / Get
        // ==================================================================

        /// <summary>
        /// Insert a new leaf proxy with the given AABB and user data.
        /// Returns an opaque proxy id that must be passed to subsequent
        /// <see cref="UpdateProxy"/> / <see cref="RemoveProxy"/> calls.
        /// The stored AABB is fattened by the configured margin.
        /// </summary>
        public int InsertProxy(in Aabb tightBox, T userData)
        {
            int proxy = AllocateNode();
            mNodes[proxy].Box = FattenBox(in tightBox);
            mNodes[proxy].UserData = userData;
            mNodes[proxy].Height = 0;
            mNodes[proxy].Child1 = NullNode;
            mNodes[proxy].Child2 = NullNode;

            InsertLeaf(proxy);
            mProxyCount++;
            return proxy;
        }

        /// <summary>
        /// Remove a previously inserted proxy.
        /// </summary>
        public void RemoveProxy(int proxyId)
        {
            ValidateProxy(proxyId);
            RemoveLeaf(proxyId);
            FreeNode(proxyId);
            mProxyCount--;
        }

        /// <summary>
        /// Update a proxy's AABB. If the new tight box still fits inside the
        /// existing fat box and <paramref name="displacement"/> is small,
        /// the tree topology is left untouched (cheap path). Otherwise the
        /// proxy is re-inserted with a new fat box that is also shifted along
        /// the displacement vector to amortize future MoveProxy calls.
        /// </summary>
        /// <param name="proxyId">Proxy to update.</param>
        /// <param name="newTightBox">New tight AABB of the object.</param>
        /// <param name="displacement">Object displacement since last update (in world units).</param>
        /// <returns>True if the tree was actually re-balanced (i.e. proxy moved).</returns>
        public bool UpdateProxy(int proxyId, in Aabb newTightBox, in DVector3 displacement)
        {
            ValidateProxy(proxyId);
            if (!mNodes[proxyId].IsLeaf)
                throw new InvalidOperationException("UpdateProxy called on a non-leaf node id.");

            // Build the new fat AABB: inflate by margin AND shift by displacement
            // so that the box "predicts" forward motion (b2DynamicTree::MoveProxy).
            var fat = FattenBox(in newTightBox);
            DVector3 d = displacement * mDisplacementMultiplier;

            DVector3 fatMin = fat.Center - fat.Extent.AsDVector();
            DVector3 fatMax = fat.Center + fat.Extent.AsDVector();
            if (d.X < 0) fatMin.X += d.X; else fatMax.X += d.X;
            if (d.Y < 0) fatMin.Y += d.Y; else fatMax.Y += d.Y;
            if (d.Z < 0) fatMin.Z += d.Z; else fatMax.Z += d.Z;
            var predicted = new Aabb(in fatMin, in fatMax);

            // Cheap path: existing fat box already covers the new tight box.
            if (Contains(mNodes[proxyId].Box, in newTightBox))
                return false;

            RemoveLeaf(proxyId);
            mNodes[proxyId].Box = predicted;
            InsertLeaf(proxyId);
            return true;
        }

        /// <summary>
        /// Returns the user data stored on the given leaf proxy.
        /// </summary>
        public T GetUserData(int proxyId)
        {
            ValidateProxy(proxyId);
            return mNodes[proxyId].UserData;
        }

        /// <summary>
        /// Returns the (fat) AABB stored on the given leaf proxy.
        /// </summary>
        public Aabb GetFatBox(int proxyId)
        {
            ValidateProxy(proxyId);
            return mNodes[proxyId].Box;
        }

        /// <summary>
        /// Removes every proxy and resets the node pool. Capacity is preserved.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < mNodeCapacity; i++)
                mNodes[i] = default(Node);
            BuildFreeList(0);
            mNodeCount = 0;
            mProxyCount = 0;
            mRoot = NullNode;
        }

        private void ValidateProxy(int proxyId)
        {
            if (proxyId < 0 || proxyId >= mNodeCapacity || mNodes[proxyId].IsFree)
                throw new ArgumentOutOfRangeException("proxyId", "Invalid or already freed proxy id.");
        }

        // ==================================================================
        // Tree mutation: InsertLeaf / RemoveLeaf / Balance
        // ==================================================================

        private void InsertLeaf(int leaf)
        {
            if (mRoot == NullNode)
            {
                mRoot = leaf;
                mNodes[leaf].Parent = NullNode;
                return;
            }

            // 1) Find the best sibling using SAH descent.
            Aabb leafBox = mNodes[leaf].Box;
            int index = mRoot;
            while (!mNodes[index].IsLeaf)
            {
                int child1 = mNodes[index].Child1;
                int child2 = mNodes[index].Child2;

                double area = SurfaceArea(mNodes[index].Box);
                Aabb combined = Combine(mNodes[index].Box, leafBox);
                double combinedArea = SurfaceArea(combined);

                // Cost of creating a new parent for leaf and current node.
                double cost = 2.0 * combinedArea;

                // Inheritance cost for descending further.
                double inheritanceCost = 2.0 * (combinedArea - area);

                // Descent cost into child1.
                double cost1;
                {
                    Aabb merged = Combine(leafBox, mNodes[child1].Box);
                    double newArea = SurfaceArea(merged);
                    if (mNodes[child1].IsLeaf)
                        cost1 = newArea + inheritanceCost;
                    else
                        cost1 = (newArea - SurfaceArea(mNodes[child1].Box)) + inheritanceCost;
                }
                // Descent cost into child2.
                double cost2;
                {
                    Aabb merged = Combine(leafBox, mNodes[child2].Box);
                    double newArea = SurfaceArea(merged);
                    if (mNodes[child2].IsLeaf)
                        cost2 = newArea + inheritanceCost;
                    else
                        cost2 = (newArea - SurfaceArea(mNodes[child2].Box)) + inheritanceCost;
                }

                if (cost < cost1 && cost < cost2)
                    break;

                index = (cost1 < cost2) ? child1 : child2;
            }

            int sibling = index;

            // 2) Create a new parent that hosts the sibling and the new leaf.
            int oldParent = mNodes[sibling].Parent;
            int newParent = AllocateNode();
            mNodes[newParent].Parent = oldParent;
            mNodes[newParent].UserData = default(T);
            mNodes[newParent].Box = Combine(leafBox, mNodes[sibling].Box);
            mNodes[newParent].Height = mNodes[sibling].Height + 1;

            if (oldParent != NullNode)
            {
                if (mNodes[oldParent].Child1 == sibling)
                    mNodes[oldParent].Child1 = newParent;
                else
                    mNodes[oldParent].Child2 = newParent;
            }
            else
            {
                mRoot = newParent;
            }

            mNodes[newParent].Child1 = sibling;
            mNodes[newParent].Child2 = leaf;
            mNodes[sibling].Parent = newParent;
            mNodes[leaf].Parent = newParent;

            // 3) Walk back up, refitting boxes and balancing.
            int idx = mNodes[leaf].Parent;
            while (idx != NullNode)
            {
                idx = Balance(idx);

                int c1 = mNodes[idx].Child1;
                int c2 = mNodes[idx].Child2;
                mNodes[idx].Height = 1 + Math.Max(mNodes[c1].Height, mNodes[c2].Height);
                mNodes[idx].Box = Combine(mNodes[c1].Box, mNodes[c2].Box);

                idx = mNodes[idx].Parent;
            }
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == mRoot)
            {
                mRoot = NullNode;
                return;
            }

            int parent = mNodes[leaf].Parent;
            int grandParent = mNodes[parent].Parent;
            int sibling = (mNodes[parent].Child1 == leaf) ? mNodes[parent].Child2 : mNodes[parent].Child1;

            if (grandParent != NullNode)
            {
                // Connect sibling directly to grandparent.
                if (mNodes[grandParent].Child1 == parent)
                    mNodes[grandParent].Child1 = sibling;
                else
                    mNodes[grandParent].Child2 = sibling;
                mNodes[sibling].Parent = grandParent;
                FreeNode(parent);

                // Refit and balance back to the root.
                int idx = grandParent;
                while (idx != NullNode)
                {
                    idx = Balance(idx);

                    int c1 = mNodes[idx].Child1;
                    int c2 = mNodes[idx].Child2;
                    mNodes[idx].Box = Combine(mNodes[c1].Box, mNodes[c2].Box);
                    mNodes[idx].Height = 1 + Math.Max(mNodes[c1].Height, mNodes[c2].Height);

                    idx = mNodes[idx].Parent;
                }
            }
            else
            {
                // Parent was the root.
                mRoot = sibling;
                mNodes[sibling].Parent = NullNode;
                FreeNode(parent);
            }
        }

        // Single rotation step at node A. Mirrors b2DynamicTree::Balance:
        // if A is too tall (|h(B) - h(C)| > 1) we rotate the lighter side up.
        private int Balance(int iA)
        {
            if (mNodes[iA].IsLeaf || mNodes[iA].Height < 2)
                return iA;

            int iB = mNodes[iA].Child1;
            int iC = mNodes[iA].Child2;

            int balance = mNodes[iC].Height - mNodes[iB].Height;

            // Rotate C up.
            if (balance > 1)
            {
                int iF = mNodes[iC].Child1;
                int iG = mNodes[iC].Child2;

                // Swap A and C.
                mNodes[iC].Child1 = iA;
                mNodes[iC].Parent = mNodes[iA].Parent;
                mNodes[iA].Parent = iC;

                if (mNodes[iC].Parent != NullNode)
                {
                    if (mNodes[mNodes[iC].Parent].Child1 == iA)
                        mNodes[mNodes[iC].Parent].Child1 = iC;
                    else
                        mNodes[mNodes[iC].Parent].Child2 = iC;
                }
                else
                {
                    mRoot = iC;
                }

                // Pick the taller grandchild to rotate further up.
                if (mNodes[iF].Height > mNodes[iG].Height)
                {
                    mNodes[iC].Child2 = iF;
                    mNodes[iA].Child2 = iG;
                    mNodes[iG].Parent = iA;
                    mNodes[iA].Box = Combine(mNodes[iB].Box, mNodes[iG].Box);
                    mNodes[iC].Box = Combine(mNodes[iA].Box, mNodes[iF].Box);

                    mNodes[iA].Height = 1 + Math.Max(mNodes[iB].Height, mNodes[iG].Height);
                    mNodes[iC].Height = 1 + Math.Max(mNodes[iA].Height, mNodes[iF].Height);
                }
                else
                {
                    mNodes[iC].Child2 = iG;
                    mNodes[iA].Child2 = iF;
                    mNodes[iF].Parent = iA;
                    mNodes[iA].Box = Combine(mNodes[iB].Box, mNodes[iF].Box);
                    mNodes[iC].Box = Combine(mNodes[iA].Box, mNodes[iG].Box);

                    mNodes[iA].Height = 1 + Math.Max(mNodes[iB].Height, mNodes[iF].Height);
                    mNodes[iC].Height = 1 + Math.Max(mNodes[iA].Height, mNodes[iG].Height);
                }
                return iC;
            }

            // Rotate B up.
            if (balance < -1)
            {
                int iD = mNodes[iB].Child1;
                int iE = mNodes[iB].Child2;

                mNodes[iB].Child1 = iA;
                mNodes[iB].Parent = mNodes[iA].Parent;
                mNodes[iA].Parent = iB;

                if (mNodes[iB].Parent != NullNode)
                {
                    if (mNodes[mNodes[iB].Parent].Child1 == iA)
                        mNodes[mNodes[iB].Parent].Child1 = iB;
                    else
                        mNodes[mNodes[iB].Parent].Child2 = iB;
                }
                else
                {
                    mRoot = iB;
                }

                if (mNodes[iD].Height > mNodes[iE].Height)
                {
                    mNodes[iB].Child2 = iD;
                    mNodes[iA].Child1 = iE;
                    mNodes[iE].Parent = iA;
                    mNodes[iA].Box = Combine(mNodes[iC].Box, mNodes[iE].Box);
                    mNodes[iB].Box = Combine(mNodes[iA].Box, mNodes[iD].Box);

                    mNodes[iA].Height = 1 + Math.Max(mNodes[iC].Height, mNodes[iE].Height);
                    mNodes[iB].Height = 1 + Math.Max(mNodes[iA].Height, mNodes[iD].Height);
                }
                else
                {
                    mNodes[iB].Child2 = iE;
                    mNodes[iA].Child1 = iD;
                    mNodes[iD].Parent = iA;
                    mNodes[iA].Box = Combine(mNodes[iC].Box, mNodes[iD].Box);
                    mNodes[iB].Box = Combine(mNodes[iA].Box, mNodes[iE].Box);

                    mNodes[iA].Height = 1 + Math.Max(mNodes[iC].Height, mNodes[iD].Height);
                    mNodes[iB].Height = 1 + Math.Max(mNodes[iA].Height, mNodes[iE].Height);
                }
                return iB;
            }

            return iA;
        }

        // ==================================================================
        // Queries
        // ==================================================================

        /// <summary>
        /// Visitor signature for AABB / point queries. Return false to abort
        /// further iteration (e.g. once "any hit" is found).
        /// </summary>
        public delegate bool FOnProxy(int proxyId, T userData);

        /// <summary>
        /// Visitor signature for ray casts. Return the new maxFraction to
        /// continue ("clip" the ray, e.g. 0 to stop, the input value to
        /// continue unchanged, or any value &lt;= input to shorten).
        /// </summary>
        public delegate double FOnRayHit(int proxyId, T userData, in DVector3 origin, in DVector3 direction, double currentMaxFraction);

        // Reusable stack to avoid per-query allocation (NOT thread safe, see class doc).
        private readonly Stack<int> mQueryStack = new Stack<int>(64);

        /// <summary>
        /// Enumerate every leaf proxy whose fat AABB overlaps <paramref name="queryBox"/>.
        /// </summary>
        public void QueryAabb(in Aabb queryBox, FOnProxy visitor)
        {
            if (mRoot == NullNode || visitor == null) return;

            mQueryStack.Clear();
            mQueryStack.Push(mRoot);
            while (mQueryStack.Count > 0)
            {
                int idx = mQueryStack.Pop();
                if (!Overlaps(mNodes[idx].Box, in queryBox)) continue;

                if (mNodes[idx].IsLeaf)
                {
                    if (!visitor(idx, mNodes[idx].UserData))
                        return;
                }
                else
                {
                    mQueryStack.Push(mNodes[idx].Child1);
                    mQueryStack.Push(mNodes[idx].Child2);
                }
            }
        }

        /// <summary>
        /// Enumerate every leaf proxy whose fat AABB contains <paramref name="point"/>.
        /// </summary>
        public void QueryPoint(in DVector3 point, FOnProxy visitor)
        {
            if (mRoot == NullNode || visitor == null) return;

            mQueryStack.Clear();
            mQueryStack.Push(mRoot);
            while (mQueryStack.Count > 0)
            {
                int idx = mQueryStack.Pop();
                if (!mNodes[idx].Box.IsContain(in point)) continue;

                if (mNodes[idx].IsLeaf)
                {
                    if (!visitor(idx, mNodes[idx].UserData))
                        return;
                }
                else
                {
                    mQueryStack.Push(mNodes[idx].Child1);
                    mQueryStack.Push(mNodes[idx].Child2);
                }
            }
        }

        // Slab test: returns true if the ray (origin + t * dir, t in [0, maxFraction])
        // intersects the AABB. invDir is precomputed 1/dir (with +Inf for zero comps).
        private static bool RaySlabIntersect(in Aabb box, in DVector3 origin, in DVector3 invDir, double maxFraction)
        {
            DVector3 mn = box.Center - box.Extent.AsDVector();
            DVector3 mx = box.Center + box.Extent.AsDVector();

            double t1 = (mn.X - origin.X) * invDir.X;
            double t2 = (mx.X - origin.X) * invDir.X;
            double tmin = Math.Min(t1, t2);
            double tmax = Math.Max(t1, t2);

            t1 = (mn.Y - origin.Y) * invDir.Y;
            t2 = (mx.Y - origin.Y) * invDir.Y;
            tmin = Math.Max(tmin, Math.Min(t1, t2));
            tmax = Math.Min(tmax, Math.Max(t1, t2));

            t1 = (mn.Z - origin.Z) * invDir.Z;
            t2 = (mx.Z - origin.Z) * invDir.Z;
            tmin = Math.Max(tmin, Math.Min(t1, t2));
            tmax = Math.Min(tmax, Math.Max(t1, t2));

            return tmax >= Math.Max(tmin, 0.0) && tmin <= maxFraction;
        }

        /// <summary>
        /// Ray cast against every leaf proxy that the ray's slab test reaches.
        /// The visitor decides whether to do an exact intersection and may
        /// return a smaller <c>maxFraction</c> to clip subsequent traversal,
        /// enabling early-out for the closest hit.
        /// </summary>
        /// <param name="origin">Ray origin in world space.</param>
        /// <param name="direction">Ray direction (does not need to be unit length).</param>
        /// <param name="maxFraction">Initial ray length (in units of <paramref name="direction"/>). Use <see cref="double.PositiveInfinity"/> for an unbounded ray.</param>
        /// <param name="onHit">Per-leaf visitor. Returns the new maxFraction (&lt;= 0 stops traversal).</param>
        public void RayCast(in DVector3 origin, in DVector3 direction, double maxFraction, FOnRayHit onHit)
        {
            if (mRoot == NullNode || onHit == null) return;

            // Precompute inverse direction (use +Inf when component is 0 so the slab test still works).
            DVector3 invDir = new DVector3(
                direction.X != 0.0 ? 1.0 / direction.X : double.PositiveInfinity,
                direction.Y != 0.0 ? 1.0 / direction.Y : double.PositiveInfinity,
                direction.Z != 0.0 ? 1.0 / direction.Z : double.PositiveInfinity);

            mQueryStack.Clear();
            mQueryStack.Push(mRoot);
            while (mQueryStack.Count > 0)
            {
                int idx = mQueryStack.Pop();
                if (!RaySlabIntersect(mNodes[idx].Box, in origin, in invDir, maxFraction))
                    continue;

                if (mNodes[idx].IsLeaf)
                {
                    double newMax = onHit(idx, mNodes[idx].UserData, in origin, in direction, maxFraction);
                    if (newMax <= 0.0) return;
                    if (newMax < maxFraction) maxFraction = newMax;
                }
                else
                {
                    mQueryStack.Push(mNodes[idx].Child1);
                    mQueryStack.Push(mNodes[idx].Child2);
                }
            }
        }

        /// <summary>
        /// Visitor for self-overlap queries.
        /// </summary>
        public delegate bool FOnPair(int proxyA, T userDataA, int proxyB, T userDataB);

        /// <summary>
        /// Enumerate every pair of proxies whose fat AABBs overlap. Each pair
        /// is reported once. Useful as a broad-phase pass for narrow-phase
        /// collision detection. Return false from the visitor to stop early.
        /// </summary>
        public void QuerySelfPairs(FOnPair onPair)
        {
            if (mRoot == NullNode || onPair == null || mNodes[mRoot].IsLeaf) return;

            // Walk every leaf, then descend the rest of the tree once per leaf,
            // skipping leaves with smaller index to avoid duplicates.
            // For typical broad-phase usage this is O(N log N) average.
            for (int i = 0; i < mNodeCapacity; i++)
            {
                if (mNodes[i].IsFree || !mNodes[i].IsLeaf) continue;
                if (!QueryPairsForLeaf(i, onPair))
                    return;
            }
        }

        private bool QueryPairsForLeaf(int leaf, FOnPair onPair)
        {
            Aabb leafBox = mNodes[leaf].Box;
            mQueryStack.Clear();
            mQueryStack.Push(mRoot);
            while (mQueryStack.Count > 0)
            {
                int idx = mQueryStack.Pop();
                if (idx == leaf) continue;
                if (!Overlaps(mNodes[idx].Box, in leafBox)) continue;

                if (mNodes[idx].IsLeaf)
                {
                    // Report each pair only once: enforce leaf < idx.
                    if (leaf < idx)
                    {
                        if (!onPair(leaf, mNodes[leaf].UserData, idx, mNodes[idx].UserData))
                            return false;
                    }
                }
                else
                {
                    mQueryStack.Push(mNodes[idx].Child1);
                    mQueryStack.Push(mNodes[idx].Child2);
                }
            }
            return true;
        }

        // ==================================================================
        // Diagnostics / debug
        // ==================================================================

        /// <summary>
        /// Visitor for debug traversal. Receives node index, depth (root = 0),
        /// the node's AABB, and whether it is a leaf.
        /// </summary>
        public delegate bool FOnDebugNode(int nodeId, int depth, in Aabb box, bool isLeaf);

        /// <summary>
        /// Depth-first traversal of every node in the tree (internal + leaves).
        /// Useful for visualization and validation. Return false to abort.
        /// </summary>
        public void DebugTraverse(FOnDebugNode visitor)
        {
            if (mRoot == NullNode || visitor == null) return;

            mQueryStack.Clear();
            // Pack (nodeId, depth) into two stack pushes (index then depth).
            // Using a tuple stack would also work but adds allocation cost on .NET Framework.
            var depthStack = new Stack<int>(64);
            mQueryStack.Push(mRoot);
            depthStack.Push(0);

            while (mQueryStack.Count > 0)
            {
                int idx = mQueryStack.Pop();
                int depth = depthStack.Pop();
                if (!visitor(idx, depth, in mNodes[idx].Box, mNodes[idx].IsLeaf))
                    return;

                if (!mNodes[idx].IsLeaf)
                {
                    mQueryStack.Push(mNodes[idx].Child1);
                    depthStack.Push(depth + 1);
                    mQueryStack.Push(mNodes[idx].Child2);
                    depthStack.Push(depth + 1);
                }
            }
        }

        /// <summary>
        /// Maximum tree depth from root. Returns 0 for an empty tree.
        /// </summary>
        public int GetHeight()
        {
            return mRoot == NullNode ? 0 : mNodes[mRoot].Height;
        }

        /// <summary>
        /// Compute the SAH cost of the current tree (sum of internal node
        /// surface areas divided by root surface area). Lower is better;
        /// values around 1-3 indicate a well-balanced tree.
        /// </summary>
        public double GetSAHCost()
        {
            if (mRoot == NullNode) return 0;
            double rootArea = SurfaceArea(mNodes[mRoot].Box);
            if (rootArea <= 0) return 0;

            double sum = 0;
            for (int i = 0; i < mNodeCapacity; i++)
            {
                if (mNodes[i].IsFree) continue;
                if (mNodes[i].IsLeaf) continue;
                sum += SurfaceArea(mNodes[i].Box);
            }
            return sum / rootArea;
        }

        /// <summary>
        /// Validates internal invariants (parent/child consistency, box union,
        /// height correctness). Returns true if the tree is consistent.
        /// Intended for unit tests / asserts; it walks the entire tree.
        /// </summary>
        public bool ValidateStructure()
        {
            if (mRoot == NullNode) return mProxyCount == 0;
            if (mNodes[mRoot].Parent != NullNode) return false;
            return ValidateRecursive(mRoot);
        }

        private bool ValidateRecursive(int idx)
        {
            if (idx == NullNode) return true;
            if (mNodes[idx].IsFree) return false;

            if (mNodes[idx].IsLeaf)
            {
                if (mNodes[idx].Child1 != NullNode || mNodes[idx].Child2 != NullNode) return false;
                if (mNodes[idx].Height != 0) return false;
                return true;
            }

            int c1 = mNodes[idx].Child1;
            int c2 = mNodes[idx].Child2;
            if (mNodes[c1].Parent != idx || mNodes[c2].Parent != idx) return false;

            int expectedHeight = 1 + Math.Max(mNodes[c1].Height, mNodes[c2].Height);
            if (mNodes[idx].Height != expectedHeight) return false;

            // Parent box must contain both children boxes (not necessarily exactly
            // equal to their union due to floating point, but must contain).
            if (!Contains(mNodes[idx].Box, mNodes[c1].Box)) return false;
            if (!Contains(mNodes[idx].Box, mNodes[c2].Box)) return false;

            return ValidateRecursive(c1) && ValidateRecursive(c2);
        }

        // ==================================================================
        // Bulk build via Morton codes (LBVH, Karras-style top-down split)
        //
        // Suitable when you have a large static (or rebuilt-per-frame) set of
        // primitives and want a much better initial tree than feeding them one
        // by one into InsertProxy. Build cost is O(N log N), dominated by the
        // sort step. The resulting tree is structurally identical to one built
        // incrementally, so all dynamic ops (Insert / Remove / Update / Query)
        // continue to work afterwards.
        // ==================================================================

        // Pack a 10-bit integer (0..1023) by spreading its bits with two zero
        // bits in between, so three of these OR'd together form a 30-bit Morton code.
        private static uint ExpandBits10(uint v)
        {
            v = (v | (v << 16)) & 0x030000FFu;
            v = (v | (v <<  8)) & 0x0300F00Fu;
            v = (v | (v <<  4)) & 0x030C30C3u;
            v = (v | (v <<  2)) & 0x09249249u;
            return v;
        }

        // Encode a normalized point in [0,1]^3 to a 30-bit Morton code.
        private static uint Morton3D(double nx, double ny, double nz)
        {
            // Clamp + scale to 10-bit grid.
            double sx = nx * 1024.0; if (sx < 0) sx = 0; else if (sx > 1023.0) sx = 1023.0;
            double sy = ny * 1024.0; if (sy < 0) sy = 0; else if (sy > 1023.0) sy = 1023.0;
            double sz = nz * 1024.0; if (sz < 0) sz = 0; else if (sz > 1023.0) sz = 1023.0;
            uint xx = ExpandBits10((uint)sx);
            uint yy = ExpandBits10((uint)sy);
            uint zz = ExpandBits10((uint)sz);
            return (xx << 2) | (yy << 1) | zz;
        }

        /// <summary>
        /// Bulk-build the tree from a list of leaf AABBs and matching user data,
        /// using a Morton-code Linear BVH (LBVH). Existing contents are cleared.
        /// Each leaf AABB is fattened by the configured margin, identical to
        /// <see cref="InsertProxy"/>. Returns the array of proxy ids whose i-th
        /// element corresponds to the i-th input leaf.
        /// </summary>
        /// <param name="tightBoxes">Tight AABB per leaf.</param>
        /// <param name="userData">User data per leaf (must have the same length).</param>
        public int[] BuildFromLeaves(IList<Aabb> tightBoxes, IList<T> userData)
        {
            if (tightBoxes == null) throw new ArgumentNullException("tightBoxes");
            if (userData == null) throw new ArgumentNullException("userData");
            if (tightBoxes.Count != userData.Count)
                throw new ArgumentException("tightBoxes and userData must have the same length.");

            Clear();

            int n = tightBoxes.Count;
            var proxyIds = new int[n];
            if (n == 0) return proxyIds;

            // 1) Allocate one leaf node per input and compute the global bounds
            //    over the (fattened) leaf centers. Centers (not full boxes) are
            //    used for Morton coding so spatially-close objects stay close.
            DVector3 globalMin = new DVector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            DVector3 globalMax = new DVector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

            for (int i = 0; i < n; i++)
            {
                int leaf = AllocateNode();
                mNodes[leaf].Box = FattenBox(tightBoxes[i]);
                mNodes[leaf].UserData = userData[i];
                mNodes[leaf].Height = 0;
                mNodes[leaf].Child1 = NullNode;
                mNodes[leaf].Child2 = NullNode;
                proxyIds[i] = leaf;

                DVector3 c = mNodes[leaf].Box.Center;
                if (c.X < globalMin.X) globalMin.X = c.X;
                if (c.Y < globalMin.Y) globalMin.Y = c.Y;
                if (c.Z < globalMin.Z) globalMin.Z = c.Z;
                if (c.X > globalMax.X) globalMax.X = c.X;
                if (c.Y > globalMax.Y) globalMax.Y = c.Y;
                if (c.Z > globalMax.Z) globalMax.Z = c.Z;
            }
            mProxyCount = n;

            // Trivial case: a single leaf becomes the root directly.
            if (n == 1)
            {
                mRoot = proxyIds[0];
                mNodes[mRoot].Parent = NullNode;
                return proxyIds;
            }

            // 2) Compute Morton codes from normalized centers. Use
            //    extent-with-epsilon to avoid divide-by-zero on degenerate axes.
            double rangeX = globalMax.X - globalMin.X;
            double rangeY = globalMax.Y - globalMin.Y;
            double rangeZ = globalMax.Z - globalMin.Z;
            double invX = rangeX > 1e-20 ? 1.0 / rangeX : 0.0;
            double invY = rangeY > 1e-20 ? 1.0 / rangeY : 0.0;
            double invZ = rangeZ > 1e-20 ? 1.0 / rangeZ : 0.0;

            var codes = new uint[n];
            var order = new int[n];
            for (int i = 0; i < n; i++)
            {
                DVector3 c = mNodes[proxyIds[i]].Box.Center;
                codes[i] = Morton3D(
                    (c.X - globalMin.X) * invX,
                    (c.Y - globalMin.Y) * invY,
                    (c.Z - globalMin.Z) * invZ);
                order[i] = proxyIds[i];
            }

            // 3) Sort leaves by Morton code (Array.Sort with a key array is
            //    a stable enough O(N log N) for our needs; collisions are
            //    handled deterministically by node-id order in the recursion).
            Array.Sort(codes, order);

            // 4) Recursively split the sorted range at the highest bit where
            //    the leftmost and rightmost code differ (Karras' classic LCP
            //    split). On collisions we fall back to the median.
            mRoot = BuildLBVHRange(order, codes, 0, n - 1, NullNode);
            return proxyIds;
        }

        /// <summary>
        /// Static convenience: build a fresh tree from leaf data in one shot
        /// using Morton/LBVH. Equivalent to constructing an empty tree then
        /// calling <see cref="BuildFromLeaves"/>.
        /// </summary>
        public static TtDynamicBVH<T> Build(IList<Aabb> tightBoxes, IList<T> userData,
                                            float margin = 0.1f, float displacementMultiplier = 2.0f)
        {
            int cap = tightBoxes != null ? Math.Max(16, tightBoxes.Count * 2) : 16;
            var tree = new TtDynamicBVH<T>(cap, margin, displacementMultiplier);
            tree.BuildFromLeaves(tightBoxes, userData);
            return tree;
        }

        // Recursively build [first..last] (inclusive) over the sorted leaf
        // arrays. Returns the index of the subtree root and wires its parent.
        private int BuildLBVHRange(int[] sortedLeafIds, uint[] sortedCodes, int first, int last, int parent)
        {
            if (first == last)
            {
                int leaf = sortedLeafIds[first];
                mNodes[leaf].Parent = parent;
                return leaf;
            }

            int split = FindSplit(sortedCodes, first, last);

            int internalNode = AllocateNode();
            mNodes[internalNode].Parent = parent;
            mNodes[internalNode].UserData = default(T);

            int c1 = BuildLBVHRange(sortedLeafIds, sortedCodes, first, split, internalNode);
            int c2 = BuildLBVHRange(sortedLeafIds, sortedCodes, split + 1, last, internalNode);

            mNodes[internalNode].Child1 = c1;
            mNodes[internalNode].Child2 = c2;
            mNodes[internalNode].Height = 1 + Math.Max(mNodes[c1].Height, mNodes[c2].Height);
            mNodes[internalNode].Box = Combine(mNodes[c1].Box, mNodes[c2].Box);
            return internalNode;
        }

        // Find the position 'split' in [first..last-1] such that codes[split]
        // and codes[split+1] differ in the highest bit among the range. If all
        // codes in the range are identical, fall back to the median.
        private static int FindSplit(uint[] sortedCodes, int first, int last)
        {
            uint firstCode = sortedCodes[first];
            uint lastCode  = sortedCodes[last];
            if (firstCode == lastCode)
                return (first + last) >> 1;

            // CountLeadingZeros of XOR gives the LCP length.
            int commonPrefix = CountLeadingZeros32(firstCode ^ lastCode);

            // Binary search for the largest 'split' in [first, last-1] such that
            // sortedCodes[split] still shares the same commonPrefix with firstCode.
            int split = first;
            int step = last - first;
            do
            {
                step = (step + 1) >> 1;
                int newSplit = split + step;
                if (newSplit < last)
                {
                    int prefix = CountLeadingZeros32(firstCode ^ sortedCodes[newSplit]);
                    if (prefix > commonPrefix)
                        split = newSplit;
                }
            } while (step > 1);
            return split;
        }

        private static int CountLeadingZeros32(uint v)
        {
            if (v == 0) return 32;
            int n = 0;
            if ((v & 0xFFFF0000u) == 0) { n += 16; v <<= 16; }
            if ((v & 0xFF000000u) == 0) { n +=  8; v <<=  8; }
            if ((v & 0xF0000000u) == 0) { n +=  4; v <<=  4; }
            if ((v & 0xC0000000u) == 0) { n +=  2; v <<=  2; }
            if ((v & 0x80000000u) == 0) { n +=  1; }
            return n;
        }

        // ==================================================================
        // SAH-based topological optimization (rotation around internal nodes)
        //
        // For every internal node A with children (B, C), at most one of B/C
        // is itself an internal node with grandchildren. We try the 4 possible
        // swaps between {B's children, C's children} that change the topology
        // and adopt the variant that yields the lowest SAH cost (sum of
        // surface areas of the two new "parent" boxes). Iterating this sweep
        // a few times converges quickly to a near-SAH-optimal tree.
        //
        // This mirrors Bullet's btDbvt::optimizeIncremental and Box2D's
        // b2DynamicTree::Balance idea, but applied as a global passes.
        // ==================================================================

        /// <summary>
        /// Improve the tree quality by performing SAH-driven local rotations
        /// around every internal node. Cheap enough to run once after a bulk
        /// <see cref="BuildFromLeaves"/> or periodically during gameplay.
        /// </summary>
        /// <param name="iterations">Number of full sweeps over the tree (>=1). Most quality gain happens within 2-4 sweeps.</param>
        /// <returns>Total number of rotations actually applied across all sweeps.</returns>
        public int Optimize(int iterations = 4)
        {
            if (mRoot == NullNode || mNodes[mRoot].IsLeaf) return 0;
            if (iterations < 1) iterations = 1;

            int totalSwaps = 0;
            for (int it = 0; it < iterations; it++)
            {
                int swaps = 0;
                for (int i = 0; i < mNodeCapacity; i++)
                {
                    if (mNodes[i].IsFree) continue;
                    if (mNodes[i].IsLeaf) continue;
                    if (TryRotateAroundNode(i)) swaps++;
                }
                if (swaps == 0) break; // Converged.
                totalSwaps += swaps;
            }
            return totalSwaps;
        }

        // Try the 4 swap variants around node A = (B, C). Apply the one with
        // the smallest combined surface area, if it beats the current layout.
        private bool TryRotateAroundNode(int iA)
        {
            int iB = mNodes[iA].Child1;
            int iC = mNodes[iA].Child2;

            // Current cost: surface area of B + C (their parent A's box is
            // re-evaluated identically across all variants since it is the
            // union of the two children, which is unchanged by rotation —
            // only the *internal split* between B and C changes).
            double bestCost = SurfaceArea(mNodes[iB].Box) + SurfaceArea(mNodes[iC].Box);
            int bestVariant = 0; // 0 = no change

            // Variant 1/2: swap B with one of C's children (only if C is internal).
            if (!mNodes[iC].IsLeaf)
            {
                int iF = mNodes[iC].Child1;
                int iG = mNodes[iC].Child2;

                // Variant 1: B <-> F. New C becomes (B, G), new B is the old F (untouched).
                {
                    Aabb newCBox = Combine(mNodes[iB].Box, mNodes[iG].Box);
                    double cost = SurfaceArea(mNodes[iF].Box) + SurfaceArea(newCBox);
                    if (cost < bestCost) { bestCost = cost; bestVariant = 1; }
                }
                // Variant 2: B <-> G. New C becomes (F, B), new B is the old G.
                {
                    Aabb newCBox = Combine(mNodes[iF].Box, mNodes[iB].Box);
                    double cost = SurfaceArea(mNodes[iG].Box) + SurfaceArea(newCBox);
                    if (cost < bestCost) { bestCost = cost; bestVariant = 2; }
                }
            }
            // Variant 3/4: swap C with one of B's children (only if B is internal).
            if (!mNodes[iB].IsLeaf)
            {
                int iD = mNodes[iB].Child1;
                int iE = mNodes[iB].Child2;

                // Variant 3: C <-> D. New B becomes (C, E), new C is the old D.
                {
                    Aabb newBBox = Combine(mNodes[iC].Box, mNodes[iE].Box);
                    double cost = SurfaceArea(newBBox) + SurfaceArea(mNodes[iD].Box);
                    if (cost < bestCost) { bestCost = cost; bestVariant = 3; }
                }
                // Variant 4: C <-> E. New B becomes (D, C), new C is the old E.
                {
                    Aabb newBBox = Combine(mNodes[iD].Box, mNodes[iC].Box);
                    double cost = SurfaceArea(newBBox) + SurfaceArea(mNodes[iE].Box);
                    if (cost < bestCost) { bestCost = cost; bestVariant = 4; }
                }
            }

            if (bestVariant == 0) return false;
            ApplyRotationVariant(iA, bestVariant);
            return true;
        }

        // Materialize one of the 4 swap variants, then refit boxes/heights at A
        // and the affected child, and propagate refit upwards.
        private void ApplyRotationVariant(int iA, int variant)
        {
            int iB = mNodes[iA].Child1;
            int iC = mNodes[iA].Child2;

            switch (variant)
            {
                case 1: // B <-> F (C's first child)
                {
                    int iF = mNodes[iC].Child1;
                    int iG = mNodes[iC].Child2;
                    // A now has children (F, C); C now has children (B, G)
                    mNodes[iA].Child1 = iF;
                    mNodes[iC].Child1 = iB;
                    // Child2 of C stays iG.
                    mNodes[iF].Parent = iA;
                    mNodes[iB].Parent = iC;
                    RefitNode(iC);
                    break;
                }
                case 2: // B <-> G (C's second child)
                {
                    int iF = mNodes[iC].Child1;
                    int iG = mNodes[iC].Child2;
                    // A now has children (G, C); C now has children (F, B)
                    mNodes[iA].Child1 = iG;
                    mNodes[iC].Child2 = iB;
                    mNodes[iG].Parent = iA;
                    mNodes[iB].Parent = iC;
                    RefitNode(iC);
                    break;
                }
                case 3: // C <-> D (B's first child)
                {
                    int iD = mNodes[iB].Child1;
                    int iE = mNodes[iB].Child2;
                    // A now has children (B, D); B now has children (C, E)
                    mNodes[iA].Child2 = iD;
                    mNodes[iB].Child1 = iC;
                    mNodes[iD].Parent = iA;
                    mNodes[iC].Parent = iB;
                    RefitNode(iB);
                    break;
                }
                case 4: // C <-> E (B's second child)
                {
                    int iD = mNodes[iB].Child1;
                    int iE = mNodes[iB].Child2;
                    // A now has children (B, E); B now has children (D, C)
                    mNodes[iA].Child2 = iE;
                    mNodes[iB].Child2 = iC;
                    mNodes[iE].Parent = iA;
                    mNodes[iC].Parent = iB;
                    RefitNode(iB);
                    break;
                }
            }
            RefitNode(iA);
        }

        private void RefitNode(int idx)
        {
            int c1 = mNodes[idx].Child1;
            int c2 = mNodes[idx].Child2;
            mNodes[idx].Box = Combine(mNodes[c1].Box, mNodes[c2].Box);
            mNodes[idx].Height = 1 + Math.Max(mNodes[c1].Height, mNodes[c2].Height);
        }

        // ==================================================================
        // GPU flattening
        //
        // Linearize the tree into a contiguous, GPU-friendly array in BFS
        // order so that:
        //   - The root always lives at index 0 (the GPU traversal kernel
        //     hard-codes "start from 0").
        //   - Sibling nodes are adjacent in memory, which gives a compute-shader
        //     stack-traversal kernel decent locality across a wave when many
        //     rays follow similar paths.
        //
        // Node payload kept on the GPU side is intentionally minimal:
        //   - Aabb (BoxMin / BoxMax, single precision — GPU doesn't have
        //     cheap fp64 and the BVH always lives in scene-local meters).
        //   - Two child indices for internal nodes; for leaves we collapse
        //     Child1 into a NullChild sentinel and reuse Child2 as a
        //     PAYLOAD INDEX into a parallel buffer the caller maintains.
        //
        // We don't ship the payload itself (T) here: it can be anything —
        // a scene-node id, a triangle list pointer, a material descriptor.
        // The caller knows its layout and writes its own buffer indexed by
        // the leafIndex parameter handed to onLeafFlattened.
        //
        // Caller contract:
        //   - boxMins / boxMaxs / child1 / child2OrPayload must each be
        //     pre-sized to NodeCountForFlatten() (= 2*ProxyCount - 1 when
        //     the tree is non-degenerate, but we just return the exact
        //     count we'll write).
        //   - onLeafFlattened(proxyId, payload, leafIndex) is called for
        //     every leaf in flatten order; leafIndex is a dense [0..L-1]
        //     value the caller can use as the slot in its own payload buffer.
        //   - LeafChildSentinel marks "this is a leaf" in the GPU layout
        //     (Child1 == LeafChildSentinel  =>  Child2 holds payload index).
        // ==================================================================
        public const uint LeafChildSentinel = 0xFFFFFFFFu;

        /// <summary>
        /// Number of GPU nodes that will be written by <see cref="FlattenToGpu"/>
        /// for the current tree. 0 when the tree is empty.
        /// </summary>
        public int FlattenedNodeCount
        {
            get
            {
                if (mRoot == NullNode) return 0;
                // 1 node + (n-1) internal nodes for a binary tree with n leaves.
                // Equivalent to "count of in-use, non-free nodes reachable from
                // root" which for a well-formed BVH is exactly 2 * mProxyCount - 1.
                return 2 * mProxyCount - 1;
            }
        }

        /// <summary>
        /// Number of leaves currently in the tree. Same as <see cref="ProxyCount"/>;
        /// exposed under a different name so flattening callers don't have to
        /// reason about whether internal proxies are leaves.
        /// </summary>
        public int FlattenedLeafCount { get { return mProxyCount; } }

        /// <summary>
        /// Walk the tree in breadth-first order, writing one packed GPU node
        /// per visited tree node into the four parallel arrays. Leaves are
        /// emitted in BFS order with monotonically increasing <c>leafIndex</c>
        /// (0, 1, 2, ...) so the caller can append to its payload buffer
        /// in lock-step.
        ///
        /// The root is written at index 0 in every output array.
        /// Returns the number of nodes written (== <see cref="FlattenedNodeCount"/>).
        /// </summary>
        /// <param name="boxMins">Output: per-node AABB lower bound (single).</param>
        /// <param name="boxMaxs">Output: per-node AABB upper bound (single).</param>
        /// <param name="child1">Output: first child index, or <see cref="LeafChildSentinel"/> for leaves.</param>
        /// <param name="child2OrPayload">Output: second child index for internal nodes,
        ///   or the leaf's payload index (== leafIndex passed to onLeafFlattened) for leaves.</param>
        /// <param name="onLeafFlattened">
        /// Callback invoked for each leaf in flatten order. Arguments:
        ///   proxyId  — the original CPU-side proxy id (same value returned by InsertProxy).
        ///   payload  — the leaf's user data of type T.
        ///   leafIndex — dense 0-based index identifying this leaf's slot in any
        ///               parallel payload buffer the caller maintains.
        /// May be null if the caller doesn't need a payload buffer.
        /// </param>
        /// <returns>Number of GPU nodes written.</returns>
        public int FlattenToGpu(
            Vector3[] boxMins,
            Vector3[] boxMaxs,
            uint[] child1,
            uint[] child2OrPayload,
            Action<int /*proxyId*/, T /*payload*/, int /*leafIndex*/> onLeafFlattened)
        {
            if (mRoot == NullNode) return 0;

            int nodeCount = FlattenedNodeCount;
            if (boxMins == null || boxMaxs == null || child1 == null || child2OrPayload == null)
                throw new ArgumentNullException("output arrays");
            if (boxMins.Length < nodeCount || boxMaxs.Length < nodeCount
                || child1.Length < nodeCount || child2OrPayload.Length < nodeCount)
                throw new ArgumentException("output arrays too small; use FlattenedNodeCount to size them");

            // Two parallel scratch arrays: traversal queue + remap from
            // CPU node id -> GPU output slot. We don't reuse mQueryStack
            // because that's a Stack, and we want stable BFS ordering.
            var queue = new int[nodeCount];
            var cpuToGpu = new Dictionary<int, int>(nodeCount);
            int qHead = 0;
            int qTail = 0;
            int leafCounter = 0;

            // Root goes to slot 0.
            queue[qTail++] = mRoot;
            cpuToGpu[mRoot] = 0;
            int writeCursor = 1;

            while (qHead < qTail)
            {
                int cpuId = queue[qHead++];
                int gpuSlot = cpuToGpu[cpuId];

                ref Node n = ref mNodes[cpuId];
                var min = n.Box.Minimum;
                var max = n.Box.Maximum;
                boxMins[gpuSlot] = new Vector3((float)min.X, (float)min.Y, (float)min.Z);
                boxMaxs[gpuSlot] = new Vector3((float)max.X, (float)max.Y, (float)max.Z);

                if (n.IsLeaf)
                {
                    int leafIdx = leafCounter++;
                    child1[gpuSlot] = LeafChildSentinel;
                    child2OrPayload[gpuSlot] = (uint)leafIdx;
                    if (onLeafFlattened != null)
                        onLeafFlattened(cpuId, n.UserData, leafIdx);
                }
                else
                {
                    // Reserve GPU slots for children before descending so
                    // siblings end up adjacent.
                    int g1 = writeCursor++;
                    int g2 = writeCursor++;
                    cpuToGpu[n.Child1] = g1;
                    cpuToGpu[n.Child2] = g2;
                    child1[gpuSlot] = (uint)g1;
                    child2OrPayload[gpuSlot] = (uint)g2;
                    queue[qTail++] = n.Child1;
                    queue[qTail++] = n.Child2;
                }
            }

            // Sanity: writeCursor must equal nodeCount and qTail == nodeCount.
            // If they don't, the tree is malformed (e.g. ProxyCount stale).
            // We don't throw because debug visualizers can call this on a tree
            // that's mid-edit, but we return the actual count.
            return writeCursor;
        }
    }
}
