using CodeGenerateSystem.Base;
using EngineNS;
using EngineNS.Bricks.Particle;
using EngineNS.GamePlay.Actor;
using Macross;
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


namespace ParticleEditor
{
    public class ParticleSceneSetter
    {
        [Category("场景设置")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EditorCommon.ViewPort.PreviewSceneControl SceneControl
        {
            get;
            set;
        }

        #region Box
        [Browsable(false)]
        public CGfxParticleSystem ParticleSystem;
        [Browsable(false)]
        public GParticleComponent ParticleComponent;
        //public void ResetParticleComponentBox()
        //{

        //}
        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float L
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.L;
                }
                return 0;
                
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.L = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
            }
        }

        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float W
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.W;
                }
                return 0;
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.W = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
            }
        }

        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float H
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.H;
                }
                return 0;
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.H = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
               
            }
        }

        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float X
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.X;
                }
                return 0;
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.X = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
                   
            }
        }

        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Y
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.Y;
                }
                return 0;
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.Y = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
            }
        }

        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Z
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.Z;
                }
                return 0;
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.Z = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
            }
        }

        [Category("包围盒")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsShowBox
        {
            get
            {
                if (ParticleSystem != null)
                {
                    return ParticleSystem.IsShowBox;
                }
                return false;
                    
            }
            set
            {
                if (ParticleSystem != null)
                {
                    ParticleSystem.IsShowBox = value;
                    var test = ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSystem);
                }
            }
        }

        #endregion
    }
    /// <summary>
    /// ParticleMacrossLinkControl.xaml 的交互逻辑
    /// </summary>
    public partial class ParticleMacrossLinkControl : MacrossLinkControlBase
    {
        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;
        public EditorCommon.ViewPort.PreviewSceneControl SceneControl
        {
            get => mPreviewSceneControl;
        }

        public ParticleSceneSetter ParticleSetter = new ParticleSceneSetter();
        public GActor PrefabActor;
        System.Timers.Timer PrefabTimer = new System.Timers.Timer(2000);
        public ParticleMacrossLinkControl()
        {
            InitializeComponent();

            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            Viewport.Content = mPreviewSceneControl.ViewPort;

            //Viewport.AfterInitializedAction = InitViewPort;

            NodesCtrlAssist = NodesCtrlAssistCtrl;
            MacrossOpPanel = MacrossOpPanelCtrl;
            MacrossOpPanelCtrl.HostParticleControl = this;
            mPG = PG;

            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.LinkedCategoryItemName = MacrossPanel.MainGraphName;
            MacrossOpPanel.HostControl = this;
            
            mPreviewSceneControl.TickLogicEvent = TickParticleEdViewport;
            //InitViewPort(Viewport);

            PropertyChanged = new PropertyChanged(this);
            ParticleSetter.SceneControl = mPreviewSceneControl;
            ProGrid_PreviewScene.Instance = ParticleSetter;

            //PrefabTimer.Elapsed += new System.Timers.ElapsedEventHandler(PlayPrefabAction);//到达时间的时候执行事件；
            //PrefabTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            //PrefabTimer.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件；
            //PrefabTimer.Start(); //启动定时器

        }

        #region Preview GMS
        //public void PlayPrefabAction(object source, System.Timers.ElapsedEventArgs e)
        //{
        //    if (PrefabActor == null || PrefabActor.CenterData == null)
        //    {
        //        return;
        //    }

        //    Type type = PrefabActor.CenterData.GetType();
        //    var Property = type.GetProperty("ShouldAttack");
        //    if (Property != null)
        //    {
        //        Property.SetValue(PrefabActor.CenterData, true);
        //    }
        //}

        GActor NewMeshActor(EngineNS.Graphics.Mesh.CGfxMesh mesh)
        {
            var rc = CEngine.Instance.RenderContext;
            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            var meshComp = new EngineNS.GamePlay.Component.GMeshComponent();
            meshComp.SetSceneMesh(rc.ImmCommandList, mesh);
            actor.AddComponent(meshComp);
            return actor;
        }

        public async System.Threading.Tasks.Task SetPrefab(EngineNS.RName resource)
        {
            if (PrefabActor != null)
            {
                mPreviewSceneControl.RemoveActor(PrefabActor);
            }

            var mesh = await EngineNS.CEngine.Instance.MeshManager.GetMeshOrigion(EngineNS.CEngine.Instance.RenderContext, resource);
            PrefabActor = NewMeshActor(mesh);
            PrefabActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            mPreviewSceneControl.AddActor(PrefabActor);
        }

        EngineNS.Bricks.Animation.AnimNode.AnimationClip mPreviewClip = null;
        public EngineNS.Bricks.Animation.AnimNode.AnimationClip PreviewClip
        {
            get => mPreviewClip;
        }

        void SetAnimationSequence2PreviewActor()
        {
            if (PrefabActor == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            EngineNS.GamePlay.Component.GAnimationComponent animationCom = null;
            var meshComp = PrefabActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            if (meshComp != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                animationCom = new EngineNS.GamePlay.Component.GAnimationComponent(RName.GetRName(skinModifier.SkeletonAsset));
            }
            animationCom.Animation = mPreviewClip;
            PrefabActor.AddComponent(animationCom);
            mPreviewClip.Bind(animationCom.Pose);
        }

        public void ChangePreviewAnimation(RName animName)
        {
            if (PrefabActor == null)
                return;
            mPreviewClip = EngineNS.Bricks.Animation.AnimNode.AnimationClip.CreateSync(animName);
            mPreviewClip.Pause = false;

            SetAnimationSequence2PreviewActor();

            mPreviewClip.IsLoop = UIEffect.Data.ShowAnimaLoop;
        }

        #endregion

        public override async Task<bool> LoadData(string absFile)
        {
            var xndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            if (xndHolder != null)
            {
                mNodesContainerDic.Clear();
                CategoryItem mainGraphItem = null;
                foreach (var category in MacrossOpPanel.CategoryDic)
                {
                    category.Value.Items.Clear();
                    var graphCategoryNode = xndHolder.Node.FindNode("Category_" + category.Key);
                    if (graphCategoryNode == null)
                        continue;

                    var itemNodes = graphCategoryNode.GetNodes();
                    foreach (var itemNode in itemNodes)
                    {
                        var item = new CategoryItem(null, category.Value);
                        item.Load(itemNode, this);
                        if (category.Key.Equals("Particle"))
                        {
                            MacrossOpPanelCtrl.Initialize(item, this);
                        }
                        else
                        {
                            item.Initialize(this, item.InitData);
                        }

                        category.Value.Items.Add(item);

                        switch (item.CategoryItemType)
                        {
                            case CategoryItem.enCategoryItemType.MainGraph:
                                {
                                    mainGraphItem = item;
                                }
                                break;
                        }
                    }
                }

                //BindMainGrid(mainGraphItem);

                if (mainGraphItem == null)
                {
                    //throw new InvalidOperationException("MainGraph丢失!");
                    mainGraphItem = new CategoryItem(null, MacrossOpPanel.CategoryDic[MacrossPanelBase.GraphCategoryName]);
                    MacrossOpPanel.SetMainGridItem(mainGraphItem);
                }
                else
                {
                    MacrossOpPanel.SetMainGridItem(mainGraphItem);
                }

                xndHolder.Node.TryReleaseHolder();
                return true;
            }
            return false;
        }

        EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile RP_EditorMobile;
        bool mViewPortInited = false;
        async System.Threading.Tasks.Task InitViewPort(EditorCommon.ViewPort.ViewPortControl vpCtrl)
        {
            if (mViewPortInited)
                return;

            mViewPortInited = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;

            RP_EditorMobile = new EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile();

            var width = (uint)vpCtrl.GetViewPortWidth();
            var height = (uint)vpCtrl.GetViewPortHeight();

            await RP_EditorMobile.Init(rc, width, height, Viewport.Camera, vpCtrl.DrawHandle);
            vpCtrl.RPolicy = RP_EditorMobile;

            RP_EditorMobile.mHitProxy.mEnabled = false;

            var world = new EngineNS.GamePlay.GWorld();
            world.Init();
            var sceneName = EngineNS.RName.GetRName("temp");
            var sg = await EngineNS.GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(world, typeof(EngineNS.GamePlay.SceneGraph.GSceneGraph), null);
            world.AddScene(sceneName, sg);
            //world.GetScene(sceneName).Add(sceneName, sg);
            vpCtrl.World = world;

            vpCtrl.TickLogicEvent = TickParticleEdViewport;

            mPreviewSceneControl.ViewPort.RPolicy.mHitProxy.mEnabled = true;

            var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
            rp.mForwardBasePass.SetPassBuiltCallBack(EngineNS.CCommandList.OnPassBuilt_WireFrameAnNoCull);
        }

        CodeDomNode.Aix AixNode;
        ShowItemNode_ParticleSystemControl ShowItemNode_ParticleSystemControl;
        PropertyChanged PropertyChanged;
        public override void ShowItemPropertys(object item)
        {
            mPG.Visibility = Visibility.Visible;
            SelectParticleItem2(null, null, null);

            var node = item as CodeDomNode.Aix;
            if (node != null)
            {
                AixNode = node;
                //AixNode.HostActor
                ParticleComponent.Host.Visible = false;
                if (TempActor != null)
                {
                    TempActor.Visible = false;
                }
                var test = AixNode.CreateActor();
                mPreviewSceneControl.AddActor(AixNode.HostActor);
                AixNode.HostActor.Placement.Location = new EngineNS.Vector3(0, 0, 0);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(AixNode.HostActor);
                mPreviewSceneControl.ViewPort.SelectActor(AixNode.HostActor);
                mPreviewSceneControl.ViewPort.ShowRotationActor();
            }
            else
            {
                //显示shape线框
                if (item as CodeDomNode.Particle.IParticleShape != null)
                {
                    var iparticleshape = item as CodeDomNode.Particle.IParticleShape;
                    var particleshape = item as BaseNodeControl;
                    var com = ParticleComponent.FindComponentBySpecialNameRecursion(particleshape.Id.ToString().Replace("-", "_"));
                    if (com != null)
                    {
                        TempMeshComponent = com as EngineNS.GamePlay.Component.GMeshComponent;
                        TempMeshComponent.Visible = UIEffect.Data.IsShowShape;
                    }

                    item = iparticleshape.GetCreateObject().GetShowPropertyObject();

                    iparticleshape.GetCreateObject().SetPropertyChangedEvent(PropertyChanged.OnPropertyChanged);
                    PropertyChanged.SetHost(iparticleshape);
                }
                else if (item as CodeDomNode.Particle.IParticleGradient != null)
                {
                    var pg = item as CodeDomNode.Particle.IParticleGradient;
                    pg.SetPGradient(ParticleComponent.ParticleModifier.ParticleSys);
                    item = pg.GetShowGradient();
                    if (PropertyChanged.ParticleGradients.Contains(pg) == false)
                    {
                        PropertyChanged.ParticleGradients.Add(pg);
                    }
                }

                if (AixNode != null)
                {
                    ParticleComponent.Host.Visible = true;
                    mPreviewSceneControl.RemoveActor(AixNode.HostActor);
                    mPreviewSceneControl.ViewPort.SelectActor(null);
                    AixNode = null;
                }

                if (item as CodeDomNode.Particle.ParticleSystemControl != null)
                {
                    CodeDomNode.Particle.ParticleSystemControl ps = item as CodeDomNode.Particle.ParticleSystemControl;
                    var systecomname = ps.Id.ToString().Replace("-", "_");

                    var com = ParticleComponent.FindComponentBySpecialNameRecursion(systecomname) as EngineNS.Bricks.Particle.GParticleSubSystemComponent;
                    if (com != null)
                    {
                        if (ShowItemNode_ParticleSystemControl == null)
                        {
                            ShowItemNode_ParticleSystemControl = new ShowItemNode_ParticleSystemControl();
                        }

                        SelectParticleCompoment(com);

                        ShowItemNode_ParticleSystemControl.CreateObject = ps.GetCreateObject().GetShowPropertyObject();
                        ShowItemNode_ParticleSystemControl.Placement = TempActor.Placement;
                        mPG.Instance = ShowItemNode_ParticleSystemControl;
                        
                        ps.GetCreateObject().SetPropertyChangedEvent(PropertyChanged.OnPropertyChanged);
                        PropertyChanged.SetHost(ps);

                       
                    }
                    else
                    {

                        mPG.Instance = ps.GetCreateObject().GetShowPropertyObject();
                    }

                }
                else
                {
                    mPG.Instance = item;
                }
            }
        }

        public void TotalParticleInfo(ref float num, ref float primitives, ref float drawcall)
        {
            if (ParticleComponent == null)
                return;

            //num = ParticleComponent.ParticleModifier.ParticleSys.ParticleNumber;
            //primitives = num * ParticleComponent.ParticleModifier.mPass.DrawPrimitive.NumPrimitives;
            //drawcall++;
            for (int i = 0; i < ParticleComponent.Components.Count; ++i)
            {
                var component = ParticleComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    TotalParticleInfo(component, ref num, ref primitives, ref drawcall);
                }
            }
        }

        public void TotalParticleInfo(GParticleSubSystemComponent ParticleSubSystemComponent, ref float num, ref float primitives, ref float drawcall)
        {
            if (ParticleSubSystemComponent.Visible)
            {
                num += ParticleSubSystemComponent.ParticleModifier.ParticleSys.ParticleNumber;
                if (ParticleSubSystemComponent.ParticleModifier.mPass != null)
                {
                    primitives += ParticleSubSystemComponent.ParticleModifier.ParticleSys.ParticleNumber * ParticleSubSystemComponent.ParticleModifier.mPass.DrawPrimitive.NumPrimitives;
                }

                drawcall++;
            }

            for (int i = 0; i < ParticleSubSystemComponent.Components.Count; ++i)
            {
                var component = ParticleSubSystemComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    TotalParticleInfo(component, ref num, ref primitives, ref drawcall);
                }
            }
        }

        public async System.Threading.Tasks.Task SetObjectToEdit()
        {
            EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempMeshEditor");
            await mPreviewSceneControl.Initialize(mSceneName);

            var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
            rp.mForwardBasePass.SetPassBuiltCallBack(EngineNS.CCommandList.OnPassBuilt_WireFrameAnNoCull);

            await InitUIDrawer();
        }

        //bool WireFrame = false;
        //private void IconTextBtn_Click_WireFrame(object sender, RoutedEventArgs e)
        //{
        //    WireFrame = !WireFrame;
        //    if (WireFrame)
        //    {
        //        var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
        //        rp.mCmdListDB_ForwardMobile[0].SetPassBuiltCallBack(EngineNS.CCommandList.OnPassBuilt_WireFrame);
        //        rp.mCmdListDB_ForwardMobile[1].SetPassBuiltCallBack(EngineNS.CCommandList.OnPassBuilt_WireFrame);
        //    }
        //    else
        //    {
        //        var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
        //        rp.mCmdListDB_ForwardMobile[0].SetPassBuiltCallBack(null);
        //        rp.mCmdListDB_ForwardMobile[1].SetPassBuiltCallBack(null);
        //    }
        //}



        async System.Threading.Tasks.Task InitUIDrawer()
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var font = CEngine.Instance.FontManager.GetFont(EngineNS.CEngine.Instance.Desc.DefaultFont, 18, 1024, 128);
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/font.instmtl"));

            var textMeshInfo = new EngineNS.Bricks.FreeTypeFont.CFontMesh();
            await textMeshInfo.SetMaterial(rc, mtl, "txDiffuse");
            textMeshInfo.DrawText(rc, font, "模型", true);

            textMeshInfo.RenderMatrix = EngineNS.Matrix.Translate(20, 0, 0);
            //textMeshInfo.Offset = new Vector2(20, 0);
            //textMeshInfo.Scale = new Vector2(1, 1);

            var rp = mPreviewSceneControl.ViewPort.RPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;

            rp.OnDrawUI += (cmd, view) =>
            {
                float num = 0;
                float primitives = 0;
                float drawcall = 0;
                string currenttime = "0.00s";
                if (ParticleComponent != null)
                {
                    currenttime = ParticleComponent.ParticleModifier.CurrentTimeToString();
                    TotalParticleInfo(ref num, ref primitives, ref drawcall);
                }

                string outInfo = $"粒子数量：{num}\n";
                outInfo += $"粒子播放时长：{currenttime}\n";
                outInfo += $"粒子面数：{primitives}\n";
                outInfo += $"渲染批次：{drawcall}\n";
                outInfo += $"帧间隔：{Math.Ceiling(1000f / EngineNS.CEngine.Instance.EngineElapseTime)}\n";
                textMeshInfo.DrawText(rc, font, outInfo, true);
                for (int i = 0; i < textMeshInfo.PassNum; i++)
                {
                    var pass = textMeshInfo.GetPass(i);
                    if (pass == null)
                        continue;

                    pass.ViewPort = view.Viewport;
                    pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
                    pass.ShadingEnv.BindResources(textMeshInfo.Mesh, pass);

                    cmd.PushPass(pass);
                }
            };
        }

        public void TickParticleEdViewport(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            vpc.World.Tick();
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (vpc.Camera.SceneView != null)
            {
                if (TempPlacementComponent != null)
                {
                    var mat = TempActor.Placement.WorldMatrix;
                    TempPlacementComponent.SetMatrix(ref mat);
                    AddMat(SelectParticleComponent.SpecialName, ref mat);
                }
                vpc.World.CheckVisible(rc.ImmCommandList, vpc.Camera);
                vpc.RPolicy.TickLogic(vpc.Camera.SceneView, rc);

            }


        }

        Dictionary<string, GActor> ParticleSystemActor = new Dictionary<string, GActor>();
        Dictionary<string, GActor> ParticleSubStateActor = new Dictionary<string, GActor>();
        public GParticleComponent ParticleComponent;
        private EngineNS.RName OldRName;//Use for Restart..
        public async Task AddPfxMacross(EngineNS.RName name)
        {
            OldRName = name;
            if (ParticleComponent != null)
            {
                ParticleComponent.Components.Clear();
                //var sceneName = EngineNS.RName.GetRName("temp");

                //Viewport.World.RemoveActor(ParticleComponent.Host.ActorId);
                //Viewport.World.GetScene(sceneName).RemoveActor(ParticleComponent.Host);
                mPreviewSceneControl.RemoveActor(ParticleComponent.Host);

            }
            ParticleComponent = new GParticleComponent();
            ParticleComponent.ParticleSystemPropertyChangedEvent -= PropertyChanged.ParticleSystemPropertyChanged;
            ParticleComponent.ParticleSystemPropertyChangedEvent += PropertyChanged.ParticleSystemPropertyChanged;
            try
            {
                var param = new EngineNS.Editor.PlantableItemCreateActorParam()
                {
                    View = mPreviewSceneControl.ViewPort.RPolicy.BaseSceneView,
                    Location = new EngineNS.Vector3(0, 0, 0),
                };
                GActor dropActor;
                string pfxcs = name.GetFileName().Replace(".macross", "");
                if (EngineNS.CEngine.Instance.FileManager.FileExists(name.Address + "/" + pfxcs + "_Client.cs"))
                {
                    dropActor = await ParticleComponent.CreateActor(param, name);
                }
                else
                {
                    dropActor = await ParticleComponent.CreateActor(param);
                }


                var meshcom = dropActor.FindComponentBySpecialName("EditorShow");
                if (meshcom != null)
                {
                    var com = meshcom as EngineNS.GamePlay.Component.GMeshComponent;
                    if (com != null)
                    {
                        com.Visible = false;
                    }
                }

                dropActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);

                //var sceneName = EngineNS.RName.GetRName("temp");

                //Viewport.World.AddActor(dropActor);
                //Viewport.World.GetScene(sceneName).AddActor(dropActor);

                mPreviewSceneControl.AddActor(dropActor, false);

                ParticleComponent.ParticleModifier.ParticleSys.UseCamera = mPreviewSceneControl.ViewPort.Camera;
                foreach (EngineNS.GamePlay.Component.GComponent component in ParticleComponent.Components)
                {
                    var syscomponent = component as EngineNS.Bricks.Particle.GParticleSubSystemComponent;
                    syscomponent.ParticleModifier.ParticleSys.UseCamera = mPreviewSceneControl.ViewPort.Camera;
                }

                //var SelectActor = new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                // {
                //     Actor = ParticleComponent.Host,
                //     StartTransMatrix = ParticleComponent.Placement.Transform,
                // };

                //ParticleSystemActor.Clear();

                //foreach (EngineNS.GamePlay.Component.GComponent subcomponent in ParticleComponent.Components.Values)
                //{
                //    var component = subcomponent as GParticleSubSystemComponent;
                //    var actor = new GActor();
                //    actor.Placement = component.Placement;
                //    ParticleSystemActor.Add(component.SpecialName, actor);
                //}


                EngineNS.CEngine.Instance.HitProxyManager.MapActor(ParticleComponent.Host);

                PropertyChanged.ParticleGradients.Clear();
                //mPreviewSceneControl.ViewPort.SelectActor(ParticleComponent.Host);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            SetCheckEventDelegate();
            ResetParticleSystemInfos();

            if (ParticleSetter.ParticleSystem != null)
            {
                await ParticleComponent.ResetDebugBox(EngineNS.CEngine.Instance.RenderContext, ParticleComponent.Host, ParticleSetter.ParticleSystem);
            }
            
            ParticleSetter.ParticleSystem = ParticleComponent.ParticleModifier.ParticleSys;
            ParticleSetter.ParticleComponent = ParticleComponent;
        }

        public async Task Restart()
        {
            //await AddPfxMacross(OldRName);
            if (PropertyChanged != null)
            {
                PropertyChanged.OnPropertyChanged(null, null, null);
            }
            
        }

        public void Stop()
        {
            if (ParticleComponent != null)
            {
                //ParticleComponent.Components.Clear();
                ////var sceneName = EngineNS.RName.GetRName("temp");

                ////Viewport.World.RemoveActor(ParticleComponent.Host.ActorId);
                ////Viewport.World.GetScene(sceneName).RemoveActor(ParticleComponent.Host);
                //mPreviewSceneControl.RemoveActor(ParticleComponent.Host);
                //ParticleComponent = null;

            }
        }

        void FocusShow(GActor actor)
        {
            if (Viewport.World == null)
                return;

            EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
            aabb.InitEmptyBox();
            EngineNS.BoundingBox actorAABB = new EngineNS.BoundingBox();
            actor.GetAABB(ref actorAABB);

            aabb.Merge2(ref actorAABB, ref aabb);
            EditorCommon.SnapshotProcess.SnapshotCreator.FocusShow(0, 0, (float)Viewport.GetViewPortWidth(), (float)Viewport.GetViewPortHeight(), aabb.Maximum, aabb.Minimum, Viewport.Camera, Viewport.World, 0.5);
        }

        //public void FocusParticle()
        //{
        //    if (mPreviewSceneControl.ViewPort.World == null)
        //        return;

        //    if (ParticleComponent == null)
        //        return;

        //    var actor = ParticleComponent.Host;
        //    if (actor == null)
        //        return;

        //    EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
        //    aabb.InitEmptyBox();
        //    EngineNS.BoundingBox actorAABB = new EngineNS.BoundingBox();
        //    actor.GetAABB(ref actorAABB);

        //    aabb.Merge2(ref actorAABB, ref aabb);
        //    EditorCommon.SnapshotProcess.SnapshotCreator.FocusShow(0, 0, (float)Viewport.GetViewPortWidth(), (float)Viewport.GetViewPortHeight(), aabb.Maximum, aabb.Minimum, Viewport.Camera, Viewport.World, 0.5);
        //}

        public void ResetAix()
        {
            if (ParticleComponent != null)
            {
                Viewport.SelectActor(ParticleComponent.Host);

                EngineNS.CEngine.Instance.HitProxyManager.MapActor(ParticleComponent.Host);
            }

            TempPlacementComponent = null;
        }

        GActor TempActor = new GActor();
        GParticlePlacementComponent TempPlacementComponent = null;
        public EngineNS.GamePlay.Component.GMeshComponent TempMeshComponent = null;
        public void ShowMeshComponent(bool show)
        {
            if (TempMeshComponent == null)
                return;

            TempMeshComponent.Visible = show;


        }

        public void SetPassUserFlags(EngineNS.GamePlay.Component.GMeshComponent meshcom, uint flag)
        {
            if (meshcom == null)
                return;

            if (meshcom.SceneMesh == null)
                return;

            meshcom.SceneMesh.SetPassUserFlags(flag);
            //component.SceneMesh.SetPassUserFlags(1);
        }

        public void SelectParticleItem(CategoryItem item, string name, string parentname)
        {
            SelectParticleItem2(item, name, ParticleComponent);
        }

        public GParticleSubSystemComponent SelectParticleComponent = null;
        public void SelectParticleItem2(CategoryItem item, string editorvisiblename, GParticleComponent ParticleComponent)
        {
            ShowMeshComponent(false);
            if (ParticleComponent == null | string.IsNullOrEmpty(editorvisiblename))
            {
                return;
            }

            GParticleSubSystemComponent ParticleSubSystemComponent = null;
            var components = ParticleComponent.GetComponents<GParticleSubSystemComponent>();
            for (int i = 0; i < components.Count; i ++)
            {
                if (string.IsNullOrEmpty(components[i].EditorVisibleName) == false && components[i].EditorVisibleName.Equals(editorvisiblename))
                {
                    ParticleSubSystemComponent = components[i];
                    break;
                }
            }
            SelectParticleCompoment(ParticleSubSystemComponent);
            PG.Instance = item.PropertyShowItem;
        }

        public void SelectParticleCompoment(GParticleSubSystemComponent ParticleSubSystemComponent)
        {
            if (ParticleSubSystemComponent == null)
                return;
            if (UIEffect.Data.IsShowSelect)
            {
                if (SelectParticleComponent != null)
                {
                    SelectParticleComponent.Visible = false;
                }
                ParticleSubSystemComponent.Visible = true;
            }
            SelectParticleComponent = ParticleSubSystemComponent;
            SetPassUserFlags(SelectParticleComponent, UIEffect.Data.IsShowWireFrame ? 1 : (uint)0);
            //if (ParticleSubSystemComponent.Placement == null)
            //{
            //    ParticleSubSystemComponent.Placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            //}
            if (TempActor.Placement == null)
            {
                TempActor.Placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            }

            TempPlacementComponent = ParticleSubSystemComponent.Placement as GParticlePlacementComponent;
            bool IsIgnore = TempPlacementComponent.IsIgnore;
            TempPlacementComponent.IsIgnore = false;
            var mat = TempPlacementComponent.WorldMatrix;
            TempActor.Placement.SetMatrix(ref mat);
            TempPlacementComponent.IsIgnore = IsIgnore;
            mPreviewSceneControl.ViewPort.SelectActor(TempActor);

            AddMat(TempPlacementComponent.SpecialName, ref mat);
        }

        private void DealRLAtomArray(EngineNS.Graphics.Mesh.CGfxMtlMesh[] MtlMeshArray)
        {
            try
            {
                if (MtlMeshArray != null)
                {
                    foreach (var mtlmesh in MtlMeshArray)
                    {
                        if (mtlmesh.PrebuildPassArray != null)
                        {
                            for (int j = 0; j < mtlmesh.PrebuildPassArray.Length; j++)
                            {
                                var rsState = mtlmesh.PrebuildPassArray[j].RenderPipeline.RasterizerState;
                                var desc = rsState.Desc;
                                desc.FillMode = EngineNS.EFillMode.FMD_WIREFRAME;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                // int xx = 0;
            }

        }

        List<string> HideParticleName = new List<string>();
        public bool ShowCheckComponent(GParticleSubSystemComponent ParticleSubSystemComponent, string name, bool visible)
        {
            if (string.IsNullOrEmpty(ParticleSubSystemComponent.EditorVisibleName) == false && ParticleSubSystemComponent.EditorVisibleName.Equals(name))
            {
                ParticleSubSystemComponent.Visible = visible;
                ParticleSubSystemComponent.IsCheckForVisible = visible;
                if (visible == false)
                {
                    if (HideParticleName.Contains(name) == false)
                    {
                        HideParticleName.Add(name);
                    }
                }
                else
                {
                    HideParticleName.Remove(name);
                }
                return true;
            }

            for (int i = 0; i < ParticleSubSystemComponent.Components.Count; ++i)
            {
                var component = ParticleSubSystemComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    if (ShowCheckComponent(ParticleSubSystemComponent, name, visible))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        Dictionary<string, EngineNS.Matrix> ParticleSysMats = new Dictionary<string, EngineNS.Matrix>();

        public void AddMat(string name, ref EngineNS.Matrix mat)
        {

            if (ParticleSysMats.ContainsKey(name))
            {
                ParticleSysMats[name] = mat;
            }
            else
            {
                ParticleSysMats.Add(name, mat);
            }
        }

        public void ResetParticleSystemInfos()
        {
            for (int i = 0; i < HideParticleName.Count; i++)
            {
                OnIsShowChanged(HideParticleName[i], false, false);
            }

            foreach (var matinfo in ParticleSysMats)
            {
                var com = ParticleComponent.FindComponentBySpecialNameRecursion(matinfo.Key) as EngineNS.Bricks.Particle.GParticleSubSystemComponent;
                if (com != null)
                {
                    var mat = matinfo.Value;
                    com.Placement.SetMatrix(ref mat);
                }
            }
        }

        public void OnIsShowChanged(CategoryItem item, bool newvalue, bool oldvalue)
        {
            if (ParticleComponent == null)
                return;

            for (int i = 0; i < ParticleComponent.Components.Count; ++i)
            {
                var component = ParticleComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    if (ShowCheckComponent(component, item.Name, newvalue))
                    {
                        break;
                    }
                }
            }
        }

        public void OnIsShowChanged(string name, bool newvalue, bool oldvalue)
        {
            if (ParticleComponent == null)
                return;

            for (int i = 0; i < ParticleComponent.Components.Count; ++i)
            {
                var component = ParticleComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    if (ShowCheckComponent(component, name, newvalue))
                    {
                        break;
                    }
                }
            }
        }

        public void SetCheckEventDelegate()
        {
            Category category;
            MacrossOpPanel.CategoryDic.TryGetValue(ParticlePanel.ParticleCategoryName, out category);
            foreach (var item in category.Items)
            {
                //if (item.Parent == null)
                //{
                item.CheckVisibility = Visibility.Visible;
                item.OnIsShowChanged -= OnIsShowChanged;
                item.OnIsShowChanged += OnIsShowChanged;
                OnIsShowChanged(item, item.IsShow, !item.IsShow);
                //}
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //InitViewPort(Viewport);
            UISlider.Control = this;

            Category category;
            MacrossOpPanel.CategoryDic.TryGetValue(ParticlePanel.ParticleCategoryName, out category);
            category.SelectParticleItem -= SelectParticleItem;
            category.SelectParticleItem += SelectParticleItem;

            UIEffect.Host = this;
        }

        private CodeGenerateSystem.Base.BaseNodeControl mCurrentParticleNode;
        private CodeGenerateSystem.Base.BaseNodeControl mCurrentOperateBaseNode;
        public override void OnSelectNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {

            if (IsParticleNode(node))
            {
                mCurrentParticleNode = node;
            }
            mCurrentOperateBaseNode = node;
            base.OnSelectNodeControl(node);
            //mCurrentOperateBaseNode = null;
        }

        public bool IsParticleNode(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if (node == null)
                return false;

            Type type = node.GetType();
            Type interfacetype = typeof(CodeDomNode.Particle.IParticleNode);
            var interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].Equals(interfacetype))
                    return true;
            }
            return false;
        }

        private Dictionary<string, Category> mCategoryDic = new Dictionary<string, Category>();
        private Dictionary<Object, Dictionary<string, Category>> mBaseNodeCategoryDic = new Dictionary<Object, Dictionary<string, Category>>();
        //private string GraphCategoryName = "Graphs";
        //打开显示图的回调事件
        public override void ShowNodesContainerEvent(CodeGenerateSystem.Controls.NodesContainerControl ctrl)
        {
            //ctrl.LinkedCategoryItemName = MacrossPanel.MainGraphName
            if (ctrl.TitleString.Equals("MainGraph") || IsParticleNode(mCurrentOperateBaseNode) == false)
            {
                //拷贝结点已经有的列表数据
                //if (IsParticleNode(mCurrentOperateBaseNode) == false)
                if ((mCurrentOperateBaseNode == null && mCurrentParticleNode != null) || ctrl.TitleString.Equals("MainGraph"))
                {
                    var csparam = mCurrentParticleNode.CSParam as CodeDomNode.Particle.StructNodeControlConstructionParams;
                    //if (csparam != null)
                    {
                        if (mBaseNodeCategoryDic.TryGetValue(mCurrentParticleNode, out csparam.CategoryDic) == false)
                        {
                            mBaseNodeCategoryDic[mCurrentParticleNode] = new Dictionary<string, Category>();
                            csparam.CategoryDic = mBaseNodeCategoryDic[mCurrentParticleNode];

                            var names = new string[] { MacrossPanelBase.GraphCategoryName, MacrossPanelBase.FunctionCategoryName, MacrossPanelBase.VariableCategoryName, MacrossPanelBase.AttributeCategoryName };
                            foreach (var name in names)
                            {
                                var category1 = new Category(MacrossOpPanelCtrl);
                                category1.CategoryName = name;
                                csparam.CategoryDic.Add(name, category1);
                                //categoryPanel.Children.Add(category);
                            }
                            foreach (var category1 in csparam.CategoryDic)
                            {
                                category1.Value.OnSelectedItemChanged = (categoryName) =>
                                {
                                    foreach (var cName in names)
                                    {
                                        if (cName == categoryName)
                                            continue;

                                        Category ctg;
                                        if (csparam.CategoryDic.TryGetValue(cName, out ctg))
                                        {
                                            ctg.UnSelectAllItems();
                                        }
                                    }
                                };
                            }
                        }
                    }
                    CopyCategorys(MacrossOpPanel.CategoryDic, csparam.CategoryDic);

                }

                if (mCurrentOperateBaseNode == null)
                {
                    ////删除结点数据
                    ClearCategorys();
                }

            }
            else
            {
                var csparam = mCurrentOperateBaseNode.CSParam as CodeDomNode.Particle.StructNodeControlConstructionParams;
                if (csparam.CategoryDic != null)
                {

                    //if (mBaseNodeCategoryDic[mCurrentOperateBaseNode] == null)
                    //{
                    //    mBaseNodeCategoryDic[mCurrentOperateBaseNode] = csparam.CategoryDic;
                    //}
                    ClearCategorys();
                    CopyCategorys(csparam.CategoryDic, MacrossOpPanel.CategoryDic);
                }
            }

            mCurrentOperateBaseNode = null;
        }

        public override async Task<NodesControlAssist> ShowNodesContainer(INodesContainerDicKey graphKey)
        {
            DockControl.Controls.DockAbleTabControl tabCtrl = null;
            DockControl.Controls.DockAbleContainerControl dockContainer = null;

            if (mNodesContainerDic.Count > 0)
            {
                foreach (var data in mNodesContainerDic)
                {
                    var parent = EditorCommon.Program.GetParent(data.Value, typeof(DockControl.Controls.DockAbleTabControl)) as DockControl.Controls.DockAbleTabControl;
                    if (parent == null)
                        continue;

                    tabCtrl = parent;
                    dockContainer = EditorCommon.Program.GetParent(parent, typeof(DockControl.Controls.DockAbleContainerControl)) as DockControl.Controls.DockAbleContainerControl;
                    break;
                }
            }

            var ctrl = await GetNodesContainer(graphKey, true);
            ctrl.ShowNodesContainerEvent -= this.ShowNodesContainerEvent;
            ctrl.ShowNodesContainerEvent += this.ShowNodesContainerEvent;

            if (tabCtrl != null)
            {
                var parentTabItem = EditorCommon.Program.GetParent(ctrl, typeof(DockControl.Controls.DockAbleTabItem)) as DockControl.Controls.DockAbleTabItem;
                if (parentTabItem == null)
                {
                    var tabItem = new DockControl.Controls.DockAbleTabItem()
                    {
                        Content = ctrl,
                    };
                    tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.HeaderProperty, new Binding("ShowName") { Source = graphKey, Mode = BindingMode.TwoWay });
                    tabItem.CanClose += () =>
                    {
                        if (ctrl.IsDirty)
                        {
                            var result = EditorCommon.MessageBox.Show($"{graphKey.Name}还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                            switch (result)
                            {
                                case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                                    Save();
                                    return true;
                                case EditorCommon.MessageBox.enMessageBoxResult.No:
                                    return true;
                                case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                                    return false;
                            }
                        }
                        return true;
                    };
                    tabItem.OnClose += () =>
                    {
                        mNodesContainerDic.Remove(graphKey);
                    };
                    tabItem.DockGroup = dockContainer.Group;
                    dockContainer.AddChild(tabItem);
                }
                else
                    parentTabItem.IsSelected = true;
            }

            return ctrl;
        }

        //最后生成代码的时候执行一次拷贝
        public void CopyLast()
        {
            if (mCurrentParticleNode == null)
                return;

            var csparam = mCurrentParticleNode.CSParam as CodeDomNode.Particle.StructNodeControlConstructionParams;
            //if (csparam != null)
            {
                //if (mBaseNodeCategoryDic.TryGetValue(mCurrentParticleNode, out csparam.CategoryDic) == false)
                //{
                //    mBaseNodeCategoryDic[mCurrentParticleNode] = new Dictionary<string, Category>();
                //    csparam.CategoryDic = mBaseNodeCategoryDic[mCurrentParticleNode];

                //    var names = new string[] { MacrossPanelBase.GraphCategoryName, MacrossPanelBase.FunctionCategoryName, MacrossPanelBase.VariableCategoryName, MacrossPanelBase.AttributeCategoryName };
                //    foreach (var name in names)
                //    {
                //        var category1 = new Category(MacrossOpPanelCtrl);
                //        category1.CategoryName = name;
                //        csparam.CategoryDic.Add(name, category1);
                //        //categoryPanel.Children.Add(category);
                //    }
                //    foreach (var category1 in csparam.CategoryDic)
                //    {
                //        category1.Value.OnSelectedItemChanged = (categoryName) =>
                //        {
                //            foreach (var cName in names)
                //            {
                //                if (cName == categoryName)
                //                    continue;

                //                Category ctg;
                //                if (csparam.CategoryDic.TryGetValue(cName, out ctg))
                //                {
                //                    ctg.UnSelectAllItems();
                //                }
                //            }
                //        };
                //    }
                //}
            }
            CopyCategorys(MacrossOpPanel.CategoryDic, csparam.CategoryDic);
        }

        public void ClearCategorys()
        {
            //删除结点数据
            Category category;
            MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanelBase.GraphCategoryName, out category);
            category.Items.Clear();
            //if (category != null)
            //{
            //    for (int i = category.Items.Count - 1; i >=0; i--)
            //    {
            //        if(category.Items[i].Name.Equals("MainGraph") == false)
            //        category.Items.RemoveAt(i);
            //    }
            //}

            MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanelBase.VariableCategoryName, out category);
            category.Items.Clear();

            MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanelBase.FunctionCategoryName, out category);
            category.Items.Clear();

            MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanelBase.AttributeCategoryName, out category);
            category.Items.Clear();
        }

        public void CopyCategorys(Dictionary<string, Category> CategoryDic1, Dictionary<string, Category> CategoryDic2)
        {
            if (CategoryDic1 == null || CategoryDic2 == null)
                return;

            CopyCategorys(MacrossPanelBase.GraphCategoryName, CategoryDic1, CategoryDic2);

            CopyCategorys(MacrossPanelBase.VariableCategoryName, CategoryDic1, CategoryDic2);

            CopyCategorys(MacrossPanelBase.FunctionCategoryName, CategoryDic1, CategoryDic2);

            CopyCategorys(MacrossPanelBase.AttributeCategoryName, CategoryDic1, CategoryDic2);
        }
        private void CopyCategorys(string name, Dictionary<string, Category> CategoryDic1, Dictionary<string, Category> CategoryDic2)
        {
            if (CategoryDic1 == null || CategoryDic2 == null)
                return;

            Category category2;
            Category category1;
            CategoryDic2.TryGetValue(name, out category2);
            CategoryDic1.TryGetValue(name, out category1);
            if (category2 == null)
                return;

            category2.Items.Clear();

            if (category1 == null)
                return;

            for (int i = 0; i < category1.Items.Count; i++)
            {
                //if (name.Equals(MacrossPanelBase.GraphCategoryName))
                //{
                //    if (category1.Items[i].Name.Equals("MainGraph"))
                //    {
                //        continue;
                //    }
                //}
                category2.Items.Add(category1.Items[i]);
            }
        }

        private void UIPGGuid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (RemoveNode != null)
            //{
            //    var node = RemoveNode as CodeDomNode.DataGradientElement;
            //    if (node != null)
            //    {
            //        node.CallSizeChanged(e.NewSize.Width, e.NewSize.Height);
            //    }
            //}

        }
    }
}