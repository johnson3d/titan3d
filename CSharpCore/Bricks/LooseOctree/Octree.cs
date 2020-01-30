using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;

namespace EngineNS.LooseOctree
{
    [Rtti.MetaClass]
    public class OctreeDesc : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        [DisplayName("最大深度")]
        public int MaxDepth
        {
            get;
            set;
        } = 10;

        [Rtti.MetaData]
        [DisplayName("最大对象数量")]
        public int MaxActorsCount
        {
            get;
            set;
        } = 20;

        [Rtti.MetaData]
        [DisplayName("最小边长")]
        public float MinSide
        {
            get;
            set;
        } = 30.0f;
    }

    [Rtti.MetaClass]
    public class Octree : IO.Serializer.Serializer, GamePlay.SceneGraph.ISceneNode, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [Browsable(false)]
        OctreeNode mRoot;
        [Browsable(false)]
        public OctreeNode Root => mRoot;

        string mName;
        [Rtti.MetaData]
        public string Name
        {
            get => mName;
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }

        [Browsable(false)]
        [Rtti.MetaData]
        public Guid LinkedActorId;

        [Browsable(false)]
        public Thread.Async.TSafeDictionary<Guid, GActor> Actors
        {
            get;
        } = new Thread.Async.TSafeDictionary<Guid, GActor>();

        [Browsable(false)]
        public List<GScenePortal> Portals
        {
            get;
        }

        [Browsable(false)]
        public GSceneGraph HostSceneGraph
        {
            get;
            set;
        }

        [Browsable(false)]
        public ISceneNode Parent
        {
            get;
            set;
        }

        [Browsable(false)]
        public List<ISceneNode> ChildrenNode
        {
            get;
        } = new List<ISceneNode>();
        bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set
            {
                mVisible = value;
                OnPropertyChanged("Visible");
            }
        }

        public bool Contain(ref Vector3 pos)
        {
            return mRoot.Contain(ref pos);
        }
        public ContainmentType Contain(ref BoundingBox aabb)
        {
            if (mRoot == null)
                return ContainmentType.Disjoint;
            return mRoot.Contain(ref aabb);
        }

        public bool LineCheck(ref Vector3 start, ref Vector3 end, ref VHitResult rst)
        {
            if(mRoot != null)
                return mRoot.LineCheck(ref start, ref end, ref rst);
            return false;
        }

        public async System.Threading.Tasks.Task<bool> BuildTree(OctreeVolumeComponent octreeComp, List<EngineNS.GamePlay.Actor.GActor> allActors, List<EngineNS.GamePlay.Actor.GActor> unPVSActors, List<EngineNS.GamePlay.Actor.GActor> containActors)
        {
            ChildrenNode.Clear();
            mRoot = new OctreeNode(null, this.HostSceneGraph);
            mRoot.Name = "Root";
            BoundingBox aabb = new BoundingBox();
            octreeComp.Host.GetAABB(ref aabb);
            mRoot.SetOrigionBoundBox(ref aabb);
            mRoot.SetDepth(0);
            var compInit = octreeComp.Initializer as OctreeVolumeComponent.OctreeVolumeComponentInitializer;
            foreach (var actor in allActors)
            {
                var actorAABB = BoundingBox.EmptyBox();
                actor.GetAABB(ref actorAABB);
                if (BoundingBox.Intersects(ref aabb, ref actorAABB))
                {
                    containActors.Add(actor);
                }
            }
            mRoot.Parent = this;
            ChildrenNode.Add(mRoot);
            return await mRoot.BuildTree(allActors, unPVSActors, compInit.Desc);
        }
        public static Profiler.TimeScope ScopeCheckVisible = Profiler.TimeScopeManager.GetTimeScope(typeof(Octree), nameof(CheckVisible));
        public void CheckVisible(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param, bool checkFrustum)
        {
            if (mRoot == null)
                return;
            if (Visible == false)
                return;
            
            var extAABB = mRoot.ExtendAABB;
            var sceneGraphMat = HostSceneGraph.SceneGraphTransMat;
            var result = camera.CullingFrustum.WhichContainTypeWithTransform(ref extAABB, ref sceneGraphMat, true);
            switch(result)
            {
                case CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                    return;
                case CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_INNER:
                    checkFrustum = false;
                    break;
                case CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_REFER:
                    checkFrustum = true;
                    break;
            }

            ScopeCheckVisible.Begin();
            mRoot.CheckVisible(cmd, camera, param, checkFrustum);
            ScopeCheckVisible.End();
        }

        public void SlowDrawAll(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param)
        {
            if (mRoot == null)
                return;
            if (Visible == false)
                return;

            mRoot.SlowDrawAll(cmd, camera, param);
        }

        public bool AddActor(GActor actor)
        {
            return mRoot.AddActor(actor);
        }

        public bool RemoveActor(GActor actor)
        {
            return mRoot.RemoveActor(actor);
        }

        public async Task<bool> Save2Xnd(IO.XndNode node)
        {
            var att = node.AddAttrib("Head");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();

            if(mRoot != null)
            {
                var childNode = node.AddNode("Root", 0, 0);
                await mRoot.Save2Xnd(childNode);
            }

            return true;
        }

        public async Task<bool> LoadXnd(CRenderContext rc, IO.XndNode node)
        {
            var att = node.FindAttrib("Head");
            att.BeginRead();
            att.ReadMetaObject(this);
            att.EndRead();

            var rootNode = node.FindNode("Root");
            if(rootNode != null)
            {
                mRoot = new OctreeNode(null, HostSceneGraph);
                mRoot.Parent = this;
                mRoot.HostSceneGraph = this.HostSceneGraph;
                await mRoot.LoadXnd(rc, rootNode);
                ChildrenNode.Add(mRoot);
            }

            // 不应该找不到，所以这里不判断空
            var actor = HostSceneGraph.FindActor(LinkedActorId);
            var octreeComp = actor.GetComponent<OctreeVolumeComponent>();
            if(octreeComp!=null)
                octreeComp.SetActorOctree(this);

            return true;
        }


        //public void Cleanup()
        //{
        //    mRoot = null;
        //}
        //public LoadOctree()
    }
}
