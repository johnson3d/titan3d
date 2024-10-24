using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.Scene.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Character
{
    [Bricks.CodeBuilder.ContextMenu("Character", "Character", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCharacterData), DefaultNamePrefix = "Character")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtCharacter : TtActor
    {
        public partial class TtCharacterData : TtActorData
        {
        }

        public TtCharacterData CharacterData
        {
            get
            {
                return NodeData as TtCharacterData;
            }
        }
        partial void CreatePxCapsuleController(ref bool result, Scene.TtScene scene, float radius, float height);
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
            {
                return false;
            }

            var bv = this.BoundVolume as UBoxBV;
            bv.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);
            return true;
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);
        }
        protected override void OnParentSceneChanged(TtScene prev, TtScene cur)
        {
            if (cur != null)
            {
                bool ok = true;
                CreatePxCapsuleController(ref ok, cur, CharacterData.Radius, CharacterData.Height);
            }
        }
    }
}
