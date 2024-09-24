using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
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
            [Rtti.Meta]
            [RName.PGRName(FilterExts = EngineNS.DesignMacross.UDesignMacross.AssetExt)]
            public RName DesignMacrossName { get; set; }
        }
        Macross.UMacrossGetter<TtDesignMacrossBase> mMacrossGetter = null;
        public override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            NodeData = data;
            return await base.InitializeNode(world, data, bvType, placementType);
        }

        public override async Thread.Async.TtTask OnNodeLoaded(TtNode parent)
        {
            await base.OnNodeLoaded(parent);
            if (DesignMacross != null && !RName.IsEmpty(DesignMacross))
            {
                mMacrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                mMacrossGetter.Name = DesignMacross;
                mMacrossGetter.Get().MacrossNode = this;
                await mMacrossGetter.Get().Initialize();
            }
        }
        [RName.PGRName(FilterExts = UDesignMacross.AssetExt)]
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
                    if (value == null)
                    {
                        mMacrossGetter = null;
                        return;
                    }
                }
                
                mMacrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                mMacrossGetter.Name = value;
                if(mMacrossGetter.Get() != null)
                {
                    mMacrossGetter.Get().MacrossNode = this;
                    _ = mMacrossGetter.Get().Initialize();
                }
            }
        }
        public override void TickLogic(TtNodeTickParameters args)
        {
            if(mMacrossGetter!= null && mMacrossGetter.Get() != null && mMacrossGetter.Get().IsInitialized)
            {
                mMacrossGetter.Get().Tick(args.World.DeltaTimeSecond);
            }
            base.TickLogic(args);
        }
    }
}
