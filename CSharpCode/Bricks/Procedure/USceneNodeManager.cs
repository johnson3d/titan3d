using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    [NodeGraph.Expandable("PGC")]
    [BufferTypeOperator(typeof(FPgcSpawnSceneNodeDescOperator))]
    public struct PgcSpawnSceneNodeDesc
    {
        public DVector3 Location;
        public Quaternion Rotation;
        public Vector3 Scale;
        public int TypeId;
    }

    public struct FPgcSpawnSceneNodeDescOperator : ISuperPixelOperator<PgcSpawnSceneNodeDesc>
    {
        public Rtti.UTypeDesc ElementType => Rtti.UTypeDescGetter<PgcSpawnSceneNodeDesc>.TypeDesc;
        public Rtti.UTypeDesc BufferType => Rtti.UTypeDescGetter<USuperBuffer<PgcSpawnSceneNodeDesc, FPgcSpawnSceneNodeDescOperator>>.TypeDesc;
        public PgcSpawnSceneNodeDesc MaxValue
        {
            get
            {
                return new PgcSpawnSceneNodeDesc()
                {
                    Location = new DVector3(double.MaxValue, double.MaxValue, double.MaxValue),
                    Rotation = Quaternion.Identity,
                    Scale = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                    TypeId = int.MaxValue
                };
            }
        }
        public PgcSpawnSceneNodeDesc MinValue
        {
            get
            {
                return new PgcSpawnSceneNodeDesc()
                {
                    Location = new DVector3(double.MinValue, double.MinValue, double.MinValue),
                    Rotation = Quaternion.Identity,
                    Scale = new Vector3(float.MinValue, float.MinValue, float.MinValue),
                    TypeId = int.MinValue
                };
            }
        }
        public unsafe void Abs(void* result, void* left)
        {
        }
        public PgcSpawnSceneNodeDesc Add(in PgcSpawnSceneNodeDesc left, in PgcSpawnSceneNodeDesc right)
        {
            return left;
        }

        public unsafe void Add(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
        }

        public int Compare(in PgcSpawnSceneNodeDesc left, in PgcSpawnSceneNodeDesc right)
        {
            return 0;
        }

        public unsafe int Compare(void* left, void* right)
        {
            return 0;
        }

        public unsafe void Copy(UTypeDesc tarTyp, void* tar, UTypeDesc srcType, void* src)
        {
            (*(PgcSpawnSceneNodeDesc*)tar) = (*(PgcSpawnSceneNodeDesc*)src);
        }

        public unsafe void Div(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
        }

        public unsafe void Lerp(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right, float factor)
        {
        }

        public unsafe void Max(void* result, void* left, void* right)
        {
        }

        public unsafe void Min(void* result, void* left, void* right)
        {
        }

        public unsafe void Mul(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
        }

        public unsafe void SetAsMaxValue(void* tar)
        {
        }

        public unsafe void SetAsMinValue(void* tar)
        {
        }

        public unsafe void SetIfGreateThan(UTypeDesc tarTyp, void* tar, UTypeDesc srcType, void* src)
        {
        }

        public unsafe void SetIfLessThan(UTypeDesc tarTyp, void* tar, UTypeDesc srcType, void* src)
        {
        }

        public unsafe void Sub(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
        }
    }

    public class PgcSceneNodeManager
    {
        List<GamePlay.Scene.TtNode> mNodes = new List<GamePlay.Scene.TtNode>();

        public void RemoveAll(GamePlay.Scene.TtNode node)
        {
            for(int i=0; i< mNodes.Count; i++)
            {
                node.Parent = null;
            }
            mNodes.Clear();
        }
        public bool UpdatePgcSceneNodes(UBufferComponent buffer, GamePlay.Scene.TtNode node)
        {
            if (buffer.BufferCreator.ElementType != Rtti.UTypeDesc.TypeOf(typeof(PgcSpawnSceneNodeDesc)))
                return false;

            RemoveAll(node);
            for(int i=0; i<buffer.Width; i++)
            {
                var desc = buffer.GetPixel<PgcSpawnSceneNodeDesc>(i);
                // todo: get node from TypeId
                var tagNode = new GamePlay.Scene.TtMeshNode();
                tagNode.Parent = node;
                tagNode.Placement.SetTransform(desc.Location, desc.Scale, desc.Rotation);
                tagNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                tagNode.NodeData.Name = "pgc_" + i;
                tagNode.IsAcceptShadow = false;
                tagNode.IsCastShadow = true;
            }

            return true;
        }
    }
}
namespace EngineNS
{
    partial class TtEngine
    {
        public EngineNS.Bricks.Procedure.PgcSceneNodeManager PgcSceneNodeManager { get; } = new Bricks.Procedure.PgcSceneNodeManager();
    }
}