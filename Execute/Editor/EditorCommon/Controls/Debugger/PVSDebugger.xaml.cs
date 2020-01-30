using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DockControl;
using EngineNS.IO;

namespace EditorCommon.Controls.Debugger
{
    /// <summary>
    /// PVSDebugger.xaml 的交互逻辑
    /// </summary>
    public partial class PVSDebugger : UserControl, INotifyPropertyChanged, DockControl.IDockAbleControl, EngineNS.ITickInfo
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region IDockAbleControl

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => "PVSDebugger";

        public int Index { get; set; }

        public string DockGroup => null;

        public void SaveElement(XmlNode node, XmlHolder holder)
        {
            throw new NotImplementedException();
        }

        public IDockAbleControl LoadElement(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public void StartDrag()
        {
            throw new NotImplementedException();
        }

        public void EndDrag()
        {
            throw new NotImplementedException();
        }

        public bool? CanClose()
        {
            throw new NotImplementedException();
        }

        public void Closed()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITickInfo
        public void BeforeFrame()
        {

        }
        public void TickLogic()
        {
            UpdateCellBox();
            UpdateCaptureBox();
        }
        public void TickRender()
        {

        }
        public void TickSync()
        {

        }
        public bool EnableTick
        {
            get;
            set;
        } = false;
        public static EngineNS.Profiler.TimeScope ScopeTickLogic = EngineNS.Profiler.TimeScopeManager.GetTimeScope(typeof(PVSDebugger), nameof(TickLogic));
        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            return ScopeTickLogic;
        }
        #endregion

        bool mShowCellBox = false;
        public bool ShowCellBox
        {
            get => mShowCellBox;
            set
            {
                mShowCellBox = value;
                var noUse = ShowCellBoxMesh(mShowCellBox);
                OnPropertyChanged("ShowCellBox");
            }
        }

        bool mShowCaptureBox = false;
        public bool ShowCaptureBox
        {
            get => mShowCaptureBox;
            set
            {
                mShowCaptureBox = value;
                var noUse = ShowCaptureBoxMesh(mShowCaptureBox);
                OnPropertyChanged("ShowCaptureBox");
            }
        }

        EngineNS.GamePlay.Actor.GActor mShowCellBoxActor;
        EngineNS.GamePlay.Actor.GActor mShowCaptureBoxActor;
        EngineNS.GamePlay.Actor.GActor mShowCaptureBoxCenterActor;

        int mResolution = 512;
        public int Resolution
        {
            get => mResolution;
            set
            {
                mResolution = value;
                OnPropertyChanged("Resolution");
            }
        }

        Image[] mPreviewImages = new Image[6];
        public PVSDebugger()
        {
            InitializeComponent();

            EngineNS.CEngine.Instance.TickManager.AddTickInfo(this);
            mPreviewImages[0] = Image_X;
            mPreviewImages[1] = Image_InvZ;
            mPreviewImages[2] = Image_InvX;
            mPreviewImages[3] = Image_Z;
            mPreviewImages[4] = Image_Y;
            mPreviewImages[5] = Image_InvY;

            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            editorIns.OnWorldLoaded += EditorIns_OnWorldLoaded;
        }

        private void EditorIns_OnWorldLoaded()
        {
            GeoScene = null;
            mCurrentGenBox = null;
            mCurrentAgentBox = null;
        }

        void UpdateCellBox()
        {
            if (mShowCellBoxActor == null)
                return;
            if (!ShowCellBox)
                return;
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            var comp = mShowCellBoxActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            var camPos = editorIns.RenderPolicy.Camera.CullingFrustum.TipPos;
            bool finded = false;
            foreach (var pvsSet in editorIns.World.DefaultScene.PvsSets)
            {
                EngineNS.Vector3 scale, trans;
                EngineNS.Quaternion rot;
                pvsSet.WorldMatrix.Decompose(out scale, out rot, out trans);
                foreach (var cell in pvsSet.PvsCells)
                {
                    EngineNS.Vector3 tempPos;
                    EngineNS.Vector3.TransformCoordinate(ref camPos, ref pvsSet.WorldMatrixInv, out tempPos);
                    if (cell.BoundVolume.Contains(ref tempPos) != EngineNS.ContainmentType.Disjoint)
                    {
                        finded = true;
                        var mat = EngineNS.Matrix.Transformation(cell.BoundVolume.GetSize(), rot, trans + cell.BoundVolume.GetCenter());
                        //comp.OnUpdateDrawMatrix(ref mat);
                        mShowCellBoxActor.Placement.SetMatrix(ref mat);
                        comp.OnUpdateDrawMatrix(ref mat);
                        break;
                    }
                }
                if (finded)
                    break;
            }
        }
        async Task ShowCellBoxMesh(bool show)
        {
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            if (mShowCellBoxActor == null)
            {
                var rc = EngineNS.CEngine.Instance.RenderContext;

                var mtlIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName("editor/icon/icon_3D/material/pvs_cameraboxcheck.instmtl", EngineNS.RName.enRNameType.Game));
                var gen = new EngineNS.Bricks.GraphDrawer.McBoxGen()
                {
                    Interval = 0.3f,
                    Segement = 0.5f,
                };
                gen.SetBox(new EngineNS.Vector3(0, 0, 0), 1, 1, 1);
                var line = new EngineNS.Bricks.GraphDrawer.GraphLines();
                line.LinesGen = gen;
                line.UseGeometry = true;
                await line.Init(mtlIns, 0);

                mShowCellBoxActor = line.GraphActor;
            }

