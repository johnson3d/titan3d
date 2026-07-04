using System;
using EngineNS.GamePlay.Scene;
using EngineNS.Profiler;
using System.Collections.Generic;
using System.ComponentModel;

// A Dynamic, Loose Octree for storing any objects that can be described with AABB bounds
// See also: PointOctree, where objects are stored as single points and some code can be simplified
// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes)
//			and places objects into the appropriate nodes. This allows fast access to objects
//			in an area of interest without having to check every object.
// Dynamic: The octree grows or shrinks as required when objects as added or removed
//			It also splits and merges nodes as appropriate. There is no maximum depth.
//			Nodes have a constant - numObjectsAllowed - which sets the amount of items allowed in a node before it splits.
// Loose:	The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
//			This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
//			A looseness value of 1.0 will make it a "normal" octree.
// T:		The content of the octree can be anything, since the bounds data is supplied separately.

// Originally written for my game Scraps (http://www.scrapsgame.com) but intended to be general-purpose.
// Copyright 2014 Nition, BSD licence (see LICENCE file). www.momentstudio.co.nz
// Unity-based, but could be adapted to work in pure C#

// Note: For loops are often used here since in some cases (e.g. the IsColliding method)
// they actually give much better performance than using Foreach, even in the compiled build.
// Using a LINQ expression is worse again than Foreach.

namespace EngineNS.Bricks.Collision.Octree
{
    public class TtBoundsOctree<T> where T : Bricks.Collision.Octree.IBoundsOctreeObject
    {
        // The total amount of objects currently in the tree
        public int Count { get; private set; }

        // Root node of the octree
        TtBoundsOctreeNode<T> RootNode
        {
            get;
            set;
        }
        public TtBoundsOctreeNode<T> GetRoot()
        {
            return RootNode;
        }

        // Should be a value between 1 and 2. A multiplier for the base size of a node.
        // 1.0 is a "normal" octree, while values > 1 have overlap
        readonly float looseness;

        // Size that the octree was on creation
        readonly float initialSize;

        // Minimum side length that a node can be - essentially an alternative to having a max depth
        readonly float minSize;
        // For collision visualisation. Automatically removed in builds.

