using System;
using System.Collections.Generic;
using System.Text;
namespace AssetImportAndExport.FBX
{
    //
    public unsafe class FBXObjecctDecs
    {

    }

    public unsafe class FBXMeshDesc : FBXObjecctDecs
    {
        public unsafe FBXMeshDesc(FBXMeshImportDesc* v)
        {
            mInner = v;

        }
        public FBXMeshImportDesc* mInner;

        #region Field
        public string Name
        {
            get
            {
                return mInner->NativeSuper->Name.Text;
            }
        }
        public AssetImportAndExport.FBX.EFBXObjectType Type
        {
            get
            {
                return mInner->NativeSuper->Type;
            }
        }
        public float Scale
        {
            get
            {
                return mInner->NativeSuper->Scale;
            }
            set
            {
                var super = mInner->NativeSuper;
                super->Scale = value;
            }
        }
        public bool Imported
        {
            get
            {
                return mInner->NativeSuper->Imported;
            }
            set
            {
                var super = mInner->NativeSuper;
                super->Imported = value;
            }
        }
        public bool ReCalculateTangent
        {
            get
            {
                return mInner->ReCalculateTangent;
            }
            set
            {
                mInner->ReCalculateTangent = value;
            }
        }
        public bool AsCollision
        {
            get
            {
                return mInner->AsCollision;
            }
            set
            {
                mInner->AsCollision = value;
            }
        }
        public bool AsLocalSpace
        {
            get
            {
                return mInner->AsLocalSpace;
            }
            set
            {
                mInner->AsLocalSpace = value;
            }
        }
        public bool HaveSkin
        {
            get
            {
                return mInner->HaveSkin;
            }
            set
            {
                mInner->HaveSkin = value;
            }
        }
        public bool AsStaticMesh
        {
            get
            {
                return mInner->AsStaticMesh;
            }
            set
            {
                mInner->AsStaticMesh = value;
            }
        }
        public uint m_RenderAtom
        {
            get
            {
                return mInner->RenderAtom;
            }
            set
            {
                mInner->RenderAtom = value;
            }
        }
        public bool TransformVertexToAbsolute
        {
            get
            {
                return mInner->TransformVertexToAbsolute;
            }
            set
            {
                mInner->TransformVertexToAbsolute = value;
            }
        }
        public bool BakePivotInVertex
        {
            get
            {
                return mInner->BakePivotInVertex;
            }
            set
            {
                mInner->BakePivotInVertex = value;
            }
        }

        #endregion

    }

    public unsafe class FBXAnimDesc : FBXObjecctDecs
    {
        public unsafe FBXAnimDesc(FBXAnimImportDesc* v)
        {
            mInner = v;

        }
        public FBXAnimImportDesc* mInner;
        #region Field
        public bool Imported
        {
            get
            {
                return mInner->NativeSuper->Imported;
            }
            set
            {
                var super = mInner->NativeSuper;
                super->Imported = value;
            }
        }
        public string Name
        {

            get
            {
                return mInner->NativeSuper->Name.Text;
            }
        }
        public float Duration
        {
            get
            {
                return mInner->Duration;
            }
        }
        public float SampleRate
        {
            get
            {
                return mInner->SampleRate;
            }
        }
        public float Scale
        {
            get
            {
                return mInner->NativeSuper->Scale;
            }
            set
            {
                var super = mInner->NativeSuper;
                super->Scale = value;
            }
        }
        #endregion
    }
    public unsafe class FBXFileDesc
    {
        public unsafe FBXFileDesc(FBXFileImportDesc* v)
        {
            mInner = new FBXFileImportDesc(v);
        }

        public FBXFileImportDesc mInner;

        #region Field
        public string FileName
        {
            get
            {
                return mInner.FileName.Text;
            }
        }
        public string Creater
        {
            get
            {
                return mInner.Creater.Text;
            }
        }
        public AssetImportAndExport.FBX.SystemUnit FileSystemUnit
        {
            get
            {
                return mInner.FileSystemUnit;
            }
            set
            {
                mInner.FileSystemUnit = value;
            }
        }
        public bool ConvertSceneUnit
        {
            get
            {
                return mInner.ConvertSceneUnit;
            }
            set
            {
                mInner.ConvertSceneUnit = value;
            }
        }
        public float ScaleFactor
        {
            get
            {
                return mInner.ScaleFactor;
            }
            set
            {
                mInner.ScaleFactor = value;
            }
        }
        public uint MeshNum
        {
            get
            {
                return mInner.MeshNum;
            }
        }
        public uint AnimNum
        {
            get
            {
                return mInner.AnimNum;
            }
        }

        private bool mImportAnim = true;
        public bool ImportAnim 
        {
            get => mImportAnim;
            set
            {
                mImportAnim = value;
                foreach(var objectDesc in ObjectList)
                {
                    if(objectDesc is FBXAnimDesc)
                    {
                        (objectDesc as FBXAnimDesc).Imported = value;
                    }
                }    
            }
        }
        bool mImportMesh = true;
        public bool ImportMesh
        {
            get => mImportMesh;
            set
            {
                mImportMesh = value;
                foreach (var objectDesc in ObjectList)
                {
                    if (objectDesc is FBXMeshDesc)
                    {
                        (objectDesc as FBXMeshDesc).Imported = value;
                    }
                }
            }
        }
        #endregion

        public List<FBXObjecctDecs> ObjectList { get; } = new List<FBXObjecctDecs>();


    }
    partial struct FBXImporter
    {
        public FBXFileDesc PreImport(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }
            unsafe
            {
                CheckFileValidedAndInitialize(filename);
                FBXFileDesc temp = new FBXFileDesc(GetFileImportDesc().CppPointer);
                for (uint i = 0; i < temp.MeshNum; ++i)
                {
                    temp.ObjectList.Add(new FBXMeshDesc(GetFBXMeshDescs(i)));
                }
                for (uint i = 0; i < temp.AnimNum; ++i)
                {
                    temp.ObjectList.Add(new FBXAnimDesc(GetFBXAnimDesc(i)));
                }
                return temp;
            }
        }
    }
    public partial class UFBXFactoryModule : EngineNS.TtModule<EngineNS.TtEngine>
    {
        public AssetImportAndExport.FBX.FBXFactory Instance = AssetImportAndExport.FBX.FBXFactory.CreateInstance();

        public override void Cleanup(EngineNS.TtEngine host)
        {
            Instance.Dispose();
        }
    }
}



namespace EngineNS
{
    partial class TtEngine
    {
        public AssetImportAndExport.FBX.UFBXFactoryModule FBXFactoryModule { get; } = new AssetImportAndExport.FBX.UFBXFactoryModule();
    }
}

