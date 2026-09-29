using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Bezier", "Graphics\\BezierSpline", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtBezierSplineNode.TtBezierSplineNodeData), DefaultNamePrefix = "BzSpline")]
    public class TtBezierSplineNode : TtVisual
    {
        public override string NodeName
        {
            get => base.NodeName;
            set
            {
                if (string.Equals(base.NodeName, value, StringComparison.Ordinal))
                    return;
                base.NodeName = value;
                TtPgcVolumeNode.NotifySplineQuerySourceChanged(this);
            }
        }

        public enum ESplineElementKind
        {
            Anchor,
            Tangent,
        }

        public class TtBezierSplineNodeData : TtNodeData
        {
            [Rtti.Meta("")]
            public TtBezier3DSpline Spline { get; set; } = new TtBezier3DSpline();
            [Rtti.Meta("")]
            public float PointRadius { get; set; } = 0.1f;
            [Rtti.Meta("")]
            public uint PointSmooth { get; set; } = 8;

            public TtBezierSplineNodeData()
            {
                HitproxyType = TtHitProxy.EHitproxyType.Root;
            }

            public void EnsureDefaultSpline()
            {
                Spline ??= new TtBezier3DSpline();
                if (Spline.PointCount >= 2)
                    return;

                var start = new Vector3(-0.5f, 0.0f, 0.0f);
                var end = new Vector3(0.5f, 0.0f, 0.0f);
                var third = Vector3.UnitX / 3.0f;
                Spline.RestoreSnapshot(new[]
                {
                    new TtBezier3DPoint(start, start - third, start + third),
                    new TtBezier3DPoint(end, end - third, end + third),
                });
            }
        }

        public sealed class TtSplinePoint : IProxiable
        {
            bool mSelected;

            public TtBezierSplineNode OwnerNode { get; internal set; }
            public int PointIndex { get; internal set; } = -1;
            public ESplineElementKind ElementKind { get; internal set; }
            public bool IsLeftTangent { get; internal set; }
            public TtHitProxy HitProxy { get; set; }
            public TtHitProxy.EHitproxyType HitproxyType { get; set; } = TtHitProxy.EHitproxyType.Root;
            internal Graphics.Mesh.TtRenderMesh DebugMesh;
            public bool Selected
            {
                get => mSelected;
                set
                {
                    if (mSelected == value)
                        return;
                    mSelected = value;
                    OwnerNode?.RequestVisualRefresh();
                }
            }

            public Vector3 Position
            {
                get
                {
                    var spline = OwnerNode?.Spline;
                    if (spline == null || PointIndex < 0 || PointIndex >= spline.PointCount)
                        return Vector3.Zero;
                    if (ElementKind == ESplineElementKind.Tangent)
                    {
                        return IsLeftTangent ? spline.GetPointLeftCtrl(PointIndex) :
                            spline.GetPointRightCtrl(PointIndex);
                    }
                    return spline.GetPointPos(PointIndex);
                }
            }

            public void OnHitProxyChanged()
            {
                OwnerNode?.RequestVisualRefresh();
            }

            public void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
            {
                if (DebugMesh != null)
                    meshes.Add(DebugMesh);
            }
        }

        [Browsable(false)]
        public TtBezierSplineNodeData SplineData => GetNodeData<TtBezierSplineNodeData>();

        [Browsable(false)]
        public TtBezier3DSpline Spline => SplineData?.Spline;

        [Category("Spline")]
        public bool IsClosed
        {
            get => Spline?.IsClosed ?? false;
            set
            {
                var spline = Spline;
                if (spline == null || spline.IsClosed == value)
                    return;
                spline.IsClosed = value;
                RequestTopologyRefresh();
            }
        }

        [Category("Spline")]
        public float PointRadius
        {
            get => SplineData.PointRadius;
            set
            {
                var clamped = Math.Max(0.001f, value);
                if (SplineData.PointRadius == clamped)
                    return;
                SplineData.PointRadius = clamped;
                // 半径改变会改控制球几何, 必须整体重建
                RequestTopologyRefresh();
            }
        }

        [Category("Spline")]
        public uint PointSmooth
        {
            get => SplineData.PointSmooth;
            set
            {
                var clamped = Math.Max(3u, value);
                if (SplineData.PointSmooth == clamped)
                    return;
                SplineData.PointSmooth = clamped;
                // 细分改变会改控制球几何, 必须整体重建
                RequestTopologyRefresh();
            }
        }

        Graphics.Mesh.TtRenderMesh mDebugSplineMesh;
        Graphics.Mesh.TtRenderMesh mDebugSplineHitProxyMesh;
        Graphics.Mesh.TtRenderMesh mDebugTangentLineMesh;
        readonly object mSplineMeshLocker = new object();
        // 已发布的 Mesh 可能仍被跨帧异步 BuildDrawCall 持有，替换后延迟到节点销毁再释放。
        readonly List<Graphics.Mesh.TtRenderMesh> mRetiredDebugMeshes = new List<Graphics.Mesh.TtRenderMesh>();
        int mControlProxyVersion;
        int mDisposed;
        // 分两级脏标记:
        //   mTopologyDirty: 控制点增删 / 半径 / 细分 / 选中态切换 (切线球显隐) 等
        //                   会改变控制球集合或几何的操作, 必须整体重建 (UpdateSplineMesh)。
        //   mVisualDirty:   仅控制点位置变化 (拖动) —— 只需重建曲线/切线线段并刷新
        //                   所有控制球的世界变换, 绝不重新 cook 控制球 (RefreshSplineShape)。
        bool mTopologyDirty = true;
        bool mVisualDirty = true;
        bool mLastSelected;

        [Browsable(false)]
        public Graphics.Mesh.TtRenderMesh DebugSplineMesh => mDebugSplineMesh;
        [Browsable(false)]
        public Graphics.Mesh.TtRenderMesh DebugPointMesh
        {
            get
            {
                for (int i = 0; i < mSplinePoints.Count; i++)
                {
                    if (mSplinePoints[i].ElementKind == ESplineElementKind.Anchor)
                        return mSplinePoints[i].DebugMesh;
                }
                return null;
            }
        }
        [Browsable(false)]
        public IReadOnlyList<TtSplinePoint> SplinePoints => mSplinePoints;

        readonly List<TtSplinePoint> mSplinePoints = new List<TtSplinePoint>();

        public override void Dispose()
        {
            System.Threading.Volatile.Write(ref mDisposed, 1);
            ClearControlProxies();
            DisposeDebugMeshes();
            base.Dispose();
        }

        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtBezierSplineNode>.Scope;
        }

        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            var spline = Spline;
            if (spline == null)
                return base.OnTickLogic(args);

            if (mLastSelected != Selected)
            {
                mLastSelected = Selected;
                // 选中态切换会显隐切线球, 属于控制球集合变化, 需要整体重建
                mTopologyDirty = true;
            }
            if (spline.IsDirty)
                mVisualDirty = true;
            // 拓扑变化优先走整体重建; 否则仅形状变化走轻量刷新 (不重新 cook 控制球)
            if (mTopologyDirty)
                UpdateSplineMesh();
            else if (mVisualDirty)
                RefreshSplineShape();
            return base.OnTickLogic(args);
        }

        // 仅形状变化 (拖动控制点): 下一帧轻量刷新曲线/切线 + 控制球变换, 不重新 cook 控制球
        public void RequestVisualRefresh()
        {
            mVisualDirty = true;
        }

        // 拓扑变化 (增删控制点 / 半径 / 细分 / 选中态): 下一帧整体重建全部 debug mesh
        void RequestTopologyRefresh()
        {
            mTopologyDirty = true;
            mVisualDirty = true;
        }

        public TtSplinePoint FindControlProxy(int pointIndex, ESplineElementKind kind, bool isLeftTangent = false)
        {
            for (int i = 0; i < mSplinePoints.Count; i++)
            {
                var proxy = mSplinePoints[i];
                if (proxy.PointIndex == pointIndex && proxy.ElementKind == kind &&
                    (kind == ESplineElementKind.Anchor || proxy.IsLeftTangent == isLeftTangent))
                    return proxy;
            }
            return null;
        }

        public bool SetControlPosition(int pointIndex, ESplineElementKind kind, bool isLeftTangent,
            in Vector3 localPosition)
        {
            var spline = Spline;
            if (spline == null)
                return false;
            var changed = kind == ESplineElementKind.Tangent ?
                spline.SetPointTangent(pointIndex, isLeftTangent, in localPosition) :
                spline.SetPointPosition(pointIndex, in localPosition, true);
            if (changed)
                RequestVisualRefresh();
            return changed;
        }

        public void RestoreSpline(TtBezier3DSplineSnapshot snapshot)
        {
            Spline?.RestoreSnapshot(snapshot);
            // 点集或闭合状态可能整体替换 (Undo/Redo), 属拓扑变化。
            RequestTopologyRefresh();
            EnsureControlProxies();
        }

        public void RestoreSpline(IReadOnlyList<TtBezier3DPoint> snapshot)
        {
            Spline?.RestoreSnapshot(snapshot);
            RequestTopologyRefresh();
            EnsureControlProxies();
        }

        public int AppendLinearPoint(float distance = 1.0f)
        {
            var spline = Spline;
            if (spline == null || spline.IsClosed)
                return -1;
            var direction = Vector3.UnitX;
            var start = Vector3.Zero;
            if (spline.PointCount > 0)
            {
                start = spline.GetPointPos(spline.PointCount - 1);
                if (spline.PointCount > 1)
                {
                    direction = start - spline.GetPointPos(spline.PointCount - 2);
                    if (direction.Normalize() < 0.0001f)
                        direction = Vector3.UnitX;
                }
            }
            var end = start + direction * distance;
            var third = (end - start) / 3.0f;
            if (spline.PointCount > 0)
                spline.SetPointRightCtrl(spline.PointCount - 1, start + third);
            var left = end - third;
            var right = end + third;
            spline.AppendPoint(in end, in left, in right);
            RequestTopologyRefresh();
            // 编辑器会立即查找并选中新点的 proxy；这里只同步 proxy，不 cook mesh
            EnsureControlProxies();
            return spline.PointCount - 1;
        }

        public int InsertPointAfter(int pointIndex)
        {
            var spline = Spline;
            if (spline == null || pointIndex < 0 || pointIndex >= spline.PointCount)
                return -1;
            if (pointIndex == spline.PointCount - 1 && !spline.IsClosed)
                return AppendLinearPoint();
            if (!spline.SplitSegmentAfter(pointIndex, 0.5f))
                return -1;
            RequestTopologyRefresh();
            EnsureControlProxies();
            return pointIndex + 1;
        }

        public bool RemovePoint(int pointIndex)
        {
            var spline = Spline;
            var minimumCount = spline?.IsClosed == true ? 3 : 2;
            if (spline == null || spline.PointCount <= minimumCount || !spline.RemovePoint(pointIndex))
                return false;
            RequestTopologyRefresh();
            EnsureControlProxies();
            return true;
        }

        void EnsureControlProxies()
        {
            var expectedCount = (Spline?.PointCount ?? 0) * 3;
            bool valid = mSplinePoints.Count == expectedCount;
            if (valid)
            {
                for (int i = 0; i < mSplinePoints.Count; i++)
                {
                    var slot = i % 3;
                    var expectedKind = slot == 0 ? ESplineElementKind.Anchor : ESplineElementKind.Tangent;
                    var expectedLeft = slot == 1;
                    var proxy = mSplinePoints[i];
                    if (proxy.PointIndex != i / 3 || proxy.ElementKind != expectedKind ||
                        (expectedKind == ESplineElementKind.Tangent && proxy.IsLeftTangent != expectedLeft))
                    {
                        valid = false;
                        break;
                    }
                }
            }
            if (valid)
                return;

            ClearControlProxies();
            if (Spline == null)
                return;
            for (int pointIndex = 0; pointIndex < Spline.PointCount; pointIndex++)
            {
                for (int slot = 0; slot < 3; slot++)
                {
                    var proxy = new TtSplinePoint
                    {
                        OwnerNode = this,
                        PointIndex = pointIndex,
                        ElementKind = slot == 0 ? ESplineElementKind.Anchor : ESplineElementKind.Tangent,
                        IsLeftTangent = slot == 1,
                    };
                    TtEngine.Instance.GfxDevice.HitproxyManager.MapProxy(proxy);
                    mSplinePoints.Add(proxy);
                }
            }
        }

        void ClearControlProxies()
        {
            var manager = TtEngine.Instance?.GfxDevice?.HitproxyManager;
            lock (mSplineMeshLocker)
            {
                mControlProxyVersion++;
                for (int i = 0; i < mSplinePoints.Count; i++)
                {
                    var proxy = mSplinePoints[i];
                    if (manager != null)
                        manager.UnmapProxy(proxy);
                    RetireDebugMesh(proxy.DebugMesh);
                    proxy.DebugMesh = null;
                    proxy.OwnerNode = null;
                }
                mSplinePoints.Clear();
            }
        }

        void RetireDebugMesh(Graphics.Mesh.TtRenderMesh mesh)
        {
            if (mesh != null)
                mRetiredDebugMeshes.Add(mesh);
        }

        void DisposeDebugMeshesOnRender()
        {
            CoreSDK.DisposeObject(ref mDebugSplineMesh);
            CoreSDK.DisposeObject(ref mDebugSplineHitProxyMesh);
            CoreSDK.DisposeObject(ref mDebugTangentLineMesh);
            for (int i = 0; i < mSplinePoints.Count; i++)
                CoreSDK.DisposeObject(ref mSplinePoints[i].DebugMesh);
            for (int i = 0; i < mRetiredDebugMeshes.Count; i++)
            {
                var mesh = mRetiredDebugMeshes[i];
                CoreSDK.DisposeObject(ref mesh);
            }
            mRetiredDebugMeshes.Clear();
        }

        void DisposeDebugMeshes()
        {
            var engine = TtEngine.Instance;
            if (engine?.Config?.UseRenderThread != true || engine.ThreadRender?.IsFinished == true ||
                engine.EventPoster.IsThread(Thread.Async.EAsyncTarget.Render))
            {
                lock (mSplineMeshLocker)
                    DisposeDebugMeshesOnRender();
                return;
            }

            using (var completedEvent = new System.Threading.AutoResetEvent(false))
            {
                engine.ThreadRender.QueueRenderAction("BezierSpline.DisposeDebugMeshes",
                    static (in Thread.TtThreadRender.FRenderAction action) =>
                    {
                        var node = action.Arg as TtBezierSplineNode;
                        lock (node.mSplineMeshLocker)
                            node.DisposeDebugMeshesOnRender();
                    }, this);
                engine.ThreadRender.WaitFinishRenderAction(completedEvent);
            }
        }

        bool PrepareControlMeshes(TtSplineMeshRenderState state)
        {
            for (int i = 0; i < state.Controls.Count; i++)
            {
                var control = state.Controls[i];
                if (control.Proxy?.OwnerNode != this || control.Provider == null)
                    continue;
                control.DebugMesh = CreateStaticMesh(control.Provider, true);
                if (control.DebugMesh == null)
                    return false;
                ApplyControlMeshState(control.DebugMesh, control, state.World);
            }
            return true;
        }

        static void DisposePreparedMeshes(ref Graphics.Mesh.TtRenderMesh splineMesh,
            ref Graphics.Mesh.TtRenderMesh splineHitProxyMesh,
            ref Graphics.Mesh.TtRenderMesh tangentMesh, TtSplineMeshRenderState state)
        {
            CoreSDK.DisposeObject(ref splineMesh);
            CoreSDK.DisposeObject(ref splineHitProxyMesh);
            CoreSDK.DisposeObject(ref tangentMesh);
            for (int i = 0; i < state.Controls.Count; i++)
                CoreSDK.DisposeObject(ref state.Controls[i].DebugMesh);
        }

        Graphics.Mesh.TtRenderMesh CreateStaticMesh(Graphics.Mesh.TtMeshDataProvider provider, bool solid = false)
        {
            var cookedMesh = provider?.ToMesh();
            if (cookedMesh == null)
                return null;
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = solid ?
                TtEngine.Instance.GfxDevice.MaterialInstanceManager.VtxColorMaterial :
                TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMaterial;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            if (!mesh.Initialize(cookedMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc))
            {
                mesh.Dispose();
                return null;
            }
            mesh.IsAcceptShadow = false;
            mesh.HostNode = this;
            return mesh;
        }

        Graphics.Mesh.TtMeshDataProvider BuildSplineHitProxyProvider(TtBezier3DSpline spline)
        {
            if (spline == null)
                return null;

            // 可见曲线仍然使用细线；HitProxy 单独用低多边形管扩大可点击范围。
            // 拾取管半径随控制点大小变化，并保留 2.5cm 下限，避免默认线宽在降采样缓冲中消失。
            var radius = Math.Max(PointRadius * 0.5f, 0.025f);
            var segmentCount = Math.Clamp(spline.Segments / 4, 12, 32);
            Graphics.Mesh.TtMeshDataProvider result = null;
            foreach (var curve in spline.Curves)
            {
                var from = curve.GetValue(0.0f);
                for (int i = 1; i <= segmentCount; i++)
                {
                    var to = curve.GetValue((float)i / segmentCount);
                    var delta = to - from;
                    var length = delta.Length();
                    if (length > 0.0001f)
                    {
                        var direction = delta / length;
                        var rotation = ComputeFromToRotation(Vector3.UnitY, direction);
                        var midpoint = (from + to) * 0.5f;
                        var transform = Matrix.Transformation(Vector3.One, rotation, midpoint);
                        var segment = Graphics.Mesh.TtMeshDataProvider.MakeCylinder(
                            radius, radius, length, 6, 1, 0xFFFFFFFF);
                        segment.ApplyTransform(in transform);
                        if (result == null)
                        {
                            result = segment;
                        }
                        else
                        {
                            if (!result.MergeFromMesh(segment, true))
                            {
                                segment.Dispose();
                                result.Dispose();
                                return null;
                            }
                            segment.Dispose();
                        }
                    }
                    from = to;
                }
            }
            return result;
        }

        static Quaternion ComputeFromToRotation(in Vector3 from, in Vector3 to)
        {
            var dot = Vector3.Dot(from, to);
            if (dot >= 1.0f - 1e-6f)
                return Quaternion.Identity;
            if (dot <= -1.0f + 1e-6f)
            {
                var axis = Vector3.Cross(Vector3.UnitX, from);
                if (axis.LengthSquared() < 1e-6f)
                    axis = Vector3.Cross(Vector3.UnitZ, from);
                axis.Normalize();
                return Quaternion.RotationAxis(axis, MathF.PI);
            }

            var cross = Vector3.Cross(from, to);
            var result = new Quaternion(cross.X, cross.Y, cross.Z, 1.0f + dot);
            result.Normalize();
            return result;
        }

        static List<Vector3> BuildTangentLines(TtBezier3DSpline spline)
        {
            var lines = new List<Vector3>();
            if (spline == null)
                return lines;

            for (int i = 0; i < spline.PointCount; i++)
            {
                var anchor = spline.GetPointPos(i);
                if (spline.IsClosed || i > 0)
                {
                    lines.Add(anchor);
                    lines.Add(spline.GetPointLeftCtrl(i));
                }
                if (spline.IsClosed || i < spline.PointCount - 1)
                {
                    lines.Add(anchor);
                    lines.Add(spline.GetPointRightCtrl(i));
                }
            }
            return lines;
        }

        sealed class TtSplineControlRenderState
        {
            public TtSplinePoint Proxy;
            public Graphics.Mesh.TtMeshDataProvider Provider;
            public Graphics.Mesh.TtRenderMesh DebugMesh;
            public FTransform WorldTransform;
            public bool DrawHitproxy;
            public Vector4 Hitproxy;
        }

        sealed class TtSplineMeshRenderState
        {
            public TtBezierSplineNode Owner;
            public bool RebuildTopology;
            public int ControlProxyVersion;
            public Graphics.Mesh.TtMeshDataProvider SplineProvider;
            public Graphics.Mesh.TtMeshDataProvider SplineHitProxyProvider;
            public Graphics.Mesh.TtMeshDataProvider TangentProvider;
            public readonly List<TtSplineControlRenderState> Controls = new List<TtSplineControlRenderState>();
            public TtWorld World;
            public FTransform NodeWorldTransform;
            public bool DrawHitproxy;
            public Vector4 Hitproxy;
        }

        TtSplineMeshRenderState BuildSplineMeshRenderState(bool rebuildTopology)
        {
            var spline = Spline;
            if (spline == null)
                return null;

            EnsureControlProxies();
            var renderSpline = new TtBezier3DSpline();
            renderSpline.RestoreSnapshot(spline.CreateSnapshot());
            var state = new TtSplineMeshRenderState
            {
                Owner = this,
                RebuildTopology = rebuildTopology,
                SplineProvider = Graphics.Mesh.TtMeshDataProvider.MakeBezier3DSpline(renderSpline, 0xFFFFFFFF),
                World = GetWorld(),
                NodeWorldTransform = Placement.AbsTransform,
                DrawHitproxy = HitProxy != null && HitproxyType != TtHitProxy.EHitproxyType.None,
            };
            if (state.DrawHitproxy)
            {
                state.Hitproxy = HitProxy.ConvertHitProxyIdToVector4();
                state.SplineHitProxyProvider = BuildSplineHitProxyProvider(renderSpline);
            }

            var selected = Selected;
            var lines = BuildTangentLines(renderSpline);
            if (selected && lines.Count > 0)
                state.TangentProvider = Graphics.Mesh.TtMeshDataProvider.MakeLines(in lines, 0xFF808080);

            TtSplinePoint[] proxies;
            lock (mSplineMeshLocker)
            {
                state.ControlProxyVersion = mControlProxyVersion;
                proxies = mSplinePoints.ToArray();
            }
            var smooth = Math.Max(3u, PointSmooth);
            for (int i = 0; i < proxies.Length; i++)
            {
                var proxy = proxies[i];
                var visible = proxy.ElementKind == ESplineElementKind.Anchor ||
                    (selected && (renderSpline.IsClosed || (proxy.IsLeftTangent ?
                        proxy.PointIndex > 0 : proxy.PointIndex < renderSpline.PointCount - 1)));
                var control = new TtSplineControlRenderState
                {
                    Proxy = proxy,
                    DrawHitproxy = proxy.HitProxy != null && proxy.HitproxyType != TtHitProxy.EHitproxyType.None,
                };
                if (control.DrawHitproxy)
                    control.Hitproxy = proxy.HitProxy.ConvertHitProxyIdToVector4();
                if (rebuildTopology && visible)
                {
                    var radius = proxy.ElementKind == ESplineElementKind.Anchor ? PointRadius : PointRadius * 0.65f;
                    var color = proxy.ElementKind == ESplineElementKind.Anchor ? 0xFFFFFF00u : 0xFF00FFFFu;
                    control.Provider = Graphics.Mesh.TtMeshDataProvider.MakeSphere(radius, smooth, smooth, color);
                }

                var position = proxy.ElementKind == ESplineElementKind.Anchor ?
                    renderSpline.GetPointPos(proxy.PointIndex) :
                    (proxy.IsLeftTangent ? renderSpline.GetPointLeftCtrl(proxy.PointIndex) :
                        renderSpline.GetPointRightCtrl(proxy.PointIndex));
                var scale = proxy.Selected ? new Vector3(1.45f) : Vector3.One;
                var localTransform = FTransform.CreateTransform(position.AsDVector(), scale, Quaternion.Identity);
                FTransform.Multiply(out control.WorldTransform, in localTransform, in state.NodeWorldTransform);
                state.Controls.Add(control);
            }

            UpdateLocalBounds();
            UpdateAABB();
            Parent?.UpdateAABB();
            spline.IsDirty = false;
            if (rebuildTopology)
                mTopologyDirty = false;
            mVisualDirty = false;
            return state;
        }

        static void ApplyControlMeshState(Graphics.Mesh.TtRenderMesh mesh,
            TtSplineControlRenderState control, TtWorld world)
        {
            if (mesh == null)
                return;
            mesh.SetWorldTransform(in control.WorldTransform, world, false);
            mesh.IsDrawHitproxy = control.DrawHitproxy;
            if (control.DrawHitproxy)
                mesh.SetHitproxy(in control.Hitproxy);
        }

        void ApplySplineMeshRenderState(TtSplineMeshRenderState state)
        {
            if (state == null || System.Threading.Volatile.Read(ref mDisposed) != 0)
                return;
            lock (mSplineMeshLocker)
            {
                if (System.Threading.Volatile.Read(ref mDisposed) != 0 ||
                    state.ControlProxyVersion != mControlProxyVersion)
                    return;

                // 绝不重构已发布 Mesh 的 Atoms/SubMesh。新 Mesh 完整初始化并配置好状态后，
                // 再一次性替换成员，避免跨帧 BuildDrawCall 继续访问被修改的 Atom。
                var newSplineMesh = CreateStaticMesh(state.SplineProvider);
                var newSplineHitProxyMesh = CreateStaticMesh(state.SplineHitProxyProvider, true);
                var newTangentMesh = CreateStaticMesh(state.TangentProvider);
                if (newSplineMesh == null ||
                    (state.SplineHitProxyProvider != null && newSplineHitProxyMesh == null) ||
                    (state.TangentProvider != null && newTangentMesh == null) ||
                    (state.RebuildTopology && !PrepareControlMeshes(state)))
                {
                    DisposePreparedMeshes(ref newSplineMesh, ref newSplineHitProxyMesh, ref newTangentMesh, state);
                    return;
                }

                SetMeshTransform(newSplineMesh, state.World, in state.NodeWorldTransform);
                SetMeshTransform(newSplineHitProxyMesh, state.World, in state.NodeWorldTransform);
                SetMeshTransform(newTangentMesh, state.World, in state.NodeWorldTransform);
                // 优先让独立拾取管写 HitProxy；若管状几何构建失败，则退回可见细线，
                // 避免 GetHitProxyDrawMesh 回退提交了细线却因 IsDrawHitproxy=false 而完全不可选。
                var useSplineHitProxyMesh = state.DrawHitproxy && newSplineHitProxyMesh != null;
                ApplyNodeHitProxyState(newSplineMesh,
                    state.DrawHitproxy && !useSplineHitProxyMesh, in state.Hitproxy);
                ApplyNodeHitProxyState(newSplineHitProxyMesh, useSplineHitProxyMesh, in state.Hitproxy);
                ApplyNodeHitProxyState(newTangentMesh, state.DrawHitproxy, in state.Hitproxy);

                RetireDebugMesh(mDebugSplineMesh);
                RetireDebugMesh(mDebugSplineHitProxyMesh);
                RetireDebugMesh(mDebugTangentLineMesh);
                mDebugSplineMesh = newSplineMesh;
                mDebugSplineHitProxyMesh = newSplineHitProxyMesh;
                mDebugTangentLineMesh = newTangentMesh;

                for (int i = 0; i < state.Controls.Count; i++)
                {
                    var control = state.Controls[i];
                    var proxy = control.Proxy;
                    if (proxy?.OwnerNode != this)
                        continue;
                    if (state.RebuildTopology)
                    {
                        RetireDebugMesh(proxy.DebugMesh);
                        proxy.DebugMesh = control.DebugMesh;
                    }
                    else
                    {
                        ApplyControlMeshState(proxy.DebugMesh, control, state.World);
                    }
                }
            }
        }

        void QueueSplineMeshRenderState(bool rebuildTopology)
        {
            if (System.Threading.Volatile.Read(ref mDisposed) != 0)
                return;
            var state = BuildSplineMeshRenderState(rebuildTopology);
            if (state == null)
                return;
            var engine = TtEngine.Instance;
            if (engine == null || engine.ThreadRender?.IsFinished == true)
                return;
            if (engine.Config?.UseRenderThread != true || engine.EventPoster.IsThread(Thread.Async.EAsyncTarget.Render))
            {
                ApplySplineMeshRenderState(state);
                return;
            }
            engine.ThreadRender.QueueRenderAction("BezierSpline.ApplyMeshState",
                static (in Thread.TtThreadRender.FRenderAction action) =>
                {
                    var renderState = action.Arg as TtSplineMeshRenderState;
                    renderState?.Owner?.ApplySplineMeshRenderState(renderState);
                }, state);
        }

        // Logic 线程只构建样条快照和 provider；所有 RenderMesh 变更都由 Render 线程串行执行。
        void RefreshSplineShape()
        {
            QueueSplineMeshRenderState(false);
        }

        public void UpdateSplineMesh()
        {
            QueueSplineMeshRenderState(true);
        }

        void UpdateLocalBounds()
        {
            var spline = Spline;
            if (spline == null || spline.PointCount == 0)
                return;
            var bounds = new BoundingBox();
            bounds.InitEmptyBox();
            for (int i = 0; i < spline.PointCount; i++)
            {
                var position = spline.GetPointPos(i);
                var left = spline.GetPointLeftCtrl(i);
                var right = spline.GetPointRightCtrl(i);
                bounds.Merge(in position);
                bounds.Merge(in left);
                bounds.Merge(in right);
            }
            var extent = new Vector3(PointRadius);
            bounds.Minimum -= extent;
            bounds.Maximum += extent;
            BoundVolume.LocalAABB = bounds;
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data,
            EBoundVolumeType bvType, Type placementType)
        {
            if (data is TtBezierSplineNodeData splineData)
                splineData.EnsureDefaultSpline();
            return await base.InitializeNode(world, data, bvType, placementType);
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            HitproxyType = HitproxyType;
            UpdateAbsTransform();
            RequestVisualRefresh();
        }

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            AddMesh(meshes, mDebugSplineHitProxyMesh ?? mDebugSplineMesh);
            for (int i = 0; i < mSplinePoints.Count; i++)
                mSplinePoints[i].GetHitProxyDrawMesh(meshes);
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if ((rp.CullFilters & TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) == 0)
                return;
            AddVisibleMesh(rp, mDebugSplineMesh);
            for (int i = 0; i < mSplinePoints.Count; i++)
                AddVisibleMesh(rp, mSplinePoints[i].DebugMesh);
            if (Selected)
                AddVisibleMesh(rp, mDebugTangentLineMesh);
        }

        static void AddMesh(List<Graphics.Mesh.TtRenderMesh> meshes, Graphics.Mesh.TtRenderMesh mesh)
        {
            if (mesh != null)
                meshes.Add(mesh);
        }

        static void AddVisibleMesh(TtWorld.TtVisParameter rp, Graphics.Mesh.TtRenderMesh mesh)
        {
            if (mesh != null)
                rp.AddVisibleMesh(mesh);
        }

        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            var previousWorld = prev?.GetWorld();
            base.OnParentChanged(prev, cur);
            TtPgcVolumeNode.NotifySplineQuerySourceChanged(this, previousWorld);
        }

        protected override void OnAbsTransformChanged()
        {
            RequestVisualRefresh();
        }

        protected override void OnAbsAABBChanged()
        {
            base.OnAbsAABBChanged();
            TtPgcVolumeNode.NotifySplineQuerySourceChanged(this);
        }

        static void SetMeshTransform(Graphics.Mesh.TtRenderMesh mesh, TtWorld world,
            in FTransform worldTransform)
        {
            if (mesh != null)
                mesh.SetWorldTransform(in worldTransform, world, false);
        }

        public override void OnHitProxyChanged()
        {
            RequestVisualRefresh();
        }

        static void ApplyNodeHitProxyState(Graphics.Mesh.TtRenderMesh mesh, bool drawHitproxy,
            in Vector4 hitproxy)
        {
            if (mesh == null)
                return;
            mesh.IsDrawHitproxy = drawHitproxy;
            if (drawHitproxy)
                mesh.SetHitproxy(in hitproxy);
        }
    }
}
