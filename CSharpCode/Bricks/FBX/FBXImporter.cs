using System;
using System.Collections.Generic;
using System.Text;
namespace AssetImportAndExport
{
    namespace FBX
    {
        //
        public unsafe class FBXObjecctDecs
        {
            
        }

        public unsafe class FBXMeshDesc :  FBXObjecctDecs
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
                    return mInner->NativeSuper->m_Name;
                }
            }
            public AssetImportAndExport.FBX.EFBXObjectType Type
            {
                get
                {
                    return mInner->NativeSuper->m_Type;
                }
            }
            public float Scale
            {
                get
                {
                    return mInner->NativeSuper->m_Scale;
                }
                set
                {
                    mInner->NativeSuper->m_Scale = value;
                }
            }
            public bool Imported
            {
                get
                {
                    return mInner->NativeSuper->m_Imported;
                }
                set
                {
                    mInner->NativeSuper->m_Imported = value;
                }
            }
            public bool ReCalculateTangent
            {
                get
                {
                    return mInner->m_ReCalculateTangent;
                }
                set
                {
                    mInner->m_ReCalculateTangent = value;
                }
            }
            public bool AsCollision
            {
                get
                {
                    return mInner->m_AsCollision;
                }
                set
                {
                    mInner->m_AsCollision = value;
                }
            }
            public bool AsLocalSpace
            {
                get
                {
                    return mInner->m_AsLocalSpace;
                }
                set
                {
                    mInner->m_AsLocalSpace = value;
                }
            }
            public bool HaveSkin
            {
                get
                {
                    return mInner->m_HaveSkin;
                }
                set
                {
                    mInner->m_HaveSkin = value;
                }
            }
            public bool AsStaticMesh
            {
                get
                {
                    return mInner->m_AsStaticMesh;
                }
                set
                {
                    mInner->m_AsStaticMesh = value;
                }
            }
            public uint m_RenderAtom
            {
                get
                {
                    return mInner->m_RenderAtom;
                }
                set
                {
                    mInner->m_RenderAtom = value;
                }
            }
            public bool TransformVertexToAbsolute
            {
                get
                {
                    return mInner->m_TransformVertexToAbsolute;
                }
                set
                {
                    mInner->m_TransformVertexToAbsolute = value;
                }
            }
            public bool BakePivotInVertex
            {
                get
                {
                    return mInner->m_BakePivotInVertex;
                }
                set
                {
                    mInner->m_BakePivotInVertex = value;
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
                    return mInner.FileName;
                }
            }
            public string Creater
            {
                get
                {
                    return mInner.Creater;
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
                    return temp;
                }
            }
        }
        public partial class UFBXFactoryModule : EngineNS.UModule<EngineNS.UEngine>
        {
            public AssetImportAndExport.FBX.FBXFactory Instance = AssetImportAndExport.FBX.FBXFactory.CreateInstance();

            public override void Cleanup(EngineNS.UEngine host)
            {
                Instance.Dispose();
            }
        }
    }
}



namespace EngineNS
{
    partial class UEngine
    {
        public AssetImportAndExport.FBX.UFBXFactoryModule FBXFactoryModule { get; } = new AssetImportAndExport.FBX.UFBXFactoryModule();
    }
}