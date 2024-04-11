using EngineNS.GamePlay;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
// A node in a BoundsOctree
// Copyright 2014 Nition, BSD licence (see LICENCE file). www.momentstudio.co.nz

namespace EngineNS.Bricks.Collision.Octree
{
    public class TtBoundsOctreeNode<T>
    {
        // Centre of this node
        public DVector3 Center { get; private set; }

        // Length of this node if it has a looseness of 1.0
        public float BaseLength { get; private set; }

        // Looseness value for this node
        float looseness;

        // Minimum size for a node in this octree
        float minSize;

        // Actual length of sides, taking the looseness value into account
        float adjLength;

        // Bounding box that represents this node
        Aabb Bounds = default(Aabb);

        // Objects in this node
        readonly List<OctreeObject> OctObjects = new List<OctreeObject>();

        // Child nodes, if any
        TtBoundsOctreeNode<T>[] Children = null;

        bool HasChildren { get { return Children != null; } }

        // BoundingBox of potential children to this node. These are actual size (with looseness taken into account), not base size
        Aabb[] childBounds;

        // If there are already NUM_OBJECTS_ALLOWED in a node, we split it into children
        // A generally good number seems to be something around 8-15
        const int NUM_OBJECTS_ALLOWED = 8;

        // An object in the octree
        struct OctreeObject
        {
            public T Obj;
            public Aabb BoundingBox;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
        /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
        /// <param name="loosenessVal">Multiplier for baseLengthVal to get the actual size.</param>
        /// <param name="centerVal">Centre position of this node.</param>
        public TtBoundsOctreeNode(float baseLengthVal, float minSizeVal, float loosenessVal, DVector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, loosenessVal, centerVal);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        /// <returns>True if the object fits entirely within this node.</returns>
        public bool Add(T obj, in Aabb objBounds)
        {
            if (!Encapsulates(in Bounds, in objBounds))
            {
                return false;
            }
            SubAdd(obj, objBounds);
            return true;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            bool removed = false;

            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (OctObjects[i].Obj.Equals(obj))
                {
                    removed = OctObjects.Remove(OctObjects[i]);
                    break;
                }
            }

            if (!removed && Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    removed = Children[i].Remove(obj);
                    if (removed) break;
                }
            }

            if (removed && Children != null)
            {
                // Check if we should merge nodes now that we've removed an item
                if (ShouldMerge())
                {
                    Merge();
                }
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
            if (!Encapsulates(Bounds, objBounds))
            {
                return false;
            }
            return SubRemove(obj, in objBounds);
        }

