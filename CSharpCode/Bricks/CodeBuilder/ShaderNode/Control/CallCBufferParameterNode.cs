using EngineNS.Bricks.NodeGraph;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Graphics.Pipeline.UserParameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    [Macross.TtMacross]
    public partial class TtCBufferSetter : Macross.AuxMacrossObject
    {
        [Rtti.Meta("")]
        public virtual NxRHI.TtCbView GetCBuffer(Graphics.Mesh.TtRenderMesh.TtAtom atom, TtCBufferParameter cbuffer, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var world = policy.GetWorld();
            if (world == null)
                return null;

            var host = atom.HostNode;
            if (host == null)
                return null;
            var pos = host.Placement.Position;
            Aabb aabb;
            aabb.Center = pos;
            aabb.Extent = new Vector3(10,10,10);
            List<TtNode> candidates = new List<TtNode>();
            world.CollideOctree.GetColliding(candidates, in aabb);

            foreach(var i in candidates)
            {
                //find TtCBufferParameterNode 
                if (i is Graphics.Pipeline.UserParameters.TtCBufferParameterNode node)
                {
                    return node.CBuffer;
                }
            }
            
            return null;
        }
    }

    [ContextMenu("CBufferParam", "Data\\CBuffer@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class TtCallCBufferParameterNode : CallNode
    {
        RName mCBufferParameterName;
        [Rtti.Meta("", Order = 1)]
        [Category("Option")]
        [RName.PGRName(FilterExts = TtCBufferParameter.AssetExt)]
        public RName CBufferParameterName
        {
            get
            {
                return mCBufferParameterName;
            }
            set
            {
                if (mCBufferParameterName == value)
                    return;
                mCBufferParameterName = value;

                CBufferParameter = value.GetAsset<TtCBufferParameter>().GetResultUntilCompleted();
                if (CBufferParameter != null)
                {
                    CBufferParameter.UpdateMethodMeta();
                    this.Initialize(CBufferParameter.MethodMeta);
                    this.Name = CBufferParameter.CallNodeName;
                }
            }
        }
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta, Bricks.CodeBuilder.ShaderNode.TtMaterialGraph MaterialGraph)
        {
            ameta.AddReferenceAsset(CBufferParameterName);
        }
        public TtCBufferParameter CBufferParameter { get; private set; }

        public TtCallCBufferParameterNode()
        {
            Icon = TtMaterialEditorStyles.Instance.FunctionIcon;
            TitleColor = 0xFF4040AF;
            BackColor = 0x80808080;
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (CBufferParameter == null)
                return;

            base.BuildStatements(pin, ref data);
        }
        [Rtti.Meta("", Order = 1)]
        [RName.PGRName(FilterExts = EngineNS.Bricks.CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtCBufferSetter))]
        [Category("Option")]
        public RName McName
        {
            get
            {
                if (mMcObject == null)
                    return null;
                return mMcObject.Name;
            }
            set
            {
                if (value == null)
                {
                    mMcObject = null;
                    return;
                }
                if (mMcObject == null)
                {
                    mMcObject = Macross.TtMacrossGetter<TtCBufferSetter>.NewInstance();
                }
                mMcObject.Name = value;
            }
        }
        Macross.TtMacrossGetter<TtCBufferSetter> mMcObject;
        public Macross.TtMacrossGetter<TtCBufferSetter> McObject
        {
            get
            {
                if (mMcObject == null)
                    mMcObject = Macross.TtMacrossGetter<TtCBufferSetter>.NewInstance();
                return mMcObject;
            }
        }
        public void OnDrawCall(Graphics.Mesh.TtRenderMesh.TtAtom atom, TtCBufferParameter cbuffer, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var binder = drawcall.FindBinder(cbuffer.GetCBufferName());
            if (binder.IsValidPointer)
            {
                drawcall.BindCBV(binder, McObject?.Get()?.GetCBuffer(atom, cbuffer, drawcall, policy));
            }
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
	partial class TtCBufferSetter
	{
		public unsafe NxRHI.TtCbView macross_GetCBuffer (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Graphics.Mesh.TtRenderMesh.TtAtom atom, TtCBufferParameter cbuffer, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetCBuffer(atom, cbuffer, drawcall, policy);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross