using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace EngineNS
{
    public partial class CEngineDesc : IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public CEngineDesc()
        {

        }
        public CEngine EngineInstance
        {
            get { return CEngine.Instance; }
        }
        public bool Editor_Save_Config
        {
            get { return true; }
            set
            {
                var rn = RName.GetRName($"{nameof(CEngineDesc)}.cfg");
                IO.XmlHolder.SaveObjectToXML(this, rn);
            }
        }
        [Rtti.MetaData]
        public bool RenderMT
        {
            get;
            set;
        } = true;
        [Rtti.MetaData]
        public bool LoadAllShaders
        {
            get;
            set;
        } = true;
        [Rtti.MetaData]
        public string Client_Directory
        {
            get;
            set;
        } = "binaries";
        [Rtti.MetaData]
        public string Server_Directory
        {
            get;
            set;
        } = "Server";
        ERHIType mRHIType = ERHIType.RHT_D3D11;
        [Rtti.MetaData]
        public ERHIType RHIType
        {
            get => mRHIType;
            set
            {
                mRHIType = value;
                OnPropertyChanged("RHIType");
            }
        }
        [Rtti.MetaData]
        public int ThreadPoolCount
        {
            get;
            set;
        } = 3;

        [Rtti.MetaData]
        public string ProfilerReporterName
        {
            get;
            set;
        } = "TitanEngine";

        [Rtti.MetaData]
        [Editor.Editor_RNameMacrossType(typeof(GamePlay.McGameInstance))]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public RName GameMacross
        {
            get;
            set;
        } = RName.GetRName("Macross/mygame.macross");

        [Rtti.MetaData]
        [Editor.Editor_RNameMacrossType(typeof(GamePlay.McGameInstance))]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public RName EngineMacross
        {
            get;
            set;
        } = RName.GetRName("Macross/engine.macross");

        public static bool ForceSaveResource
        {
            get;
            set;
        } = false;

        #region FileExtension
        public static string MeshSourceExtension
        {
            get;
            set;
        } = ".vms";
        public static string SkeletonExtension
        {
            get;
            set;
        } = ".skt";
        public static string AnimationClipExtension
        {
            get;
            set;
        } = ".anim";
        public static string AnimationClipNotifyExtension
        {
            get;
            set;
        } = ".notify";
        public static string AnimationSequenceExtension
        {
            get;
            set;
        } = ".vanims";
        public static string AnimationBlendSpace1DExtension
        {
            get;
            set;
        } = ".vanimbs1d";
        public static string AnimationBlendSpaceExtension
        {
            get;
            set;
        } = ".vanimbs";
        public static string AnimationAdditiveBlendSpace1DExtension
        {
            get;
            set;
        } = ".vanimabs1d";
        public static string AnimationAdditiveBlendSpaceExtension
        {
            get;
            set;
        } = ".vanimabs";
        public static string SkeletonActionExtension
        {
            get;
            set;
        } = ".vanims";
        public static string MeshExtension
        {
            get;
            set;
        } = ".gms";
        public static string PrefabExtension
        {
            get;
            set;
        } = ".prefab";
        public static string MeshSocketExtension
        {
            get;
            set;
        } = ".socket";
        public static string MaterialExtension
        {
            get;
            set;
        } = ".material";
        public static string MaterialInstanceExtension
        {
            get;
            set;
        } = ".instmtl";
        public static string ShadingEnvExtension
        {
            get;
            set;
        } = ".senv";
        public static string TextureExtension
        {
            get;
            set;
        } = ".txpic";
        public static string MacrossExtension
        {
            get;
            set;
        } = ".macross";
        public static string MacrossEnumExtension
        {
            get;
            set;
        } = ".macross_enum";
        public static string ExcelExtension
        {
            get;
            set;
        } = ".xls";
        public static string ParticleExtension
        {
            get;
            set;
        } = ".particle";
        public static string PhyConvexGeom
        {
            get;
            set;
        } = ".PhyConvexGeom";
        public static string PhyTriangleMeshGeom
        {
            get;
            set;
        } = ".PhyTriangleMeshGeom";
        public static string PhyHeightFieldGeom
        {
            get;
            set;
        } = ".PhyHeightFieldGeom";
        public static string PhysicsGeom
        {
            get;
            set;
        } = ".phygeom";
        public static string PhysicsMaterial
        {
            get;
            set;
        } = ".phymtl"; 
        public static string VertexCloudExtension
        {
            get;
            set;
        } = ".vtc";
        public static string MeshCluster
        {
            get;
            set;
        } = ".cluster";
        public static string SceneExtension
        {
            get;
            set;
        } = ".map";
        #endregion

        #region SysteResource
        [Editor.Editor_PackData()]
        public static RName FullScreenRectName
        {
            get;
            set;
        } = RName.GetRName("Cook/FullScreenRect.vms");

        [Editor.Editor_PackData()]
        public static RName ScreenAlignedTriangleName
        {
            get;
            set;
        } = RName.GetRName("Cook/ScreenAlignedTriangle.vms");

        [Editor.Editor_PackData()]
        public static RName OctreeMaterialName
        {
            get;
            set;
        } = RName.GetRName("editor/volume/mi_volume_octree.instmtl", RName.enRNameType.Game);

        [Editor.Editor_PackData()]
        public static RName PlayerStartMesh
        {
            get;
            set;
        } = RName.GetRName("editor/icon/icon_3D/mesh/play_start.gms", RName.enRNameType.Game);
        [Editor.Editor_PackData()]
        public static RName PlayerStartMeshVMS
        {
            get;
            set;
        } = RName.GetRName("editor/icon/icon_3D/mesh/play_start.vms", RName.enRNameType.Game);

        [Editor.Editor_PackData()]
        public static RName CameraFrustumMtl
        {
            get;
            set;
        } = RName.GetRName("editor/icon/icon_3D/material/camerafrustum.instmtl", RName.enRNameType.Game);
        #endregion
    }

    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    public class McEngine
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnEditorStarted(CEngine engine)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnStartGame(CEngine engine)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public delegate System.Threading.Tasks.Task FOpenEditor(RName name);
        public static FOpenEditor mOpenEditor;
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task OpenMapEditor(
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Scene)]
            RName mapName)
        {
            if (mOpenEditor == null)
                return;
            await mOpenEditor(mapName);

            await Thread.AsyncDummyClass.DummyFunc();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetGameMacross(
            CEngine engine,
            [Editor.Editor_RNameMacrossType(typeof(GamePlay.McGameInstance))]
            RName rn)
        {
            engine.Desc.GameMacross = rn;
        }
    }

    public partial class CEngine
    {
        protected RName mEngineMacross;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McEngine))]
        public RName EngineMacross
        {
            get { return mEngineMacross; }
            set
            {
                if (mEngineMacross == value)
                    return;

                mEngineMacross = value;
                mMcEngineGetter = MacrossDataManager.NewObjectGetter<McEngine>(value);
            }
        }

        protected Macross.MacrossGetter<McEngine> mMcEngineGetter;
        [Browsable(false)]
        public Macross.MacrossGetter<McEngine> McEngineGetter
        {
            get { return mMcEngineGetter; }
        }
    }
}
