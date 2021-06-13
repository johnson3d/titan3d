using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class IGameBase
    {
        [Rtti.Meta]
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay(UGameBase host)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        [Rtti.Meta]
        public virtual void Tick(UGameBase host, int elapsedMillisecond)
        {

        }
        [Rtti.Meta]
        public virtual void BeginDestroy(UGameBase host)
        {

        }
    }
    [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.NoMacrossCreate)]
    public partial class UGameBase : UModuleHost<UGameBase>
    {
        Macross.UMacrossGetter<IGameBase> mMcObject;
        public Macross.UMacrossGetter<IGameBase> McObject
        {
            get
            {
                if (mMcObject == null)
                    mMcObject = Macross.UMacrossGetter<IGameBase>.NewInstance();
                return mMcObject;
            }
        }
        protected override UGameBase GetHost()
        {
            return this;
        }
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay()
        {
            return await McObject?.Get()?.BeginPlay(this);
        }
        public virtual void Tick(int elapsedMillisecond)
        {
            McObject?.Get()?.Tick(this, elapsedMillisecond);
        }
        public virtual void BeginDestroy()
        {
            McObject?.Get()?.BeginDestroy(this);
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public GamePlay.UGameBase GameInstance
        {
            get;
            set;
        }
    }
}