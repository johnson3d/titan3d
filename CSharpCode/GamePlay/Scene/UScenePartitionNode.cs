﻿using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EngineNS.Bricks.Terrain.CDLOD.TtTerrainNode;

namespace EngineNS.GamePlay.Scene
{
    /// <summary>
    /// 包含Scene中该Level的node指针，用来
    /// </summary>
    public partial class UScenePartitionLevel : TtNode
    {

    }

    /// <summary>
    /// 该节点用来流式管理Scene，节点和TerrainNode的Levels个数相同，大小也一致。
    /// 动态加载当前Node和相邻的节点
    /// 可Partial的Node在加入UScene后会通过PartitioningScene()自动加入UScenePartitionLevel
    /// 非partial的Node仍然在Scene的Children中
    /// </summary>
    //[DependencyNode(UTerrainNode)]
    public partial class UScenePartitionNode : TtNode
    {
        public class UScenePartitionNodeData : TtSceneData
        {
            [Rtti.Meta]
            public int NumOfLevelX { get; set; } = 100;
            [Rtti.Meta]
            public int NumOfLevelZ { get; set; } = 100;
        }
        UScenePartitionLevel[,] Levels = null;
        UScenePartitionLevel[,] ActiveLevels = null;

        public int NumOfLevelX;
        public int NumOfLevelZ;
        public int ActiveLevel;
        public float LevelSize;
        public DVector3 EyeCenter;
        public Vector3 EyeLocalCenter;

        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            return await base.InitializeNode(world, data, bvType, placementType);
        }

        Vector2i CurrentActiveCenterLevel = new Vector2i(-1, -1);
        public bool SetActiveCenter(in DVector3 pos)
        {
            var idxLevel = GetLevelIndex(in pos);
            if (idxLevel.X < 0 || idxLevel.Y < 0 || idxLevel.X >= NumOfLevelX || idxLevel.Y >= NumOfLevelZ)
            {
                return false;
            }

            if (idxLevel == CurrentActiveCenterLevel)
            {
                return false;
            }
            CurrentActiveCenterLevel = idxLevel;

            int xMin = idxLevel.X - ActiveLevel;
            int xMax = idxLevel.X + ActiveLevel;

            int zMin = idxLevel.Y - ActiveLevel;
            int zMax = idxLevel.Y + ActiveLevel;
            int activeX = 0, activeZ = 0;
            for(int i = xMin; i <= xMax; i++, activeX++)
            {
                for(int j = zMin; j <= zMax; j++, activeZ++)
                {
                    ActiveLevels[activeX, activeZ] = Levels[i, j];
                }
            }
            return true;
        }

        void PartitioningScene()
        {
            System.Diagnostics.Debug.Assert(Parent is TtScene);
            var parentScene = Parent as TtScene;
            foreach (var i in parentScene.Children)
            {
                if (i == this)
                    continue;

                //可partial的node才可以
                //if(!(i is PartialNode))
                {
                    var nodePos = i.Placement.AbsTransform.Position;
                    var level = GetLevel(nodePos);
                    if (level != null)
                    {
                        parentScene.Children.Remove(i);
                        level.Children.Add(i);
                    }
                }
            }


            //子Level BVH Update
            //foreach(var i in Levels)
            //{
            //    i.UpdateBVH()
            //}
        }

        public override void TickLogic(TtNodeTickParameters args)
        {
            //可能不需要每帧都分割场景
            PartitioningScene();

            if (OnTickLogic(args.World, args.Policy) == false)
                return;

            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.TickLogic(args);
            }

            if (args.IsTickChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].TickLogic(args);
                }
            }
        }
        public override bool OnTickLogic(TtWorld world, TtRenderPolicy policy)
        {
            EyeCenter = policy.DefaultCamera.mCoreObject.GetPosition();
            EyeLocalCenter = policy.DefaultCamera.mCoreObject.GetLocalPosition();

            if (SetActiveCenter(in EyeCenter))
            {
                world.CameraOffset = EyeCenter;
                policy.DefaultCamera.mCoreObject.SetMatrixStartPosition(in EyeCenter);
            }

            return base.OnTickLogic(world, policy);
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            foreach (var i in ActiveLevels)
            {
                if (i == null)
                    continue;
                i.OnGatherVisibleMeshes(rp);
            }
        }


        public Vector2i GetLevelIndex(in DVector3 pos)
        {
            var nsPos = pos - this.Placement.AbsTransform.mPosition;
            Vector2i result;
            result.X = (int)(nsPos.X / LevelSize);
            result.Y = (int)(nsPos.Z / LevelSize);
            return result;
        }
        public UScenePartitionLevel GetLevel(in DVector3 pos)
        {
            var idxLevel = GetLevelIndex(in pos);
            if (idxLevel.X < 0 || idxLevel.Y < 0 || idxLevel.X >= NumOfLevelX || idxLevel.Y >= NumOfLevelZ)
            {
                return null;
            }
            return Levels[idxLevel.Y, idxLevel.X];
        }
    }
}
