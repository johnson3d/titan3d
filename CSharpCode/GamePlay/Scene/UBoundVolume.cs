using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public enum EBoundVolumeType
    {
        None,
        Box,
        Sphere,
    }

    public class TtBoundVolume : IO.ISerializer
    {
        public TtBoundVolume()
        {
            
        }
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            HostNode = tagObject as TtNode;
        }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public TtNode HostNode { get; set; } = null;
        public BoundingBox mLocalAABB;
        public BoundingBox LocalAABB
        {
            get => mLocalAABB;
            set
            {
                mLocalAABB = value;
                OnVolumeChanged();
            }
        }
        public DBoundingBox AABB;//包含HostNode Child的AABB
        public DBoundingBox AbsAABB;//经过AbsTransform变换的AABB
        protected virtual void OnVolumeChanged()
        {
            HostNode.UpdateAABB();
        }
        //public UBoundVolume ContainCheck(ref Vector3 pos)
        //{
        //    var aabb = AABB;
        //    if (aabb.Contains(ref pos) == ContainmentType.Disjoint)
        //        return null;
        //    var localPos = WorldToLocal(ref pos);
        //    if (mLocalAABB.Contains(localPos) != ContainmentType.Disjoint)
        //        return this;

        //    foreach (var i in HostNode.Children)
        //    {
        //        var cldbv = i.Placement?.BoundVolume;
        //        if (cldbv != null)
        //        {
        //            var result = cldbv.ContainCheck(ref pos);
        //            if (result != null)
        //                return result;
        //        }
        //    }
        //    return null;
        //}
    }
    public class UBoxBV : TtBoundVolume
    {     
        public Vector3 mExtent = new Vector3(1,1,1);        
        protected override void OnVolumeChanged()
        {
            HostNode.UpdateAABB();
        }
    }
    public class USphereBV : TtBoundVolume
    {
        Vector3 mCenter;
        [Rtti.Meta]
        public Vector3 Center
        {
            get => mCenter;
            set
            {
                mCenter = value;
                OnVolumeChanged();
            }
        }
        float mRadius = 1;
        [Rtti.Meta]
        public float Radius
        {
            get => mRadius;
            set
            {
                mRadius = value;
                OnVolumeChanged();
            }
        }
        protected override void OnVolumeChanged()
        {
            var extent = new Vector3(Radius);
            mLocalAABB = new BoundingBox(mCenter - extent, mCenter + extent);
            HostNode.UpdateAABB();
        }
    }
}
