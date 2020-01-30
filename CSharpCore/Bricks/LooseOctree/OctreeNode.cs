using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;
using EngineNS.IO;

namespace EngineNS.LooseOctree
{
    // 对象存储为位置簇（x_y_z.dat）
    [Rtti.MetaClass]
    public class OctreeNode : IO.Serializer.Serializer, GamePlay.SceneGraph.ISceneNode, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        // 0, 1
        // 3, 2
        public enum enNodeType
        {
            T_LT = 0,
            T_RT = 1,
            T_RB = 2,
            T_LB = 3,
            B_LT = 4,
            B_RT = 5,
            B_RB = 6,
            B_LB = 7,
        }

        public enum enNeighbour
        {
            Top = 0,        // Y
            Right = 1,      // X
            Bottom = 2,     // -Y
            Left = 3,       // -X
            Front = 4,      // Z
            Back = 5,       // -Z
        }
        
        BoundingBox mOrigionAABB;
        [Rtti.MetaData]
        [Browsable(false)]
        public BoundingBox OrigionAABB
        {
            get => mOrigionAABB;
            set
            {
                mOrigionAABB = value;
            }
        }

        BoundingBox mExtendAABB;
        [Rtti.MetaData]
        [Browsable(false)]
        public BoundingBox ExtendAABB
        {
            get => mExtendAABB;
            set
            {
                mExtendAABB = value;
            }
        }

        public OctreeNode()
        {

        }
        public OctreeNode(OctreeNode parent, GamePlay.SceneGraph.GSceneGraph scene)
        {
            HostSceneGraph = scene;
            Parent = parent;
        }

        public void SetOrigionBoundBox(ref BoundingBox aabb)
        {
            mOrigionAABB = aabb;
        }

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

        // 只有叶子才会有对象
        [Browsable(false)]
        public Thread.Async.TSafeDictionary<Guid, GActor> Actors
        {
            get;
            private set;
        } = new Thread.Async.TSafeDictionary<Guid, GActor>();
        public Thread.Async.TSafeDictionary<Guid, GActor> UnPVSActors
        {
            get;
            private set;
        } = new Thread.Async.TSafeDictionary<Guid, GActor>();
        [Browsable(false)]
        public List<GamePlay.SceneGraph.GScenePortal> Portals
        {
            get;
        }
        public bool Contain(ref Vector3 pos)
        {
            return (ExtendAABB.Contains(ref pos) == ContainmentType.Contains);
        }
        public ContainmentType Contain(ref BoundingBox aabb)
        {
            return ExtendAABB.Contains(ref aabb);
        }
        public bool LineCheck(ref Vector3 start, ref Vector3 end, ref GamePlay.SceneGraph.VHitResult rst)
        {
            return false;
        }

