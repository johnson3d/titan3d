using MathNet.Numerics;
using System.Collections.Generic;
using System.Linq;


namespace EngineNS.Bricks.Collision.Octree
{
    // A node in a PointOctree
    // Copyright 2014 Nition, BSD licence (see LICENCE file). www.momentstudio.co.nz
    public class TtPointOctreeNode<T>
    {
        // Centre of this node
        public DVector3 Center { get; private set; }

        // Length of the sides of this node
        public float SideLength { get; private set; }

        // Minimum size for a node in this octree
        float minSize;

        // Bounding box that represents this node
        Aabb AABB = default(Aabb);

        // Objects in this node
        readonly List<OctreeObject> OctObjects = new List<OctreeObject>();

        // Child nodes, if any
        TtPointOctreeNode<T>[] Children = null;

        bool HasChildren { get { return Children != null; } }

        // bounds of potential children to this node. These are actual size (with looseness taken into account), not base size
        Aabb[] childBounds;

        // If there are already NUM_OBJECTS_ALLOWED in a node, we split it into children
        // A generally good number seems to be something around 8-15
        const int NUM_OBJECTS_ALLOWED = 8;

        // For reverting the bounds size after temporary changes
        Vector3 actualBoundsSize;

        // An object in the octree
        class OctreeObject
        {
            public T Obj;
            public DVector3 Pos;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
        /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
        /// <param name="centerVal">Centre position of this node.</param>
        public TtPointOctreeNode(float baseLengthVal, float minSizeVal, DVector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, in centerVal);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objPos">Position of the object.</param>
        /// <returns></returns>
        public bool Add(T obj, in DVector3 objPos)
        {
            if (!Encapsulates(in AABB, in objPos))
            {
                return false;
            }
            SubAdd(obj, in objPos);
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
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, in DVector3 objPos)
        {
            if (!Encapsulates(AABB, objPos))
            {
                return false;
            }
            return SubRemove(obj, objPos);
        }

        /// <summary>
        /// Return objects that are within maxDistance of the specified ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <param name="result">List result.</param>
        /// <returns>Objects within range.</returns>
        public void GetNearby(in DRay ray, double maxDistance, List<T> result)
        {
            // Does the ray hit this node at all?
            // Note: Expanding the bounds is not exactly the same as a real distance check, but it's fast.
            // TODO: Does someone have a fast AND accurate formula to do this check?
            AABB.Expand(new Vector3((float)maxDistance * 2, (float)maxDistance * 2, (float)maxDistance * 2));
            bool intersected = DRay.Intersects(in ray, AABB, out var distance);
            AABB.Extent = actualBoundsSize;
            if (!intersected)
            {
                return;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                if (SqrDistanceToRay(ray, OctObjects[i].Pos) <= (maxDistance * maxDistance))
                {
                    result.Add(OctObjects[i].Obj);
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].GetNearby(in ray, maxDistance, result);
                }
            }
        }

