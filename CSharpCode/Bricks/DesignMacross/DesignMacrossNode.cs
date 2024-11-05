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
            [Rtti.Meta]
            [RName.PGRName(FilterExts = EngineNS.DesignMacross.UDesignMacross.AssetExt)]
            public RName DesignMacrossName { get; set; }
        }
        Macross.UMacrossGetter<TtDesignMacrossBase> mMacrossGetter = null;
        public Macross.UMacrossGetter<TtDesignMacrossBase> MacrossGetter
        {
            get
            {
                if(mMacrossGetter != null)
                {
                    return mMacrossGetter;
                }
                else
                {
                    if (DesignMacross != null && !RName.IsEmpty(DesignMacross))
                    {
                        var macrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                        macrossGetter.Name = DesignMacross;
                        if (macrossGetter.Get() != null)
                        {
                            mMacrossGetter = macrossGetter;
                            mMacrossGetter.Get().MacrossNode = this;
                            _ = mMacrossGetter.Get().Initialize();
                            return mMacrossGetter;
                        }
                    }
                    return null;
                }
            }
        }
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
                if(mMacrossGetter == null)
                {
                    mMacrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                }
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
                if (mMacrossGetter == null)
                {
                    mMacrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                }
                if (NodeData is TtDesignMacrossNodeData data)
                {
                    data.DesignMacrossName = value;
                    if (value == null)
                    {
                        mMacrossGetter.Name = null;
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
        public override bool OnTickLogic(TtWorld world, TtRenderPolicy policy)
        {
            if (MacrossGetter != null && MacrossGetter.Get() != null && MacrossGetter.Get().IsInitialized)
            {
                MacrossGetter.Get().PreTick(world.DeltaTimeSecond);
                MacrossGetter.Get().Tick(world.DeltaTimeSecond);
                MacrossGetter.Get().AfterTick(world.DeltaTimeSecond);
            }
            return base.OnTickLogic(world, policy);
        }
        public override void TickLogic(TtNodeTickParameters args)
        {
            
            base.TickLogic(args);
        }
    }
}