        // 高2位预留, 低30位记录节点ID, 树结构最大展开为10层,10层全部展开为(512*512*512个叶子)
        //UInt32 mIndex;
        Byte mDepth;
        public Byte Depth
        {
            get => mDepth;
        }
        public void SetDepth(Byte depth)
        {
            mDepth = depth;
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
        public static UInt64 GetParentId(UInt32 id)
        {
            UInt32 lvlIdx = id &  0xc0000000;
            UInt32 tempIdx = id & 0x3fffffff;
            return lvlIdx + (tempIdx >> 3);
        }
        public static UInt64 GetChildId(UInt64 id, enNodeType nodeType)
        {
            UInt64 lvlIdx = id &  0xc0000000;
            UInt64 tempIdx = id & 0x3fffffff;
            return lvlIdx + (tempIdx << 3) + (UInt64)nodeType;
        }

        public static Profiler.TimeScope ScopeCheckVisitNode = Profiler.TimeScopeManager.GetTimeScope(typeof(OctreeNode), "CheckVisitNode");
        public void CheckVisible(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param, bool checkFrustum)
        {
            if (Visible == false)
                return;

            var cullingFrustum = camera.CullingFrustum;
            using (new Profiler.TimeScopeWindows(ScopeCheckVisitNode))
            {
                Thread.Async.TSafeDictionary<Guid, GActor> actorsDic;
                if (param.UsePVS)
                    actorsDic = UnPVSActors;
                else
                    actorsDic = Actors;
                using (var it = actorsDic.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var actor = it.Current.Value;

                        if (actor.NeedCheckVisible(param) == false)
                            continue;
                        if (param.FrustumCulling && actor.CheckContain(cullingFrustum, checkFrustum) == CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                            continue;

                        actor.OnCheckVisible(cmd, HostSceneGraph, camera, param);
                    }
                }
            
                CGfxCamera.CFrustum.CONTAIN_TYPE containType = CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_INNER;
                if (checkFrustum)
                    containType = cullingFrustum.WhichContainType(ref mOrigionAABB, (ChildrenNode.Count > 0));
                switch(containType)
                {
                    case CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                        return;
                    case CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_INNER:
                        checkFrustum = false;
                        break;
                    case CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_REFER:
                        {
                            checkFrustum = true;
                        }
                        break;
                }
            }
            for (int i=0; i<ChildrenNode.Count; i++)
            {
                ChildrenNode[i].CheckVisible(cmd, camera, param, checkFrustum);
            }

            if (ShowBoundingBox || ShowExtBoundingBox)
            {
                var noUse = ShowDrawMesh(cmd, camera, param);
            }
        }
        public void SlowDrawAll(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param)
        {
            if (Visible == false)
                return;

            var cullingFrustum = camera.CullingFrustum;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    GActor actor = i.Current.Value;
                    actor.OnCheckVisible(cmd, HostSceneGraph, camera, param);
                }
            }
            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                ChildrenNode[i].SlowDrawAll(cmd, camera, param);
            }
        }
        async Task ShowDrawMesh(CCommandList cmd, CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (ShowBoundingBox)
            {
                if (mBoundingBoxLineMeshComponent == null)
                    await InitDrawMesh();
                mBoundingBoxLineMeshComponent?.CommitVisual(cmd, camera, param);
            }
            if(ShowExtBoundingBox)
            {
                if (mExtBoundingBoxLineMeshComponent == null)
                    await InitExtDrawMesh();
                mExtBoundingBoxLineMeshComponent?.CommitVisual(cmd, camera, param);
            }
        }

        public async System.Threading.Tasks.Task<bool> BuildTree(List<GActor> allActors, List<EngineNS.GamePlay.Actor.GActor> unPVSActors, OctreeDesc desc)
        {
            Actors.Clear();
            if (allActors.Count <= desc.MaxActorsCount || Depth >= desc.MaxDepth)
            {
                // 不能再拆分
                for(int i=0; i<allActors.Count; i++)
                {
                    var actor = allActors[i];
                    Actors.Add(actor.ActorId, actor);
                }
                for(int i=0; i<unPVSActors.Count; i++)
                {
                    var actor = unPVSActors[i];
                    UnPVSActors.Add(actor.ActorId, actor);
                }
            }
            else
            {
                var size = mOrigionAABB.Maximum - mOrigionAABB.Minimum;
                if(System.Math.Max(System.Math.Max(size.X, size.Y), size.Z) < desc.MinSide)
                {
                    // 不能再拆分
                    for (int i = 0; i < allActors.Count; i++)
                    {
                        var actor = allActors[i];
                        Actors.Add(actor.ActorId, actor);
                    }
                    for(int i=0; i<unPVSActors.Count; i++)
                    {
                        var actor = unPVSActors[i];
                        UnPVSActors.Add(actor.ActorId, actor);
                    }
                }
                else
                {
                    ChildrenNode.Clear();
                    // 拆分并计算归属
                    var corners = mOrigionAABB.GetCorners();
                    var center = mOrigionAABB.GetCenter();
                    for(int i=0; i<8; i++)
                    {
                        var childNode = new OctreeNode(this, HostSceneGraph);
                        childNode.Name = "OctreeNode_" + i;
                        childNode.SetDepth((byte)(Depth + 1));
                        var min = new Vector3();
                        min.X = System.Math.Min(corners[i].X, center.X);
                        min.Y = System.Math.Min(corners[i].Y, center.Y);
                        min.Z = System.Math.Min(corners[i].Z, center.Z);
                        var max = new Vector3();
                        max.X = System.Math.Max(corners[i].X, center.X);
                        max.Y = System.Math.Max(corners[i].Y, center.Y);
                        max.Z = System.Math.Max(corners[i].Z, center.Z);
                        childNode.mOrigionAABB = new BoundingBox(min, max);

                        var childActors = new List<GActor>();
                        for(int actorIdx=allActors.Count - 1; actorIdx >= 0; actorIdx--)
                        {
                            var actor = allActors[actorIdx];
                            BoundingBox actorAABB = new BoundingBox();
                            actor.GetAABB(ref actorAABB);

                            if (BoundingBox.Intersects(ref childNode.mOrigionAABB, ref actorAABB))
                            {
                                childActors.Add(actor);
                            }
                        }
                        var childUnPVSActors = new List<GActor>();
                        for(int actorIdx=unPVSActors.Count - 1; actorIdx >= 0; actorIdx--)
                        {
                            var actor = unPVSActors[actorIdx];
                            var actorAABB = new BoundingBox();
                            actor.GetAABB(ref actorAABB);

                            if(BoundingBox.Intersects(ref childNode.mOrigionAABB, ref actorAABB))
                            {
                                childUnPVSActors.Add(actor);
                            }
                        }
                        // 内部对象为0则不加这个子节点
                        if(childActors.Count != 0)
                        {
                            await childNode.BuildTree(childActors, childUnPVSActors, desc);
                            ChildrenNode.Add(childNode);
                        }
                        else if(childActors.Count == allActors.Count)
                        {
                            // 子与父对象相同，不再进行拆分
                            ChildrenNode.Clear();
                            for(int actIdx=0; actIdx < allActors.Count; actIdx++)
                            {
                                var actor = allActors[actIdx];
                                Actors.Add(actor.ActorId, actor);
                            }
                            for(int actIdx = 0; actIdx < unPVSActors.Count; actIdx++)
                            {
                                var actor = unPVSActors[actIdx];
                                UnPVSActors.Add(actor.ActorId, actor);
                            }
                            break;
                        }
                    }
                }
            }
            // 根据Actors和子节点计算扩展包围盒
            mExtendAABB = mOrigionAABB;
            foreach(var actorRef in Actors)
            {
                GActor actor = actorRef.Value;
                if(actor != null)
                {
                    BoundingBox actorAABB = new BoundingBox();
                    actor.GetAABB(ref actorAABB);
                    mExtendAABB.Merge2(ref actorAABB, ref mExtendAABB);
                }
            }
            foreach(var child in ChildrenNode)
            {
                var childNode = child as OctreeNode;
                var childNodeAABB = childNode.ExtendAABB;
                mExtendAABB.Merge2(ref childNodeAABB, ref mExtendAABB);
            }

            return true;
        }

        public bool AddActor(GActor actor)
        {
            var actorAABB = new BoundingBox();
            actor.GetAABB(ref actorAABB);
            if (ExtendAABB.Contains(ref actorAABB) == ContainmentType.Disjoint)
                return false;

            if(ChildrenNode.Count == 0)
            {
                // 叶子节点
                if (Actors.ContainsKey(actor.ActorId))
                    return false;

                Actors[actor.ActorId] = actor;
                return true;
            }
            else
            {
                bool retValue = false;
                for(int i=0; i<ChildrenNode.Count; i++)
                {
                    var childNode = ChildrenNode[i] as OctreeNode;
                    retValue = retValue || childNode.AddActor(actor);
                }

                return retValue;
            }
        }

        public bool RemoveActor(GActor actor)
        {
            var actorAABB = new BoundingBox();
            actor.GetAABB(ref actorAABB);
            if (ExtendAABB.Contains(ref actorAABB) == ContainmentType.Disjoint)
                return false;

            if(ChildrenNode.Count == 0)
            {
                // 叶子节点
                return Actors.Remove(actor.ActorId);
            }
            else
            {
                bool retValue = false;
                for(int i=0; i<ChildrenNode.Count; i++)
                {
                    var childNode = ChildrenNode[i] as OctreeNode;
                    var result = childNode.RemoveActor(actor);
                    retValue = retValue || result;
                }

                return retValue;
            }
        }

        public async Task<bool> Save2Xnd(XndNode node)
        {
            var headAtt = node.AddAttrib("Head");
            headAtt.BeginWrite();
            headAtt.WriteMetaObject(this);
            headAtt.EndWrite();

            var actorIdsAtt = node.AddAttrib("ActorIds");
            actorIdsAtt.BeginWrite();
            actorIdsAtt.Write((int)(Actors.Count));
            foreach (var actId in Actors.Keys)
            {
                actorIdsAtt.Write(actId);
            }
            actorIdsAtt.EndWrite();

            var unPvsActorIdsAtt = node.AddAttrib("UnPVSActorIds");
            unPvsActorIdsAtt.BeginWrite();
            unPvsActorIdsAtt.Write((int)(UnPVSActors.Count));
            foreach(var actId in UnPVSActors.Keys)
            {
                unPvsActorIdsAtt.Write(actId);
            }
            unPvsActorIdsAtt.EndWrite();

            var graphNode = node.AddNode("GraphNodes", 0, 0);
            foreach (var child in ChildrenNode)
            {
                var ts = Rtti.RttiHelper.GetTypeSaveString(child.GetType());
                var childNode = graphNode.AddNode(ts, 0, 0);
                await child.Save2Xnd(childNode);
            }

            return true;
        }

        public async Task<bool> LoadXnd(CRenderContext rc, XndNode node)
        {
            var headAtt = node.FindAttrib("Head");
            headAtt.BeginRead();
            headAtt.ReadMetaObject(this);
            headAtt.EndRead();

            var actorIdsAtt = node.FindAttrib("ActorIds");
            actorIdsAtt.BeginRead();
            int count;
            actorIdsAtt.Read(out count);
            for(int i=0; i<count; i++)
            {
                Guid actorId;
                actorIdsAtt.Read(out actorId);
                var actor = HostSceneGraph.FindActor(actorId);
                if (actor == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Actor", $"SceneNode can't find actor with id {actorId}");
                }
                else
                {
                    Actors[actorId] = actor;
                }
            }
            actorIdsAtt.EndRead();

            var unPVSActorIdsAtt = node.FindAttrib("UnPVSActorIds");
            if(unPVSActorIdsAtt != null)
            {
                unPVSActorIdsAtt.BeginRead();
                int unPVSActorsCount;
                unPVSActorIdsAtt.Read(out unPVSActorsCount);
                for (int i = 0; i < unPVSActorsCount; i++)
                {
                    Guid actorId;
                    unPVSActorIdsAtt.Read(out actorId);
                    var actor = HostSceneGraph.FindActor(actorId);
                    if (actor == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Actor", $"SceneNode can't find actor with id {actorId}");
                    }
                    else
                    {
                        UnPVSActors[actorId] = actor;
                    }
                }
                unPVSActorIdsAtt.EndRead();
            }

            var graphNode = node.FindNode("GraphNodes");
            foreach (var childNode in graphNode.GetNodes())
            {
                var ts = childNode.GetName();
                var type = Rtti.RttiHelper.GetTypeFromSaveString(ts);
                var child = System.Activator.CreateInstance(type) as ISceneNode;
                child.Parent = this;
                child.HostSceneGraph = this.HostSceneGraph;
                await child.LoadXnd(rc, childNode);
                ChildrenNode.Add(child);
            }

            return true;
        }

        #region 包围盒绘制

        EngineNS.Bricks.GraphDrawer.GraphLines mBoundingGraphLine;
        GamePlay.Component.GMeshComponent mBoundingBoxLineMeshComponent;
        EngineNS.Bricks.GraphDrawer.GraphLines mExtBoundingGraphLine;
        GamePlay.Component.GMeshComponent mExtBoundingBoxLineMeshComponent;

        public bool ShowBoundingBox
        {
            get;
            set;
        } = false;

        bool mInitializingDrawMesh = false;
        async Task InitDrawMesh()
        {
            if (mInitializingDrawMesh)
                return;
            mInitializingDrawMesh = true;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            var mtlInst = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, CEngineDesc.OctreeMaterialName);
            var gen = new Bricks.GraphDrawer.McBoxGen()
            {
                Interval = 0f,
                Segement = 5f,
            };
            gen.SetBox(new Vector3(0, 0, 0), 1, 1, 1);
            mBoundingGraphLine = new Bricks.GraphDrawer.GraphLines();
            mBoundingGraphLine.LinesGen = gen;
            mBoundingGraphLine.UseGeometry = true;
            await mBoundingGraphLine.Init(mtlInst, 0);
            mBoundingBoxLineMeshComponent = mBoundingGraphLine.GraphActor.GetComponent<GamePlay.Component.GMeshComponent>();
            var mat = Matrix.Transformation(mOrigionAABB.GetSize(), Quaternion.Identity, mOrigionAABB.GetCenter());
            mBoundingBoxLineMeshComponent.OnUpdateDrawMatrix(ref mat);

            mInitializingDrawMesh = false;
        }

        public bool ShowExtBoundingBox
        {
            get;
            set;
        } = false;

        bool mInitializingExtDrawMesh = false;
        async Task InitExtDrawMesh()
        {
            if (mInitializingExtDrawMesh)
                return;
            mInitializingExtDrawMesh = true;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            var mtlInst = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, CEngineDesc.OctreeMaterialName);
            var gen = new Bricks.GraphDrawer.McBoxGen()
            {
                Interval = 0f,
                Segement = 5f,
            };
            gen.SetBox(new Vector3(0, 0, 0), 1, 1, 1);
            mExtBoundingGraphLine = new Bricks.GraphDrawer.GraphLines();
            mExtBoundingGraphLine.LinesGen = gen;
            mExtBoundingGraphLine.UseGeometry = true;
            await mExtBoundingGraphLine.Init(mtlInst, 0);
            mExtBoundingBoxLineMeshComponent = mExtBoundingGraphLine.GraphActor.GetComponent<GamePlay.Component.GMeshComponent>();
            var mat = Matrix.Transformation(mExtendAABB.GetSize(), Quaternion.Identity, mExtendAABB.GetCenter());
            mExtBoundingBoxLineMeshComponent.OnUpdateDrawMatrix(ref mat);

            mInitializingExtDrawMesh = false;
        }

        #endregion
    }
}
