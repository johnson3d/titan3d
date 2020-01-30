using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.SceneGraph
{
    public partial class GSceneGraph
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McSceneGraph))]
        [Editor.Editor_PackData()]
        public RName SceneMacross
        {
            get
            {
                if (Desc == null)
                    return null;
                return Desc.SceneMacross;
            }
            set
            {
                if (McSceneGetter != null && Desc.SceneMacross == value)
                    return;
                Desc.SceneMacross = value;
                mMcSceneGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McSceneGraph>(value);
                if (McSceneGetter != null)
                {
                    var getter = McSceneGetter.Get();
                    if (getter != null)
                        getter.Host = this;
                }
            }
        }
        protected Macross.MacrossGetter<McSceneGraph> mMcSceneGetter;
        [System.ComponentModel.Browsable(false)]
        public Macross.MacrossGetter<McSceneGraph> McSceneGetter
        {
            get { return mMcSceneGetter; }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public McSceneGraph McScene
        {
            get
            {
                return McSceneGetter?.Get(false);
            }
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcscene_64.txpic", RName.enRNameType.Editor)]
    public class McSceneGraph : Input.IInputable
    {
        #region IInputable
        public virtual void OnRegisterInput()
        {

        }
        public virtual void OnUnRegisterInput()
        {

        }
        #endregion
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public GSceneGraph Host
        {
            get;
            set;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task OnSceneLoaded(GSceneGraph scene)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnSceneTick(GSceneGraph scene)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnSceneCleanup(GSceneGraph scene)
        {

        }
    }
}
