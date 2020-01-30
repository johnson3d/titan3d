using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.View;

namespace EngineNS.GamePlay.Component
{
   // [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GamePlay.Component.GComponent.GComponentInitializer), "模型组组件", "Mesh", "MutiMeshComponent")]
    public class GMutiMeshComponent : GVisualComponent, IComponentHostSelectOperation
    {
        public GMutiMeshComponent()
        {
            OnlyForGame = false;
        }
        private Dictionary<RName, Graphics.Mesh.CGfxMesh> mMeshes = new Dictionary<RName, Graphics.Mesh.CGfxMesh>(new RName.EqualityComparer());
        public Dictionary<RName, CGfxMesh> Meshes { get => mMeshes; }

        protected Dictionary<RName, Graphics.Mesh.CGfxMtlMesh[]> mRLAtomArray = new Dictionary<RName, Graphics.Mesh.CGfxMtlMesh[]>();

        public bool SetMaterialInstance(CRenderContext RHICtx, RName mesh, UInt32 index, Graphics.CGfxMaterialInstance material, Graphics.CGfxShadingEnv[] senv, bool BuildTechnique = false)
        {
            if (!mMeshes.ContainsKey(mesh))
                return false;
            var subMesh = mMeshes[mesh];
            if (index >= mMeshes[mesh].MtlMeshArray.Length)
                return false;
            subMesh.SetMaterialInstance(RHICtx, index, material, senv);
            //if (BuildTechnique)
            //{
            //    subMesh.MtlMeshArray[index].RefreshEffects(RHICtx);
            //}

            return true;
        }

