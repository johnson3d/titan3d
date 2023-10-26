using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class Tt2DUIShading : Shader.UGraphicsShadingEnv
    {
        public Tt2DUIShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/2DUI.cginc", RName.ERNameType.Engine);
            this.UpdatePermutation();
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[]
            {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Color,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_SkinIndex,
            };
        }
    }
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class Tt2DUINode : USceenSpaceNode
    {
        public Tt2DUINode()
        {
            Name = "2DUINode";

        }
        public override void InitNodePins()
        {

            base.InitNodePins();
        }
    }
}
