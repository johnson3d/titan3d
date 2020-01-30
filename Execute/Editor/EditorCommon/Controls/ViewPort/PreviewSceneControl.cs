using EngineNS;
using EngineNS.Bricks.GraphDrawer;
using EngineNS.Bricks.MeshProcessor;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EditorCommon.ViewPort.ViewPortControl;

namespace EditorCommon.ViewPort
{
    public class EditableMeshElementVisual
    {
        protected EditableMesh mEditableMesh = null;
        protected GraphLines mGraphLines = new GraphLines();

        public EditableMesh EditableMesh { get => mEditableMesh; set => mEditableMesh = value; }
        public GraphLines GraphLines { get => mGraphLines; set => mGraphLines = value; }

        public EditableMeshElementVisual(EditableMesh editableMesh)
        {
            mEditableMesh = editableMesh;
        }
        public virtual async Task Init(RName lineMaterial)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
    }
    public class VertexsTBNVisual : EditableMeshElementVisual
    {
        protected float mLineLength = 1;
        protected McMulSegmentsGen mLineGen = new McMulSegmentsGen();
        EngineNS.Support.NativeList<Vector3> Vector3Points = new EngineNS.Support.NativeList<Vector3>();
        public VertexsTBNVisual(EditableMesh editableMesh) : base(editableMesh)
        {

        }
        public void UpdateLength(float length)
        {
            SetVector3Points(ref Vector3Points, length);
            unsafe
            {
                mLineGen.UnsafeSetVector3Points((Vector3*)Vector3Points.GetBufferPtr(), Vector3Points.Count);
            }
            mGraphLines.UpdateGeomMesh(CEngine.Instance.RenderContext, 0);
        }
        public async Task Init(RName lineMaterial, float length)
        {
            var start = new Vector3(0, 0, 0);
            mGraphLines = new GraphLines();
            mGraphLines.LinesGen = mLineGen;
            mGraphLines.LinesGen.Interval = 0;
            mGraphLines.LinesGen.Segement = 0.1f;
            mLineGen.SetVector3Points(new Vector3[] { start, start + Vector3.UnitX * 0.1f });
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
            CEngine.Instance.RenderContext,
            lineMaterial);
            await mGraphLines.Init(mtl, 1);
            mGraphLines.LinesGen.Start = mEditableMesh.Vertices[0].Position;
            SetVector3Points(ref Vector3Points, length);
            unsafe
            {
                mLineGen.UnsafeSetVector3Points((Vector3*)Vector3Points.GetBufferPtr(), Vector3Points.Count);
            }
            mGraphLines.UpdateGeomMesh(CEngine.Instance.RenderContext, 0);
            mGraphLines.GraphActor.Placement.Location = Vector3.Zero;
        }
        public virtual void SetVector3Points(ref EngineNS.Support.NativeList<Vector3> data, float length)
        {

        }
    }
    public class VertexTangentVisual : VertexsTBNVisual
    {
        public VertexTangentVisual(EditableMesh editableMesh) : base(editableMesh)
        {

        }
        public override void SetVector3Points(ref EngineNS.Support.NativeList<Vector3> data, float length)
        {
            for (int i = 0; i < mEditableMesh.Vertices.Count; ++i)
            {
                var pos = mEditableMesh.Vertices[i].Position;
                var t = mEditableMesh.Vertices[i].Tangent;
                var Tangent = pos + new Vector3(t.X, t.Y, t.Z) * length;
                data.Add(pos);
                data.Add(Tangent);
            }
        }
    }
    public class VertexNormalVisual : VertexsTBNVisual
    {
        public VertexNormalVisual(EditableMesh editableMesh) : base(editableMesh)
        {

        }
        public override void SetVector3Points(ref EngineNS.Support.NativeList<Vector3> data, float length)
        {
            for (int i = 0; i < mEditableMesh.Vertices.Count; ++i)
            {
                var pos = mEditableMesh.Vertices[i].Position;
                var end = pos + mEditableMesh.Vertices[i].Normal * length;
                data.Add(pos);
                data.Add(end);
            }
        }
    }
    public class EditableMeshVisual
    {
        Dictionary<string, EditableMeshElementVisual> mElementVisuals = new Dictionary<string, EditableMeshElementVisual>();
        public EditableMeshVisual(EditableMesh editableMesh)
        {
            mEditableMesh = editableMesh;
        }
        EditableMesh mEditableMesh = new EditableMesh();
        public async Task<GActor> GetNormalVisual(RName lineMaterial, float length)
        {
            if (mElementVisuals.ContainsKey("Normal"))
                return mElementVisuals["Normal"].GraphLines.GraphActor;
            else
            {
                var normalVisual = new VertexNormalVisual(mEditableMesh);
                await normalVisual.Init(lineMaterial, length);
                mElementVisuals.Add("Normal", normalVisual);
                return normalVisual.GraphLines.GraphActor;
            }
        }
        public void SetNormalLength(float length)
        {
            if (mElementVisuals.ContainsKey("Normal"))
            {
                var normal = mElementVisuals["Normal"] as VertexNormalVisual;
                normal.UpdateLength(length);
            }
        }
        public async Task<GActor> GetTangentVisual(RName lineMaterial, float length)
        {
            if (mElementVisuals.ContainsKey("Tangent"))
                return mElementVisuals["Tangent"].GraphLines.GraphActor;
            else
            {
                var tangentVisual = new VertexTangentVisual(mEditableMesh);
                await tangentVisual.Init(lineMaterial, length);
                mElementVisuals.Add("Tangent", tangentVisual);
                return tangentVisual.GraphLines.GraphActor;
            }
        }
        public void SetTangentLength(float length)
        {
            if (mElementVisuals.ContainsKey("Tangent"))
            {
                var tangent = mElementVisuals["Tangent"] as VertexTangentVisual;
                tangent.UpdateLength(length);
            }
        }
    }
    public class EditableMeshVisualCache
    {
        Dictionary<RName, EditableMeshVisual> mEditableMeshDic = new Dictionary<RName, EditableMeshVisual>();
        public EditableMeshVisual Get(CGfxMeshPrimitives mesh)
        {
            if (mEditableMeshDic.ContainsKey(mesh.Name))
            {
                return mEditableMeshDic[mesh.Name];

            }
            else
            {
                var editableMesh = new EditableMesh();
                if (editableMesh.InitMesh(EngineNS.CEngine.Instance.RenderContext, mesh))
                {
                    var visual = new EditableMeshVisual(editableMesh);
                    mEditableMeshDic.Add(mesh.Name, visual);
                    return visual;
                }
            }
            return null;
        }

    }
    public class PreviewSceneControl
    {
        static EditableMeshVisualCache mEditableMeshVisualCache = new EditableMeshVisualCache();
        ViewPort.ViewPortControl mViewPort = new EditorCommon.ViewPort.ViewPortControl();
        [Browsable(false)]
        public ViewPort.ViewPortControl ViewPort => mViewPort;
        public event EventHandler<SelectActorData[]> OnSelectAcotrs;
        public PreviewSceneControl()
        {
        }
        EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile RP_EditorMobile;

        Color4 mBgColor = new Color4(1f, 0f, 0f, 0f);
        [EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public Color4 BgColor
        {
            get
            {
                return mBgColor;
            }

            set
            {
                mBgColor = value;
                if (mViewPort.RPolicy != null)
                {
                    mViewPort.RPolicy.SetClearColor(ref mBgColor);
                }
            }
        }

        bool mIsShowSkyBox = true;
        public bool IsShowSkyBox
        {
            get { return mIsShowSkyBox; }
            set
            {
                if (mIsShowSkyBox == value)
                    return;
                mIsShowSkyBox = value;
                if (mSkyBoxActor != null)
                {
                    mSkyBoxActor.Visible = mIsShowSkyBox;
                    //if (mIsShowSkyBox)
                    //{
                    //    mPreviewWorld?.AddEditorActor(mSkyBoxActor);
                    //}
                    //else
                    //{
                    //    mPreviewWorld?.RemoveEditorActor(mSkyBoxActor.ActorId);
                    //}
                }
            }
        }
        bool mIsShowFloor = true;
        public bool IsShowFloor
        {
            get { return mIsShowFloor; }
            set
            {
                if (mIsShowFloor == value)
                    return;
                mIsShowFloor = value;
                if (mFloorActor != null)
                {
                    mFloorActor.Visible = mIsShowFloor;
                    //if (mIsShowFloor)
                    //{
                    //    mPreviewWorld?.AddEditorActor(mFloorActor);
                    //}
                    //else
                    //{
                    //    mPreviewWorld?.RemoveEditorActor(mFloorActor.ActorId);
                    //}
                }
            }
        }

        EngineNS.GamePlay.GWorld mPreviewWorld = null;
        [Browsable(false)]
        public EngineNS.GamePlay.GWorld PreviewWorld
        {
            get => mPreviewWorld;
        }
        EngineNS.GamePlay.Actor.GActor mSkyBoxActor = null;
        EngineNS.GamePlay.Actor.GActor mFloorActor = null;
        EngineNS.GamePlay.Actor.GActor mDirLightActor = null;
        List<EngineNS.GamePlay.Actor.GActor> mPreviewActorList = new List<EngineNS.GamePlay.Actor.GActor>();
        [Browsable(false)]
        public List<EngineNS.GamePlay.Actor.GActor> PreviewActorList
        {
            get => mPreviewActorList;
            set => mPreviewActorList = value;
        }
        RName mSceneName = RName.GetRName("temp");
        public FTickViewPortControl TickLogicEvent = TickViewport_Default;
        public async Task Initialize(RName sceneName)
        {
            await mViewPort.WaitInitComplated();
            mSceneName = sceneName;
            mDirLightActor = ViewPort.DirLightActor;
            ViewPort.OnSelectAcotrs += ViewPort_OnSelectAcotrs;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            RP_EditorMobile = new EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile();

            var width = (uint)mViewPort.GetViewPortWidth();
            var height = (uint)mViewPort.GetViewPortHeight();

            //RPolicyDS.Init(rc, width, height, Viewport.Camera, vpCtrl.DrawHandle);
            //vpCtrl.RPolicy = RPolicyDS;

            await RP_EditorMobile.Init(rc, width, height, mViewPort.Camera, mViewPort.DrawHandle);
            mViewPort.RPolicy = RP_EditorMobile;

            //RP_EditorMobile.mHitProxy.mEnabled = false;

            mPreviewWorld = new EngineNS.GamePlay.GWorld();
            mPreviewWorld.Init();
            EngineNS.GamePlay.SceneGraph.GSceneGraph sg = null;
            var sgRName = EngineNS.RName.GetRName("editor/map/exhibition_hall_001.map");
            var xnd = await EngineNS.IO.XndHolder.LoadXND(sgRName.Address + "/scene.map");
            if (xnd != null)
            {
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type != null)
                {
                    sg = EngineNS.GamePlay.SceneGraph.GSceneGraph.NewSceneGraphWithoutInit(mViewPort.World, type, new EngineNS.GamePlay.SceneGraph.GSceneGraphDesc());
                    sg.World = mPreviewWorld;
                    if ((await sg.LoadXnd(rc, xnd.Node, sgRName)) == false)
                        sg = null;
                }
            }
            if (sg == null)
            {
                sg = await EngineNS.GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(mViewPort.World, typeof(EngineNS.GamePlay.SceneGraph.GSceneGraph), null);
            }

            foreach (var actor in mPreviewWorld.Actors.Values)
            {
                actor.PreUse(true);
                if (actor.SpecialName == "skybox")
                    mSkyBoxActor = actor;
                else if (actor.SpecialName == "floor")
                    mFloorActor = actor;
            }

            mPreviewWorld.AddScene(sceneName, sg);
            mViewPort.World = mPreviewWorld;

            if (sg.SunActor != null)
            {
                var sunComp = sg.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                if (sunComp != null)
                {
                    sunComp.View = RP_EditorMobile.BaseSceneView;
                }
            }

            //mSkyBoxActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.RName.GetRName("Mesh/sky.gms"));
            //mSkyBoxActor.Placement.Scale = new EngineNS.Vector3(0.1F, 0.1F, 0.1F);
            //mFloorActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.RName.GetRName(@"editor/floor.gms"));
            //mFloorActor.Placement.Scale = new EngineNS.Vector3(100, 0.5f, 100);
            //mFloorActor.Placement.Location = new Vector3(0, -0.251f, 0);

            //mPreviewWorld.AddEditorActor(mSkyBoxActor);
            //mPreviewWorld.AddEditorActor(mFloorActor);
            mViewPort.TickLogicEvent = TickLogicEvent;
            var eye = new EngineNS.Vector3();
            eye.SetValue(0.0f, 3.0f, -3.6f);
            var at = new EngineNS.Vector3();
            at.SetValue(0.0f, 0.0f, 0.0f);
            var up = new EngineNS.Vector3();
            up.SetValue(0.0f, 1.0f, 0.0f);
            ViewPort.Camera.LookAtLH(eye, at, up);
            ViewPort.FocusShow(mFloorActor);
            mViewPort.FocusFunc = FocusHandle;
        }

        private void ViewPort_OnSelectAcotrs(object sender, SelectActorData[] e)
        {
            OnSelectAcotrs?.Invoke(this, e);
        }

        void FocusHandle()
        {
            ViewPort.FocusShow(mPreviewActorList);
        }
        private static void TickViewport_Default(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            vpc.World.Tick();
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (vpc.Camera.SceneView != null)
            {
                vpc.World.CheckVisible(rc.ImmCommandList, vpc.Camera);
                vpc.RPolicy.TickLogic(vpc.Camera.SceneView, rc);
            }
        }

        public bool AddActor(EngineNS.GamePlay.Actor.GActor actor, bool isfocus = false)
        {
            if (mPreviewWorld == null || actor == null)
                return false;
            mPreviewWorld.AddActor(actor);
            mPreviewWorld.GetScene(mSceneName).AddActor(actor);
            mPreviewActorList.Add(actor);
            if (isfocus)
            {
                ViewPort.FocusShow(actor);
            }
            return true;
        }

        public bool AddDynamicActor(EngineNS.GamePlay.Actor.GActor actor, bool isfocus = false)
        {
            if (mPreviewWorld == null || actor == null)
                return false;
            mPreviewWorld.AddActor(actor);
            mPreviewWorld.GetScene(mSceneName).AddDynamicActor(actor);
            mPreviewActorList.Add(actor);
            if (isfocus)
            {
                ViewPort.FocusShow(actor);
            }
            return true;
        }
        public void RemoveActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (mPreviewWorld == null || actor == null)
                return;

            mPreviewWorld.RemoveActor(actor.ActorId);
            mPreviewWorld.GetScene(mSceneName).RemoveActor(actor.ActorId);
            mPreviewActorList.Remove(actor);
        }
        void ClearPreviewActorList()
        {
            if (mPreviewWorld == null)
                return;
            foreach (var actor in mPreviewActorList)
            {
                mPreviewWorld.RemoveActor(actor.ActorId);
                mPreviewWorld.GetScene(mSceneName).RemoveActor(actor.ActorId);
            }
            mPreviewActorList.Clear();
        }

        public bool AddUniqueActor(EngineNS.GamePlay.Actor.GActor actor, bool isfocus = true)
        {
            ClearPreviewActorList();
            var result = AddActor(actor, isfocus);
            mViewPort._DAddActor(actor);
            EngineNS.CEngine.Instance.HitProxyManager.MapActor(actor);
            return result;
        }
        public async void ShowNormal(GActor actor, RName material, float length)
        {
            var mp = actor.GetComponentMesh();
            var editableMeshVisual = mEditableMeshVisualCache.Get(mp.MeshPrimitives);
            var visualActor = await editableMeshVisual.GetNormalVisual(material, length);
            mPreviewWorld.AddEditorActor(visualActor);
        }
        public void SetNormalLength(GActor actor, float length)
        {
            var mp = actor.GetComponentMesh();
            var editableMeshVisual = mEditableMeshVisualCache.Get(mp.MeshPrimitives);
            editableMeshVisual.SetNormalLength(length);
        }
        public async void ShowTangent(GActor actor, RName material, float length)
        {
            var mp = actor.GetComponentMesh();
            var editableMeshVisual = mEditableMeshVisualCache.Get(mp.MeshPrimitives);
            var visualActor = await editableMeshVisual.GetTangentVisual(material, length);
            mPreviewWorld.AddEditorActor(visualActor);
        }
        public void SetTangentLength(GActor actor, float length)
        {
            var mp = actor.GetComponentMesh();
            var editableMeshVisual = mEditableMeshVisualCache.Get(mp.MeshPrimitives);
            editableMeshVisual.SetTangentLength(length);
        }
        public async System.Threading.Tasks.Task<GActor> CreateActor(RName meshName)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var movement = new EngineNS.GamePlay.Component.GMovementComponent();
            actor.AddComponent(movement);
            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var drawMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, meshName);
            if (drawMesh == null)
                return null;
            meshComp.SetSceneMesh(rc.ImmCommandList, drawMesh);
            actor.AddComponent(meshComp);
            AddActor(actor);
            return actor;
        }
        public async System.Threading.Tasks.Task<GActor> CreateActor(List<RName> meshList)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var movement = new EngineNS.GamePlay.Component.GMovementComponent();
            actor.AddComponent(movement);
            var meshComp = new EngineNS.GamePlay.Component.GMutiMeshComponent();
            foreach (var gms in meshList)
            {
                var drawMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, gms);
                if (drawMesh == null)
                    return null;
                meshComp.AddSubMesh(rc, drawMesh);
            }
            actor.AddComponent(meshComp);
            AddActor(actor);
            return actor;
        }

        public async System.Threading.Tasks.Task<GActor> CreateUniqueActor(RName meshName)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var movement = new EngineNS.GamePlay.Component.GMovementComponent();
            actor.AddComponent(movement);
            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            var drawMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, meshName);
            if (drawMesh == null)
                return null;
            meshComp.SetSceneMesh(rc.ImmCommandList, drawMesh);
            actor.AddComponent(meshComp);
            AddUniqueActor(actor);
            return actor;
        }
        public async System.Threading.Tasks.Task<GActor> CreateUniqueActor(List<RName> meshList)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var movement = new EngineNS.GamePlay.Component.GMovementComponent();
            actor.AddComponent(movement);
            var meshComp = new EngineNS.GamePlay.Component.GMutiMeshComponent();
            foreach (var gms in meshList)
            {
                var drawMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, gms);
                if (drawMesh == null)
                    return null;
                meshComp.AddSubMesh(rc, drawMesh);
            }
            actor.AddComponent(meshComp);
            AddUniqueActor(actor);
            return actor;
        }

    }
}
