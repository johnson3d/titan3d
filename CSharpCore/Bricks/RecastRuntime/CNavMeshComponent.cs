using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace EngineNS.Bricks.RecastRuntime
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(CNavMeshComponentInitializer), "导航网格", "Navigation", "NavMeshComponent")]
    public class CNavMeshComponent : GamePlay.Component.GComponent
    {
        [Rtti.MetaClassAttribute]
        public class CNavMeshComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public RName NavName
            {
                get;
                set;
            }
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);

            var nvInit = v as CNavMeshComponentInitializer;

            if (nvInit.NavName != null)
            {
                await CEngine.Instance.EventPoster.Post(() =>
                {
                    using (var xnd = IO.XndHolder.SyncLoadXND(nvInit.NavName.Address))
                    {
                        NavMesh = new CNavMesh();
                        NavMesh.LoadXnd(xnd.Node);
                    }
                    return true;
                }, Thread.Async.EAsyncTarget.AsyncIO);
            }

            if (NavMesh != null)
            {
                var meshComp = Host.FindComponentBySpecialName("NavMeshDebugger") as GamePlay.Component.GMeshComponent;
                if (meshComp == null)
                {
                    //meshComp = new GamePlay.Component.GMeshComponent();
                    //var renderMesh = CEngine.Instance.MeshManager.CreateMesh(rc, NavMesh.CreateRenderMesh(rc));
                    //var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, RName.GetRName("Material/RecastDebugger.instmtl"));
                    //await renderMesh.SetMaterial(rc, 0, mtl, null);
                    //meshComp.SetSceneMesh(rc, renderMesh);

                    //host.AddComponent(meshComp);
                    //meshComp.SpecialName = "NavMeshDebugger";
                }
            }

            return true;
        }
        public CNavMesh NavMesh
        {
            get;
            set;
        }
        
    }
}
