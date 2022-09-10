using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Bezier", "BezierSpline", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(UBezierSplineNode.UBezierSplineNodeData), DefaultNamePrefix = "BzSpline")]
    public class UBezierSplineNode : USceneActorNode
    {
        public class UBezierSplineNodeData : UNodeData
        {
            [Rtti.Meta]
            public UBezier3DSpline Spline { get; set; }
            [Rtti.Meta]
            public float PointRadius { get; set; } = 0.1f;
            [Rtti.Meta]
            public uint PointSmooth { get; set; } = 8;
            public UBezierSplineNodeData()
            {
                //test code
                Spline = new UBezier3DSpline();
                
                Spline.AppendPoint(in Vector3.Zero, in Vector3.One);
                
                var end = new Vector3(1, 1, 0);
                Spline.AppendPoint(in end, end - Vector3.One);

                end = new Vector3(10, 5, 3);
                Spline.AppendPoint(in end, end + Vector3.One);
            }
        }

        public UBezier3DSpline Spline
        {
            get
            {
                return GetNodeData<UBezierSplineNodeData>()?.Spline;
            }
        }
        Graphics.Mesh.UMesh mDebugSplineMesh;
        public Graphics.Mesh.UMesh DebugSplineMesh
        {
            get
            {
                return mDebugSplineMesh;
            }
        }
        Graphics.Mesh.UMesh mDebugPointMesh;
        public Graphics.Mesh.UMesh DebugPointMesh
        {
            get
            {
                return mDebugPointMesh;
            }
        }
        public override bool OnTickLogic(UWorld world, URenderPolicy policy)
        {
            //if (UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.UtilityDebug) == false)
            //    return true;

            //if ((rp.CullFilters & GamePlay.UWorld.UVisParameter.EVisCullFilter.UtilityDebug) == 0)
            //    return true;

            var tmp = Spline;
            if (tmp == null)
                return true;
            if (tmp.IsDirty)
            {
                UpdateSplineMesh();
                tmp.IsDirty = false;
            }
            return base.OnTickLogic(world, policy);
        }
        public class USplinePoint : Graphics.Pipeline.IProxiable
        {
            ~USplinePoint()
            {
                UEngine.Instance?.GfxDevice.HitproxyManager.UnmapProxy(this);
            }
            public Graphics.Pipeline.UHitProxy HitProxy { get; set; }
            public Graphics.Pipeline.UHitProxy.EHitproxyType HitproxyType
            {
                get
                {
                    return Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                }
                set
                {
                    
                }
            }
            public void OnHitProxyChanged()
            {

            }
            public void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
            {

            }
            public int CurveIndex { get; set; } = -1;
            internal UBezier3DSpline Spline;
            public Vector3 Position
            {
                get
                {
                    return Spline.GetPointPos(CurveIndex);
                }
            }
            public Vector3 LeftCtrl
            {
                get
                {
                    return Spline.GetPointLeftCtrl(CurveIndex);
                }
            }
            public Vector3 RightCtrl
            {
                get
                {
                    return Spline.GetPointRightCtrl(CurveIndex);
                }
            }
        }
        public List<USplinePoint> SplinePoints = new List<USplinePoint>(); 
        public void UpdateSplineMesh()
        {
            {
                var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBezier3DSpline(Spline, 0xFFFFFFFF).ToMesh();
                if (cookedMesh == null)
                    return;
                var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                materials1[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine));
                var mesh2 = new Graphics.Mesh.UMesh();
                var ok1 = mesh2.Initialize(cookedMesh, materials1,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                if (ok1)
                {
                    mesh2.IsUnlit = true;
                    mDebugSplineMesh = mesh2;

                    mDebugSplineMesh.HostNode = this;

                    BoundVolume.LocalAABB = mDebugSplineMesh.MaterialMesh.Mesh.mCoreObject.mAABB;
                }
            }

            this.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;

            {
                var radius = 0.1f;
                var pointSmooth = 4u;
                var splineData = GetNodeData<UBezierSplineNodeData>();
                if (splineData != null)
                {
                    radius = splineData.PointRadius;
                    pointSmooth = splineData.PointSmooth;
                }
                
                var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeSphere(radius, pointSmooth, pointSmooth, 0xffffffff).ToMesh();
                if (cookedMesh == null)
                    return;
                var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                materials1[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));
                var mesh2 = new Graphics.Mesh.UMesh();
                var ok1 = mesh2.Initialize(cookedMesh, materials1,
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfInstanceStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                if (ok1)
                {
                    mesh2.IsUnlit = true;
                    mDebugPointMesh = mesh2;

                    mDebugPointMesh.HostNode = this;

                    //BoundVolume.LocalAABB = mDebugPointMesh.MaterialMesh.Mesh.mCoreObject.mAABB;

                    var instantMesh = mDebugPointMesh.MdfQueue as Graphics.Mesh.UMdfInstanceStaticMesh;

                    instantMesh.InstanceModifier.SureBuffers((uint)Spline.Curves.Count);

                    foreach(var i in SplinePoints)
                    {
                        UEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(i);
                    }
                    SplinePoints.Clear();

                    for (int i = 0; i < Spline.Curves.Count; i++)
                    {
                        //if (i.Start == i.End)
                        //    continue;

                        var sPoint = new USplinePoint();
                        sPoint.CurveIndex = i + 1;
                        sPoint.Spline = Spline;
                        UEngine.Instance.GfxDevice.HitproxyManager.MapProxy(sPoint);
                        SplinePoints.Add(sPoint);
                        //instantMesh.InstanceModifier.PushInstance(in i.End, in Vector3.UnitXYZ, in Quaternion.Identity, in UInt32_4.Zero, this.HitProxy.ProxyId);
                        instantMesh.InstanceModifier.PushInstance(in Spline.Curves[i].mEnd, in Vector3.One, in Quaternion.Identity, in UInt32_4.Zero, sPoint.HitProxy.ProxyId);

                        //var cache = i.GetPointCache(Spline.Segments);
                        //for (int j = 0; j < cache.CachedPoints.Length; j++)
                        //{
                        //    ref var start = ref cache.CachedPoints[j].Position;
                        //    instantMesh.InstanceModifier.PushInstance(in start, in Vector3.UnitXYZ, in Quaternion.Identity, in UInt32_4.Zero);
                        //}
                    }

                    OnHitProxyChanged();
                }
            }

            
            UpdateAbsTransform();
            UpdateAABB();
            Parent?.UpdateAABB();
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);
            UpdateAbsTransform();
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
        {
            if (mDebugSplineMesh == null)
                return;
            meshes.Add(mDebugSplineMesh);
            if (mDebugPointMesh != null)
                meshes.Add(mDebugPointMesh);
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent)
                    i.GetHitProxyDrawMesh(meshes);
            }
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            if (mDebugSplineMesh == null)
                return;

            //if (UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.UtilityDebug) == false)
            //    return;

            if ((rp.CullFilters & GamePlay.UWorld.UVisParameter.EVisCullFilter.UtilityDebug) == 0)
                return;

            if (DebugSplineMesh != null)
                rp.VisibleMeshes.Add(mDebugSplineMesh);

            if (DebugPointMesh != null)
                rp.VisibleMeshes.Add(mDebugPointMesh);
        }
        protected override void OnAbsTransformChanged()
        {
            if (mDebugSplineMesh == null)
                return;

            var world = this.GetWorld();            
            mDebugSplineMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
            if (mDebugPointMesh != null)
            {
                mDebugPointMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
            }
        }
        public override void OnHitProxyChanged()
        {
            if (mDebugSplineMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mDebugSplineMesh.IsDrawHitproxy = false;
                if (mDebugPointMesh != null)
                    mDebugPointMesh.IsDrawHitproxy = false;
                return;
            }

            if (HitproxyType != Graphics.Pipeline.UHitProxy.EHitproxyType.None)
            {
                mDebugSplineMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mDebugSplineMesh.SetHitproxy(in value);

                if (mDebugPointMesh != null)
                {
                    mDebugPointMesh.IsDrawHitproxy = true;
                    mDebugPointMesh.SetHitproxy(in value);
                }
            }
            else
            {
                mDebugSplineMesh.IsDrawHitproxy = false;
                if (mDebugPointMesh != null)
                    mDebugPointMesh.IsDrawHitproxy = false;
            }
        }
    }
}
