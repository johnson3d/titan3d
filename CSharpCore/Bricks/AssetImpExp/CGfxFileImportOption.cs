using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.AssetImpExp
{
    public enum SystemUnit
    {
        SU_mm,
        SU_dm,
        SU_cm,
        SU_m,
        SU_km,
        SU_Inch,
        SU_Foot,
        SU_Mile,
        SU_Yard,
        SU_Custom,
    };
    public class MutiFilesImportOption
    {
        List<CGfxFileImportOption> mFileImportOptions = new List<CGfxFileImportOption>();
        [Browsable(false)]
        public List<CGfxFileImportOption> FileImportOptions
        {
            get => mFileImportOptions;
            set => mFileImportOptions = value;
        }
        bool mImportMesh = true;
        public bool ImportMesh
        {
            get => mImportMesh;
            set
            {
                mImportMesh = value;
                using (var it = mFileImportOptions.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var option = it.Current;
                        {
                            option.ImportMesh = value;
                        }
                    }
                }
            }
        }
        bool mImportAnimation = true;
        public bool ImportAnimation
        {
            get => mImportAnimation;
            set
            {
                mImportAnimation = value;
                using (var it = mFileImportOptions.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var option = it.Current;
                        {
                            option.ImportAnimation = value;
                        }
                    }
                }
            }
        }
        float mScale = 1.0f;
        public float Scale
        {
            get => mScale;
            set
            {
                mScale = value;
                using (var it = mFileImportOptions.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var option = it.Current;
                        {
                            option.Scale = value;
                        }
                    }
                }
            }
        }
        [DisplayName("骨骼资产"), EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Skeleton)]
        public RName Skeleton { get; set; } = RName.EmptyName;
        string mCreater = "";
        [Browsable(false)]
        public string Creater
        {
            get
            {
                return mCreater;
            }
        }
        bool mConvertSceneUnit = true;
        public bool ConvertSceneUnit
        {
            get
            {
                return mConvertSceneUnit;
            }
            set
            {
                mConvertSceneUnit = value;
            }
        }
    }
    public class CGfxFileImportOption : CGfxAssetImportOption
    {
        public string mName = "";
        //[Category("Option"), DisplayName("名称"), Editor.Editor_PropertyGridSortIndex(1)]
        [Browsable(false)]
        public override string Name
        {
            get { return mName; }
            set { mName = value; OnPropertyChanged("Name"); }
        }
        public string mAbsSavePath = "";
        [Browsable(false)]
        public override string AbsSavePath
        {
            get { return mAbsSavePath; }
            set { mAbsSavePath = value; }
        }
        bool mImportMesh = true;
        [Category("Option"), DisplayName("导入Mesh")]
        public bool ImportMesh
        {
            get => mImportMesh;
            set
            {
                mImportMesh = value;
                BatchSetImoprtOrNot(value, ImportAssetType.IAT_Mesh);
            }
        }
        bool mImportAnimation = true;
        [Category("Option"), DisplayName("导入动作")]
        public bool ImportAnimation
        {
            get => mImportAnimation;
            set
            {
                mImportAnimation = value;
                BatchSetImoprtOrNot(value, ImportAssetType.IAT_Animation);
                OnPropertyChanged("ImportAnimation");
            }
        }
        bool mIsImport = true;
        [Category("Option"), DisplayName("导入")]
        public override bool IsImport
        {
            get => mIsImport;
            set
            {
                mIsImport = value;
                BatchSetImoprtOrNot(value);
                OnPropertyChanged("IsImport");
            }
        }
        bool mAsInteractiveCinematicsAnimation = false;
        //if true all tracks in one file,if false break skeleton animation
        [Category("Option"), DisplayName("交互式场景动画")]
        public bool AsInteractiveCinematicsAnimation
        {
            get => mAsInteractiveCinematicsAnimation;
            set
            {
                mAsInteractiveCinematicsAnimation = value;
                //BatchSetImoprtOrNot(value, ImportAssetType.IAT_Animation);
                OnPropertyChanged("AsInteractiveCinematicsAnimation");
            }
        }
        float mScale = 1.0f;
        [Category("Option"), DisplayName("缩放")]
        public override float Scale
        {
            get => mScale;
            set
            {
                mScale = value;
                using (var it = ObjectOptions.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var option = it.Current.Value;
                        {
                            option.Scale = value;
                        }
                    }
                }
                OnPropertyChanged("Scale");
            }
        }
        [Category("Option"), DisplayName("文件版本")]
        public string Creater
        {
            get
            {
                return SDK_GfxFileImportOption_GetFileCreater(CoreObject);
            }
        }
        [Category("Option")]
        public bool ConvertSceneUnit
        {
            get
            {
                return SDK_GfxFileImportOption_GetConvertSceneUnit(CoreObject);
            }
            set
            {
                SDK_GfxFileImportOption_SetConvertSceneUnit(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        [Category("Option"), DisplayName("文件系统单位")]
        public SystemUnit SystemUint
        {
            get
            {
                return SDK_GfxFileImportOption_GetFileSystemUnit(CoreObject);
            }
        }

        public CGfxFileImportOption() : base("GfxFileImportOption")
        {

        }
        public void BuildOptionsDictionary()
        {
            var count = SDK_GfxFileImportOption_GetAssetCount(CoreObject);
            mAssetOptions = new CGfxAssetImportOption[count];
            for (uint i = 0; i < count; ++i)
            {
                var option = GetAssetImportOption(i, SDK_GfxFileImportOption_GetAssetType(CoreObject, i));
                ObjectOptions.Add(option.Hash, option);
                mAssetOptions[i] = option;
            }
        }
        public void InitializeSavePath(string absSavePath)
        {
            AbsSavePath = absSavePath;
            for (uint i = 0; i < mAssetOptions.Length; ++i)
            {
                mAssetOptions[i].AbsSavePath = absSavePath;
            }
        }
        CGfxAssetImportOption GetAssetImportOption(uint index, ImportAssetType type)
        {
            CGfxAssetImportOption option = null;
            switch (type)
            {
                case ImportAssetType.IAT_Mesh:
                    {
                        option = new CGfxMeshImportOption(SDK_GfxFileImportOption_GetAssetImportOption(CoreObject, index));
                    }
                    break;
                case ImportAssetType.IAT_Animation:
                    {
                        option = new CGfxAnimationImportOption(SDK_GfxFileImportOption_GetAssetImportOption(CoreObject, index));
                    }
                    break;
                default:
                    break;
            }
            return option;
        }
        [Browsable(false)]
        public Dictionary<uint, CGfxAssetImportOption> ObjectOptions { get; set; } = new Dictionary<uint, CGfxAssetImportOption>();
        public CGfxAssetImportOption[] mAssetOptions;
        [Category("Option"), DisplayName("详细")]
        public CGfxAssetImportOption[] AssetOptions
        {
            get
            {
                return mAssetOptions;
            }
        }
        //public object GetShowProPerty()
        //{
        //    var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
        //    cpInfos.Add(EditorCommon.CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "IsLoop", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
        //    mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, true);
        //}
        public void BatchSetImoprtOrNot(bool isImport, ImportAssetType assetType)
        {
            using (var it = ObjectOptions.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var option = it.Current.Value;
                    if (option.AssetType == assetType)
                    {
                        option.IsImport = isImport;
                    }
                }
            }
        }
        public void BatchSetImoprtOrNot(bool isImport)
        {
            using (var it = ObjectOptions.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var option = it.Current.Value;
                    option.IsImport = isImport;
                }
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static uint SDK_GfxFileImportOption_GetAssetCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static ImportAssetType SDK_GfxFileImportOption_GetAssetType(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxAssetImportOption.NativePointer SDK_GfxFileImportOption_GetAssetImportOption(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxFileImportOption_GetFileCreater(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxFileImportOption_GetConvertSceneUnit(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxFileImportOption_SetConvertSceneUnit(NativePointer self, vBOOL convert);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static SystemUnit SDK_GfxFileImportOption_GetFileSystemUnit(NativePointer self);
        #endregion
    }

}
