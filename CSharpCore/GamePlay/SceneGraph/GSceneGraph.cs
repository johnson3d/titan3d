using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.SceneGraph
{
    [Rtti.MetaClassAttribute]
    public class GSceneGraphDesc : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public bool CreatePhysics
        {
            get;
            set;
        } = true;
        [Rtti.MetaData]
        [EngineNS.Editor.Editor_PackData()]
        public RName SceneMacross
        {
            get;
            set;
        }
        RName mEnvMapName;
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName EnvMapName
        {
            get { return mEnvMapName; }
            set
            {
                mEnvMapName = value;
                if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                {
                    GamePlay.IModuleInstance module = null;
#if PWindow
                    module = CEngine.Instance.GameEditorInstance as GamePlay.IModuleInstance;
                    if (module == null)
                    {
                        module = CEngine.Instance.GameInstance as GamePlay.IModuleInstance;
                    }
#endif
                    if (module != null)
                    {
                        module.RenderPolicy.SetEnvMap(value);
                    }
                }
            }
        }
        RName mEyeEnvMapName;
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName EyeEnvMapName
        {
            get { return mEyeEnvMapName; }
            set
            {
                mEyeEnvMapName = value;
            }
        }
    }
    [Editor.Editor_MacrossClassAttribute(ECSType.All, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public partial class GSceneGraph : ISceneNode, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public static async System.Threading.Tasks.Task<GSceneGraph> CreateSceneGraph(GWorld world, Type type, GSceneGraphDesc parameter)
        {
            var sg = System.Activator.CreateInstance(type) as GSceneGraph;
            sg.World = world;
            if (sg == null)
                return null;

            if (parameter == null)
            {
                return sg;
            }

            if (await sg.Init(parameter) == false)
                return null;
            return sg;
        }
        //不走初始化流程，只是创建一个GSceneGraph对象
        public static GSceneGraph NewSceneGraphWithoutInit(GWorld world, Type type, GSceneGraphDesc parameter)
        {
            var sg = System.Activator.CreateInstance(type) as GSceneGraph;
            sg.World = world;
            if (sg == null)
                return null;

            if (parameter == null)
            {
                return sg;
            }

            return sg;
        }
        public static int InstanceNumber = 0;
        [Browsable(false)]
        public Matrix SceneGraphTransMat  // 当整个场景图能够运动时（如能进入的星舰）才使用此矩阵
        {
            get;
        } = Matrix.Identity;
        GSceneNode mDefaultSceneNode;
        [Browsable(false)]
        public GSceneNode DefaultSceneNode => mDefaultSceneNode;

        string mName;
        public string Name
        {
            get => mName;
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }

        public GSceneGraph()
        {
            InstanceNumber++;

            mDefaultSceneNode = new GSceneNode(this);
            mDefaultSceneNode.Name = "Default";
            ChildrenNode.Add(mDefaultSceneNode);

            mActorsSerialIdData = new Support.BitSet();
            mActorsSerialIdData.Init(mSerialIdIncreaseSize);
        }
        ~GSceneGraph()
        {
            Cleanup();
            InstanceNumber--;
        }

        public virtual BoundingBox GetBoundingBox()
        {
            BoundingBox result = new BoundingBox();
            BoundingBox tmp = new BoundingBox();
            result.InitEmptyBox();
            using (var iter = Actors.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    Actor.GActor actor = iter.Current.Value;
                    if (actor.IsBoundingVolume)
                    {
                        actor.GetAABB(ref tmp);
                        result = BoundingBox.Merge(result, tmp);
                    }
                }
            }
            return result;
        }

        #region ActorSerialId

        EngineNS.Support.BitSet mActorsSerialIdData;
        readonly UInt32 mSerialIdIncreaseSize = 10000;
        public UInt32 GetAvaliableSerialId()
        {
            var idx = mActorsSerialIdData.FindFirstBit(false);
            if (idx < 0)
            {
                idx = mActorsSerialIdData.BitCount;
                mActorsSerialIdData.Resize(mActorsSerialIdData.BitCount + mSerialIdIncreaseSize);
            }
            return (UInt32)idx;
        }

        #endregion

        partial void _CreatePhysicsScene(GSceneGraphDesc parameter);
        partial void _CleanupPhysicsScene();
        GSceneGraphDesc mDesc = new GSceneGraphDesc();

        public GSceneGraphDesc Desc => mDesc;

        public virtual async System.Threading.Tasks.Task<bool> Init(GSceneGraphDesc parameter)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            mDesc = parameter;
            this.SceneMacross = parameter.SceneMacross;
            //SunActor = await EngineNS.GamePlay.Actor.GActor.NewSunActorAsync(RName.GetRName("editor/sun.gms", RName.enRNameType.Game));
            //this.World.AddActor(SunActor);
            //this.AddActor(SunActor);
            //CEngine.Instance.HitProxyManager.MapActor(SunActor);

            //SunActor.Placement.Location = new Vector3(0, 20, 0);

            //SunActor.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitX, 3.14f / 4.0f);// * Quaternion.RotationAxis(Vector3.UnitY, 3.14f / 4.0f);

            //var sunComp = SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
            //if (sunComp != null)
            //{
            //    //var erp = this.RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
            //    //sunComp.View = erp.mBaseSceneView;

            //    sunComp.DirLightInitializer.DirLightColor = Color.White;
            //    sunComp.DirLightInitializer.DirLightIntensity = 2.5f;

            //    sunComp.DirLightInitializer.SkyLightColor = Color.White;
            //    sunComp.DirLightInitializer.SkyLightIntensity = 1.0f;
            //}

            if (parameter.CreatePhysics)
            {
                _CreatePhysicsScene(parameter);
            }
            return true;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public GWorld World
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public Guid SceneId
        {
            get;
            set;
        } = Guid.NewGuid();
        [Browsable(false)]
        public bool NeedTick
        {
            get;
            set;
        } = true;
        [Browsable(false)]
        public GamePlay.Actor.GActor SunActor
        {
            get;
            set;
        }
        public virtual void Cleanup()
        {
            McSceneGetter?.Get(false)?.OnUnRegisterInput();
            McSceneGetter?.Get()?.OnSceneCleanup(this);

            _CleanupPhysicsScene();
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (actor != null)
                    {
                        actor.OnRemoveSceneGraph(this);
                        actor.Scene = null;
                    }
                }
            }
            Actors.Clear();

            Cleanup_Modules();
        }

        // 场景中的所有对象
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
        protected GSceneGraph mHostSceneGraph;
        [Browsable(false)]
        public GSceneGraph HostSceneGraph
        {
            get
            {
                if (mHostSceneGraph == null)
                    return this;
                return mHostSceneGraph;
            }
            set
            {
                mHostSceneGraph = value;
            }
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

        partial void UpdateCrowd(float t);
       // private bool mIsActorForeaching = false;
        public virtual void Tick()
        {
            McSceneGetter?.Get()?.OnSceneTick(this);
            Tick_Modules();

            using (var it = Actors.GetEnumerator())
            {
                //mIsActorForeaching = true;
                try
                {
                    while (it.MoveNext())
                    {
                        Actor.GActor actor = it.Current.Value;
                        actor.TryTick();
                    }
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }
           // mIsActorForeaching = false;

            UpdateCrowd(EngineNS.CEngine.Instance.EngineElapseTimeSecond);
        }
        public event GWorld.Delegate_OnOperationActor OnAddActor;
        public event GWorld.Delegate_OnOperationActor OnReAddActor;
        public event GWorld.Delegate_OnOperationActor OnRemoveActor;
        public event GWorld.Delegate_OnOperationActor OnAddDynamicActor;
        public event GWorld.Delegate_OnOperationActor OnRemoveDynamicActor;
        public bool AddDynamicActor(Actor.GActor actor)
        {
            if (Actors.ContainsKey(actor.ActorId))
                return false;
            Actors[actor.ActorId] = actor;
            actor.Scene = this;

            actor.PVSId = GetAvaliableSerialId();
            mActorsSerialIdData.SetBit(actor.PVSId, true);

            actor.OnAddToSceneGraph(this);
            foreach (var child in actor.Children)
            {
                AddDynamicActor(child);
            }

            // 场景图辅助用Actor全部放入Default里
            mDefaultSceneNode.AddActor(actor);
            OnAddDynamicActor?.Invoke(actor);
            return true;
        }
        public bool RemoveDynamicActor(Actor.GActor actor)
        {
            if (actor == null)
                return false;

            if (!Actors.ContainsKey(actor.ActorId))
                return false;

            // 场景图辅助用Actor全部在Default里
            mDefaultSceneNode.RemoveActor(actor);

            foreach (var child in actor.Children)
            {
                RemoveDynamicActor(child);
            }

            actor.OnRemoveSceneGraph(this);
            actor.Scene = null;
            Actors.Remove(actor.ActorId);
            if (actor.PVSId == UInt32.MaxValue)
                throw new InvalidOperationException("SerialId没有赋值");
            mActorsSerialIdData.SetBit(actor.PVSId, false);
            actor.PVSId = UInt32.MaxValue;

            OnRemoveDynamicActor?.Invoke(actor);
            return true;
        }
        public virtual bool AddActor(Actor.GActor actor)
        {
            //System.Diagnostics.Debug.Assert(mIsActorForeaching == false);
            if (Actors.ContainsKey(actor.ActorId))
                return false;
            Actors[actor.ActorId] = actor;
            actor.Scene = this;

            actor.PVSId = GetAvaliableSerialId();
            mActorsSerialIdData.SetBit(actor.PVSId, true);

            actor.OnAddToSceneGraph(this);
            foreach (var child in actor.Children)
            {
                AddActor(child);
            }

            // 场景图辅助用Actor全部放入Default里
            if (actor.GetComponent(typeof(GamePlay.Component.ISceneGraphComponent)) != null)
            {
                mDefaultSceneNode.AddActor(actor);
            }
            else
            {
                var aabb = BoundingBox.EmptyBox();
                actor.GetAABB(ref aabb);
                bool added = false;
                for (int i = 0; i < ChildrenNode.Count; i++)
                {
                    var child = ChildrenNode[i];
                    if (child == mDefaultSceneNode)
                        continue;
                    if (child.Contain(ref aabb) != ContainmentType.Disjoint)
                        added = added || ChildrenNode[i].AddActor(actor);
                }
                if (!added)
                    mDefaultSceneNode.AddActor(actor);
            }
            OnAddActor?.Invoke(actor);
            return true;
        }

        //删除后重新加入，需要编辑器Outliner做额外的操作，和Add又不一样

        public virtual bool ReAddActor(Actor.GActor actor)
        {
            //System.Diagnostics.Debug.Assert(mIsActorForeaching == false);
            if (Actors.ContainsKey(actor.ActorId))
                return false;
            Actors[actor.ActorId] = actor;
            actor.Scene = this;

            actor.PVSId = GetAvaliableSerialId();
            mActorsSerialIdData.SetBit(actor.PVSId, true);

            actor.OnAddToSceneGraph(this);
            foreach (var child in actor.Children)
            {
                ReAddActor(child);
            }

            // 场景图辅助用Actor全部放入Default里
            if (actor.GetComponent(typeof(GamePlay.Component.ISceneGraphComponent)) != null)
            {
                mDefaultSceneNode.AddActor(actor);
            }
            else
            {
                var aabb = BoundingBox.EmptyBox();
                actor.GetAABB(ref aabb);
                bool added = false;
                for (int i = 0; i < ChildrenNode.Count; i++)
                {
                    var child = ChildrenNode[i];
                    if (child == mDefaultSceneNode)
                        continue;
                    if (child.Contain(ref aabb) != ContainmentType.Disjoint)
                        added = added || ChildrenNode[i].AddActor(actor);
                }
                if (!added)
                    mDefaultSceneNode.AddActor(actor);
            }
            OnReAddActor?.Invoke(actor);
            return true;
        }
        public bool RemoveActor(Actor.GActor actor)
        {
            //System.Diagnostics.Debug.Assert(mIsActorForeaching == false);
            if (actor == null)
                return false;

            if (!Actors.ContainsKey(actor.ActorId))
                return false;

            // 场景图辅助用Actor全部在Default里
            if (actor.GetComponent(typeof(GamePlay.Component.ISceneGraphComponent)) != null)
            {
                mDefaultSceneNode.RemoveActor(actor);
            }
            else
            {
                var aabb = BoundingBox.EmptyBox();
                actor.GetAABB(ref aabb);
                bool removed = false;
                for (int i = 0; i < ChildrenNode.Count; i++)
                {
                    var child = ChildrenNode[i];
                    if (child == mDefaultSceneNode)
                        continue;
                    if (child.Contain(ref aabb) != ContainmentType.Disjoint)
                        removed = removed || ChildrenNode[i].RemoveActor(actor);
                }
                if (!removed)
                {
                    mDefaultSceneNode.RemoveActor(actor);
                }
            }

            foreach (var child in actor.Children)
            {
                RemoveActor(child);
            }

            actor.OnRemoveSceneGraph(this);
            actor.Scene = null;
            Actors.Remove(actor.ActorId);
            if (actor.PVSId != UInt32.MaxValue)
            {
                //throw new InvalidOperationException("SerialId没有赋值");
                mActorsSerialIdData.SetBit(actor.PVSId, false);
            }
            actor.PVSId = UInt32.MaxValue;

            OnRemoveActor?.Invoke(actor);
            return true;
        }
        public virtual void RemoveActor(Guid actorId)
        {
            Actor.GActor actor;
            if (Actors.TryGetValue(actorId, out actor) == false)
            {
                return;
            }
            RemoveActor(actor);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor FindActor(Guid actorId)
        {
            Actor.GActor actor;
            if (Actors.TryGetValue(actorId, out actor) == false)
            {
                return null;
            }
            return actor;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable, "根据名称查找对象（该函数较慢，请慎用）")]
        public Actor.GActor FindActorByName(string actorName)
        {
            using (var ite = Actors.GetEnumerator())
            {
                while (ite.MoveNext())
                {
                    Actor.GActor actor = ite.Current.Value;
                    if (actor.SpecialName == actorName)
                        return actor;
                }
            }
            return null;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 GetPlayerStartLocation(int index)
        {
            var curIdx = 0;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (actor.GetComponent<Component.GPlayerStartComponent>() != null)
                    {
                        if (curIdx == index)
                            return actor.Placement.Location;
                        curIdx++;
                    }
                }
            }
            return Vector3.Zero;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public int GetPlayerStartSize()
        {
            int curIdx = 0;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (actor.GetComponent<Component.GPlayerStartComponent>() != null)
                        curIdx++;
                }
            }
            return curIdx;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion GetPlayerStartRotation(int index)
        {
            var curIdx = 0;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (actor.GetComponent<Component.GPlayerStartComponent>() != null)
                    {
                        if (curIdx == index)
                            return actor.Placement.Rotation;
                        curIdx++;
                    }
                }
            }
            return Quaternion.Identity;
        }
        // 得到绕Y轴旋转的角度值
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float GetPlayerStartRotationAngle(int index)
        {
            var curIdx = 0;
            var up = Vector3.UnitY;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (actor.GetComponent<Component.GPlayerStartComponent>() != null)
                    {
                        if (curIdx == index)
                        {
                            return (float)(actor.Placement.Rotation.GetAngleWithAxis(up) * 180 / System.Math.PI);
                        }
                        curIdx++;
                    }
                }
            }
            return 0.0f;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 GetPlayerStartScale(int index)
        {
            var curIdx = 0;
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor actor = i.Current.Value;
                    if (actor.GetComponent<Component.GPlayerStartComponent>() != null)
                    {
                        if (curIdx == index)
                            return actor.Placement.Scale;
                        curIdx++;
                    }
                }
            }
            return Vector3.Zero;
        }

        #region PVS

        public Actor.GActor[] PVSActors;
        public List<EngineNS.GamePlay.SceneGraph.GPvsSet> PvsSets = new List<GPvsSet>();
        public EngineNS.GamePlay.SceneGraph.GPVSOctree PVSTree;

        #endregion

        public virtual void CheckVisible(CCommandList cmd, Graphics.CGfxCamera camera, CheckVisibleParam param, bool checkFrustum)
        {
            if (Visible == false)
                return;

            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                if (param.ForShadow == false)
                    CEngine.Instance.MeshManager.InstancingManager.Reset(Component.GInstancingManager.EPoolType.Normal);
                else
                    CEngine.Instance.MeshManager.InstancingManager.Reset(Component.GInstancingManager.EPoolType.Shadow);
            }

            if (CEngine.UsePVS)
            {
                // PVS管理过的对象不再放入子节点中
                if (PVSTree?.CheckVisible(cmd, this, camera, param) == true)
                    param.UsePVS = true;
            }
            else
                param.UsePVS = false;

            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                ChildrenNode[i].CheckVisible(cmd, camera, param, checkFrustum);
            }

            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                if (param.ForShadow == false)
                    CEngine.Instance.MeshManager.InstancingManager.Commit(cmd, camera, param, Component.GInstancingManager.EPoolType.Normal);
                else
                    CEngine.Instance.MeshManager.InstancingManager.Commit(cmd, camera, param, Component.GInstancingManager.EPoolType.Shadow);
            }
        }

        public virtual void SlowDrawAll(CCommandList cmd, Graphics.CGfxCamera camera, CheckVisibleParam param)
        {
            if (Visible == false)
                return;

            if (param.ForShadow == false)
                CEngine.Instance.MeshManager.InstancingManager.Reset(Component.GInstancingManager.EPoolType.Normal);
            else
                CEngine.Instance.MeshManager.InstancingManager.Reset(Component.GInstancingManager.EPoolType.Shadow);

            Actor.GActor actor = null;
            foreach (var i in Actors)
            {
                actor = i.Value;
                if (actor != null)
                    actor.OnCheckVisible(cmd, this, camera, param);
            }

            if (param.ForShadow == false)
                CEngine.Instance.MeshManager.InstancingManager.Commit(cmd, camera, param, Component.GInstancingManager.EPoolType.Normal);
            else
                CEngine.Instance.MeshManager.InstancingManager.Commit(cmd, camera, param, Component.GInstancingManager.EPoolType.Shadow);
        }
        RName mSceneFilename = RName.EmptyName;
        public RName SceneFilename
        {
            get
            {
                return mSceneFilename;
            }
            set
            {
                mSceneFilename = value;
            }
        }
        public delegate void FOnSaveScene(GSceneGraph scene, EngineNS.IO.XndHolder xnd);
        public async System.Threading.Tasks.Task SaveScene(RName SceneFilename, EngineNS.Graphics.CGfxCamera camera, FOnSaveScene onSaveScene = null)
        {
            if (SceneFilename == null)
                return;

            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
            await Save2Xnd(xnd.Node);

            if (camera != null)
            {
                var att = xnd.Node.AddAttrib("ED_info");
                att.BeginWrite();
                att.Write(camera.CameraData.Position);
                att.Write(camera.CameraData.LookAt);
                att.Write(camera.CameraData.Up);
                att.EndWrite();
            }

            if (onSaveScene != null)
            {
                onSaveScene(this, xnd);
            }

            EngineNS.IO.XndHolder.SaveXND(SceneFilename.Address + "/scene.map", xnd);

            SaveNavMesh();
        }
        partial void SaveNavMesh();
        public EngineNS.GamePlay.Actor.GActor NavAreaActor;

        public virtual async System.Threading.Tasks.Task<bool> Save2Xnd(IO.XndNode node)
        {
            Save_Modules(node);
            var tn = Rtti.RttiHelper.GetTypeSaveString(this.GetType());
            node.SetName(tn);

            var attr = node.AddAttrib("SceneDesc");
            attr.BeginWrite();
            attr.WriteMetaObject(mDesc);
            attr.EndWrite();

            var actorsNode = node.AddNode("Actors", 0, 0);
            // Scene是场景存取的最小单元，如果以后有需要可以将一个Scene拆分成多个Scene存储，每个Scene依然是存取的最小单元
            using (var i = Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    Actor.GActor act = i.Current.Value;
                    if (act == null)
                        continue;

                    if (act.Parent != null)
                        continue;

                    var ts = Rtti.RttiHelper.GetTypeSaveString(act.GetType());
                    var actNode = actorsNode.AddNode(ts, 0, 0);
                    act.Save2Xnd(actNode);
                }
            }

            //var actornode = node.AddNode("Actors", 0, 0);
            //foreach (var i in Actors.Values)
            //{
            //    Actor.GActor actor;
            //    if (i.TryGetTarget(out actor) == false)
            //        continue;
            //    var ts = Rtti.RttiHelper.GetTypeSaveString(actor.GetType());
            //    var n = actornode.AddNode(ts, 0, 0);
            //    actor.Save2Xnd(n);
            //}
            var special = node.AddNode("SpecialActors", 0, 0);
            if (SunActor != null)
            {
                var sunAtt = special.AddAttrib("Sun");
                sunAtt.BeginWrite();
                sunAtt.Write(SunActor.ActorId);
                sunAtt.EndWrite();
            }

            var graphNode = node.AddNode("GraphNodes", 0, 0);
            foreach (var child in ChildrenNode)
            {
                var ts = Rtti.RttiHelper.GetTypeSaveString(child.GetType());
                var childNode = graphNode.AddNode(ts, 0, 0);
                await child.Save2Xnd(childNode);
            }

            var pvsNode = node.AddNode("PVS", 0, 0);
            var pvsActorsAtt = pvsNode.AddAttrib("PVSActors");
            pvsActorsAtt.BeginWrite();
            if (PVSActors == null)
                pvsActorsAtt.Write((int)0);
            else
            {
                pvsActorsAtt.Write((int)(PVSActors.Length));
                foreach (var actor in PVSActors)
                {
                    if(actor == null)
                    {
                        pvsActorsAtt.Write(Guid.Empty);
                    }
                    else
                    {
                        pvsActorsAtt.Write(actor.ActorId);
                    }
                }
            }
            pvsActorsAtt.EndWrite();
            var pvsSetAtt = pvsNode.AddAttrib("PVSSet");
            pvsSetAtt.BeginWrite();
            pvsSetAtt.Write((int)(PvsSets.Count));
            foreach (var set in PvsSets)
            {
                pvsSetAtt.WriteMetaObject(set);
            }
            pvsSetAtt.EndWrite();

            if (PVSTree != null)
            {
                var pvsTreeNode = pvsNode.AddNode("PVSTree", 0, 0);
                await PVSTree.Save2Xnd(pvsTreeNode, this);
            }

            return true;
        }
        [Browsable(false)]
        public bool IsLoading
        {
            get;
            private set;
        } = false;

        float mLoadingProgress = 1.0f;
        [Browsable(false)]
        public float LoadingProgress => mLoadingProgress;

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

        public virtual async System.Threading.Tasks.Task<bool> LoadNavMesh(RName address)
        {
            if (NavMesh != null)
                return false;

            var xnd = await EngineNS.IO.XndHolder.LoadXND(address.Address + "/navmesh.dat");
            if (xnd == null)
                return false;

            NavMesh = new EngineNS.Bricks.RecastRuntime.CNavMesh();
            NavMesh.LoadXnd(xnd.Node);

            NavQuery = NavMesh.CreateQuery(65535); // TODO.
            xnd.Node.TryReleaseHolder();
            return true;

        }

        // 根据位置加载包含此位置的对象区域
        public virtual async System.Threading.Tasks.Task<bool> LoadXnd(CRenderContext rc, IO.XndNode node, RName filename = null)
        {
            bool create = await LoadXnd(rc, node);
            await LoadNavMesh(filename);
            if (mDesc.EnvMapName != null)
            {
                GamePlay.IModuleInstance module = null;
#if PWindow
                module = CEngine.Instance.GameEditorInstance as GamePlay.IModuleInstance;
                if (module == null)
                {
                    module = CEngine.Instance.GameInstance as GamePlay.IModuleInstance;
                }
#endif
                if (module != null)
                {
                    module.RenderPolicy.SetEnvMap(mDesc.EnvMapName);
                }
            }
            return create;
        }

        public virtual async System.Threading.Tasks.Task<bool> LoadXnd(CRenderContext rc, IO.XndNode node)
        {
            IsLoading = true;

            mLoadingProgress = 0.0f;
            //IO.XndNode node
            var attr = node.FindAttrib("SceneDesc");
            if (attr != null)
            {
                attr.BeginRead();
                attr.ReadMetaObject(mDesc);
                attr.EndRead();
                await Init(mDesc);

                await Load_Modules(rc, node);

                // 加载Actors
                var actorsNode = node.FindNode("Actors");
                var actNodes = actorsNode.GetNodes();

                var progressDelta = 0.6f / actNodes.Count;

                var smp = Thread.ASyncSemaphore.CreateSemaphore(actNodes.Count);
                while (actNodes.Count > 0)
                {
                    var delayNodes = new List<IO.XndNode>();
                    for (int i = 0; i < actNodes.Count; i++)
                    {
                        try
                        {
                            var actNode = actNodes[i];
                            bool dependFinished = true;
                            var depends = actNode.FindAttrib("DependActors");
                            if (depends != null)
                            {
                                depends.BeginRead();
                                int actNum = 0;
                                depends.Read(out actNum);
                                for (int dpIdx = 0; dpIdx < actNum; dpIdx++)
                                {
                                    Guid dependActorId;
                                    depends.Read(out dependActorId);
                                    if (this.FindActor(dependActorId) == null)
                                    {
                                        dependFinished = false;
                                        break;
                                    }
                                }
                                depends.EndRead();
                            }
                            if (dependFinished)
                            {
                                CEngine.Instance.EventPoster.RunOn(async () =>
                                {
                                    var actTypeName = actNode.GetName();
                                    var type = Rtti.RttiHelper.GetTypeFromSaveString(actTypeName);
                                    if (type == null)
                                    {
                                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{actTypeName} is invalid");
                                        smp.Release();
                                        mLoadingProgress += progressDelta;
                                        return false;
                                    }
                                    var act = System.Activator.CreateInstance(type) as Actor.GActor;
                                    if (act == null)
                                    {
                                        smp.Release();
                                        mLoadingProgress += progressDelta;
                                        return false;
                                    }
                                    if (false == await act.LoadXnd(rc, actNode))
                                    {
                                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{actTypeName} LoadXnd failed");
                                    }

                                    World?.AddActor(act);

                                    Actors[act.ActorId] = act;
                                    act.Scene = this;
                                    if (act.PVSId == UInt32.MaxValue)
                                    {
                                        act.PVSId = GetAvaliableSerialId();
                                        mActorsSerialIdData.SetBit(act.PVSId, true);
                                    }
                                    else
                                    {
                                        mActorsSerialIdData.SetBit(act.PVSId, true);
                                    }
                                    act.OnAddToSceneGraph(this);
                                    foreach (var child in act.Children)
                                    {
                                        AddActor(child);
                                    }

                                    smp.Release();
                                    mLoadingProgress += progressDelta;
                                    return true;
                                }, Thread.Async.EAsyncTarget.AsyncIO);
                            }
                            else
                            {
                                delayNodes.Add(actNode);
                            }
                        }
                        catch (System.Exception e)
                        {
                            EngineNS.Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Actor Load", e.ToString());
                            smp.Release();
                        }
                    }
                    actNodes = delayNodes;
                }
                await smp.Await();
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "IO", $"Scene Load ok");

                var special = node.FindNode("SpecialActors");
                if (special != null)
                {
                    var sunAtt = special.FindAttrib("Sun");
                    if (sunAtt != null)
                    {
                        sunAtt.BeginRead();
                        Guid sunActorId;
                        sunAtt.Read(out sunActorId);
                        SunActor = FindActor(sunActorId);
                        sunAtt.EndRead();
                    }
                }

                ChildrenNode.Clear();
                await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
                {
                    var graphNode = node.FindNode("GraphNodes");
                    var graphNodes = graphNode.GetNodes();
                    progressDelta = 0.2f / graphNodes.Count;
                    foreach (var childNode in graphNodes)
                    {
                        var ts = childNode.GetName();
                        var type = Rtti.RttiHelper.GetTypeFromSaveString(ts);
                        var child = System.Activator.CreateInstance(type) as ISceneNode;
                        child.HostSceneGraph = this;
                        child.Parent = this;
                        await child.LoadXnd(rc, childNode);
                        //if(child.Name == "Default")
                        //{
                        //    mDefaultSceneNode = child as GSceneNode;
                        //}
                        ChildrenNode.Add(child);

                        mLoadingProgress += progressDelta;
                    }
                }, Thread.Async.EAsyncTarget.AsyncIO);

                var pvsNode = node.FindNode("PVS");
                if (pvsNode != null)
                {
                    var pvsActorsAtt = pvsNode.FindAttrib("PVSActors");
                    pvsActorsAtt.BeginRead();
                    int count;
                    pvsActorsAtt.Read(out count);

                    progressDelta = 0.1f / count;
                    PVSActors = new Actor.GActor[count];
                    for (int i = 0; i < count; i++)
                    {
                        mLoadingProgress += progressDelta;
                        Guid actorId;
                        pvsActorsAtt.Read(out actorId);
                        if (actorId == Guid.Empty)
                            continue;
                        GamePlay.Actor.GActor actorRef;
                        if (Actors.TryGetValue(actorId, out actorRef))
                        {
                            PVSActors[i] = actorRef;
                        }
                    }
                    pvsActorsAtt.EndRead();
                    var pvsSetAtt = pvsNode.FindAttrib("PVSSet");
                    PvsSets.Clear();
                    if (pvsSetAtt != null)
                    {
                        pvsSetAtt.BeginRead();
                        int pvsSetCount = 0;
                        pvsSetAtt.Read(out pvsSetCount);
                        progressDelta = 0.1f / pvsSetCount;
                        for (int i = 0; i < pvsSetCount; i++)
                        {
                            var set = new GPvsSet();
                            pvsSetAtt.ReadMetaObject(set);
                            PvsSets.Add(set);
                            mLoadingProgress += progressDelta;
                        }
                        pvsSetAtt.EndRead();
                    }

                    await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
                    {
                        var pvsTreeNode = pvsNode.FindNode("PVSTree");
                        if (pvsTreeNode != null)
                        {
                            PVSTree = new GPVSOctree();
                            await PVSTree.LoadXnd(pvsTreeNode, this);
                        }
                    }, Thread.Async.EAsyncTarget.AsyncIO);
                }
                mDefaultSceneNode = ChildrenNode[0] as GSceneNode;
            }
            else
            {
                await LoadXnd_Old(rc, node);
            }

            mLoadingProgress = 1.0f;
            IsLoading = false;

            return true;
        }

        public virtual async System.Threading.Tasks.Task<bool> LoadXnd_Old(CRenderContext rc, IO.XndNode node)
        {
            //先创建物理场景，然后加载地图时，Actor物理组件才能加到物理场景里面去
            if (mDesc.CreatePhysics)
                _CreatePhysicsScene(null);

            var actornode = node.FindNode("Actors");
            var actNodes = actornode.GetNodes();

            var progressDelta = 1.0f / actNodes.Count;
            var smp = Thread.ASyncSemaphore.CreateSemaphore(actNodes.Count);

            while (actNodes.Count > 0)
            {
                var delayNodes = new List<IO.XndNode>();
                foreach (var i in actNodes)
                {
                    bool dependfinished = true;
                    var depends = i.FindAttrib("DependActors");
                    if (depends != null)
                    {
                        depends.BeginRead();
                        int actNum = 0;
                        depends.Read(out actNum);
                        for (int j = 0; j < actNum; j++)
                        {
                            Guid dependActorId;
                            depends.Read(out dependActorId);
                            if (this.FindActor(dependActorId) == null)
                            {
                                dependfinished = false;
                                break;
                            }
                        }
                        depends.EndRead();
                    }
                    if (dependfinished)
                    {
                        CEngine.Instance.EventPoster.RunOn(async () =>
                        {
                            var type = Rtti.RttiHelper.GetTypeFromSaveString(i.GetName());
                            if (type == null)
                            {
                                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{i.GetName()} is invalid");
                                smp.Release();
                                mLoadingProgress += progressDelta;
                                return false;
                            }
                            var act = System.Activator.CreateInstance(type) as Actor.GActor;
                            if (act == null)
                            {
                                smp.Release();
                                mLoadingProgress += progressDelta;
                                return false;
                            }
                            if (false == await act.LoadXnd(rc, i))
                            {
                                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{i.GetName()} LoadXnd failed");
                            }

                            World?.AddActor(act);
                            this.AddActor(act);
                            smp.Release();
                            mLoadingProgress += progressDelta;
                            return true;
                        }, Thread.Async.EAsyncTarget.AsyncIO);
                    }
                    else
                    {
                        delayNodes.Add(i);
                    }
                }
                actNodes = delayNodes;
            }
            //foreach (var i in actNodes)
            //{
            //    CEngine.Instance.EventPoster.RunOn(async () =>
            //    {
            //        var type = Rtti.RttiHelper.GetTypeFromSaveString(i.GetName());
            //        if (type == null)
            //        {
            //            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{i.GetName()} is invalid");
            //            smp.Release();
            //            return false;
            //        }
            //        var act = System.Activator.CreateInstance(type) as Actor.GActor;
            //        if (act == null)
            //        {
            //            smp.Release();
            //            return false;
            //        }
            //        if (false == await act.LoadXnd(rc, i))
            //        {
            //            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"Actor Type{i.GetName()} LoadXnd failed");
            //        }

            //        World?.AddActor(act);
            //        this.AddActor(act);
            //        smp.Release();
            //        return true;
            //    }, Thread.Async.EAsyncTarget.AsyncIO);
            //}
            await smp.Await();

            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "IO", $"Scene Load ok");

            var special = node.FindNode("SpecialActors");
            if (special != null)
            {
                var sun = special.FindNode("Sun");
                if (sun != null)
                {
                    var act = new Actor.GActor();
                    if (false == await act.LoadXnd(rc, sun))
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Actor", $"SunActor LoadXnd failed");
                    }

                    SunActor = this.FindActor(act.ActorId);
                    if (SunActor == null)
                    {
                        SunActor = act;
                        World?.AddActor(act);
                        this.AddActor(act);
                    }
                }
            }

            return true;
        }


        public virtual bool Contain(ref Vector3 pos)
        {
            return false;
        }
        public ContainmentType Contain(ref BoundingBox aabb)
        {
            return ContainmentType.Disjoint;
        }
        partial void LineCheckByPhysics(ref Vector3 start, ref Vector3 end, ref VHitResult rst, ref bool isChecked);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual bool LineCheck(ref Vector3 start, ref Vector3 end, ref VHitResult rst)
        {
            bool isChecked = false;
            LineCheckByPhysics(ref start, ref end, ref rst, ref isChecked);
            if (isChecked == true)
                return true;

            if (rst.Distance == 0)
                rst.Distance = float.MaxValue;
            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                VHitResult result = VHitResult.Default;
                if (ChildrenNode[i].LineCheck(ref start, ref end, ref result))
                {
                    if (result.Distance < rst.Distance)
                    {
                        isChecked = true;
                        rst = result;
                    }
                }
            }

            return isChecked;
        }
        partial void LineCheckWithFilterByPhysics(ref Vector3 start, ref Vector3 end, ref Bricks.PhysicsCore.PhyQueryFilterData queryFilterData, ref VHitResult rst, ref bool isChecked);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual bool LineCheckWithFilter(ref Vector3 start, ref Vector3 end, ref Bricks.PhysicsCore.PhyQueryFilterData queryFilterData, ref VHitResult rst)
        {
            bool isChecked = false;
            LineCheckWithFilterByPhysics(ref start, ref end, ref queryFilterData, ref rst, ref isChecked);
            if (isChecked == true)
                return true;

            if (rst.Distance == 0)
                rst.Distance = float.MaxValue;
            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                VHitResult result = VHitResult.Default;
                if (ChildrenNode[i].LineCheck(ref start, ref end, ref result))
                {
                    if (result.Distance < rst.Distance)
                    {
                        isChecked = true;
                        rst = result;
                    }
                }
            }

            return isChecked;
        }

        public static bool LineCheckWithPlane(ref Vector3 start, ref Vector3 end, ref Plane plane, ref VHitResult rst)
        {
            unsafe
            {
                fixed (Plane* pPlane = &plane)
                fixed (VHitResult* prst = &rst)
                fixed (Vector3* pstart = &start)
                fixed (Vector3* pend = &end)
                {
                    if (IntPtr.Zero == (IntPtr)MathHelper.v3dxPlaneIntersectLine(&prst->Position, pPlane, pstart, pend))
                        return false;
                    prst->Normal = Vector3.UnitY;
                    prst->ExtData = IntPtr.Zero;
                }
            }
            return true;
        }

        partial void GetHitActorByPhysics(ref SceneGraph.VHitResult hitResult, ref Actor.GActor outActor);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Actor.GActor GetHitActor(ref SceneGraph.VHitResult hitResult)
        {
            Actor.GActor actorByPhysx = null;
            GetHitActorByPhysics(ref hitResult, ref actorByPhysx);
            if (actorByPhysx != null)
                return actorByPhysx;

            return null;
        }
    }
}