        /// <summary>
        /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkBounds">BoundingBox to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool IsColliding(in Aabb checkBounds)
        {
            // Are the input bounds at least partially in this node?
            if (Aabb.IsIntersect(in Bounds, in checkBounds) == false)
            {
                return false;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (Aabb.IsIntersect(OctObjects[i].BoundingBox, in checkBounds) != false)
                {
                    return true;
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Children[i].IsColliding(in checkBounds))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkRay">Ray to check.</param>
        /// <param name="maxDistance">Distance to check.</param>
        /// <returns>True if there was a collision.</returns>
        public bool IsColliding(in DRay checkRay, double maxDistance = double.PositiveInfinity)
        {
            // Is the input ray at least partially in this node?
            double distance;
            if (!DRay.Intersects(in checkRay, in Bounds, out distance) || distance > maxDistance)
            {
                return false;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                var box = OctObjects[i].BoundingBox;
                if (DRay.Intersects(in checkRay, in box, out distance) && distance <= maxDistance)
                {
                    return true;
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Children[i].IsColliding(in checkRay, maxDistance))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="checkBounds">BoundingBox to check. Passing by ref as it improves performance with structs.</param>
        /// <param name="result">List result.</param>
        /// <returns>Objects that intersect with the specified bounds.</returns>
        public void GetColliding(in Aabb checkBounds, List<T> result)
        {
            // Are the input bounds at least partially in this node?
            if (Aabb.IsIntersect(in Bounds, in checkBounds) == false)
            {
                return;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (Aabb.IsIntersect(OctObjects[i].BoundingBox, in checkBounds) != false)
                {
                    result.Add(OctObjects[i].Obj);
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].GetColliding(in checkBounds, result);
                }
            }
        }

        /// <summary>
        /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="checkRay">Ray to check. Passing by ref as it improves performance with structs.</param>
        /// <param name="maxDistance">Distance to check.</param>
        /// <param name="result">List result.</param>
        /// <returns>Objects that intersect with the specified ray.</returns>
        public void GetColliding(in DRay checkRay, List<T> result, double maxDistance = double.PositiveInfinity)
        {
            double distance;
            // Is the input ray at least partially in this node?
            if (!DRay.Intersects(in checkRay, in Bounds, out distance) || distance > maxDistance)
            {
                return;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (DRay.Intersects(in checkRay, OctObjects[i].BoundingBox, out distance) && distance <= maxDistance)
                {
                    result.Add(OctObjects[i].Obj);
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].GetColliding(in checkRay, result, maxDistance);
                }
            }
        }

        public unsafe void GetWithinFrustum(DPlane* planes, int num, List<T> result)
        {
            // Is the input node inside the frustum?
            
            if (!DPlane.Intersects(planes, num, Bounds))
            {
                return;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (DPlane.Intersects(planes, num, OctObjects[i].BoundingBox))
                {
                    result.Add(OctObjects[i].Obj);
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].GetWithinFrustum(planes, num, result);
                }
            }
        }

        /// <summary>
        /// Set the 8 children of this octree.
        /// </summary>
        /// <param name="childOctrees">The 8 new child nodes.</param>
        public void SetChildren(TtBoundsOctreeNode<T>[] childOctrees)
        {
            if (childOctrees.Length != 8)
            {
                Console.WriteLine("Child octree array must be length 8. Was length: " + childOctrees.Length);
                return;
            }

            Children = childOctrees;
        }

        public Aabb GetBounds()
        {
            return Bounds;
        }

        /// <summary>
        /// Draws node boundaries visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
        /// </summary>
        /// <param name="depth">Used for recurcive calls to this method.</param>
        public void DrawAllBounds(GamePlay.UWorld.UVisParameter vp, GamePlay.UPlacement placement, float depth)
        {
            float tintVal = depth / 7; // Will eventually get values > 1. Color rounds to 1 automatically
            var color = new Color4f(tintVal, 0, 1.0f - tintVal);

            var thisBounds = new Aabb(Center, new Vector3(adjLength, adjLength, adjLength));
            vp.AddAABB(in thisBounds, in color, in placement.AbsTransform);

            if (Children != null)
            {
                depth++;
                for (int i = 0; i < 8; i++)
                {
                    Children[i].DrawAllBounds(vp, placement, depth);
                }
            }
        }

        /// <summary>
        /// Draws the bounds of all objects in the tree visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllBounds.
        /// </summary>
        public void DrawAllObjects(GamePlay.UWorld.UVisParameter vp, GamePlay.UPlacement placement)
        {
            float tintVal = BaseLength / 20;
            var color = new Color4f(1.0f - tintVal, tintVal, 0.25f);

            foreach (OctreeObject obj in OctObjects)
            {
                vp.AddAABB(in obj.BoundingBox, in color, in placement.AbsTransform);
            }

            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].DrawAllObjects(vp, placement);
                }
            }
        }

        /// <summary>
        /// We can shrink the octree if:
        /// - This node is >= double minLength in length
        /// - All objects in the root node are within one octant
        /// - This node doesn't have children, or does but 7/8 children are empty
        /// We can also shrink it if there are no objects left at all!
        /// </summary>
        /// <param name="minLength">Minimum dimensions of a node in this octree.</param>
        /// <returns>The new root, or the existing one if we didn't shrink.</returns>
        public TtBoundsOctreeNode<T> ShrinkIfPossible(float minLength)
        {
            if (BaseLength < (2 * minLength))
            {
                return this;
            }
            if (OctObjects.Count == 0 && (Children == null || Children.Length == 0))
            {
                return this;
            }

            // Check objects in root
            int bestFit = -1;
            for (int i = 0; i < OctObjects.Count; i++)
            {
                OctreeObject curObj = OctObjects[i];
                int newBestFit = BestFitChild(in curObj.BoundingBox.Center);
                if (i == 0 || newBestFit == bestFit)
                {
                    // In same octant as the other(s). Does it fit completely inside that octant?
                    if (Encapsulates(in childBounds[newBestFit], in curObj.BoundingBox))
                    {
                        if (bestFit < 0)
                        {
                            bestFit = newBestFit;
                        }
                    }
                    else
                    {
                        // Nope, so we can't reduce. Otherwise we continue
                        return this;
                    }
                }
                else
                {
                    return this; // Can't reduce - objects fit in different octants
                }
            }

            // Check objects in children if there are any
            if (Children != null)
            {
                bool childHadContent = false;
                for (int i = 0; i < Children.Length; i++)
                {
                    if (Children[i].HasAnyObjects())
                    {
                        if (childHadContent)
                        {
                            return this; // Can't shrink - another child had content already
                        }
                        if (bestFit >= 0 && bestFit != i)
                        {
                            return this; // Can't reduce - objects in root are in a different octant to objects in child
                        }
                        childHadContent = true;
                        bestFit = i;
                    }
                }
            }

            // Can reduce
            if (Children == null)
            {
                // We don't have any children, so just shrink this node to the new size
                // We already know that everything will still fit in it
                SetValues(BaseLength / 2, minSize, looseness, childBounds[bestFit].Center);
                return this;
            }

            // No objects in entire octree
            if (bestFit == -1)
            {
                return this;
            }

            // We have children. Use the appropriate child as the new root node
            return Children[bestFit];
        }

        /// <summary>
        /// Find which child node this object would be most likely to fit in.
        /// </summary>
        /// <param name="objBounds">The object's bounds.</param>
        /// <returns>One of the eight child octants.</returns>
        public int BestFitChild(in DVector3 objBoundsCenter)
        {
            return (objBoundsCenter.X <= Center.X ? 0 : 1) + (objBoundsCenter.Y >= Center.Y ? 0 : 4) + (objBoundsCenter.Z <= Center.Z ? 0 : 2);
        }

        /// <summary>
        /// Checks if this node or anything below it has something in it.
        /// </summary>
        /// <returns>True if this node or any of its children, grandchildren etc have something in them</returns>
        public bool HasAnyObjects()
        {
            if (OctObjects.Count > 0) return true;

            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Children[i].HasAnyObjects()) return true;
                }
            }

            return false;
        }

        /*
        /// <summary>
        /// Get the total amount of objects in this node and all its children, grandchildren etc. Useful for debugging.
        /// </summary>
        /// <param name="startingNum">Used by recursive calls to add to the previous total.</param>
        /// <returns>Total objects in this node and its children, grandchildren etc.</returns>
        public int GetTotalObjects(int startingNum = 0) {
            int totalObjects = startingNum + objects.Count;
            if (children != null) {
                for (int i = 0; i < 8; i++) {
                    totalObjects += children[i].GetTotalObjects();
                }
            }
            return totalObjects;
        }
        */

        // #### PRIVATE METHODS ####

        /// <summary>
        /// Set values for this node. 
        /// </summary>
        /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
        /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
        /// <param name="loosenessVal">Multiplier for baseLengthVal to get the actual size.</param>
        /// <param name="centerVal">Centre position of this node.</param>
        void SetValues(float baseLengthVal, float minSizeVal, float loosenessVal, DVector3 centerVal)
        {
            BaseLength = baseLengthVal;
            minSize = minSizeVal;
            looseness = loosenessVal;
            Center = centerVal;
            adjLength = looseness * baseLengthVal;

            // Create the bounding box.
            Vector3 size = new Vector3(adjLength, adjLength, adjLength);
            Bounds = new Aabb(Center, size);

            float quarter = BaseLength / 4f;
            float childActualLength = (BaseLength / 2) * looseness;
            Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
            childBounds = new Aabb[8];
            childBounds[0] = new Aabb(Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
            childBounds[1] = new Aabb(Center + new Vector3(quarter, quarter, -quarter), childActualSize);
            childBounds[2] = new Aabb(Center + new Vector3(-quarter, quarter, quarter), childActualSize);
            childBounds[3] = new Aabb(Center + new Vector3(quarter, quarter, quarter), childActualSize);
            childBounds[4] = new Aabb(Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            childBounds[5] = new Aabb(Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
            childBounds[6] = new Aabb(Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
            childBounds[7] = new Aabb(Center + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        /// <summary>
        /// Private counterpart to the public Add method.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        void SubAdd(T obj, in Aabb objBounds)
        {
            // We know it fits at this level if we've got this far

            // We always put things in the deepest possible child
            // So we can skip some checks if there are children aleady
            if (!HasChildren)
            {
                // Just add if few objects are here, or children would be below min size
                if (OctObjects.Count < NUM_OBJECTS_ALLOWED || (BaseLength / 2) < minSize)
                {
                    OctreeObject newObj = new OctreeObject { Obj = obj, BoundingBox = objBounds };
                    OctObjects.Add(newObj);
                    return; // We're done. No children yet
                }

                // Fits at this level, but we can go deeper. Would it fit there?
                // Create the 8 children
                int bestFitChild;
                if (Children == null)
                {
                    Split();
                    if (Children == null)
                    {
                        Profiler.Log.WriteLineSingle("Child creation failed for an unknown reason. Early exit.");
                        return;
                    }

                    // Now that we have the new children, see if this node's existing objects would fit there
                    for (int i = OctObjects.Count - 1; i >= 0; i--)
                    {
                        OctreeObject existingObj = OctObjects[i];
                        // Find which child the object is closest to based on where the
                        // object's center is located in relation to the octree's center
                        bestFitChild = BestFitChild(existingObj.BoundingBox.Center);
                        // Does it fit?
                        if (Encapsulates(Children[bestFitChild].Bounds, existingObj.BoundingBox))
                        {
                            Children[bestFitChild].SubAdd(existingObj.Obj, existingObj.BoundingBox); // Go a level deeper					
                            OctObjects.Remove(existingObj); // Remove from here
                        }
                    }
                }
            }

            // Handle the new object we're adding now
            int bestFit = BestFitChild(objBounds.Center);
            if (Encapsulates(Children[bestFit].Bounds, objBounds))
            {
                Children[bestFit].SubAdd(obj, objBounds);
            }
            else
            {
                // Didn't fit in a child. We'll have to it to this node instead
                OctreeObject newObj = new OctreeObject { Obj = obj, BoundingBox = objBounds };
                OctObjects.Add(newObj);
            }
        }

        /// <summary>
        /// Private counterpart to the public <see cref="Remove(T, BoundingBox)"/> method.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        bool SubRemove(T obj, in Aabb objBounds)
        {
            bool removed = false;

            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (OctObjects[i].Obj.Equals(obj))
                {
                    removed = OctObjects.Remove(OctObjects[i]);
                    break;
                }
            }

            if (!removed && Children != null)
            {
                int bestFitChild = BestFitChild(in objBounds.Center);
                removed = Children[bestFitChild].SubRemove(obj, in objBounds);
            }

            if (removed && Children != null)
            {
                // Check if we should merge nodes now that we've removed an item
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        /// <summary>
        /// Splits the octree into eight children.
        /// </summary>
        void Split()
        {
            float quarter = BaseLength / 4f;
            float newLength = BaseLength / 2;
            Children = new TtBoundsOctreeNode<T>[8];
            Children[0] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(-quarter, quarter, -quarter));
            Children[1] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(quarter, quarter, -quarter));
            Children[2] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(-quarter, quarter, quarter));
            Children[3] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(quarter, quarter, quarter));
            Children[4] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(-quarter, -quarter, -quarter));
            Children[5] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(quarter, -quarter, -quarter));
            Children[6] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(-quarter, -quarter, quarter));
            Children[7] = new TtBoundsOctreeNode<T>(newLength, minSize, looseness, Center + new Vector3(quarter, -quarter, quarter));
        }

        /// <summary>
        /// Merge all children into this node - the opposite of Split.
        /// Note: We only have to check one level down since a merge will never happen if the children already have children,
        /// since THAT won't happen unless there are already too many objects to merge.
        /// </summary>
        void Merge()
        {
            // Note: We know children != null or we wouldn't be merging
            for (int i = 0; i < 8; i++)
            {
                TtBoundsOctreeNode<T> curChild = Children[i];
                int numObjects = curChild.OctObjects.Count;
                for (int j = numObjects - 1; j >= 0; j--)
                {
                    OctreeObject curObj = curChild.OctObjects[j];
                    OctObjects.Add(curObj);
                }
            }
            // Remove the child nodes (and the objects in them - they've been added elsewhere now)
            Children = null;
        }

        /// <summary>
        /// Checks if outerBounds encapsulates innerBounds.
        /// </summary>
        /// <param name="outerBounds">Outer bounds.</param>
        /// <param name="innerBounds">Inner bounds.</param>
        /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
        static bool Encapsulates(in Aabb outerBounds, in Aabb innerBounds)
        {
            return (outerBounds.IsContain(innerBounds.Minimum)) && (outerBounds.IsContain(innerBounds.Maximum));
        }

        /// <summary>
        /// Checks if there are few enough objects in this node and its children that the children should all be merged into this.
        /// </summary>
        /// <returns>True there are less or the same abount of objects in this and its children than numObjectsAllowed.</returns>
        bool ShouldMerge()
        {
            int totalObjects = OctObjects.Count;
            if (Children != null)
            {
                foreach (TtBoundsOctreeNode<T> child in Children)
                {
                    if (child.Children != null)
                    {
                        // If any of the *children* have children, there are definitely too many to merge,
                        // or the child woudl have been merged already
                        return false;
                    }
                    totalObjects += child.OctObjects.Count;
                }
            }
            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }
    }
}