        public void AddSubMesh(CRenderContext RHICtx, Graphics.Mesh.CGfxMesh value)
        {
            //重复添加是要刷新
            if (mMeshes.ContainsKey(value.Name))
            {
                mMeshes.Remove(value.Name);
            }
            if (mRLAtomArray.ContainsKey(value.Name))
            {
                mRLAtomArray.Remove(value.Name);
            }

            mMeshes.Add(value.Name, value);
            //mPassArray = new CPass[mSceneMesh.mMtlMeshArray.Length];
            mRLAtomArray.Add(value.Name, new Graphics.Mesh.CGfxMtlMesh[value.MtlMeshArray.Length]);


            for (UInt32 i = 0; i < value.MtlMeshArray.Length; i++)
            {
                var MtlMesh = value.MtlMeshArray[i];
                if (MtlMesh == null || MtlMesh.MtlInst == null)
                    continue;

                for (int MPI = 0; MPI < (int)PrebuildPassIndex.PPI_Gizmos; MPI++)
                {
                    var refPass = MtlMesh.GetPass((PrebuildPassIndex)MPI);
                    if (refPass == null)
                        continue;

                    var refPrebuildPassData = CEngine.Instance.PrebuildPassData;

                    switch (MtlMesh.MtlInst.mRenderLayer)
                    {
                        case ERenderLayer.RL_Opaque:
                            {
                                refPass.RenderPipeline.RasterizerState = refPrebuildPassData.mOpaqueRasterStat;
                                refPass.RenderPipeline.DepthStencilState = refPrebuildPassData.mOpaqueDSStat;
                                refPass.RenderPipeline.BlendState = refPrebuildPassData.mOpaqueBlendStat;
                            }
                            break;
                        case ERenderLayer.RL_Translucent:
                            {
                                refPass.RenderPipeline.RasterizerState = refPrebuildPassData.mTranslucentRasterStat;
                                refPass.RenderPipeline.DepthStencilState = refPrebuildPassData.mTranslucentDSStat;
                                refPass.RenderPipeline.BlendState = refPrebuildPassData.mTranslucentBlendStat;
                            }
                            break;
                        case ERenderLayer.RL_CustomOpaque:
                            {
                                refPass.RenderPipeline.RasterizerState = MtlMesh.MtlInst.CustomRasterizerState;
                                refPass.RenderPipeline.DepthStencilState = MtlMesh.MtlInst.CustomDepthStencilState;
                                refPass.RenderPipeline.BlendState = refPrebuildPassData.mOpaqueBlendStat;
                            }
                            break;
                        case ERenderLayer.RL_CustomTranslucent:
                        case ERenderLayer.RL_Gizmos:
                        case ERenderLayer.RL_Sky:
                            {
                                refPass.RenderPipeline.RasterizerState = MtlMesh.MtlInst.CustomRasterizerState;
                                refPass.RenderPipeline.DepthStencilState = MtlMesh.MtlInst.CustomDepthStencilState;
                                refPass.RenderPipeline.BlendState = MtlMesh.MtlInst.CustomBlendState;
                            }
                            break;
                    }

                }

                MtlMesh.MtlInstVersion = MtlMesh.MtlInst.Version;
            }
        }
        private async System.Threading.Tasks.Task UpdateSceneMeshForEditerMode(CCommandList cmd)
        {
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                var rc = CEngine.Instance.RenderContext;
                var it = mMeshes.GetEnumerator();
                while (it.MoveNext())
                {
                    var subMesh = it.Current.Value;
                    if (await subMesh.UpdatedOrigionMesh(rc))
                    {
                        AddSubMesh(rc, subMesh);
                        return;
                    }
                    else
                    {
                        for (UInt32 i = 0; i < subMesh.MtlMeshArray.Length; i++)
                        {
                            var MtlMesh = subMesh.MtlMeshArray[i];
                            if (MtlMesh == null)
                            {
                                continue;
                            }

                            //if(MtlMesh.MeshPassShaderProgram != MtlMesh.Effect.mShaderProgram)
                            //{
                            //    //MtlMesh.UpdateMaterialCBuffer(rc, true);
                            //    SetSceneMesh(rc, _mSceneMesh);
                            //    return;
                            //}
                            /*else */
                            if (MtlMesh.MtlInstVersion != MtlMesh.MtlInst.Version)
                            {
                                //MtlMesh.UpdateMaterialCBuffer(rc, true);
                                AddSubMesh(rc, subMesh);
                                //MtlMesh.mNeedToRefreshCBOnly = true;
                                return;
                            }

                        }
                    }
                }
                it.Dispose();
            }
        }
        public bool IsIdentityDrawTransform
        {
            get;
            set;
        } = false;
        public void CookSceneMeshToRenderLayer(CCommandList cmd,Graphics.CGfxCamera Camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (mRLAtomArray == null)
            {
                return;
            }

            var noUsed = UpdateSceneMeshForEditerMode(cmd);
            var it = mMeshes.GetEnumerator();
            while (it.MoveNext())
            {

                var subMesh = it.Current.Value;
                var subMeshName = it.Current.Key;

                subMesh.UpdatePerSceneMeshVars(cmd, Host.HitProxyId, Host.PVSId, Camera);
                subMesh.MeshPrimitives.PreUse(false);
                if (subMesh.MeshPrimitives.ResourceState.StreamState != EStreamingState.SS_Valid)
                {
                    return;
                }

                for (UInt32 index = 0; index < subMesh.MtlMeshArray.Length; index++)
                {
                    var MtlMesh = subMesh.MtlMeshArray[index];
                    if (MtlMesh == null)
                    {
                        continue;
                    }

                    //MtlMesh.UpdateMaterialCBuffer(RHICtx, null);
                    mRLAtomArray[subMeshName][index] = MtlMesh;
                    //mRLAtomArray[subMeshName][index].mSceneMesh = subMesh;

                    if (Camera != null)
                    {
                        if (MtlMesh.MtlInst.mRenderLayer == ERenderLayer.RL_Num)
                        {
                            MtlMesh.MtlInst.mRenderLayer = ERenderLayer.RL_Num - 1;
                        }
                        Camera.mSceneRenderLayer[(int)MtlMesh.MtlInst.mRenderLayer].AddRenderLayerAtom(mRLAtomArray[subMeshName][index]);
                    }
                }
            }
            it.Dispose();
        }

        public override void CommitVisual(CCommandList cmd, Graphics.CGfxCamera Camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            CookSceneMeshToRenderLayer(cmd, Camera, param);
        }
        public override void OnAdded()
        {
            if (Host == null)
                return;

            var it = mMeshes.GetEnumerator();
            while (it.MoveNext())
            {
                var subMesh = it.Current.Value;
                var aabb = subMesh.MeshPrimitives.AABB;
                Host.LocalBoundingBox.Merge(ref aabb.Minimum);
                Host.LocalBoundingBox.Merge(ref aabb.Maximum);
            }
            it.Dispose();
            base.OnAdded();
        }

        public override void Tick(GPlacementComponent placement)
        {
            var it = mMeshes.GetEnumerator();
            while (it.MoveNext())
            {
                var subMesh = it.Current.Value;
                subMesh.MdfQueue.TickLogic(CEngine.Instance.RenderContext, subMesh, CEngine.Instance.EngineElapseTime);
            }
            it.Dispose();
            base.Tick(placement);
        }

        public void OnHostSelected(bool isSelect)
        {
            var it = mMeshes.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Value.mSelected = isSelect;
            }
            it.Dispose();
        }
    }
}
