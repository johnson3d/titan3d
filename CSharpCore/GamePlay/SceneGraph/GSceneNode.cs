using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.SceneGraph
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct VHitResult
    {
        public static VHitResult Default
        {
            get
            {
                return new VHitResult(Bricks.PhysicsCore.PhyHitFlag.eDEFAULT);
            }
        }
        public UInt32 HitFlags;
        public UInt32 GroupData;//物理分组

        public UInt32 Reserved;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float Distance;

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)] 
        public int FaceId;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float U;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float V;

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Vector3 Position;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Vector3 Normal;

        public IntPtr ExtData;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Actor.GActor Actor
        {
            get
            {
                if (ExtData == IntPtr.Zero)
                    return null;
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ExtData);
                var phyActor = handle.Target as Bricks.PhysicsCore.CPhyActor;
                if (phyActor == null)
                    return null;

                return phyActor.HostActor;
            }
        }
        public VHitResult(Bricks.PhysicsCore.PhyHitFlag hitFlags)
        {
            HitFlags = (UInt32)hitFlags;
            GroupData = 0;
            Reserved = 0;
            Distance = 0;
            FaceId = -1;
            U = 0;
            V = 0;
            Position = Vector3.Zero;
            Normal = Vector3.Zero;
            ExtData = IntPtr.Zero;

        }
    }

    public class CheckVisibleParam
    {
        public UInt64 SerialID = 1;

        public bool UsePVS = false;
        public bool FrustumCulling = true;
        public bool ForShadow = false;
        public float mShadowCullDistance = -1.0f;

        public void Reset()
        {
            UsePVS = false;
            FrustumCulling = true;
            ForShadow = false;
            mShadowCullDistance = -1.0f;
        }
    }

    public partial interface ISceneNode
    {
        Thread.Async.TSafeDictionary<Guid, Actor.GActor> Actors
        {
            get;
        }
        List<GScenePortal> Portals
        {
            get;
        }
        GSceneGraph HostSceneGraph
        {
            get;
            set;
        }
        ISceneNode Parent
        {
            get;
            set;
        }
        List<ISceneNode> ChildrenNode
        {
            get;
        }
        string Name
        {
            get;
            set;
        }
        bool Visible { get; set; }
        bool Contain(ref Vector3 pos);
        ContainmentType Contain(ref BoundingBox aabb);
        bool LineCheck(ref Vector3 start, ref Vector3 end, ref VHitResult rst);
        void CheckVisible(CCommandList cmd, Graphics.CGfxCamera camera, CheckVisibleParam param, bool checkFrustum);
        void SlowDrawAll(CCommandList cmd, Graphics.CGfxCamera camera, CheckVisibleParam param);
        bool AddActor(Actor.GActor actor);
        bool RemoveActor(Actor.GActor actor);

        System.Threading.Tasks.Task<bool> Save2Xnd(EngineNS.IO.XndNode node);
        System.Threading.Tasks.Task<bool> LoadXnd(EngineNS.CRenderContext rc, EngineNS.IO.XndNode node);
    }

    [Rtti.MetaClassAttribute]
    public class GSceneNode : IO.Serializer.Serializer, ISceneNode, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        [Browsable(false)]
        public Thread.Async.TSafeDictionary<Guid, Actor.GActor> Actors
        {
            get;
        } = new Thread.Async.TSafeDictionary<Guid, Actor.GActor>();
        [Browsable(false)]
        public List<GScenePortal> Portals
        {
            get;
        } = new List<GScenePortal>();
        [Browsable(false)]
        public GSceneGraph HostSceneGraph
        {
            get;
            set;
        }
        [Rtti.MetaData]
        public string Name
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
        public GSceneNode()
        {

        }
        public GSceneNode(GSceneGraph graph)
        {
            HostSceneGraph = graph;
        }

        public bool Contain(ref Vector3 pos)
        {
            return true;
        }
        public ContainmentType Contain(ref BoundingBox aabb)
        {
            return ContainmentType.Contains;
        }
        public virtual bool LineCheck(ref Vector3 start, ref Vector3 end, ref VHitResult rst)
        {
            if (rst.Distance == 0)
                rst.Distance = float.MaxValue;
            bool isChecked = false;
            using (var i = Actors.GetEnumerator())
            {
                while(i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    var mesh = actor.GetComponent<Component.GMeshComponent>();
                    if (mesh == null)
                        continue;
                    var tempRst = VHitResult.Default;
                    if (mesh.LineCheck(ref start, ref end, ref tempRst))
                    {
                        if (tempRst.Distance < rst.Distance)
                        {
                            isChecked = true;
                            rst = tempRst;
                        }
                    }
                }
            }

            return isChecked;
        }
        public virtual void CheckVisible(CCommandList cmd, Graphics.CGfxCamera camera, CheckVisibleParam SceneCullDesc, bool checkFrustum)
        {
            if (Visible == false)
                return;

            var cullingFrustum = camera.CullingFrustum;
            //foreach (var i in Actors.Values)
            using (var i = Actors.GetEnumerator())
            {
                while(i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (SceneCullDesc.FrustumCulling == true)
                    {
                        if (cullingFrustum.WhichContainType(ref actor.Placement.ActorAABB, false)
                                                == Graphics.CGfxCamera.CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                        {
                            continue;
                        }
                    }

                    if (SceneCullDesc.ForShadow == true)
                    {
                        var extend = actor.LocalBoundingBox.GetSize();
                        float radius = extend.Length() * 0.5f;
                        var ViewerCamPos = camera.CameraData.Position;
                        float Actor2CamDist = Vector3.Distance(ref ViewerCamPos, ref actor.Placement.mPlacementData.mLocation);
                        if (Actor2CamDist - radius > SceneCullDesc.mShadowCullDistance)
                        {
                            continue;
                        }
                    }

                    //var matrix = i.Value.Placement.WorldMatrix;

                    actor.OnCheckVisible(cmd, HostSceneGraph, camera, SceneCullDesc);
                }
            }

            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                ChildrenNode[i].CheckVisible(cmd, camera, SceneCullDesc, checkFrustum);
            }
        }
        public virtual void SlowDrawAll(CCommandList cmd, Graphics.CGfxCamera camera, CheckVisibleParam param)
        {
            if (Visible == false)
                return;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    actor.OnCheckVisible(cmd, HostSceneGraph, camera, param);
                }
            }
            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                ChildrenNode[i].SlowDrawAll(cmd, camera, param);
            }
        }
        public bool AddActor(Actor.GActor actor)
        {
            if (Actors.ContainsKey(actor.ActorId))
                return false;
            Actors[actor.ActorId] = actor;
            return true;
        }
        public bool RemoveActor(Actor.GActor actor)
        {
            if (actor == null)
                return false;
            return Actors.Remove(actor.ActorId);
        }

        public async System.Threading.Tasks.Task<bool> Save2Xnd(EngineNS.IO.XndNode node)
        {
            var headAtt = node.AddAttrib("nodeHead");
            headAtt.BeginWrite();
            headAtt.WriteMetaObject(this);
            headAtt.EndWrite();

            var actorIdsAtt = node.AddAttrib("ActorIds");
            actorIdsAtt.BeginWrite();
            
            actorIdsAtt.Write(Actors.Count);
            foreach (var i in Actors.Keys)
            {
                actorIdsAtt.Write(i);
            }

            actorIdsAtt.EndWrite();

            var graphNode = node.AddNode("GraphNodes", 0, 0);
            foreach(var child in ChildrenNode)
            {
                var ts = Rtti.RttiHelper.GetTypeSaveString(child.GetType());
                var childNode = graphNode.AddNode(ts, 0, 0);
                await child.Save2Xnd(childNode);
            }

            return true;
        }
        public async System.Threading.Tasks.Task<bool> LoadXnd(CRenderContext rc, EngineNS.IO.XndNode node)
        {
            var headAtt = node.FindAttrib("nodeHead");
            headAtt.BeginRead();
            headAtt.ReadMetaObject(this);
            headAtt.EndRead();

            var actorIdsAtt = node.FindAttrib("ActorIds");
            actorIdsAtt.BeginRead();
            int count;
            actorIdsAtt.Read(out count);
            for(int i=0; i< count; i++)
            {
                Guid actorId;
                actorIdsAtt.Read(out actorId);
                var actor = HostSceneGraph.FindActor(actorId);
                if(actor == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Actor", $"SceneNode can't find actor with id {actorId}");
                }
                else
                {
                    Actors[actorId] = actor;
                }
            }
            actorIdsAtt.EndRead();

            var graphNode = node.FindNode("GraphNodes");
            foreach(var childNode in graphNode.GetNodes())
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
    }
}
