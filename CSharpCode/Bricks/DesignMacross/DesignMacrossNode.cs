using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.DesignMacross
{
    [Bricks.CodeBuilder.ContextMenu("DesignMacrossNode", "DesignMacrossNode", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(TtDesignMacrossNodeData), DefaultNamePrefix = "DM")]
    public class TtDesignMacrossNode : USceneActorNode
    {
        public class TtDesignMacrossNodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = EngineNS.DesignMacross.UDesignMacross.AssetExt)]
            public RName DesignMacrossName { get; set; }
        }
        Macross.UMacrossGetter<TtDesignMacrossBase> mMacrossGetter = null;
        public override TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if(DesignMacross!=null && !RName.IsEmpty(DesignMacross))
            {
                mMacrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                mMacrossGetter.Name = DesignMacross;
                mMacrossGetter.Get().MacrossNode = this;
                mMacrossGetter.Get().Initialize();
            }
            return base.InitializeNode(world, data, bvType, placementType);
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
                }
                mMacrossGetter = Macross.UMacrossGetter<TtDesignMacrossBase>.NewInstance();
                mMacrossGetter.Name = value;
                if(mMacrossGetter.Get() != null)
                {
                    mMacrossGetter.Get().MacrossNode = this;
                    mMacrossGetter.Get().Initialize();
                }
            }
        }
        public override void TickLogic(TtNodeTickParameters args)
        {
            if(mMacrossGetter!= null && mMacrossGetter.Get() != null)
            {
                mMacrossGetter.Get().Tick(args.World.TimeSecond);
            }
            base.TickLogic(args);
        }
    }
}
