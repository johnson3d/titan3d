using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Actor
{
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mccenterdata_64.txpic", RName.enRNameType.Editor)]
    [Rtti.MetaClass]
    public class GCenterData : IO.Serializer.Serializer
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.IgnoreCopy | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public GActor HostActor
        {
            get;
            set;
        }
    }
    public partial class GActor : IEntity
    {
        [Editor.Editor_RNameMacrossType(typeof(GCenterData))]
        [Category("Macross")]
        public RName CenterDataName
        {
            get
            {
                if(this.Initializer==null)
                {
                    return RName.EmptyName;
                }
                return this.Initializer.CenterDataName;
            }
            set
            {
                if (this.Initializer != null)
                {
                    this.Initializer.CenterDataName = value;
                }
                CreateCenterData(value);
            }
        }
        protected Macross.MacrossGetter<GCenterData> mCenterDataGetter;
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("Macross")]
        public GCenterData CenterData
        {
            get { return mCenterDataGetter?.Get(false); }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Position
        {
            get
            {
                return this.Placement.Location;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public GCenterData GetCenterData(
            [Editor.Editor_TypeFilterAttribute(typeof(GCenterData))]
            System.Type type)
        {
            return CenterData;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void CreateCenterData(
            [Editor.Editor_RNameMacrossType(typeof(GCenterData))]
            RName value)
        {
            if (mCenterDataGetter != null && mCenterDataGetter.Name == value)
                return;
            mCenterDataGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<GCenterData>(value);
            if(CenterData!=null)
                CenterData.HostActor = this;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetCenterData(GCenterData cd)
        {
            mCenterDataGetter = new Macross.MacrossGetter<GCenterData>(RName.GetRName(cd.GetType().FullName), (GCenterData)cd);
            if (CenterData != null)
                CenterData.HostActor = this;
        }
    }
}
