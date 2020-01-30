using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Particle.EmitShape
{
    [Editor.MacrossPanelPath("粒子系统/创建模型的粒子发射器(Create CGfxParticleEmitterShapeMesh)")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public partial class CGfxParticleEmitterShapeMesh : CGfxParticleEmitterShape
    {
        public CGfxParticleEmitterShapeMesh()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleEmitterShapeMesh");
        }
        RName mMesh;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.VertexCloud)]
        public RName Mesh
        {
            get
            {
                return mMesh;
            }
            set
            {
                mMesh = value;
                Action action = async () =>
                {
                    var vc = await CEngine.Instance.VertexCoudManager.GetVertexCloud(value);
                    if (vc == null)
                        return;

                    unsafe
                    {
                        fixed (Vector3* p = &vc.Positions[0])
                        {
                            SDK_GfxParticleEmitterShapeMesh_SetPoints(CoreObject, p, vc.Positions.Length);
                        }
                    }
                };
                action();
                //var mesh = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(CEngine.Instance.RenderContext, value, true);
                //if (mesh == null)
                //    return;
                //mesh.PreUse(true);
                //if (mMesh == value)
                //    return;
                //mMesh = value;

                //var vb = mesh.GeometryMesh.GetVertexBuffer(EVertexSteamType.VST_Position);
                //var posBlob = new Support.CBlobObject();
                //Action action = async () =>
                //{
                //    await vb.GetBufferData(CEngine.Instance.RenderContext, posBlob);

                //    unsafe
                //    {
                //        SDK_GfxParticleEmitterShapeMesh_SetPoints(CoreObject, (Vector3*)posBlob.Data, (int)posBlob.Size / sizeof(Vector3));
                //    }
                //};

                //action();
            }
        }
        
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe float SDK_GfxParticleEmitterShapeMesh_SetPoints(NativePointer self, Vector3* points, int num);
        #endregion
    }
}