        /// <summary>
        /// Constructor for the bounds octree.
        /// </summary>
        /// <param name="initialWorldSize">Size of the sides of the initial node, in metres. The octree will never shrink smaller than this.</param>
        /// <param name="initialWorldPos">Position of the centre of the initial node.</param>
        /// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this (metres).</param>
        /// <param name="loosenessVal">Clamped between 1 and 2. Values > 1 let nodes overlap.</param>
        public TtBoundsOctree(float initialWorldSize, DVector3 initialWorldPos, float minNodeSize, float loosenessVal)
        {
            if (minNodeSize > initialWorldSize)
            {
                Profiler.Log.WriteLineSingle("Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }
            Count = 0;
            initialSize = initialWorldSize;
            minSize = minNodeSize;
            looseness = MathHelper.Clamp(loosenessVal, 1.0f, 2.0f);
            RootNode = new TtBoundsOctreeNode<T>(initialSize, minSize, looseness, initialWorldPos);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Remove all objects from the entire octree, resetting it to an empty state.
        /// The root node structure is preserved at its current position and initial size.
        /// </summary>
        public void Clear()
        {
            RootNode.Clear();
            RootNode = new TtBoundsOctreeNode<T>(initialSize, minSize, looseness, RootNode.Center);
            Count = 0;
        }

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        public void Add(T obj, in Aabb objBounds)
        {
            // Add object or expand the octree until it can be added
            int count = 0; // Safety check against infinite/excessive growth
            while (!RootNode.Add(obj, objBounds))
            {
                Grow(objBounds.Center - RootNode.Center);
                if (++count > 20)
                {
                    Profiler.Log.WriteLineSingle("Aborted Add operation as it seemed to be going on forever (" + (count - 1) + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(in T obj)
        {
            bool removed = RootNode.Remove(in obj);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, in Aabb objBounds)
        {
            bool removed = RootNode.Remove(obj, in objBounds);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Move an object to a new bounding box. Uses a fast path when the object
        /// stays within its current OctreeNode, avoiding a full Remove+Add cycle.
        /// </summary>
        /// <param name="obj">Object to move.</param>
        /// <param name="newBounds">New 3D bounding box.</param>
        public void Move(T obj, in Aabb newBounds)
        {
            var ownerNode = obj.OctreeOwner as TtBoundsOctreeNode<T>;
            if (ownerNode != null && ownerNode.TryUpdateBounds(obj, in newBounds))
                return;

            // Slow path: object left its current node — full Remove + Add
            Remove(in obj);
            Add(obj, in newBounds);
        }

        /// <summary>
        /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkBounds">bounds to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool IsColliding(in Aabb checkBounds)
        {
            return RootNode.IsColliding(in checkBounds);
        }

        /// <summary>
        /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkRay">ray to check.</param>
        /// <param name="maxDistance">distance to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool IsColliding(in DRay checkRay, double maxDistance)
        {
            return RootNode.IsColliding(in checkRay, maxDistance);
        }

        /// <summary>
        /// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="collidingWith">list to store intersections.</param>
        /// <param name="checkBounds">bounds to check.</param>
        /// <returns>Objects that intersect with the specified bounds.</returns>
        public void GetColliding(List<T> collidingWith, in Aabb checkBounds)
        {            
            RootNode.GetColliding(in checkBounds, collidingWith);
        }

        /// <summary>
        /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="collidingWith">list to store intersections.</param>
        /// <param name="checkRay">ray to check.</param>
        /// <param name="maxDistance">distance to check.</param>
        /// <returns>Objects that intersect with the specified ray.</returns>
        public void GetColliding(List<T> collidingWith,in DRay checkRay, double maxDistance = double.PositiveInfinity)
        {
            RootNode.GetColliding(in checkRay, collidingWith, maxDistance);
        }

        //public List<T> GetWithinFrustum(Camera cam)
        //{
        //    var planes = GeometryUtility.CalculateFrustumPlanes(cam);

        //    var list = new List<T>();
        //    rootNode.GetWithinFrustum(planes, list);
        //    return list;
        //}

        public Aabb GetMaxBounds()
        {
            return RootNode.GetBounds();
        }

        /// <summary>
        /// Draws node boundaries visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
        /// </summary>
        public void DrawAllBounds(GamePlay.TtPlacementBase Placement, GamePlay.TtWorld.TtVisParameter vp)
        {
            if (Placement == null)
                return;
            RootNode.DrawAllBounds(vp, Placement, 0);
        }

        /// <summary>
        /// Draws the bounds of all objects in the tree visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllBounds.
        /// </summary>
        public void DrawAllObjects(GamePlay.TtPlacementBase Placement, GamePlay.TtWorld.TtVisParameter vp)
        {
            if (Placement == null)
                return;
            RootNode.DrawAllObjects(vp, Placement);
        }
        /// <summary>
        /// Grow the octree to fit in all objects.
        /// </summary>
        /// <param name="direction">Direction to grow.</param>
        void Grow(DVector3 direction)
        {
            int xDirection = direction.X >= 0 ? 1 : -1;
            int yDirection = direction.Y >= 0 ? 1 : -1;
            int zDirection = direction.Z >= 0 ? 1 : -1;
            TtBoundsOctreeNode<T> oldRoot = RootNode;
            float half = RootNode.BaseLength / 2;
            float newLength = RootNode.BaseLength * 2;
            var newCenter = RootNode.Center + new DVector3(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            RootNode = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                // Create 7 new octree children to go with the old root as children of the new root
                int rootPos = RootNode.BestFitChild(oldRoot.Center);
                TtBoundsOctreeNode<T>[] children = new TtBoundsOctreeNode<T>[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == rootPos)
                    {
                        children[i] = oldRoot;
                    }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new TtBoundsOctreeNode<T>(oldRoot.BaseLength, minSize, looseness, newCenter + new DVector3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                // Attach the new children to the new root node
                RootNode.SetChildren(children);
            }
        }

        /// <summary>
        /// Shrink the octree if possible, else leave it the same.
        /// </summary>
        void Shrink()
        {
            RootNode = RootNode.ShrinkIfPossible(initialSize);
        }
    }

    /// <summary>
    /// Wrapper class that sits inside the Octree and weakly references a TtNode.
    /// The Octree strongly references this wrapper (cheap, ~32 bytes), while the
    /// actual TtNode is only weakly held, so it can be GC'd independently.
    /// </summary>
    public class TtOctreeEntry : IBoundsOctreeObject
    {
        public object OctreeOwner { get; set; }
        public WeakReference<GamePlay.Scene.TtNode> WeakNode;

        public TtOctreeEntry(GamePlay.Scene.TtNode node)
        {
            WeakNode = new WeakReference<GamePlay.Scene.TtNode>(node);
        }

        public bool TryGetNode(out GamePlay.Scene.TtNode node)
        {
            return WeakNode.TryGetTarget(out node);
        }

        public bool IsAlive => WeakNode.TryGetTarget(out _);
    }

    public partial class TtCollideOctree : IDisposable
    {
        public Bricks.Collision.Octree.TtBoundsOctree<TtOctreeEntry> mOctree = null;
        public NxRHI.TtTransientBuffer TransientVB = new();
        public NxRHI.TtTransientBuffer TransientIB = new();
        [Category("Option")]
        public bool IsDrawBounds { get; set; } = false;
        public async Thread.Async.TtTask<bool> Initialize(DVector3 position)
        {
            mOctree = new Bricks.Collision.Octree.TtBoundsOctree<TtOctreeEntry>(0.5f, position, 1, 1.25f);
            return true;
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref TransientVB);
            CoreSDK.DisposeObject(ref TransientIB);
            mOctree.GetRoot().IterateObject(static (obj, arg) =>
            {
                obj.Obj.OctreeOwner = null;
                if (obj.Obj.TryGetNode(out var node))
                    node.OctreeEntry = null;
                return true;
            }, null);
        }
        public void TickLogic()
        {
            TransientVB.Reset();
            TransientIB.Reset();
            PurgeDeadEntries();
        }

        /// <summary>
        /// Remove all entries from the octree and clear their back-references on TtNode.
        /// </summary>
        public void Clear()
        {
            if (mOctree == null)
                return;

            mOctree.GetRoot().IterateObject(static (obj, arg) =>
            {
                obj.Obj.OctreeOwner = null;
                if (obj.Obj.TryGetNode(out var node))
                    node.OctreeEntry = null;
                return true;
            }, null);

            mOctree.Clear();
            mDeadEntries.Clear();
        }
        public void Add(GamePlay.Scene.TtNode node, in Aabb bounds)
        {
            if (node.IsCollide == false)
                return;
            var entry = node.OctreeEntry;
            if (entry == null)
            {
                entry = new TtOctreeEntry(node);
                node.OctreeEntry = entry;
            }
            mOctree.Add(entry, in bounds);
        }
        public void Remove(GamePlay.Scene.TtNode node)
        {
            var entry = node.OctreeEntry;
            if (entry == null)
                return;
            mOctree.Remove(in entry);
            entry.OctreeOwner = null;
            node.OctreeEntry = null;

            foreach (var c in node.Children)
            {
                this.Remove(c);
            }
        }
        public void Move(GamePlay.Scene.TtNode node, in Aabb newBounds)
        {
            if (node.IsCollide == false)
                return;
            var entry = node.OctreeEntry;
            if (entry == null)
                return;
            mOctree.Move(entry, in newBounds);
        }
        public void GetColliding(List<GamePlay.Scene.TtNode> nodes, in Aabb bound)
        {
            mEntryBuffer.Clear();
            mOctree.GetColliding(mEntryBuffer, in bound);
            ResolveEntries(mEntryBuffer, nodes);
        }
        [Rtti.Meta("")]
        public bool IsColliding(in DRay checkRay, double maxDistance)
        {
            return mOctree.IsColliding(in checkRay, maxDistance);
        }
        public void GetColliding(List<GamePlay.Scene.TtNode> collidingWith, in DRay checkRay, double maxDistance = double.PositiveInfinity)
        {
            mEntryBuffer.Clear();
            mOctree.GetColliding(mEntryBuffer, in checkRay, maxDistance);
            ResolveEntries(mEntryBuffer, collidingWith);
        }
        public unsafe bool OctreeHitTest(in DVector3 start, in DVector3 end, ref List<GamePlay.Scene.TtNode> candidates, VHitResult* hitResult)
        {
            if (mOctree == null)
                return false;

            var dir = end - start;
            var maxDistance = dir.Length();
            if (maxDistance < 1e-6)
                return false;

            var normalizedDir = dir / maxDistance;
            var ray = new DRay()
            {
                Position = start,
                Direction = new Vector3((float)normalizedDir.X, (float)normalizedDir.Y, (float)normalizedDir.Z),
            };

            if (candidates == null)
                candidates = new List<GamePlay.Scene.TtNode>();

            mEntryBuffer.Clear();
            mOctree.GetColliding(mEntryBuffer, in ray, maxDistance);
            ResolveEntries(mEntryBuffer, candidates);

            if (candidates.Count == 0)
                return false;

            if (hitResult == (VHitResult*)IntPtr.Zero.ToPointer())
                return true;

            bool hasHit = false;
            double closestDistSq = double.MaxValue;
            ref VHitResult tempResult = ref *hitResult;

            for (int i = 0; i < candidates.Count; i++)
            {
                var node = candidates[i];
                if (node.LineCheck(in start, in end, ref tempResult))
                {
                    var hitDist = (tempResult.Position - start).LengthSquared();
                    if (hitDist < closestDistSq)
                    {
                        closestDistSq = hitDist;
                        *hitResult = tempResult;
                        hasHit = true;
                    }
                }
            }

            return hasHit;
        }

        private readonly List<TtOctreeEntry> mEntryBuffer = new();
        private readonly List<TtOctreeEntry> mDeadEntries = new();

        /// <summary>
        /// Resolve WeakReference entries to live TtNode instances.
        /// Dead entries (GC'd nodes) are collected for deferred cleanup in TickLogic.
        /// </summary>
        private void ResolveEntries(List<TtOctreeEntry> entries, List<GamePlay.Scene.TtNode> outNodes)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].TryGetNode(out var node))
                    outNodes.Add(node);
                else
                    mDeadEntries.Add(entries[i]);
            }
        }

        /// <summary>
        /// Remove dead (GC'd) entries from the Octree. Called once per frame in TickLogic.
        /// </summary>
        private void PurgeDeadEntries()
        {
            if (mDeadEntries.Count == 0)
                return;
            for (int i = 0; i < mDeadEntries.Count; i++)
            {
                var dead = mDeadEntries[i];
                mOctree.Remove(in dead);
            }
            mDeadEntries.Clear();
        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    public partial class TtNode
    {
        /// <summary>
        /// The Octree wrapper entry that weakly references this node.
        /// Null if this node is not in the world Octree.
        /// </summary>
        public Bricks.Collision.Octree.TtOctreeEntry OctreeEntry { get; set; }

        /// <summary>
        /// The OctreeNode this node currently resides in (via its entry wrapper).
        /// </summary>
        public Bricks.Collision.Octree.TtBoundsOctreeNode<Bricks.Collision.Octree.TtOctreeEntry> OctreeNode
        {
            get => OctreeEntry?.OctreeOwner as Bricks.Collision.Octree.TtBoundsOctreeNode<Bricks.Collision.Octree.TtOctreeEntry>;
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.Collision.Octree
{
	partial class TtCollideOctree
	{
		public unsafe bool macross_IsColliding (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, in DRay checkRay, double maxDistance) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = IsColliding(in checkRay, maxDistance);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross