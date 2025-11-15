using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.DesignMacross
{
    [Bricks.CodeBuilder.ContextMenu("DesignMacrossNode", "DesignMacrossNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtDesignMacrossNodeData), DefaultNamePrefix = "DM")]
    public class TtDesignMacrossNode : TtSceneActorNode
    {
        public class TtDesignMacrossNodeData : TtNodeData
        {
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = EngineNS.DesignMacross.UDesignMacross.AssetExt)]
            public RName DesignMacrossName { get; set; }
        }
        Macross.TtMacrossGetter<TtDesignMacrossBase> mMacrossGetter = null;
        public Macross.TtMacrossGetter<TtDesignMacrossBase> MacrossGetter
        {
            get
            {
                return mMacrossGetter;
                //if (mMacrossGetter != null)
                //{
                //    return mMacrossGetter;
                //}
                //else
                //{
                //    if (DesignMacross != null && !RName.IsEmpty(DesignMacross))
                //    {
                //        var macrossGetter = Macross.TtMacrossGetter<TtDesignMacrossBase>.NewInstance();
                //        macrossGetter.Name = DesignMacross;
                //        if (macrossGetter.Get() != null)
                //        {
                //            mMacrossGetter = macrossGetter;
                //            mMacrossGetter.Get().MacrossNode = this;
                //            var task = mMacrossGetter.Get().Initialize();
                //            TtEngine.Instance.TaskCollector.AddWaitTask(task);
                //            return mMacrossGetter;
                //        }
                //    }
                //    return null;
                //}
            }
        }
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            NodeData = data;
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            DesignMacross = DesignMacross;
            return ret;
        }
        protected override void OnNodeCopyTreeData(TtNode src, ref FTreeCopyStat stat)
        {
            var g = MacrossGetter?.Get();
            if (g != null)
            {
                g.MacrossNode = null;
            }
            mMacrossGetter = null;
        }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            base.OnParentChanged(prev, cur);
            //DesignMacross = DesignMacross;
        }
        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            DesignMacross = DesignMacross;
        }
        //[RName.PGRName(FilterExts = UDesignMacross.AssetExt)]
        [RName.PGMacrossRName<TtDesignMacrossBase>(FilterExts = UDesignMacross.AssetExt)]
        [Category("Option")]
        public RName DesignMacross
        {
            get
            {
                if(NodeData is TtDesignMacrossNodeData data)
                {
                    return data.DesignMacrossName;
                }
                return null;
            }
            set
            {
                if (NodeData is TtDesignMacrossNodeData data)
                {
                    data.DesignMacrossName = value;
                }

                if (mMacrossGetter == null)
                {
                    mMacrossGetter = Macross.TtMacrossGetter<TtDesignMacrossBase>.NewInstance(value);
                }
                else
                {
                    mMacrossGetter.Name = value;
                }

                if (mMacrossGetter.Get() != null)
                {
                    mMacrossGetter.Get().MacrossNode = this;
                    if(Parent != null)
                    {
                        var task = mMacrossGetter.Get().Initialize();
                        TtEngine.Instance.TaskCollector.AddWaitTask(task);
                    }
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"Design Macross({value}) Get failed");
                }
            }
        }
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtDesignMacrossNode>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            var mcrs = MacrossGetter?.Get();
            if (mcrs != null && mcrs.IsInitialized)
            {
                mcrs.PreTick(args.World.DeltaTimeSecond);
                mcrs.Tick(args.World.DeltaTimeSecond);
                mcrs.AfterTick(args.World.DeltaTimeSecond);
            }
            return base.OnTickLogic(args);
        }
    }
}