        /// <summary>
        /// Return objects that are within <paramref name="maxDistance"/> of the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <param name="result">List result.</param>
        /// <returns>Objects within range.</returns>
        public void GetNearby(in DVector3 position, double maxDistance, List<T> result)
        {
            double sqrMaxDistance = maxDistance * maxDistance;

		    // Does the node intersect with the sphere of center = position and radius = maxDistance?
		    if ((AABB.ClosestPoint(position) - position).LengthSquared() > sqrMaxDistance) 
            {
                return;
            }

            // Check against any objects in this node
            for (int i = 0; i < OctObjects.Count; i++)
            {
                if ((position - OctObjects[i].Pos).LengthSquared() <= sqrMaxDistance)
                {
                    result.Add(OctObjects[i].Obj);
                }
            }

            // Check children
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].GetNearby(in position, maxDistance, result);
                }
            }
        }

        /// <summary>
        /// Return all objects in the tree.
        /// </summary>
        /// <returns>All objects.</returns>
        public void GetAll(List<T> result)
        {
            // add directly contained objects
            result.AddRange(OctObjects.Select(o => o.Obj));

            // add children objects
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].GetAll(result);
                }
            }
        }

        /// <summary>
        /// Set the 8 children of this octree.
        /// </summary>
        /// <param name="childOctrees">The 8 new child nodes.</param>
        public void SetChildren(TtPointOctreeNode<T>[] childOctrees)
        {
            if (childOctrees.Length != 8)
            {
                Profiler.Log.WriteLineSingle("Child octree array must be length 8. Was length: " + childOctrees.Length);
                return;
            }

            Children = childOctrees;
        }

        /// <summary>
        /// Draws node boundaries visually for debugging.
        /// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
        /// </summary>
        /// <param name="depth">Used for recurcive calls to this method.</param>
        public void DrawAllBounds(GamePlay.TtWorld.UVisParameter vp, GamePlay.TtPlacement placement, float depth)
        {
            float tintVal = depth / 7; // Will eventually get values > 1. Color rounds to 1 automatically
            var color = new Color4f(tintVal, 0, 1.0f - tintVal);

            var thisBounds = new Aabb(Center, new Vector3(SideLength, SideLength, SideLength));
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
        /// NOTE: marker.tif must be placed in your Unity /Assets/Gizmos subfolder for this to work.
        /// </summary>
        public void DrawAllObjects()
        {
            //float tintVal = SideLength / 20;
            //Gizmos.color = new Color(0, 1.0f - tintVal, tintVal, 0.25f);

            //foreach (OctreeObject obj in objects)
            //{
            //    Gizmos.DrawIcon(obj.Pos, "marker.tif", true);
            //}

            //if (children != null)
            //{
            //    for (int i = 0; i < 8; i++)
            //    {
            //        children[i].DrawAllObjects();
            //    }
            //}

            //Gizmos.color = Color.white;
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
        public TtPointOctreeNode<T> ShrinkIfPossible(float minLength)
        {
            if (SideLength < (2 * minLength))
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
                int newBestFit = BestFitChild(curObj.Pos);
                if (i == 0 || newBestFit == bestFit)
                {
                    if (bestFit < 0)
                    {
                        bestFit = newBestFit;
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
                SetValues(SideLength / 2, minSize, in childBounds[bestFit].Center);
                return this;
            }

            // We have children. Use the appropriate child as the new root node
            return Children[bestFit];
        }

        /// <summary>
        /// Find which child node this object would be most likely to fit in.
        /// </summary>
        /// <param name="objPos">The object's position.</param>
        /// <returns>One of the eight child octants.</returns>
        public int BestFitChild(in DVector3 objPos)
        {
            return (objPos.X <= Center.X ? 0 : 1) + (objPos.Y >= Center.Y ? 0 : 4) + (objPos.Z <= Center.Z ? 0 : 2);
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
        /// <param name="centerVal">Centre position of this node.</param>
        void SetValues(float baseLengthVal, float minSizeVal, in DVector3 centerVal)
        {
            SideLength = baseLengthVal;
            minSize = minSizeVal;
            Center = centerVal;

            // Create the bounding box.
            actualBoundsSize = new Vector3(SideLength, SideLength, SideLength);
            AABB = new Aabb(Center, actualBoundsSize);

            float quarter = SideLength / 4f;
            float childActualLength = SideLength / 2;
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
        /// <param name="objPos">Position of the object.</param>
        void SubAdd(T obj, in DVector3 objPos)
        {
            // We know it fits at this level if we've got this far

            // We always put things in the deepest possible child
            // So we can skip checks and simply move down if there are children aleady
            if (!HasChildren)
            {
                // Just add if few objects are here, or children would be below min size
                if (OctObjects.Count < NUM_OBJECTS_ALLOWED || (SideLength / 2) < minSize)
                {
                    OctreeObject newObj = new OctreeObject { Obj = obj, Pos = objPos };
                    OctObjects.Add(newObj);
                    return; // We're done. No children yet
                }

                // Enough objects in this node already: Create the 8 children
                int bestFitChild;
                if (Children == null)
                {
                    Split();
                    if (Children == null)
                    {
                        Profiler.Log.WriteLineSingle("Child creation failed for an unknown reason. Early exit.");
                        return;
                    }

                    // Now that we have the new children, move this node's existing objects into them
                    for (int i = OctObjects.Count - 1; i >= 0; i--)
                    {
                        OctreeObject existingObj = OctObjects[i];
                        // Find which child the object is closest to based on where the
                        // object's center is located in relation to the octree's center
                        bestFitChild = BestFitChild(in existingObj.Pos);
                        Children[bestFitChild].SubAdd(existingObj.Obj, in existingObj.Pos); // Go a level deeper					
                        OctObjects.Remove(existingObj); // Remove from here
                    }
                }
            }

            // Handle the new object we're adding now
            int bestFit = BestFitChild(objPos);
            Children[bestFit].SubAdd(obj, objPos);
        }

        /// <summary>
        /// Private counterpart to the public <see cref="Remove(T, Vector3)"/> method.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        bool SubRemove(T obj, in DVector3 objPos)
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
                int bestFitChild = BestFitChild(in objPos);
                removed = Children[bestFitChild].SubRemove(obj, in objPos);
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
            float quarter = SideLength / 4f;
            float newLength = SideLength / 2;
            Children = new TtPointOctreeNode<T>[8];
            Children[0] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, quarter, -quarter));
            Children[1] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, quarter, -quarter));
            Children[2] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, quarter, quarter));
            Children[3] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, quarter, quarter));
            Children[4] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, -quarter, -quarter));
            Children[5] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, -quarter, -quarter));
            Children[6] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(-quarter, -quarter, quarter));
            Children[7] = new TtPointOctreeNode<T>(newLength, minSize, Center + new Vector3(quarter, -quarter, quarter));
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
                TtPointOctreeNode<T> curChild = Children[i];
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
        /// Checks if outerBounds encapsulates the given point.
        /// </summary>
        /// <param name="outerBounds">Outer bounds.</param>
        /// <param name="point">Point.</param>
        /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
        static bool Encapsulates(in Aabb outerBounds, in DVector3 point)
        {
            return outerBounds.IsContain(point);
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
                foreach (TtPointOctreeNode<T> child in Children)
                {
                    if (child.Children != null)
                    {
                        // If any of the *children* have children, there are definitely too many to merge,
                        // or the child would have been merged already
                        return false;
                    }
                    totalObjects += child.OctObjects.Count;
                }
            }
            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }

        /// <summary>
        /// Returns the closest distance to the given ray from a point.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="point">The point to check distance from the ray.</param>
        /// <returns>Squared distance from the point to the closest point of the ray.</returns>
        public static double SqrDistanceToRay(DRay ray, in DVector3 point)
        {
            return DVector3.Cross(ray.Direction.AsDVector(), point - ray.Position).LengthSquared();
        }
    }

}

