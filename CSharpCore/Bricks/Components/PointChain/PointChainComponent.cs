using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;

namespace EngineNS.Bricks.Componets.PointChain
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(PointChainComponentInitializer), "PointChainComponent", "ElementsGroup", "PointChainComponent")]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    public class PointChainComponent : GamePlay.Component.GComponent
    {
        [Rtti.MetaClassAttribute]
        public class PointChainComponentInitializer : GamePlay.Component.GComponent.GComponentInitializer
        {
            [Rtti.MetaData]
            public List<PointElement> Elements
            {
                get;
                set;
            } = new List<PointElement>();
        }
        private PointChainComponentInitializer PCInitializer
        {
            get
            {
                return Initializer as PointChainComponentInitializer;
            }
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            return true;
        }
        [Rtti.MetaClassAttribute]
        public class PointElement : IO.Serializer.Serializer
        {
            [Rtti.MetaData]
            public Vector3 Position
            {
                get;
                set;
            }
            public GamePlay.Actor.HitProxyManager.ActorProxy ShowActor = null;
        }
        public List<PointElement> Elements
        {
            get
            {
                if (PCInitializer == null)
                    return null;
                return PCInitializer.Elements;
            }
        }
        RName mElementMesh;
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Mesh)]
        public RName ElementMesh
        {
            get { return mElementMesh; }
            set
            {
                mElementMesh = value;
            }
        }
        private bool SetCompleted = true;
        private bool mShowEditorVisual;
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool ShowEditorVisual
        {
            get { return mShowEditorVisual; }
            set
            {
                if (SetCompleted == false)
                    return;
                mShowEditorVisual = value;
                if (value)
                {
                    SetCompleted = false;
                    CEngine.Instance.EventPoster.RunOn(async () =>
                    {
                        for (int i = 0; i < Elements.Count; i++)
                        {
                            if (Elements[i].ShowActor != null)
                            {
                                CEngine.Instance.HitProxyManager.ReleaseActor(Elements[i].ShowActor);
                            }
                            var ElementShowMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(CEngine.Instance.RenderContext, mElementMesh);
                            Elements[i].ShowActor = CEngine.Instance.HitProxyManager.QueryActor(ElementShowMesh);
                            Elements[i].ShowActor.SetTarget(Elements[i]);
                            Elements[i].ShowActor.Actor.Placement.Location = Elements[i].Position;
                        }
                        SetCompleted = true;
                        return true;
                    }, Thread.Async.EAsyncTarget.Logic);
                    
                }
                else
                {
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        if (Elements[i].ShowActor != null)
                        {
                            Elements[i].Position = Elements[i].ShowActor.Actor.Placement.Location;
                            CEngine.Instance.HitProxyManager.ReleaseActor(Elements[i].ShowActor);
                            Elements[i].ShowActor = null;
                        }
                    }
                }
            }
        }
        public override void OnEditorCommitVisual(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param)
        {
            if (ShowEditorVisual == false)
                return;
            var scene = this.Host.Scene;
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].ShowActor?.Actor.OnCheckVisible(cmd, scene, camera, param);
            }
            base.OnEditorCommitVisual(cmd, camera, param);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void AddElement(Vector3 offset)
        {
            var elem = new PointElement();
            elem.Position = Host.Placement.Location + offset;
            Elements.Add(elem);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void RemoveElement(int index)
        {
            if (index < 0 || index >= Elements.Count)
                return;
            CEngine.Instance.HitProxyManager.ReleaseActor(Elements[index].ShowActor);
            Elements.RemoveAt(index);
        }
        int AddIndex = 1;
        public bool Test_AddElements
        {
            get { return true; }
            set
            {
                var offset = new Vector3(5 * AddIndex, 0, 0);
                AddIndex++;
                AddElement(offset);
            }
        }
    }
}