            if (show)
            {
                UpdateCellBox();
                editorIns.World.AddEditorActor(mShowCellBoxActor);
            }
            else
            {
                editorIns.World.RemoveEditorActor(mShowCellBoxActor.ActorId);
            }
        }

        public static EngineNS.Bricks.HollowMaker.GeomScene GeoScene;

        static EngineNS.Bricks.HollowMaker.Agent.GeoBox mCurrentGenBox;
        public static EngineNS.Bricks.HollowMaker.Agent.GeoBox CurrentGenBox => mCurrentGenBox;
        static EngineNS.Bricks.HollowMaker.GeomScene.AgentBoxs mCurrentAgentBox;
        public static EngineNS.Bricks.HollowMaker.GeomScene.AgentBoxs CurrentAgentBox => mCurrentAgentBox;
        void UpdateCaptureBox()
        {
            if (mShowCaptureBoxActor == null || mShowCaptureBoxCenterActor == null)
                return;
            if (!ShowCaptureBox)
                return;
            if (GeoScene == null)
                return;
            
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            var comp = mShowCaptureBoxActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            var camPos = editorIns.RenderPolicy.Camera.CullingFrustum.TipPos;
            bool finded = false;
            for(int j=0; j<GeoScene.AgentDatas.Count; j++)
            {
                EngineNS.Vector3 scale, trans;
                EngineNS.Quaternion rot;
                mCurrentAgentBox = GeoScene.AgentDatas[j];
                GeoScene.AgentDatas[j].Mat.Decompose(out scale, out rot, out trans);
                for (int i=0; i<GeoScene.AgentDatas[j].AgentData.Count; i++)
                {
                    var geoBox = GeoScene.AgentDatas[j].AgentData[i];
                    mCurrentGenBox = geoBox;
                    EngineNS.Vector3 tempPos;
                    var invMat = EngineNS.Matrix.Invert(ref GeoScene.AgentDatas[j].Mat);
                    EngineNS.Vector3.TransformCoordinate(ref camPos, ref invMat, out tempPos);
                    if(geoBox.Box.Contains(tempPos) != EngineNS.ContainmentType.Disjoint)
                    {
                        finded = true;
                        var loc = trans + geoBox.Box.GetCenter();
                        var mat = EngineNS.Matrix.Transformation(geoBox.Box.GetSize(), rot, loc);
                        mShowCaptureBoxActor.Placement.SetMatrix(ref mat);
                        mShowCaptureBoxCenterActor.Placement.Location = loc;
                        comp.OnUpdateDrawMatrix(ref mat);
                        break;
                    }
                }
                if (finded)
                    break;
            }
        }
        async Task ShowCaptureBoxMesh(bool show)
        {
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            if (mShowCaptureBoxActor == null)
            {
                var rc = EngineNS.CEngine.Instance.RenderContext;

                var mtlIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName("editor/icon/icon_3D/material/pvs_capturebox.instmtl", EngineNS.RName.enRNameType.Game));
                var gen = new EngineNS.Bricks.GraphDrawer.McBoxGen()
                {
                    Interval = 0.1f,
                    Segement = 0.2f,
                };
                gen.SetBox(new EngineNS.Vector3(0, 0, 0), 1, 1, 1);
                var line = new EngineNS.Bricks.GraphDrawer.GraphLines();
                line.LinesGen = gen;
                line.UseGeometry = true;
                await line.Init(mtlIns, 0);

                mShowCaptureBoxActor = line.GraphActor;

                mShowCaptureBoxCenterActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(EngineNS.RName.GetRName("editor/basemesh/sphere.gms", EngineNS.RName.enRNameType.Game));
                mShowCaptureBoxCenterActor.Placement.Scale = new EngineNS.Vector3(0.1f, 0.1f, 0.1f);
                var meshComp = mShowCaptureBoxCenterActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                if(meshComp != null)
                {
                    await meshComp.SetMaterialInstanceAsync(rc, 0, mtlIns, null);
                }
            }

            if (GeoScene == null)
            {
                GeoScene = await EngineNS.Bricks.HollowMaker.GeomScene.CreateGeomScene(editorIns.World.DefaultScene.SceneFilename.Address + "/geoscene.dat");
            }

            if (show)
            {
                UpdateCaptureBox();
                editorIns.World.AddEditorActor(mShowCaptureBoxActor);
                editorIns.World.AddEditorActor(mShowCaptureBoxCenterActor);
            }
            else
            {
                editorIns.World.RemoveEditorActor(mShowCaptureBoxActor.ActorId);
                editorIns.World.RemoveEditorActor(mShowCaptureBoxCenterActor.ActorId);
            }
        }

        private void Button_Capture_Click(object sender, RoutedEventArgs e)
        {
            var noUse = CaptureProcess();
        }

        async Task CaptureProcess()
        {
            if (mCurrentAgentBox == null || mCurrentGenBox == null)
                return;

            foreach(var img in mPreviewImages)
            {
                img.Width = mResolution;
                img.Height = mResolution;
            }

            var rc = EngineNS.CEngine.Instance.RenderContext;
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            var scene = editorIns.World.DefaultScene;

            EngineNS.Vector3[] camDirs = { EngineNS.Vector3.UnitX, -EngineNS.Vector3.UnitZ, -EngineNS.Vector3.UnitX, EngineNS.Vector3.UnitZ, EngineNS.Vector3.UnitY, -EngineNS.Vector3.UnitY };
            EngineNS.Vector3[] camUps = { EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitY, EngineNS.Vector3.UnitZ, -EngineNS.Vector3.UnitZ };

            var camera = new EngineNS.Graphics.CGfxCamera();
            camera.Init(rc, false);
            camera.PerspectiveFovLH((float)(System.Math.PI * 0.6f), mResolution, mResolution);

            var allActors = new List<EngineNS.GamePlay.Actor.GActor>(scene.Actors.Count);
            var pvsActors = new List<EngineNS.GamePlay.Actor.GActor>(scene.Actors.Count);
            var unPVSActors = new List<EngineNS.GamePlay.Actor.GActor>(scene.Actors.Count);
            var buildItems = new List<EngineNS.GamePlay.Component.ISceneGraphComponent>();
            EditorCommon.PVSAssist.GetBuildActors(scene, allActors, pvsActors, unPVSActors, buildItems);

            var rp = new EngineNS.Graphics.RenderPolicy.CGfxRP_SceneCapture();
            await rp.Init(rc, (UInt32)mResolution, (UInt32)mResolution, camera, IntPtr.Zero);
            rp.CaptureRGBData = true;
            rp.UseCapture = true;

            var checkVisibleParam = new EngineNS.GamePlay.SceneGraph.CheckVisibleParam();
            foreach (var actor in pvsActors)
            {
                actor.OnCheckVisible(rc.ImmCommandList, scene, camera, checkVisibleParam);
            }

            var actorCount = (UInt32)pvsActors.Count;

            // Compute shader
            var cmd = rc.ImmCommandList;
            UInt32 cbIndex;
            EngineNS.CConstantBuffer cbuffer;
            EngineNS.CGpuBuffer buffer_visible;
            EngineNS.CShaderDesc csMain_visible;
            EngineNS.CComputeShader cs_visible;
            EngineNS.CUnorderedAccessView uav_visible;
            EngineNS.CGpuBuffer buffer_setBit;
            EngineNS.CShaderDesc csMain_setBit;
            EngineNS.CComputeShader cs_setBit;
            EngineNS.CUnorderedAccessView uav_setBit;
            EngineNS.CShaderDesc csMain_Clear;
            EngineNS.CComputeShader cs_clear;
            UInt32 textureIdx;
            EditorCommon.PVSAssist.ComputeShaderInit(actorCount, out cbIndex, out cbuffer, out buffer_visible, out csMain_visible, out cs_visible, out uav_visible, 
                out buffer_setBit, out csMain_setBit, out cs_setBit, out uav_setBit, 
                out csMain_Clear , out cs_clear,
                out textureIdx);

            List<EngineNS.Support.BitSet> savedBitsets = new List<EngineNS.Support.BitSet>();
            EditorCommon.PVSAssist.CaptureGeoBox(mCurrentGenBox, mCurrentAgentBox, camera, rc, actorCount, rp, cmd, cbIndex, cbuffer, buffer_visible, csMain_visible, cs_visible, uav_visible, buffer_setBit, csMain_setBit, cs_setBit, uav_setBit, cs_clear, savedBitsets, textureIdx, (camIdx, idBlob, picBlob) =>
            {
                var bitmap = new WriteableBitmap(mResolution, mResolution, 96, 96, PixelFormats.Bgra32, null);
                bitmap.WritePixels(new Int32Rect(0, 0, mResolution, mResolution), picBlob.ToBytes(), mResolution * 4, 0);
                mPreviewImages[camIdx].Source = bitmap;
            });
        }

        bool mShowAllGenBox = false;
        public bool ShowAllGenBox
        {
            get => mShowAllGenBox;
            set
            {
                mShowAllGenBox = value;
                var noUse = ShowAllGenBoxProcess(value);
                OnPropertyChanged("ShowAllGenBox");
            }
        }

        List<EngineNS.GamePlay.Actor.GActor> mAllGenboxActors = new List<EngineNS.GamePlay.Actor.GActor>();
        async Task ShowAllGenBoxProcess(bool show)
        {
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
            if (GeoScene == null)
                GeoScene = await EngineNS.Bricks.HollowMaker.GeomScene.CreateGeomScene(editorIns.World.DefaultScene.SceneFilename.Address + "/geoscene.dat");

            foreach (var actor in mAllGenboxActors)
            {
                editorIns.World.RemoveEditorActor(actor.ActorId);
            }
            mAllGenboxActors.Clear();
            if (show)
            {
                var rc = EngineNS.CEngine.Instance.RenderContext;
                var mtlIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName("editor/icon/icon_3D/material/pvs_geobox.instmtl", EngineNS.RName.enRNameType.Game));
                foreach (var agent in GeoScene.AgentDatas)
                {
                    EngineNS.Vector3 scale, trans;
                    EngineNS.Quaternion rot;
                    agent.Mat.Decompose(out scale, out rot, out trans);
                    foreach (var geoBox in agent.AgentData)
                    {
                        var gen = new EngineNS.Bricks.GraphDrawer.McBoxGen()
                        {
                            Interval = 0.0f,
                            Segement = 1.0f,
                        };
                        gen.SetBox(new EngineNS.Vector3(0, 0, 0), 1, 1, 1);
                        var line = new EngineNS.Bricks.GraphDrawer.GraphLines();
                        line.LinesGen = gen;
                        line.UseGeometry = true;
                        await line.Init(mtlIns, 0);

                        var actor = line.GraphActor;
                        var comp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                        mAllGenboxActors.Add(actor);

                        var loc = trans + geoBox.Box.GetCenter();
                        var mat = EngineNS.Matrix.Transformation(geoBox.Box.GetSize(), rot, loc);
                        actor.Placement.SetMatrix(ref mat);
                        comp.OnUpdateDrawMatrix(ref mat);

                        editorIns.World.AddEditorActor(actor);
                    }
                }
            }
        }

        bool mShowVoxel = false;
        public bool ShowVoxel
        {
            get => mShowVoxel;
            set
            {
                mShowVoxel = value;
                var noUse = ShowVoxelProcess(value);
                OnPropertyChanged("ShowVoxel");
            }
        }
        List<EngineNS.GamePlay.Actor.GActor> mVoxelShowActors = new List<EngineNS.GamePlay.Actor.GActor>();
        async Task ShowVoxelProcess(bool show)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var mtlIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.RName.GetRName("editor/icon/icon_3D/material/pvs_voxel.instmtl", EngineNS.RName.enRNameType.Game));
            var editorIns = EngineNS.CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;

            foreach(var actor in mVoxelShowActors)
            {
                editorIns.World.RemoveEditorActor(actor.ActorId);
            }
            mVoxelShowActors.Clear();

            if(show)
            {
                foreach (var pvsSet in editorIns.World.DefaultScene.PvsSets)
                {
                    EngineNS.Vector3 scale, trans;
                    EngineNS.Quaternion rot;
                    pvsSet.WorldMatrix.Decompose(out scale, out rot, out trans);
                    foreach (var cell in pvsSet.PvsCells)
                    {
                        var gen = new EngineNS.Bricks.GraphDrawer.McBoxGen()
                        {
                            Interval = 0.0f,
                            Segement = 1.0f,
                        };
                        gen.SetBox(new EngineNS.Vector3(0, 0, 0), 1, 1, 1);
                        var line = new EngineNS.Bricks.GraphDrawer.GraphLines();
                        line.LinesGen = gen;
                        line.UseGeometry = true;
                        await line.Init(mtlIns, 0);

                        var actor = line.GraphActor;
                        var comp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();

                        var mat = EngineNS.Matrix.Transformation(cell.BoundVolume.GetSize(), rot, trans + cell.BoundVolume.GetCenter());
                        actor.Placement.SetMatrix(ref mat);
                        comp.OnUpdateDrawMatrix(ref mat);

                        mVoxelShowActors.Add(actor);
                        editorIns.World.AddEditorActor(actor);
                    }
                }
            }
        }
    }
}
